using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class IrcCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "irc"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public static void Init()
        {
            Program.CommandHandler.Register(new IrcCommand() as ICommand);
            Coding.Methods.Log(typeof(IrcCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            if (!e.AfterCMD.Equals("restart", StringComparison.CurrentCultureIgnoreCase))
                return;

            try
            {
                Program.VerificationHandler.IrcStop();
            }
            catch (Exception ex)
            {
                Program.Logger.Log(ex.ToString(), showConsole: Program.Config.Debug);
            }

            try
            {
                Program.VerificationHandler.IrcStart();
            }
            catch (Exception ex)
            {
                Program.Logger.Log(ex.ToString(), showConsole: Program.Config.Debug);
            }

            Coding.Methods.SendMessage(e.ChannelID, "Restarting irc");
        }
    }
}
