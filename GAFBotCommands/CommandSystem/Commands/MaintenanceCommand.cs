using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using GAFBot.MessageSystem;

namespace GAFBot.Commands
{
    public class MaintenanceCommand : ICommand
    {
        public char Activator => '!';

        public string CMD => "maintenance";

        public char ActivatorSpecial => char.MinValue;

        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Turns maintenance on or off";

        public string DescriptionUsage => "Usage: !maintenance true/false <message>";

        public static void Init()
        {
            Program.CommandHandler.Register(new MaintenanceCommand());
            Logger.Log(nameof(MaintenanceCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            int indexSpace = e.AfterCMD.IndexOf(' ');

            if (indexSpace <= 0)
            {
                Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                return;
            }
            if (!bool.TryParse(e.AfterCMD.Substring(0, indexSpace), out bool status))
            {
                Coding.Discord.SendMessage(e.ChannelID, "Could not parse status");
                return;
            }

            string message = null;
            if (e.AfterCMD.Length != indexSpace - 1)
                message = e.AfterCMD.Remove(0, indexSpace + 1);

            using (Database.GAFContext context = new Database.GAFContext())
            {
                int id = context.BotMaintenance.Max(m => m.Id);
                Database.Models.BotMaintenance maint = context.BotMaintenance.FirstOrDefault(m => m.Id == id);

                if (maint == null)
                    context.BotMaintenance.Add(new Database.Models.BotMaintenance()
                    {
                        Enabled = status,
                        Notification = message
                    });
                else
                {
                    maint.Enabled = status;

                    if (message != null)
                        maint.Notification = message;
                }

                context.SaveChanges();
            }
        }
    }
}
