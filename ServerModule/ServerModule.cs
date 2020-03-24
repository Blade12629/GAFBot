using System;
using GAFBot;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.Modules;
using ServerModule.Network.Packets;

namespace ServerModule
{
    public class ServerModule : IModule
    {
        private bool? _enabled;
        public bool Enabled
        {
            get
            {
                if (!_enabled.HasValue)
                    _enabled = true;

                return _enabled.Value;
            }
            set
            {
                _enabled = value;
            }
        }

        public string ModuleName => "server";

        private Network.Server _server;

        public void Disable()
        {
            _server?.Dispose();
        }

        public void Dispose()
        {
            Disable();
        }

        public void SendPickToAllClients(Data.Pick p = null)
        {
        }

        public void Enable()
        {
            _server = new Network.Server(System.Net.IPAddress.Any, 40015);
            _server.Start();
        }

        public void Initialize()
        {
            Enable();
        }
    }
}
