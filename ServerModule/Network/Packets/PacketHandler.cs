using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerModule.Network.Packets
{
    public static class PacketHandler
    {
        private static bool _initialized;
        private static Dictionary<byte, IPacket> _packets;

        public static IReadOnlyDictionary<byte, IPacket> Packets
        {
            get
            {
                if (_packets == null)
                    Initialize();

                return _packets;
            }
        }

        private static void Initialize()
        {
            if (_initialized)
                return;

            _packets = new Dictionary<byte, IPacket>();
            RegisterDefaultPackets();
            _initialized = true;
        }

        public static void RegisterDefaultPackets()
        {
            List<IPacket> packets = new List<IPacket>()
            {
                //1
                new Packets.AuthPacket(),
                //2
                new Packets.MatchPicksPacket(),
                //3
                new Packets.PingPacket(),
                //
                new Packets.RegisterApiKeyPacket(),
            };

            foreach (IPacket p in packets)
                RegisterPacket(p);
        }

        public static void RegisterPacket(IPacket packet)
        {
            _packets.Add(packet.Id, packet);
        }
        
    }
}
