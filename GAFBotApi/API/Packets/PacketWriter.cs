using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Packets
{
    public class PacketWriter
    {
        public byte[] PacketData { get; private set; }
        private int _index;

        public PacketWriter(int length)
        {
            PacketData = new byte[length];
        }

        public void WriteByte(int index, byte value)
        {
            PacketData[index] = value;
        }

        public void WriteBytes(int index, byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
                PacketData[index] = value[i];
        }

        public void WriteInt(int index, int value)
        {
            byte[] integer = BitConverter.GetBytes(value);

            for (int i = 0; i < 4; i++)
                PacketData[index + i] = integer[i];
        }

        public void WriteDouble(int index, double value)
        {
            byte[] double_ = BitConverter.GetBytes(value);

            for (int i = 0; i < 8; i++)
                PacketData[index + i] = double_[i];
        }

        public void WriteULong(int index, ulong value)
        {
            byte[] ulong_ = BitConverter.GetBytes(value);

            for (int i = 0; i < 8; i++)
                PacketData[index + i] = ulong_[i];
        }

        public void WriteString(int index, string value)
        {
            byte[] string_ = Encoding.UTF8.GetBytes(value);
            byte[] length = BitConverter.GetBytes(string_.Length);

            for (int i = 0; i < 4; i++)
                PacketData[index + i] = length[i];

            for (int i = 0; i < string_.Length; i++)
                PacketData[index + 4 + i] = string_[i];
        }

        public Packet Build()
            => new Packet(PacketData);
    }

}
