using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GAFBot;
using GAFBot.Database;
using GAFBot.Database.Models;
using ServerModule.Data;

namespace ServerModule.Network.Packets
{
    public static class Packets
    {
        public enum PacketId
        {
            AuthPacket = 1,
            MatchPicksPacket,
            PingPacket,
            RegisterApiKeyPacket
        }

        //1
        public class AuthPacket : IPacket
        {
            public byte Id => (int)PacketId.AuthPacket;

            [Obsolete("Test")]
            public void Handle(PacketReader reader, Client client)
            {
                string encryptedKey = reader.ReadString();

                Console.WriteLine("Recieved Auth Key: " + encryptedKey);

                byte authenticated = 0;
                BotApiKey key = new BotApiKey()
                {
                    DiscordId = 140896783717892097,
                    Key = "1+UdChRTEQK996rY3iF9Uw=="
                };
                authenticated = 1;
                //using (GAFContext context = new GAFContext())
                //{
                //    key = context.BotApiKey.FirstOrDefault(k => k.Key.Equals(encryptedKey));

                //    if (key != null)
                //        authenticated = 1;
                //}
                
                if (authenticated == 1)
                {
                    client.LoadEncryptionKey((ulong)key.DiscordId);
                    client.Authenticated = true;
                }

                PacketWriter writer = new PacketWriter(this);
                Send(writer, client, authenticated);
            }
            
            /// <summary>
            /// Requires: string GAF API Key
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="client"></param>
            /// <param name="data">string GAF API Key</param>
            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                writer.Write((byte)data[0]);
                client.WriteAsync(writer.Build());
            }
        }
        //2
        public class MatchPicksPacket : IPacket
        {
            public byte Id => (int)PacketId.MatchPicksPacket;
            public Pick[] Picks;

            public MatchPicksPacket(Pick[] picks)
            {
                Picks = picks;
            }

            public MatchPicksPacket()
            {
            }

            public void Handle(PacketReader reader, Client client)
            {
                Console.WriteLine("Reading Match");
                string match = reader.ReadString();
                Console.WriteLine("Got Match " + match);

                BotPick[] picks = new BotPick[]
                {
                    new BotPick()
                    {
                        Image = "https://cdn.discordapp.com/avatars/592057331777273867/264ac131be1a2d653ffd8c6e0c280c33.png",
                        Match = "Test1 vs Test2",
                        PickedBy = "Test User 1",
                        Team = "Test2"
                    },
                    new BotPick()
                    {
                        Image = "https://cdn.discordapp.com/avatars/592057331777273867/264ac131be1a2d653ffd8c6e0c280c33.png",
                        Match = "Test1 vs Test2",
                        PickedBy = "Test User 2",
                        Team = "Test2"
                    },
                    new BotPick()
                    {
                        Image = "https://cdn.discordapp.com/avatars/592057331777273867/264ac131be1a2d653ffd8c6e0c280c33.png",
                        Match = "Test1 vs Test2",
                        PickedBy = "Test User 3",
                        Team = "Test1"
                    },
                };
                //using (GAFContext context = new GAFContext())
                //    picks = context.BotPick.Where(p => p.Match.Equals(match, StringComparison.CurrentCultureIgnoreCase)).ToArray();

                Picks = new Pick[picks.Length];

                for (int i = 0; i < picks.Length; i++)
                    Picks[i] = new Pick(picks[i].PickedBy, picks[i].Team, picks[i].Match, picks[i].Image);

                Console.WriteLine("Sending " + Picks.Length);

                PacketWriter writer = new PacketWriter(this);
                
                Send(writer, client);
            }

            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                if (Picks == null)
                {
                    writer.Write(0);

                    client.WriteAsync(writer.Build());
                    return;
                }

                writer.Write(Picks.Length);

                foreach(Pick p in Picks)
                {
                    writer.Write(p.PickedBy);
                    writer.Write(p.Team);
                    writer.Write(p.Match);
                    writer.Write(p.Image);
                }
                
                client.WriteAsync(writer.Build());
            }
        }
        //3
        public class PingPacket : IPacket
        {
            public byte Id => (int)PacketId.PingPacket;
            
            public void Handle(PacketReader reader, Client client)
            {
                Console.WriteLine("Recieved ping");

                string str = reader.ReadString();

                Console.WriteLine(str + " from: " + client.Host);

                if (str.Equals("Ping"))
                    Send(new PacketWriter(reader.Packet), client, new object[] { true });
            }
            
            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                if (data != null && data.Length > 0)
                    writer.Write("Pong");
                else
                    writer.Write("Ping");

                client.WriteAsync(writer.Build());
            }
        }
        //4
        public class RegisterApiKeyPacket : IPacket
        {
            public byte Id => (int)PacketId.RegisterApiKeyPacket;

            public void Handle(PacketReader reader, Client client)
            {
                try
                {
                    string apiKeyEncrypted = reader.ReadString();
                    string code = reader.ReadString();
                    int keyLength = reader.ReadInt32();
                    byte[] key = reader.ReadBytes(keyLength);

                    byte result = 0;
                    long discordId = 0;
                    BotApiRegisterCode registerCode;
                    using (GAFContext context = new GAFContext())
                    {
                        registerCode = context.BotApiRegisterCode.FirstOrDefault(c => c.Code.Equals(code));

                        if (registerCode != null)
                        {
                            result = 1;
                            discordId = registerCode.DiscordId;
                            context.BotApiRegisterCode.Remove(registerCode);
                        }

                        context.BotApiKey.Add(new BotApiKey()
                        {
                            DiscordId = registerCode.DiscordId,
                            Key = apiKeyEncrypted
                        });

                        context.SaveChanges();
                    }

                    if (result == 1)
                        client.UpdateEncryptionKey((ulong)discordId, key);

                    PacketWriter writer = new PacketWriter(this);
                    Send(writer, client, result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                writer.Write((byte)data[0]);
                client.WriteAsync(writer.Build());
            }
        }

    }
}
