using System;
using System.Threading;

namespace GAFBot.API
{

    /// <summary>
    /// D = Discord,
    /// O = Osu,
    /// C = Challonge,
    /// B = Betting,
    /// V = Verification,
    /// CS = Command System
    /// </summary>
    public enum ApiEndPoints : byte
    {
        DChannelMessage = 0x1,
        DUserMessage = 0x2,


        DGuildExist = 0x3,
        DChannelExist = 0x4,
        DUserExist = 0x5,

        DCreateEmbed = 0x6,
    }

    public class ApiHandler
    {
        private APIServer _server;

        public ApiHandler()
        {
            _server = new APIServer();
            _server.OnPacketRecieved += _server_OnPacketRecieved;
            _server.OnPacketSent += _server_OnPacketSent;
            _server.Start();
        }

        private void SendPacket(Packets.Packet p, APIServerClient client)
        {
            new Action<Packets.Packet>(client.Write).Invoke(p);
        }

        private void _server_OnPacketSent(Packets.Packet p, APIServerClient client)
        {

        }

        private void _server_OnPacketRecieved(Packets.Packet p, APIServerClient client)
        {

        }
    }
}
