using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class CSCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "cs"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public string Description => "Reloads commands";

        public string DescriptionUsage => "!cs r";

        public static void Init()
        {
            Program.CommandHandler.Register(new CSCommand() as ICommand);
            Logger.Log(nameof(CSCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                string[] paramS = e.AfterCMD.Split(' ');
                
                switch(paramS[0].ToLower())
                {
                    case "r":
                    case "reload":
                        Coding.Discord.SendMessage(e.ChannelID, "Reloading Commands...");

                        Program.LoadCommands();

                        Coding.Discord.SendMessage(e.ChannelID, "Commands reloaded");
                        Logger.Log("Commands reloaded");
                        break;
                }
            }
            catch (Exception ex)
            {
                Coding.Discord.SendMessage(e.ChannelID, ex.ToString());
            }
        }
    }
}
