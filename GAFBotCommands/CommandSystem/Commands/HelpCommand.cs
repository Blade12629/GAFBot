using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using GAFBot.MessageSystem;

namespace GAFBot.Commands
{
    public class HelpCommand : ICommand
    {
        public char Activator => '!';

        public string CMD => "help";

        public char ActivatorSpecial => char.MinValue;

        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Displays a list of available commands";

        public string DescriptionUsage => "Usage: !help";

        public static void Init()
        {
            Program.CommandHandler.Register(new HelpCommand());
            Logger.Log(nameof(HelpCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            ICommand[] commands = (Program.CommandHandler as CommandHandler).ActiveCommands.ToArray();
            string response = "";

            Database.Models.BotUsers user;

            using (Database.GAFContext context = new Database.GAFContext())
                user = context.BotUsers.First(u => u.DiscordId == (long)e.DUserID);

            AccessLevel access = user == null ? AccessLevel.User : (AccessLevel)user.AccessLevel;

            foreach (ICommand cmd in commands)
                if (cmd.AccessLevel <= access)
                    response += Environment.NewLine + $"{cmd.Activator}{cmd.CMD}: {cmd.Description}";

            Coding.Discord.SendMessage(e.ChannelID, $"```{Environment.NewLine}{response}{Environment.NewLine}```");
        }
    }
}
