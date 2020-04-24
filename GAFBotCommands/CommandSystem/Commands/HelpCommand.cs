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

        public string DescriptionUsage => "Usage:" + Environment.NewLine +
                                          "!help" + Environment.NewLine +
                                          "!help command";

        public static void Init()
        {
            Program.CommandHandler.Register(new HelpCommand());
            Logger.Log(nameof(HelpCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            if (!string.IsNullOrEmpty(e.AfterCMD))
            {
                ICommand command = (Program.CommandHandler as CommandHandler).ActiveCommands.FirstOrDefault(c => (c.Activator.Equals(e.AfterCMD[0]) &&
                                                                                                                  c.CMD.Equals(e.AfterCMD.Remove(0, 1), StringComparison.CurrentCultureIgnoreCase)) ||
                                                                                                                  c.CMD.Equals(e.AfterCMD, StringComparison.CurrentCultureIgnoreCase));

                if (command == null)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Command not found");
                    return;
                }

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = "Command Info for " + command.Activator + command.CMD
                };

                builder.AddField((!string.IsNullOrEmpty(command.Description) ? command.Description : "null"), (!string.IsNullOrEmpty(command.DescriptionUsage) ? command.DescriptionUsage : "null"));

                Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: builder.Build());
                return;
            }

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
