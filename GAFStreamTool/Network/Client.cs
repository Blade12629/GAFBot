
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GAFStreamTool.Encryption;
using GAFStreamTool.Network.Packets;

namespace GAFStreamTool.Network
{
    public partial class Client : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public State CurrentState { get; set; }

        private Socket _socket;

        public Client(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public void ConnectAsync()
        {
            try
            {
                CurrentState = State.Connecting;

                if (_socket != null && _socket.Connected)
                    Disconnect();

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(Host), Port), new AsyncCallback(EndConnect), null);
            }
            catch (Exception ex)
            {
                CurrentState = State.Failed;
            }
        }

        private void EndConnect(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);

                if (_socket.Connected)
                    CurrentState = State.Connected;
                else
                    CurrentState = State.Failed;
            }
            catch (Exception ex)
            {
                CurrentState = State.Failed;
            }
        }

        public void Disconnect()
        {
            CurrentState = State.Disconnecting;
            _socket.Dispose();
            CurrentState = State.Disconnected;
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
            Failed
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
            CurrentState = State.Reading;

            _readerQueue = new ConcurrentQueue<byte[]>();
            _readerSource = new CancellationTokenSource();
            _readerToken = _readerSource.Token;
            _readerTask = new Task(ReadAsync, _readerToken);
            _readerHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
            _readerTask.Start();
        }

        private void ReadAsync()
        {
            while(!_readerSource.IsCancellationRequested && _socket.Connected && CurrentState == State.Reading)
            {
                _readerHandle.WaitOne();

                StateObject so = new StateObject(2048);
                _socket.BeginReceive(so.Array, 0, 2048, SocketFlags.None, new AsyncCallback(EndRead), so);
            }
        }

        private void EndRead(IAsyncResult ar)
        {
            try
            {
                int length = _socket.EndReceive(ar);
                StateObject so = (StateObject)ar.AsyncState;

                if (length <= 0)
                {
                    _readerHandle.Set();
                    return;
                }

                so.Resize(length);

                string dataStr = "";

                foreach (byte b in so.Array)
                    dataStr += b;

                _readerQueue.Enqueue(so.Array);
                _readerHandle.Set();

                Task.Run(() => HandleData());
            }
            catch(Exception ex)
            {
                CurrentState = State.Failed;
            }
        }

        private void HandleData()
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

                IPacket packet = PacketHandler.Packets[packetId];

                PacketReader reader = new PacketReader(packet, data.GetRange(index, packetLength));

                packet.Handle(reader, this);
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

                string dataStr = "";

                foreach (byte b in tosend)
                    dataStr += b;

                _socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(EndWrite), null);
            }
            catch (Exception ex)
            {
                CurrentState = State.Failed;
            }
        }

        private void EndWrite(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
            }
            catch (Exception ex)
            {
                CurrentState = State.Failed;
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
