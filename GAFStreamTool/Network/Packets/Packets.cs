using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GAFStreamTool.Data;
using GAFStreamTool.Encryption;

namespace GAFStreamTool.Network.Packets
{
    public static class Packets
    {
        //1
        public class AuthPacket : IPacket
        {
            public byte Id => 1;
            public string ApiKey;
            public static Action<bool> OnAuthenticated;

            public AuthPacket(string apiKey, Action<bool> onAuthenticated)
            {
                ApiKey = apiKey;
                OnAuthenticated = onAuthenticated;
            }

            public AuthPacket()
            {
            }

            public void Handle(PacketReader reader, Client client)
            {
                byte result = reader.ReadByte();

                if (result == 1)
                {
                    OnAuthenticated?.Invoke(true);
                    client.Authenticated = true;
                }
                else
                    OnAuthenticated?.Invoke(false);
            }
            
            /// <summary>
            /// Requires: string GAF API Key
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="client"></param>
            /// <param name="data">string GAF API Key</param>
            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                string apiKeyEncrypted = AESEncryption.EncryptString(ApiKey);
                writer.Write(apiKeyEncrypted);

                client.WriteAsync(writer.Build());
            }
        }
        //2
        public class MatchPicksPacket : IPacket
        {
            public byte Id => 2;
            public string Match;

            public MatchPicksPacket(string match)
            {
                Match = match;
            }

            public MatchPicksPacket()
            {
            }
            
            public void Handle(PacketReader reader, Client client)
            {
                int totalPicks = reader.ReadInt32();

                List<Pick> picks = new List<Pick>();
                for (int i = 0; i < totalPicks; i++)
                {
                    string pickedBy = reader.ReadString();
                    string team = reader.ReadString();
                    string match = reader.ReadString();
                    string image = reader.ReadString();

                    picks.Add(new Pick(pickedBy, team, match, image));
                    Pick.Picks.RemoveAll(p => p.PickedBy.Equals(pickedBy, StringComparison.CurrentCultureIgnoreCase) && 
                                              p.Match.Equals(match, StringComparison.CurrentCultureIgnoreCase));
                }

                Pick.Picks.AddRange(picks);

                Program.MainForm.CheckForUpdatedPics(picks.ToArray());
            }

            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                writer.Write(Match);
                client.WriteAsync(writer.Build());
            }
        }
        //3
        public class PingPacket : IPacket
        {
            public byte Id => 3;
            public static Action<string> OnPingRecieved;
            public bool Pong;
            
            public PingPacket(bool pong, Action<string> onPingRecieved)
            {
                if (OnPingRecieved == null)
                    OnPingRecieved = onPingRecieved;
                else
                    OnPingRecieved += onPingRecieved;

                Pong = pong;
            }

            public PingPacket()
            {

            }

            public void Handle(PacketReader reader, Client client)
            {
                string str = reader.ReadString();

                if (str.Equals("Ping"))
                {
                    Pong = true;
                    Send(new PacketWriter(this), client);
                }

                OnPingRecieved?.Invoke(str);
                OnPingRecieved = null;
            }
            
            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                if (Pong)
                    writer.Write("Pong");
                else
                    writer.Write("Ping");

                byte[] toSend = writer.Build();

                client.WriteAsync(toSend);
            }
        }
        //4
        public class RegisterApiKeyPacket : IPacket
        {
            public byte Id => 4;
            public static Action<byte> OnRegisterResponse;
            public string RegisterCode;
            public string ApiKey;
            public byte[] EncryptionKey;

            public RegisterApiKeyPacket(Action<byte> onRegisterResponse, string registerCode, string apiKey, byte[] encryptionKey)
            {
                OnRegisterResponse = onRegisterResponse;
                RegisterCode = registerCode;
                ApiKey = apiKey;
                EncryptionKey = encryptionKey;
            }

            public RegisterApiKeyPacket()
            {
            }

            public void Handle(PacketReader reader, Client client)
            {
                OnRegisterResponse?.Invoke(reader.ReadByte());
            }

            public void Send(PacketWriter writer, Client client, params object[] data)
            {
                writer.Write(AESEncryption.EncryptString(ApiKey));
                writer.Write(RegisterCode);
                writer.Write(EncryptionKey.Length);
                writer.Write(-1, EncryptionKey);

                client.WriteAsync(writer.Build());
            }
        }

    }
}
