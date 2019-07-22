using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAFBot.API
{
    public class BufferWriter
    {
        private List<byte> _buffer;

        public BufferWriter()
        {
            _buffer = new List<byte>();
        }

        private void CheckReverse(ref byte[] source)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(source, 0, source.Length);
        }

        public void Reset()
            => _buffer.Clear();

        public byte[] ToArray()
            => _buffer.ToArray();

        public void WriteByte(byte b)
            => _buffer.Add(b);

        public void WriteString(string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            WriteInt32(buffer.Length);

            _buffer.AddRange(buffer);
        }

        public void WriteUlong(ulong val)
        {
            byte[] buffer = BitConverter.GetBytes(val);
            CheckReverse(ref buffer);
            _buffer.AddRange(buffer);
        }

        public void WriteInt32(int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            CheckReverse(ref buffer);
            _buffer.AddRange(buffer);
        }
    }

    public class BufferReader
    {
        private byte[] _buffer;
        private int _index;

        public bool ReachedEnd { get { return _index >= _buffer.Length; } }

        public BufferReader(byte[] buffer, bool checkReverse = true)
        {
            _buffer = buffer;
        }

        private void CheckReverse(int indexStart, int length)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_buffer, indexStart, length);
        }

        public void NormalizeJavaArray()
        {
            sbyte[] sbyteArray = new sbyte[_buffer.Length - 2];
            Buffer.BlockCopy(_buffer, 2, sbyteArray, 0, _buffer.Length - 2);
            Buffer.BlockCopy(sbyteArray, 0, _buffer, 2, sbyteArray.Length);
        }

        public void ResetIndex()
            => _index = 0;

        public int ReadInt32()
        {
            _index += 4;
            CheckReverse(_index - 4, 4);
            return BitConverter.ToInt32(_buffer, _index - 4);
        }

        public byte ReadByte()
        {
            _index++;
            return _buffer[_index - 1];
        }

        public string ReadString()
        {
            int length = ReadInt32();

            _index += length;

            return Encoding.UTF8.GetString(_buffer, _index - length, length);
        }

        public ulong ReadUlong()
        {
            _index += 8;
            CheckReverse(_index - 8, 8);
            return BitConverter.ToUInt64(_buffer, _index - 8);
        }
    }
}
