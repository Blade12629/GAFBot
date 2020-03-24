
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GAFBot;
using ServerModule.Network.Packets;

namespace ServerModule.Network
{
    public partial class Client : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public State CurrentState { get; set; }
        public Guid Id { get; private set; }
        public Server Server;
        public DateTime LastPingResponse;

        private Socket _socket;

        public Client(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public Client(Socket s)
        {
            _socket = s;
            IPEndPoint ep = (IPEndPoint)s.RemoteEndPoint;
            Host = ep.Address.ToString();
            Id = Guid.NewGuid();
        }

        public void ConnectAsync()
        {
            CurrentState = State.Connecting;

            if (_socket != null && _socket.Connected)
                Disconnect();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(Host), Port), new AsyncCallback(EndConnect), null);
        }

        private void EndConnect(IAsyncResult ar)
        {
            _socket.EndConnect(ar);
            
            if (_socket.Connected)
                CurrentState = State.Connected;
            else
                CurrentState = State.Failed;
        }

        public void Disconnect()
        {
            CurrentState = State.Disconnecting;
            _socket.Dispose();
            CurrentState = State.Disposed;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public enum State
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting,
            Reading,
            Authenticating,
            Failed,
            Disposed,
        }
    }

    public partial class Client
    {
        private byte[] _encryptionKey;
        public byte[] EncryptionKey => _encryptionKey;

        public void CreateEncryptionKey(ulong discordId)
        {
            string file = Path.Combine(Server.EncKeyDir.FullName, discordId + ".key");

            using (Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.None;
                aes.IV = new byte[16];

                aes.GenerateKey();
                using (FileStream fstream = File.OpenWrite(file))
                {
                    fstream.Write(aes.Key, 0, aes.Key.Length);
                    fstream.Flush();
                }
            }
        }

        public void UpdateEncryptionKey(ulong discordId, byte[] newKey)
        {
            string file = Path.Combine(Server.EncKeyDir.FullName, discordId + ".key");
            _encryptionKey = newKey;

            using (FileStream fstream = File.OpenWrite(file))
            {
                fstream.Write(newKey, 0, newKey.Length);
                fstream.Flush();
            }
        }

        public void LoadEncryptionKey(ulong discordId)
        {
            string file = Path.Combine(Server.EncKeyDir.FullName, discordId + ".key");

            if (!File.Exists(file))
                return;

            using (FileStream fstream = File.OpenRead("enc.key"))
            {
                _encryptionKey = new byte[fstream.Length];
                fstream.Read(_encryptionKey, 0, _encryptionKey.Length);
            }
        }

        public string EncryptString(string plainInput)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainInput);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public byte[] EncryptBytes(byte[] decrypted)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.Padding = PaddingMode.None;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter writer = new BinaryWriter((Stream)cryptoStream))
                        {
                            writer.Write(decrypted);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return array;
        }

        public byte[] DecryptBytes(byte[] encrypted)
        {
            byte[] iv = new byte[16];
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = iv;
                aes.Padding = PaddingMode.None;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(encrypted))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader reader = new BinaryReader((Stream)cryptoStream))
                        {
                            long length = reader.BaseStream.Length;
                            byte[] result = new byte[length];

                            reader.BaseStream.Read(result, 0, result.Length);

                            return result;
                        }
                    }
                }
            }
        }

        public string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public partial class Client
    {
        public bool Authenticated;

        private Task _readerTask;
        private CancellationToken _readerToken;
        private CancellationTokenSource _readerSource;
        private EventWaitHandle _readerHandle;

        private ConcurrentQueue<byte[]> _readerQueue;

        public void StartReading()
        {
            Console.WriteLine("Starting to read client output");

            CurrentState = State.Reading;

            _readerQueue = new ConcurrentQueue<byte[]>();
            _readerSource = new CancellationTokenSource();
            _readerToken = _readerSource.Token;
            _readerTask = new Task(ReadAsync, _readerToken);
            _readerHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _readerTask.Start();
        }

        private void ReadAsync()
        {
            while (!_readerSource.IsCancellationRequested && _socket.Connected && CurrentState == State.Reading)
            {
                Console.WriteLine("Waiting for data");

                StateObject so = new StateObject(2048);

                try
                {
                    _socket.BeginReceive(so.Array, 0, 2048, SocketFlags.None, new AsyncCallback(EndRead), so);
                }
                catch (Exception)
                {
                    Console.WriteLine("Lost connection to client: " + Id);
                    Server.RemoveClient(Id);
                    return;
                }

                _readerHandle.WaitOne();
            }
        }

        private void EndRead(IAsyncResult ar)
        {
            StateObject so;
            int length;
            try
            {
                length = _socket.EndReceive(ar);
                so = (StateObject)ar.AsyncState;
            }
            catch (Exception)
            {
                Console.WriteLine("Lost connection to client: " + Id);
                Server.RemoveClient(Id);
                return;
            }

            if (length <= 0)
            {
                _readerHandle.Set();
                return;
            }

            so.Resize(length);

            _readerQueue.Enqueue(so.Array);
            _readerHandle.Set();

            Task.Run(() => HandleData());
        }

        private void HandleData()
        {
            try
            {
                if (!_readerQueue.TryDequeue(out byte[] data))
                    return;

                int index = 0;
                while (index < data.Length)
                {
                    int packetLength = BitConverter.ToInt32(data, index);
                    index += 4;
                    byte packetId = data[index];
                    index++;

                    Console.WriteLine("PacketId: " + packetId);

                    if ((packetId != (int)Packets.Packets.PacketId.AuthPacket &&
                        packetId != (int)Packets.Packets.PacketId.RegisterApiKeyPacket) &&
                        !Authenticated)
                        return;

                    IPacket packet = PacketHandler.Packets[packetId];
                    byte[] packetData = data.GetRange(index, packetLength);
                    PacketReader reader = new PacketReader(packet, packetData);

                    packet.Handle(reader, this);

                    index += packetLength;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), LogLevel.ERROR);
            }
        }

        public void Login(string key)
        {
            CurrentState = State.Authenticating;
            Packets.Packets.AuthPacket auth = new Packets.Packets.AuthPacket();
            PacketWriter writer = new PacketWriter(auth);

            auth.Send(writer, this, key);
        }

        public void WriteAsync(byte[] data)
        {
            try
            {
                byte[] tosend = data;

                _socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(EndWrite), null);

                string dataStr = "";

                foreach (byte b in data)
                    dataStr += b;

                Console.WriteLine($"Sent new data: length: {data.Length}, array: {dataStr}");
            }
            catch (Exception)
            {
                Console.WriteLine("Lost connection to client: " + Id);
                Server.RemoveClient(Id);
            }
        }

        private void EndWrite(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
            }
            catch (Exception)
            {
                Console.WriteLine("Lost connection to client: " + Id);
                Server.RemoveClient(Id);
            }
        }
        
        private class StateObject
        {
            public byte[] Array;

            public StateObject(byte[] array)
            {
                Array = array;
            }

            public StateObject(int length)
            {
                Array = new byte[length];
            }

            public void Resize(int newLength)
            {
                if (Array.Length == newLength)
                    return;

                System.Array.Resize(ref Array, newLength);
            }
        }
    }
}
