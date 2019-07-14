using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Packets
{
    public class Packet
    {
        public int Length { get; private set; }
        public byte Command { get; private set; }
        public byte[] Data { get; set; }

        public Packet(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);

            Length = (int)data[0];
            Command = data[1];
            Data = data.SubArray(2, data.Length - 1);
        }

        internal Packet(int length, byte command, byte[] data)
        {
            Length = length;
            Command = command;
            Data = data;
        }

        public static explicit operator PacketInfo(Packet p)
            => new PacketInfo() { PacketCmd = p.Command, PacketData = p.Data, PacketLength = p.Length};

        public bool Equals(Packet p)
            => p.Command == Command && p.Length == Length;

        public bool Equals(PacketInfo pInfo)
            => pInfo.PacketCmd == Command && pInfo.PacketLength == Length;

        public override bool Equals(object obj)
        {
            if (obj is Packet p)
                return Equals(p);
            if (obj is PacketInfo pInfo)
                return Equals(pInfo);

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => Command.GetHashCode() + Length.GetHashCode() + Data.GetHashCode();
    }
}
