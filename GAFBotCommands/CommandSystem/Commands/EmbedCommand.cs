using DSharpPlus.Entities;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Net;

namespace GAFBot.Commands
{
    public class EmbedCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "embed"; }
        public AccessLevel AccessLevel => AccessLevel.Moderator;

        private static readonly string _info = "Usage:" + Environment.NewLine +
                                               "```" + Environment.NewLine + 
                                               "!embed create channelId embedCode/-url:www.example.com/raw/text" + Environment.NewLine +
                                               "!embed edit channelId messageId embedCode/-url:www.example.com/raw/text" + Environment.NewLine +
                                               "!embed reverse channelId messageId" + Environment.NewLine +
                                               "```";

        public static void Init()
        {
            Program.CommandHandler.Register(new EmbedCommand() as ICommand);
            Coding.Methods.Log(typeof(EmbedCommand).Name + " Registered");
        }
        


        public void Activate(CommandEventArg e)
        {
            try
            {
                if (!e.GuildID.HasValue)
                {
                    Coding.Methods.SendMessage(e.ChannelID, "This command is only usable in a discord channel");
                    return;
                }
                else if (string.IsNullOrEmpty(e.AfterCMD))
                {
                    Coding.Methods.SendMessage(e.ChannelID, _info);
                    return;
                }

                string download = null;

                int urlStart = e.AfterCMD.IndexOf("-url:");
                string urlString = e.AfterCMD.Remove(0, urlStart + 4);

                if (!string.IsNullOrEmpty(urlString))
                {
                    using (WebClient wc = new WebClient())
                        download = wc.DownloadString(urlString);
                }
                
                string @params = e.AfterCMD;

                if (download != null)
                    @params = @params.Replace("-url:" + urlString, download);

                int index = @params.IndexOf(' ');
                if (index <= 1)
                {
                    Coding.Methods.SendMessage(e.ChannelID, _info);
                    return;
                }

                string cmdType = @params.Substring(0, index).TrimStart(' ').TrimEnd(' ').ToLower();
                @params = @params.Remove(0, index + 1);

                string channelIdStr;
                ulong channelId;
                ulong messageId;
                DiscordChannel dchannel;
                DiscordMessage dmessage;
                Json.EmbedJson embedJson;
                DiscordEmbed embed;
                if (cmdType.Equals("edit"))
                {
                    index = @params.IndexOf(' ');
                    if (index <= 1)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, _info);
                        return;
                    }

                    channelIdStr = @params.Substring(0, index);

                    @params = @params.Remove(0, index + 1);

                    if (!ulong.TryParse(channelIdStr, out channelId))
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Could not parse channel id: " + channelIdStr);
                        return;
                    }

                    index = @params.IndexOf(' ');
                    if (index <= 1)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, _info);
                        return;
                    }

                    string messageIdStr = @params.Substring(0, index);
                    @params = @params.Remove(0, index + 1);
                    
                    if (!ulong.TryParse(messageIdStr, out messageId))
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Could not parse message id: " + messageIdStr);
                        return;
                    }

                    dchannel = Coding.Methods.GetChannel(channelId);
                    dmessage = dchannel.GetMessageAsync(messageId).Result;

                    if (dmessage == null)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Could not find message " + messageId);
                        return;
                    }

                    embedJson = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.EmbedJson>(@params);

                    if (embedJson == null)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Failed to parse your embed json");
                        return;
                    }

                    embed = embedJson.BuildEmbed();
                    dmessage.ModifyAsync(embedJson.content ?? default(Optional<string>), embed);

                    return;
                }
                else if (cmdType.Equals("reverse"))
                {
                    index = @params.IndexOf(' ');
                    if (index <= 1)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, _info);
                        return;
                    }

                    channelIdStr = @params.Substring(0, index);
                    @params = @params.Remove(0, index + 1);

                    if (!ulong.TryParse(channelIdStr, out channelId))
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Could not parse channel id: " + channelIdStr);
                        return;
                    }
                    
                    string messageIdStr = @params;

                    if (!ulong.TryParse(messageIdStr, out messageId))
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Could not parse message id: " + messageIdStr);
                        return;
                    }

                    dchannel = Coding.Methods.GetChannel(channelId);
                    dmessage = dchannel.GetMessageAsync(messageId).Result;

                    if (dmessage == null)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Could not find message " + messageId);
                        return;
                    }

                    var reversed = Json.EmbedJson.ReverseEmbed(dmessage);
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(reversed, Newtonsoft.Json.Formatting.Indented);

                    Coding.Methods.SendMessage(e.ChannelID, "```js" + Environment.NewLine + json + Environment.NewLine + "```");
                    return;
                }

                index = @params.IndexOf(' ');
                if (index <= 1)
                {
                    Coding.Methods.SendMessage(e.ChannelID, _info);
                    return;
                }

                channelIdStr = @params.Substring(0, index);
                @params = @params.Remove(0, index + 1);
                
                if (@params.Length <= 1)
                {
                    Coding.Methods.SendMessage(e.ChannelID, _info);
                    return;
                }

                if (!ulong.TryParse(channelIdStr, out channelId))
                {
                    Coding.Methods.SendMessage(e.ChannelID, "Could not parse channel id: " + channelIdStr);
                    return;
                }

                Logger.Log(channelIdStr + " : " + channelId);

                dchannel = Coding.Methods.GetChannel(channelId);
                embedJson = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.EmbedJson>(@params);

                if (embedJson == null)
                {
                    Coding.Methods.SendMessage(e.ChannelID, "Failed to parse your embed json");
                    return;
                }

                embed = embedJson.BuildEmbed();
                dchannel.SendMessageAsync(embedJson.content, false, embed);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
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