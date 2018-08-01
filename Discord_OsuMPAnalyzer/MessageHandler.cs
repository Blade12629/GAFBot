using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer
{
    public class MessageHandler
    {
        public DiscordClient dclient = Program._DClient;
        public static Stopwatch CurStopWatch = new Stopwatch();
        public static string StopWatchSyntax = "!sw";

        public void NewMessage(MessageCreateEventArgs input)
        {
            if (input.Message.Author == Program._DClient.CurrentUser || input.Message == null) return;

            string MessageContent = input.Message.Content;

            string CommandString = MessageContent.Split(' ')[0];

            string[] messageSplit = MessageContent.Split(' ');
            if (messageSplit[messageSplit.Count() - 1].ToLower() == StopWatchSyntax)
            {
                if (CurStopWatch.IsRunning) Task.Run(async () => { await SendMessage(input.Channel, "StopWatch is already running, wait until it's finished."); });
                else
                {
                    CurStopWatch.Reset();
                    CurStopWatch.Start();
                }
                MessageContent.Remove(MessageContent.IndexOf(messageSplit[messageSplit.Count() - 1]), messageSplit[messageSplit.Count() - 1].Length);
            }

            if (CommandString.StartsWith("!"))
            {
                if (CanAccessChannel(input.Channel.Id))
                {
                    string sub = input.Message.Content.Substring(CommandString.Length);
                    
                    AccessLevel Acc = GetAccessLevel(input.Author.Id);

                    if (Acc == AccessLevel.user)
                    {
                        foreach (DiscordMember member in input.Guild.Members)
                        {
                            if (member.Id == input.Author.Id)
                            {
                                foreach (DiscordRole drole in member.Roles)
                                {
                                    AccessLevel newAcc = GetAccessLevelForRole(drole.Id);
                                    if ((int)newAcc > (int)Acc) Acc = newAcc;
                                }
                            }
                        }
                    }

                    OnCommandUsed(CommandString, sub, input, Acc);

                    if (Acc == AccessLevel.banned)
                    {
                        Console.WriteLine("Terminating execution user {0} ({1}) is banned!", input.Author.Username, input.Author.Id);
                        return;
                    }

                    if (!CanAccessChannel(input.Channel.Name))
                        return;

                    switch (Acc)
                    {
                        case AccessLevel.user:
                            Command(CommandString, sub, input);
                            break;
                        case AccessLevel.Admin:
                            CommandAdmin(CommandString, sub, input);
                            break;
                        case AccessLevel.Developer:
                            CommandDeveloper(CommandString, sub, input);
                            break;
                    }
                }
            }
            else Console.WriteLine("{0} {1}: {2}", DateTime.UtcNow, input.Message.Author.Username, input.Message.Content);

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

                case 0:
                    return AccessLevel.banned;

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

                case 0:
                    return AccessLevel.banned;

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
                case 279248018107006976: //
                case 450178998228746240: //development_staff
                    return true;
            }
        }
        public bool CanAccessChannel(string channelname)
        {
            switch (channelname)
            {
                default:
                    return false;
                case "match_results":
                case "development_staff":
                    return true;
            }
        }

        public enum AccessLevel
        {
            banned = -1,
            user,
            Admin,
            Developer
        }

        public void Command(string arg, string input, MessageCreateEventArgs e)
        {
            switch (arg.ToLower())
            {
                default:
                    return;
                #region !roll
                case "!roll":
                        int number = 0;
                        int number2 = 0;

                        if (int.TryParse(input, out number))
                        {
                            Task.Run(async () => { await SendMessage(e.Channel, string.Format("You rolled {0}!", new Random().Next(0, number).ToString())); });
                            return;
                        }

                        string[] sub2 = input.Split(' ');

                        if (int.TryParse(sub2[0], out number) && int.TryParse(sub2[1], out number2))
                        {
                            Task.Run(async () => { await SendMessage(e.Channel, string.Format("You rolled {0}", new Random().Next(number, number2))); });
                            return;
                        }

                        Task.Run(async () => { await SendMessage(e.Channel, string.Format("unkown command, try !roll number or !roll {0} {1}", int.MinValue, int.MaxValue)); });
                    break;
                #endregion
                case "!ping":
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    int ping = dclient.Ping;
                    sw.Stop();

                    Task.Run(async () => { await SendMessage(e.Channel, string.Format("Current Ping: {0} ({1})", ping, sw.ElapsedMilliseconds)); });
                    break;
            }
        }

        public void CommandAdmin(string arg, string input, MessageCreateEventArgs e)
        {
            switch (arg.ToLower())
            {
                default:
                    Command(arg, input, e);
                    break;
                #region !analyze
                case "!analyze":

                    if (CanAccessChannel(e.Channel.Id))
                    {
                        bool error = false;
                        string toSend = "```";
                        try
                        {
                            Analyze_Format.OsuAnalyzer.HistoryReader hr = Analyze_Format.OsuAnalyzer.ParseMatch(input);
                            hr.Output.ToList().ForEach(ob => toSend += Environment.NewLine + ob);
                            toSend += Environment.NewLine + "```";
                        }
                        catch (Exception ex)
                        {
                            error = true;
                            Console.WriteLine(string.Format("{0}: Executed by {1} Command Args {2}", DateTime.Now, e.Author.Username, arg, ex));
                        }
                        if (!error)
                            Task.Run(async () => { await SendMessage(e.Channel, toSend); });
                    }
                    break;
                    #endregion
            }
        }

        public void CommandDeveloper(string arg, string input, MessageCreateEventArgs e)
        {
            switch (arg.ToLower())
            {
                default:
                    CommandAdmin(arg, input, e);
                    break;
            }
            OnCommandDone(arg, input, e);
        }

        private void OnCommandUsed(string arg, string input, MessageCreateEventArgs e, AccessLevel CurAccessLevel)
        {
            Console.WriteLine("{0}#{1} ({2}) | {3} used command {4} with input {5} on channel {6} ({7}) @{8}({9})", e.Author.Username, e.Author.Discriminator, e.Author.Id, CurAccessLevel.ToString(), arg, input, e.Channel.Name, e.Channel.Id, e.Guild.Name, e.Guild.Id);
        }

        private void OnCommandDone(string arg, string input, MessageCreateEventArgs e)
        {
            if (CurStopWatch.IsRunning)
            {
                CurStopWatch.Stop();
                Task.Run(async () => { await SendMessage(e.Channel, "Stopwatch ended. result: {0} ms"); });
            }
        }
    }
}
