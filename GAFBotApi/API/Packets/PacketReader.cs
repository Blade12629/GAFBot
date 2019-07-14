using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Packets
{

    public class PacketReader
    {
        public Packet Packet { get; private set; }
        private int _index;

        public PacketReader(Packet packet)
        {
            Packet = packet;
        }

        public int ReadInt(int index)
        {
            byte[] range = new byte[4];
            Buffer.BlockCopy(Packet.Data, index, range, 0, range.Length);

            return Convert.ToInt32(range);
        }

        public double ReadDouble(int index)
        {
            byte[] range = new byte[8];
            Buffer.BlockCopy(Packet.Data, index, range, 0, range.Length);

            return Convert.ToDouble(range);
        }

        public ulong ReadUlong(int index)
        {
            byte[] range = new byte[8];
            Buffer.BlockCopy(Packet.Data, index, range, 0, range.Length);

            return Convert.ToUInt64(range);
        }

        public string ReadString(int index)
        {
            int length = ReadInt(index);
            byte[] range = new byte[length];

            Buffer.BlockCopy(Packet.Data, index, range, 0, range.Length);

            return Encoding.UTF8.GetString(range);
        }

    }
}
