using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer
{
    public class MessageHandler
    {
        public DiscordClient dclient = Program._DClient;

        public void NewMessage(MessageCreateEventArgs input)
        {
            if (input.Message.Author == Program._DClient.CurrentUser || input.Message == null) return;
            
            string CommandString = input.Message.Content.Split(' ')[0];

            if (CommandString.StartsWith("!"))
            {
                string sub = input.Message.Content.Substring(CommandString.Length);

                if (CommandString.StartsWith("!roll"))
                {
                    int number = 0;
                    if (int.TryParse(sub, out number))
                    {
                        Task.Run(async () => { await SendMessage(input.Channel, string.Format("You rolled {0}!", new Random().Next(0, number).ToString())); });
                        return;
                    }
                    string[] sub2 = sub.Split(' ');
                    int number2 = 0;
                    if (int.TryParse(sub2[0], out number) && int.TryParse(sub2[1], out number2))
                    {
                        Task.Run(async () => { await SendMessage(input.Channel, string.Format("You rolled {0}", new Random().Next(number, number2)); });
                        return;
                    }
                    Task.Run(async () => { await SendMessage(input.Channel, string.Format("unkown command, try !roll number or !roll numberMin numberMax")); });
                    return;
                }
                if (CommandString.StartsWith("!analyze"))
                {
                    if (CanAccessChannel(input.Channel.Id))
                    {
                        int matchid = 0;
                        if (sub.Contains("osu.ppy.sh/community/matches/"))
                        {
                            if (sub.EndsWith("/")) sub.Remove(sub.Length - 1, 1);
                            matchid = int.Parse(sub.Substring(sub.LastIndexOf('/')));
                        }
                        if (matchid == 0)
                        {
                            int.TryParse(sub, out matchid);
                        }
                        Analyze_Format.Analyzer.MultiplayerMatch mpmatch = new Analyze_Format.Analyzer.MultiplayerMatch();
                        Analyze_Format.Analyzed.MultiMatch mpMatch = mpmatch.Analyze(API.OsuApi.GetMatch(matchid));
                        Task.Run(async () => 
                        {
                            await SendMessage(input.Channel, mpMatch.AnalyzedData.ToArray());
                        });
                    }
                }
            }

            Console.WriteLine("{0} {1}: {2}", DateTime.UtcNow, input.Message.Author.Username, input.Message.Content);

            //string MatchString = "https://osu.ppy.sh/community/matches/";
            //if (Message.StartsWith(MatchString))
            //{
            //    string sub = Message.Substring(MatchString.Length - 1);
            //    //ToDo
            //}
            

        }
        
        public async Task SendMessage(DiscordChannel channel, params string[] toSend)
        {
            foreach (string s in toSend)
            {
                await dclient.SendMessageAsync(channel, s);
            }
        }

        public AccessLevel GetAccessLevel(ulong userid)
        {
            switch (userid)
            {
                default:
                    return AccessLevel.user;
                case 139769063948681217: //Haruki
                    return AccessLevel.Admin;
                case 140896783717892097: //Skyfly
                    return AccessLevel.Developer;
            }
        }

        public AccessLevel GetAccessLevelForRole(ulong roleid)
        {
            switch(roleid)
            {
                default:
                    return AccessLevel.user;
                case 147256133491490817: //Organizer
                case 147339910926303232: //Co-Organizer
                    return AccessLevel.Admin;
                case 450038874102693888:
                    return AccessLevel.Developer;
            }
        }

        public static bool Match_Results_Access = false;
        public bool CanAccessChannel(ulong channelid)
        {
            switch(channelid)
            {
                default:
                    return false;
                case 450178998228746240: //development_staff
                    return true;
                case 279248018107006976: //match_results
                    return Match_Results_Access;
            }
        }

        public enum AccessLevel
        {
            Developer,
            Admin,
            user
        }

        public void Command(string arg, string input, MessageCreateEventArgs e)
        {
            switch (arg.ToLower())
            {
                default:
                    return;
            }
        }
        public void CommandAdmin(string arg, string input, MessageCreateEventArgs e)
        {
            switch (arg.ToLower())
            {
                default:
                    Command(arg, input, e);
                    return;
            }
        }
        public void CommandDeveloper(string arg, string input, MessageCreateEventArgs e)
        {
            switch (arg.ToLower())
            {
                default:
                    CommandAdmin(arg, input, e);
                    return;
            }
        }
    }
}
