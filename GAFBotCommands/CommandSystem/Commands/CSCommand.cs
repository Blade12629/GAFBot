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

        public static void Init()
        {
            Program.CommandHandler.Register(new CSCommand() as ICommand);
            Coding.Methods.Log(typeof(CSCommand).Name + " Registered");
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
                        Program.LoadCommands();
                        Coding.Methods.SendMessage(e.ChannelID, "Commands reloaded");
                        Coding.Methods.Log("Commands reloaded");
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
