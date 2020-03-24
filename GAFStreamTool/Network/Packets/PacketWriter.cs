using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAFStreamTool.Network.Packets
{
    public class PacketWriter
    {
        public IPacket Packet { get; }

        private const int _PACKET_HEADER_LENGTH = 4 + 1;
        private static readonly Encoding _encoding = Encoding.UTF8;
        
        private List<byte> _data;

        public PacketWriter(IPacket packet)
        {
            Packet = packet;
            _data = new List<byte>();
        }

        public void Write(short v, int index = -1)
        {
            Write(index, BitConverter.GetBytes(v));
        }
        public void Write(int v, int index = -1)
        {
            Write(index, BitConverter.GetBytes(v));
        }
        public void Write(long v, int index = -1)
        {
            Write(index, BitConverter.GetBytes(v));
        }
        
        public void Write(float v, int index = -1)
        {
            Write(index, BitConverter.GetBytes(v));
        }
        public void Write(double v, int index = -1)
        {
            Write(index, BitConverter.GetBytes(v));
        }

        public void Write(bool v, int index = -1)
        {
            Write(index, BitConverter.GetBytes(v));
        }
        
        public void Write(byte b, int index = -1)
        {
            Write(index, b);
        }

        public void Write(int index = -1, params byte[] bytes)
        {
            if (index > -1 && index < _data.Count)
            {
                for (int i = 0; i < bytes.Length; i++)
                    _data.Insert(index + i, bytes[i]);

                return;
            }

            for (int i = 0; i < bytes.Length; i++)
                _data.Add(bytes[i]);
        }
        
        public void Write(string v, int index = -1)
        {
            Write(v.Length, index);
            byte[] stringBuffer = _encoding.GetBytes(v);
            Write(index == -1 ? -1 : index + 4, stringBuffer);
        }

        public byte[] Build()
        {
            byte[] length = BitConverter.GetBytes(_data.Count);
            byte[] packetId = new byte[] { Packet.Id };
            int index = 0;

            byte[] data = new byte[_PACKET_HEADER_LENGTH + _data.Count];
            data = data.Replace(length, index);
            index += 4;

            data = data.Replace(packetId, index);
            index += 1;

            data = data.Replace(_data.ToArray(), index);

            return data;
        }
    }


    public class PacketReader
    {
        public IPacket Packet { get; }
        public int Index => _index;
        public int Length => _data.Length;
        public bool ReachedEnd => _index >= _data.Length;

        private static readonly Encoding _encoding = Encoding.UTF8;

        private byte[] _data;
        private int _index;
        
        public PacketReader(IPacket packet, byte[] data)
        {
            Packet = packet;
            _data = data;
        }

        public short ReadInt16()
        {
            byte[] bytes = ReadBytes(2);
            return BitConverter.ToInt16(bytes, 0);
        }
        public int ReadInt32()
        {
            byte[] bytes = ReadBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }
        public long ReadInt64()
        {
            byte[] bytes = ReadBytes(8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public float ReadFloat()
        {
            byte[] bytes = ReadBytes(4);
            return BitConverter.ToSingle(bytes, 0);
        }
        public double ReadDouble()
        {
            byte[] bytes = ReadBytes(8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public bool ReadBool()
        {
            byte b = ReadByte();
            return b == 0 ? false : true;
        }
        
        public string ReadString()
        {
            int length = ReadInt32();
            byte[] stringBuffer = ReadBytes(length);

            return _encoding.GetString(stringBuffer);
        }

        public byte[] ReadBytes(int length)
        {
            if (_index + length > _data.Length)
                length = _data.Length - _index;

            byte[] bytes = new byte[length];

            Array.Copy(_data, _index, bytes, 0, length);

            _index += length;

            return bytes;
        }
        
        public byte ReadByte()
        {
            byte b = _data[_index];

            _index++;

            return b;
        }
    }
}
