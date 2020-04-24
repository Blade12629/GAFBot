using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using DSharpPlus.Entities;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.Database.Readers;
using GAFBot.MessageSystem;

namespace GAFBot.Commands
{
    public class RemindMeCommand : ICommand
    {
        public char Activator => '!';

        public string CMD => "remindme";

        public char ActivatorSpecial => char.MinValue;

        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "!remindme";

        public string DescriptionUsage =>  "```\n" + 
                                           "!remindme days:hours:minutes [message you should be pinged with]\n" + 
                                           "!remindme list [pageNumber]\n" +
                                           "!remindme delete timerId\n" +
                                           "([] = optional), Max: days: 31 hours: 23 minutes: 59)\n" +
                                           "```";

        private static object _timersLock = new object();
        private static Timer _timer;

        public static void Init()
        {
            try
            {
                Program.CommandHandler.Register(new RemindMeCommand());
                Logger.Log(nameof(RemindMeCommand) + " Registered");

                if (_timer != null && _timer.Enabled)
                    _timer.Stop();

                _timer = new Timer(30000)
                {
                    AutoReset = true
                };
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.AfterCMD))
                {
                    Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: BuildUsageInfo());
                    return;
                }

                string[] paramSplit = e.AfterCMD.Split(' ');

                int pageIndex = 1;
                switch (paramSplit.Length)
                {
                    default:
                        break;

                    case 2:
                        if (!paramSplit[0].Equals("delete", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(paramSplit[1], out pageIndex);
                            goto case 1;
                        }

                        if (!int.TryParse(paramSplit[1], out int timerId))
                        {
                            Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(content: "Could not parse timer id " + paramSplit[1], embed: BuildUsageInfo());
                            return;
                        }

                        lock(_timersLock)
                        {
                            using (BaseDBReader<BotTimer> timerReader = new BaseDBReader<BotTimer>())
                            {
                                timerReader.Remove(bt => bt.Id == timerId);
                                timerReader.Save();
                            }
                        }

                        Coding.Discord.SendMessage(e.ChannelID, "Removed timer with id " + timerId);
                        return;
                    case 1:
                        if (paramSplit[0].Equals("list"))
                        {
                            List<BotTimer> timers = GetTimersByDiscordId(e.DUserID);
                            double totalPages = 1;

                            if (timers.Count > 10)
                                totalPages = timers.Count / 10.0;

                            if ((int)totalPages < totalPages)
                                totalPages = (int)totalPages + 1;

                            timers = GetAvailableIndexes((pageIndex - 1) * 10, 10, timers);

                            if (timers.Count == 0)
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "No reminders found");
                                return;
                            }

                            Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: BuildRemindMeList(timers, pageIndex, (int)totalPages, e.DUserID, e.ChannelID, e.GuildID));
                            return;
                        }
                        break;
                    case 0:
                        Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: BuildUsageInfo());
                        return;
                }

                string[] timeSplit = paramSplit[0].Split(':');

                if (timeSplit.Length < 3)
                {
                    Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: BuildUsageInfo());
                    return;
                }

                if (!int.TryParse(timeSplit[0], out int days) ||
                    !int.TryParse(timeSplit[1], out int hours) ||
                    !int.TryParse(timeSplit[2], out int minutes))
                {
                    Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: BuildUsageInfo());
                    return;
                }

                days = Math.Max(0, Math.Min(days, 31));
                hours = Math.Max(0, Math.Min(hours, 23));
                minutes = Math.Max(0, Math.Min(minutes, 59));

                if (days == 0 &&
                    hours == 0 &&
                    minutes == 0)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Days, hours and minutes cannot be 0 all at once!");
                    return;
                }

                string message = null;

                if (paramSplit.Length >= 2)
                {
                    message = paramSplit[1];

                    for (int i = 2; i < paramSplit.Length; i++)
                        message += ' ' + paramSplit[i];
                }

                BotTimer timer;
                using (BaseDBReader<BotTimer> timerReader = new BaseDBReader<BotTimer>())
                {
                    DateTime startTime = DateTime.UtcNow;
                    DateTime endTime = startTime.AddHours(hours).AddMinutes(minutes).AddDays(days);

                    timer = timerReader.Add(new BotTimer(true, DateTime.UtcNow, endTime, (long)e.DUserID, false, (long)e.ChannelID, !e.GuildID.HasValue, message));
                    timerReader.Save();
                }

                Coding.Discord.SendMessage(e.ChannelID, $"Created timer {timer.Id}, expires on: {GetExpirationDate(timer)} UTC");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs args)
        {
            try
            {
                using (BaseDBReader<BotTimer> timerReader = new BaseDBReader<BotTimer>())
                {
                    List<BotTimer> timers = timerReader.Where(bt => !bt.Expired);

                    for (int i = 0; i < timers.Count; i++)
                    {
                        BotTimer btimer = timers[i];

                        if (btimer.EndTime >= DateTime.UtcNow)
                            return;
                            
                        btimer.Expired = true;
                        timerReader.Update(btimer);

                        string msg = $"<@!{btimer.CreatedByDiscordId}> ";

                        if (!string.IsNullOrEmpty(btimer.PingMessage))
                            msg += "Reminding you of: " + Environment.NewLine + btimer.PingMessage;

                        Coding.Discord.SendMessage((ulong)btimer.DiscordChannelId, msg);
                    }

                    timerReader.Save();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }

        private DiscordEmbed BuildUsageInfo()
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            builder.Title = "Help for RemindMe Command";
            builder.Timestamp = DateTime.UtcNow;
            builder.AddField(".", DescriptionUsage);

            return builder.Build();
        }

        private List<BotTimer> GetTimersByDiscordId(ulong discordId)
        {
            return GetTimersByDiscordId((long)discordId);
        }

        private List<BotTimer> GetTimersByDiscordId(long discordId)
        {
            using (BaseDBReader<BotTimer> reader = new Database.Readers.BaseDBReader<BotTimer>())
            {
                return reader.Where(t => t.CreatedByDiscordId == discordId && !t.Expired);
            }
        }

        private List<BotTimer> GetAvailableIndexes(int start, int count, List<BotTimer> timers)
        {
            List<BotTimer> result = new List<BotTimer>(count);

            if (timers.Count == 0)
                return result;

            int end = start + count;
            end = Math.Min(end, timers.Count);

            for (int i = start; i < end; i++)
                result.Add(timers[i]);

            return result;
        }

        private DiscordEmbed BuildRemindMeList(List<BotTimer> timers, int pageIndex, int totalPages, ulong caller, ulong channelId, ulong? guildId = null)
        {
            string user;
            if (guildId.HasValue)
                user = Coding.Discord.GetMember(caller, guildId.Value).Username;
            else
                user = Coding.Discord.GetUser(caller).Username;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "RemindMe Timers for " + user,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Page {pageIndex}/{totalPages}"
                },
                Timestamp = DateTime.UtcNow
            };

            StringBuilder responseBuilder = new StringBuilder();
            AppendResponseString(timers[0], ref responseBuilder);

            for (int i = 1; i < timers.Count; i++)
            {
                responseBuilder.AppendLine();
                AppendResponseString(timers[i], ref responseBuilder);
            }

            builder.AddField(".", responseBuilder.ToString());

            return builder.Build();

            void AppendResponseString(BotTimer timer, ref StringBuilder builder_)
            {
                DateTime expiresOn = GetExpirationDate(timer);

                builder_.AppendLine();
                builder_.Append($"Id: {timer.Id}, Expires At: {expiresOn}, Message: {(string.IsNullOrEmpty(timer.PingMessage) ? "none" : timer.PingMessage)}");

                if (!string.IsNullOrEmpty(timer.PingMessage))
                    builder_.Append(", Msg: " + timer.PingMessage);
            }
        }

        private DateTime GetExpirationDate(BotTimer timer)
        {
            return timer.EndTime;
        }
    }
}
