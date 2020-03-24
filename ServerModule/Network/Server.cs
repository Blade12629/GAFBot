using GAFBot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ServerModule.Network
{
    public partial class Server
    {
        public IPAddress IP { get; private set; }
        public int Port { get; private set; }
        public DirectoryInfo EncKeyDir { get; private set; }

        private Socket _socket;

        public Server(IPAddress ip, int port)
        {
            _clients = new ConcurrentDictionary<Guid, Client>();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(ip, port));
            EncKeyDir = new DirectoryInfo(Path.Combine(Program.CurrentPath, "serverkeys\\"));

            if (!EncKeyDir.Exists)
                EncKeyDir.Create();
        }

        public void Start()
        {
            Logger.Log("Starting Server", LogLevel.Trace);
            _socket.Listen(10);

            _recieveSource = new CancellationTokenSource();
            _recieveToken = _recieveSource.Token;
            _recieveTask = new Task(() => RecieveClients(), _recieveToken);

            _recieveTask.Start();

            Logger.Log("Server Started", LogLevel.Trace);
        }

        public void Dispose()
        {
            Logger.Log("Closing Server", LogLevel.Trace);
            _socket.Close();
        }
    }

    //Client handle part
    public partial class Server : IDisposable
    {
        private Task _recieveTask;
        private ConcurrentDictionary<Guid, Client> _clients;
        private EventWaitHandle _recieveHandle;
        private CancellationToken _recieveToken;
        private CancellationTokenSource _recieveSource;
        private const int _PING_TIMEOUT = 3;

        public void RecieveClients()
        {
            _recieveHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            while(!_recieveToken.IsCancellationRequested)
            {
                _socket.BeginAccept(new AsyncCallback(EndRecieveClient), null);
                _recieveHandle.WaitOne();
            }
        }

        private void EndRecieveClient(IAsyncResult ar)
        {
            Socket s = _socket.EndAccept(ar);

            _recieveHandle.Set();

            if (s == null)
                return;
            
            Client c = new Client(s)
            {
                Server = this
            };

            Logger.Log($"New client connected {c.Host} ({c.Id})");

            _clients.TryAdd(c.Id, c);
            c.StartReading();
        }

        public void RemoveClient(Guid guid)
        {
            _clients.TryRemove(guid, out Client value);
        }

        private void KeepAliveCheck(object sender, ElapsedEventArgs e)
        {
            List<Client> clients = _clients.ToList().Select(p => p.Value).ToList();
            Packets.Packets.PingPacket ping = new Packets.Packets.PingPacket();
            Packets.PacketWriter writer = new Packets.PacketWriter(ping);

            foreach (Client c in clients)
            {
                ping.Send(writer, c);

                if (DateTime.UtcNow.Hour == c.LastPingResponse.Hour)
                {
                    if (DateTime.UtcNow.Minute - c.LastPingResponse.Minute > _PING_TIMEOUT)
                    {
                        _clients.TryRemove(c.Id, out Client cl);
                        c.Dispose();
                        continue;
                    }
                }
                else
                {
                    int left = 60 - c.LastPingResponse.Minute;
                    int passed = left + DateTime.UtcNow.Minute;

                    if (passed > _PING_TIMEOUT)
                    {
                        _clients.TryRemove(c.Id, out Client cl);
                        c.Dispose();
                        continue;
                    }
                }
            }
        }
    }

    //Write/Send part
    public partial class Server
    {

    }
}
