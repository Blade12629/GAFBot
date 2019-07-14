using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using GAFBot.API.Packets;

namespace GAFBot.API
{
    public class APIServer
    {
        private Thread _listenerThread;
        private List<APIServerClient> _clients;
        private TcpListener _listener;

        public event Action<Packet, APIServerClient> OnPacketRecieved;
        public event Action<Packet, APIServerClient> OnPacketSent;

        public bool Active { get; set; }

        public APIServer()
        {
            _clients = new List<APIServerClient>();
            _listenerThread = new Thread(new ThreadStart(ListenerTStart));
        }

        public void Start()
        {
            if (Active)
                return;

            Monitor.Enter(_listenerThread);

            Active = true;

            _listenerThread.Start();

            Monitor.Exit(_listenerThread);
        }

        public void Stop()
        {
            if (!Active)
                return;
            
            Monitor.Enter(_listenerThread);

            //ignore abort exception
            try
            {
                _listenerThread.Abort();
            }
            catch (Exception ex)
            {

            }

            Active = false;
            Monitor.Exit(_listenerThread);
        }

        public void Dispose()
        {
            if (Active)
                Stop();

            if (_clients == null)
                return;

            Monitor.Enter(_clients);

            foreach(APIServerClient client in _clients)
            {
                if (!client.Disposed)
                    client.Dispose();
            }

            _clients.Clear();

            Monitor.Exit(_clients);
        }

        private void ListenerTStart()
        {
            _listener = new TcpListener(IPAddress.IPv6Any, 20009);
            _listener.Start();

            while (true)
            {
                TcpClient incomingClient = _listener.AcceptTcpClient();
                if (incomingClient == null)
                    continue;

                APIServerClient client = new APIServerClient(incomingClient);
                client.OnMessageRead += Client_OnMessageRead;
                client.OnBeforeMessageSent += Client_OnBeforeMessageSent;
                _clients.Add(client);

                client.Read();
            }
        }

        private void Client_OnBeforeMessageSent(byte[] data, APIServerClient client)
        {
            Packet p = new Packet(data);
            OnPacketSent?.Invoke(p, client);
        }

        private void Client_OnMessageRead(byte[] data, APIServerClient client)
        {
            Packet p = new Packet(data);
            OnPacketRecieved?.Invoke(p, client);
        }
    }
}
