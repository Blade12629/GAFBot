using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class ConfigCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "config"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public static void Init()
        {
            Program.CommandHandler.Register(new ConfigCommand() as ICommand);
            Coding.Methods.Log(typeof(ConfigCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                string[] paramS = e.AfterCMD.Split(' ');

                switch (paramS[0].ToLower())
                {
                    case "r":
                    case "reload":
                        Program.LoadConfig();
                        Program.LoadCommands();
                        Coding.Methods.SendMessage(e.ChannelID, "Config reloaded");
                        Coding.Methods.Log("Config reloaded");
                        break;
                }
            }
            catch (Exception ex)
            {
                Coding.Methods.SendMessage(e.ChannelID, ex.ToString());
            }
        }
    }
}
