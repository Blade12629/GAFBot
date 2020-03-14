using GAFBot;
using GAFBot.Commands;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace GAFBotCommands.CommandSystem.Commands
{
    class BirthdayCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "birthday"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Sets your current birthday";

        public string DescriptionUsage => "Please use a valid date format: " + Environment.NewLine + "```" + Environment.NewLine +
                                                            "DD/MM[/YYYY]" + Environment.NewLine +
                                                           @"DD\MM[\YYYY]" + Environment.NewLine +
                                                            "DD.MM[.YYYY]" + Environment.NewLine + "```" + Environment.NewLine +
                                                            "([] = optional)";

        private static Timer _refreshTimer;

        public static void Init()
        {
            Program.CommandHandler.Register(new BirthdayCommand());
            Coding.Methods.Log(typeof(BirthdayCommand).Name + " Registered");

            _refreshTimer = new Timer()
            {
                Interval = 1 * 60 * 60 * 1000,
                AutoReset = true
            };

            _refreshTimer.Elapsed += RefreshTimerElapsed;
            _refreshTimer.Start();
        }

        private static void RefreshTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.UtcNow.Hour != 1)
                return;

            List<BotBirthday> birthdays;

            using (GAFBot.Database.GAFContext context = new GAFBot.Database.GAFContext())
                birthdays = context.BotBirthday.Where(b => b.Day == DateTime.UtcNow.Day && b.Month == DateTime.UtcNow.Month).ToList();

            if (birthdays == null || birthdays.Count == 0)
                return;

            //General staff 239677046274392066
            foreach (BotBirthday bday in birthdays)
            {
                string toSend = $"Happy{(bday.Year == 0 ? "" : $" {DateTime.UtcNow.Year - bday.Year}.")} birthday <@!{(ulong)bday.DiscordId}>";
                Coding.Methods.SendMessage(239677046274392066, toSend);
            }
        }

        public void Activate(CommandEventArg e)
        {
            string[] split;

            if (e.AfterCMD.Contains('.'))
                split = e.AfterCMD.Split('.');
            else if (e.AfterCMD.Contains('\\'))
                split = e.AfterCMD.Split('\\');
            else if (e.AfterCMD.Contains('/'))
                split = e.AfterCMD.Split('/');
            else
            {
                Coding.Methods.SendMessage(e.ChannelID, DescriptionUsage);
                return;
            }

            if (!int.TryParse(split[0], out int day))
            {
                Coding.Methods.SendMessage(e.ChannelID, "Could not parse: " + split[0]);
                return;
            }
            
            day = Math.Min(31, Math.Max(0, day));

            if (!int.TryParse(split[1], out int month))
            {
                Coding.Methods.SendMessage(e.ChannelID, "Could not parse: " + split[1]);
                return;
            }

            month = Math.Min(31, Math.Max(0, month));

            int year = 0;
            if (split.Length == 3 && !int.TryParse(split[2], out year))
            {
                Coding.Methods.SendMessage(e.ChannelID, "Could not parse: " + split[2]);
                return;
            }

            BotBirthday bbday;

            using (GAFBot.Database.GAFContext context = new GAFBot.Database.GAFContext())
            {
                bbday = context.BotBirthday.FirstOrDefault(b => (ulong)b.DiscordId == e.DUserID);

                if (bbday == null)
                {
                    bbday = new BotBirthday()
                    {
                        DiscordId = (long)e.DUserID,
                        Day = day,
                        Month = month,
                        Year = year
                    };

                    context.BotBirthday.Add(bbday);
                    context.SaveChanges();

                    Coding.Methods.SendMessage(e.ChannelID, $"Set your birthday to {bbday.Day}/{bbday.Month}/{bbday.Year}");
                    Logger.Log($"Set birthday of {e.DUserID} to {bbday.Day}/{bbday.Month}/{bbday.Year}");
                    return;
                }

                bbday.Day = day;
                bbday.Month = month;
                bbday.Year = year;

                context.BotBirthday.Update(bbday);
                context.SaveChanges();

                Coding.Methods.SendMessage(e.ChannelID, $"Set your birthday to {bbday.Day}/{bbday.Month}/{bbday.Year}");
                Logger.Log($"Set birthday of {e.DUserID} to {bbday.Day}/{bbday.Month}/{bbday.Year}");
            }
        }
    }
}
