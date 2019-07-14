using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Packets
{
    public class PacketInfo
    {
        public int PacketLength { get; set; }
        public byte PacketCmd { get; set; }
        public byte[] PacketData { get; set; }

        public static explicit operator Packet(PacketInfo packetInfo)
            => new Packet(packetInfo.PacketLength, packetInfo.PacketCmd, packetInfo.PacketData);

        public bool Equals(Packet p)
            => p.Command == PacketCmd && p.Length == PacketLength;

        public bool Equals(PacketInfo pInfo)
            => pInfo.PacketCmd == PacketCmd && pInfo.PacketLength == PacketLength;

        public override bool Equals(object obj)
        {
            if (obj is Packet p)
                return Equals(p);
            if (obj is PacketInfo pInfo)
                return Equals(pInfo);

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => PacketLength.GetHashCode() + PacketCmd.GetHashCode();

    }
}
