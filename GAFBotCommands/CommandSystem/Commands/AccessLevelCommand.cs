using GAFBot.Database.Models;
using GAFBot.Database.Readers;
using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class AccessCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "access"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Shows your accesslevel";

        public string DescriptionUsage => "!access";

        public static void Init()
        {
            Program.CommandHandler.Register(new AccessCommand() as ICommand);
            Logger.Log(nameof(AccessCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            AccessLevel access = AccessLevel.User;
            
            using (BaseDBReader<BotUsers> botusersReader = new BaseDBReader<BotUsers>())
            {
                BotUsers botuser = botusersReader.Get(bu => (ulong)bu.DiscordId == e.DUserID);

                if (botuser != null)
                    access = (AccessLevel)botuser.AccessLevel;
            }

            Coding.Discord.SendMessage(e.ChannelID, access.ToString());
        }
    }
}

