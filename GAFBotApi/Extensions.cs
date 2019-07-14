using GAFBot.API.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot
{
    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        public static Packet ToPacket(this PacketInfo packetInfo)
            => (Packet)packetInfo;

        public static PacketInfo ToPacketInfo(this Packet packet)
            => (PacketInfo)packet;
    }
}
