using GAFBot;
using GAFBot.Commands;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace GAFBot.Commands
{
    class BirthdayCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "birthday"; }
        public AccessLevel AccessLevel => AccessLevel.User;
        
        public string Description
        {
            get
            {
                using (GAFContext context = new GAFContext())
                    return context.BotLocalization.First(l => l.Code.Equals("cmdDescriptionBirthday")).String;
            }
        }

        public string DescriptionUsage
        {
            get
            {
                using (GAFContext context = new GAFContext())
                    return context.BotLocalization.First(l => l.Code.Equals("cmdUsageBirthday")).String;
            }
        }

        private static Timer _refreshTimer;

        public static void Init()
        {

            _refreshTimer = new Timer()
            {
                Interval = 1 * 60 * 60 * 1000,
                AutoReset = true
            };

            _refreshTimer.Elapsed += RefreshTimerElapsed;
            _refreshTimer.Start();

            Program.CommandHandler.Register(new BirthdayCommand());
            Logger.Log(nameof(BirthdayCommand) + " Registered");
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

            BotLocalization birthdayLocale;

            using (GAFContext context = new GAFContext())
                birthdayLocale = context.BotLocalization.First(l => l.Code.Equals("cmdOutputBirthday"));

            //General staff 239677046274392066
            foreach (BotBirthday bday in birthdays)
            {
                string toSend = birthdayLocale.Format((bday.Year == 0 ? "" : $" {DateTime.UtcNow.Year - bday.Year}."), bday.DiscordId.ToString());
                Coding.Discord.SendMessage(239677046274392066, toSend);
            }
        }

        public void Activate(CommandEventArg e)
        {
            string[] split;

            BotLocalization couldNotParseLocale;
            using (GAFContext context = new GAFContext())
                couldNotParseLocale = context.BotLocalization.First(l => l.Code.Equals("cmdErrorBirthdayParse"));

            if (e.AfterCMD.Contains('.'))
                split = e.AfterCMD.Split('.');
            else if (e.AfterCMD.Contains('\\'))
                split = e.AfterCMD.Split('\\');
            else if (e.AfterCMD.Contains('/'))
                split = e.AfterCMD.Split('/');
            else
            {
                Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                return;
            }

            if (!int.TryParse(split[0], out int day))
            {
                Coding.Discord.SendMessage(e.ChannelID, couldNotParseLocale.Format(split[0]));
                return;
            }
            
            day = Math.Min(31, Math.Max(0, day));

            if (!int.TryParse(split[1], out int month))
            {
                Coding.Discord.SendMessage(e.ChannelID, couldNotParseLocale.Format(split[1]));
                return;
            }

            month = Math.Min(31, Math.Max(0, month));

            int year = 0;
            if (split.Length == 3 && !int.TryParse(split[2], out year))
            {
                Coding.Discord.SendMessage(e.ChannelID, couldNotParseLocale.Format(split[2]));
                return;
            }

            BotBirthday bbday;

            string message;
            using (GAFContext context = new GAFBot.Database.GAFContext())
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
                }
                else
                {
                    bbday.Day = day;
                    bbday.Month = month;
                    bbday.Year = year;

                    context.BotBirthday.Update(bbday);
                    context.SaveChanges();
                }

                message = context.BotLocalization.First(l => l.Code.Equals("cmdBirthdaySet")).Format(bbday.Day.ToString(), bbday.Month.ToString(), bbday.Year.ToString());
            }

            Coding.Discord.SendMessage(e.ChannelID, message);
            Logger.Log(message);
        }
    }
}
