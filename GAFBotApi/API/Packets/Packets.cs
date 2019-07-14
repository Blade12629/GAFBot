using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Packets
{
    public static class Packets
    {
        public static List<PacketInfo> RegisteredPackets { get; private set; }
        public static int Count { get { return RegisteredPackets == null ? -1 : RegisteredPackets.Count; } }

        public static void Initialize()
        {
            //Discord message packet, unspecified size, gets size on invoking
            Register(new PacketInfo()
            {
                PacketCmd = (byte)ApiEndPoints.DChannelMessage,
                PacketLength = -1,
            });
        }

        public static void Register(PacketInfo pInfo)
        {
            if (RegisteredPackets == null)
                RegisteredPackets = new List<PacketInfo>();

            if (RegisteredPackets.Contains(pInfo))
                return;

            RegisteredPackets.Add(pInfo);
        }

        public static void UnRegister(PacketInfo pInfo)
        {
            if (RegisteredPackets == null)
                RegisteredPackets = new List<PacketInfo>();

            if (!RegisteredPackets.Contains(pInfo))
                return;

            RegisteredPackets.Remove(pInfo);
        }

        public static void InvokePacket(Packet p, APIServerClient client)
        {
            PacketReader reader = new PacketReader(p);
            ulong channel;
            ulong guild;
            ulong userid;
            string message;
            string username;
            string channelname;
            switch (p.Command)
            {
                case (byte)ApiEndPoints.DChannelMessage:
                    channel = reader.ReadUlong(0);
                    message = reader.ReadString(8);
                    DiscordMessage(channel, message, client);
                    break;
            }
        }

        private static readonly Action<ulong, string> discordMessage = new Action<ulong, string>(Coding.Methods.SendMessage);
        private static readonly Func<ulong, DSharpPlus.Entities.DiscordChannel> discordGetChannel = new Func<ulong, DSharpPlus.Entities.DiscordChannel>(Coding.Methods.GetChannel);
        private static readonly Func<ulong, DSharpPlus.Entities.DiscordGuild> discordGetGuild = new Func<ulong, DSharpPlus.Entities.DiscordGuild>(Coding.Methods.GetGuild);
        private static readonly Func<ulong, DSharpPlus.Entities.DiscordUser> discordGetUser = new Func<ulong, DSharpPlus.Entities.DiscordUser>(Coding.Methods.GetUser);
        private static readonly Func<ulong, ulong, DSharpPlus.Entities.DiscordMember> discordGetGetMember = new Func<ulong, ulong, DSharpPlus.Entities.DiscordMember>(Coding.Methods.GetMember);

        private static void DiscordMessage(ulong channel, string message, APIServerClient client)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = "404: Message cannot be null or empty";

                //Build byte array so we know size
                byte[] data = Encoding.UTF8.GetBytes(message);
                byte length = (byte)(data.Length + 2);
                PacketWriter packetWriter = new PacketWriter((int)length);

                packetWriter.WriteByte(0, (byte)packetWriter.PacketData.Length);
                packetWriter.WriteByte(1, (byte)ApiEndPoints.DChannelMessage);
                packetWriter.WriteBytes(2, data);

                //Create packet out of array
                Packet p = packetWriter.Build();

                //Send the error message back
                new Action<Packet>(client.Write).Invoke(p);
                return;
            }

            var dchannel = discordGetChannel.Invoke(channel);

            if (dchannel == null)
            {
                message = "404: Channel not found";

                //Build byte array so we know size
                byte[] data = Encoding.UTF8.GetBytes(message);
                byte length = (byte)(data.Length + 2);
                PacketWriter packetWriter = new PacketWriter((int)length);

                packetWriter.WriteByte(0, (byte)packetWriter.PacketData.Length);
                packetWriter.WriteByte(1, (byte)ApiEndPoints.DChannelMessage);
                packetWriter.WriteBytes(2, data);

                //Create packet out of array
                Packet p = packetWriter.Build();

                //Send the error message back
                new Action<Packet>(client.Write).Invoke(p);
                return;
            }

            dchannel.SendMessageAsync(message).Wait();
        }
    }
}
