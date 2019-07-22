using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;

namespace GAFBot.Commands
{
    public class EmbedCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "embed"; }
        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public static void Init()
        {
            Program.CommandHandler.Register(new EmbedCommand() as ICommand);
            Coding.Methods.Log(typeof(EmbedCommand).Name + " Registered");
        }
        
        /*
         * !embed channelId>R>G>B>Title>Message>Title>Message
         */
        public void Activate(CommandEventArg e)
        {
            try
            {
                string[] split = e.AfterCMD.Split('|');

                if (split == null || split.Length == 0)
                    return;

                //Values have to be 1 or 0, inbetween somehow bugs
                float[] rgb = new float[3];

                if (!ulong.TryParse(split[0], out ulong channelId) || !float.TryParse(split[1], out rgb[0]) || !float.TryParse(split[2], out rgb[1]) || !float.TryParse(split[3], out rgb[2]))
                    return;

                ulong id;
                ProcessChannelId(ref split[4]);
                ProcessChannelId(ref split[5]);

                var builder = new DSharpPlus.Entities.DiscordEmbedBuilder()
                {
                    Color = new DSharpPlus.Entities.DiscordColor(rgb[0], rgb[1], rgb[2]),
                    Title = split[4],
                    Description = split[5]
                };

                for (int i = 6; i < split.Length; i += 2)
                {
                    if (i == split.Length || i + 1 == split.Length)
                        break;

                    ProcessChannelId(ref split[i]);
                    ProcessChannelId(ref split[i + 1]);

                    builder.AddField(split[i], split[i + 1]);
                }

                var embed = builder.Build();
                var channel = Coding.Methods.GetChannel(channelId);
                channel.SendMessageAsync(embed: embed).Wait();

                void ProcessChannelId(ref string replaceString)
                {
                    if ((id = GetChannelId(replaceString)) > 0)
                    {
                        var ch = Coding.Methods.GetChannel(id);

                        if (ch != null)
                            replaceString = replaceString.Replace("<#" + ch.Id + ">", ch.Mention);
                    }
                }
            }
            catch (Exception ex)
            {
                Coding.Methods.GetChannel(e.ChannelID).SendMessageAsync(ex.ToString() + Environment.NewLine + "parameters:" + Environment.NewLine + "```" + Environment.NewLine + e.AfterCMD + Environment.NewLine + "```").Wait();
            }
        }
        

        private ulong GetChannelId(string channel)
        {
            int indexStart = channel.IndexOf('<');

            if (indexStart == -1)
                return 0;

            string id = channel.Remove(0, indexStart + 1);

            if (!id[0].Equals('#'))
                return 0;

            id = id.Remove(0, 1);
            indexStart = channel.IndexOf('>');
            if (indexStart == -1)
                return 0;
            try
            {
                id = id.Substring(0, indexStart - 1);
            }
            catch (Exception ex)
            {
                id = id.Substring(0, indexStart - 2);
            }

            if (ulong.TryParse(id, out ulong result))
                return result;

            return 0;
        }
    }
}