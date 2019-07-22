using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace GAFBot.API
{
    public class APIServer
    {
        private TcpListener _listener;
        private Dictionary<long, APIClient> _clients;

        private Task _listenerTask;
        private CancellationTokenSource _listenerTokenSource;
        private CancellationToken _listenerToken;

        public int Port { get; private set; }
        public IPAddress ListenerIP { get; private set; }
        public bool Active { get; private set; }

        public event Action<long> OnClientConnected;

        public APIServer(int port, IPAddress listenerIP)
        {
            Port = port;
            ListenerIP = listenerIP;
            _clients = new Dictionary<long, APIClient>();
        }
        
        public void Start()
        {
            if (Active)
                return;

            Active = true;

            _listener = new TcpListener(ListenerIP, Port);

            _listenerTokenSource = new CancellationTokenSource();
            _listenerToken = _listenerTokenSource.Token;

            _listenerTask = Task.Run(StartListening, _listenerToken);
        }

        public void Stop()
        {
            if (!Active)
                return;

            _listenerTokenSource.Cancel(false);
            
            Active = false;
        }
        
        private async Task StartListening()
        {
            _listener.Start();

            TcpClient tcpClient;
            APIClient apiClient;
            long sessionId;
            while (Active)
            {
                tcpClient = await _listener.AcceptTcpClientAsync();

                if (tcpClient == null)
                    continue;

                sessionId = DateTime.UtcNow.Ticks;
                apiClient = new APIClient(tcpClient, sessionId);
                apiClient.OnDataRecieved += OnAPIDataRecieved;
                apiClient.OnClientDisconnected += OnClientDisconnected;
                _clients.Add(sessionId, apiClient);
                apiClient.StartReading();

                OnClientConnected?.Invoke(sessionId);
            }
        }

        private void OnClientDisconnected(long obj)
        {
            //safe-check
            if (_clients.ContainsKey(obj))
                _clients.Remove(obj);
        }

        private void OnAPIDataRecieved(long sessionId, byte[] data)
        {
            try
            {
                string result = "";
                foreach (var b in data)
                    result += b + ", ";
                Program.Logger.Log("API: Incoming data: " + result);

                BufferReader reader = new BufferReader(data);

                byte cmd = reader.ReadByte();
                if (data.Length > 1)
                {
                    byte java = reader.ReadByte();

                    if (java == 1)
                        reader.NormalizeJavaArray();
                }

                APICalls.Invoke(cmd, reader, sessionId);

                ////TODO: if cmd exists pass reader to api handler for invocation

                ////temporary implementation
                //if (cmd != 1)
                //    return;

                //ulong channel = reader.ReadUlong();

                //if (channel <= 1000)
                //    return;

                //string message = reader.ReadString();

                //if (string.IsNullOrEmpty(message))
                //    return;

                //Coding.Methods.SendMessage(channel, message);
            }
            catch (Exception ex)
            {
                Program.Logger.Log("API: " + ex.ToString(), showConsole: Program.Config.Debug);
            }
        }
        
        internal void Write(long sessionId, byte[] buffer)
        {
            if (!_clients.ContainsKey(sessionId) || !_clients[sessionId].Valid || _clients[sessionId].Disconnected)
                return;

            _clients[sessionId]?.Write(buffer);
        }
    }
}
