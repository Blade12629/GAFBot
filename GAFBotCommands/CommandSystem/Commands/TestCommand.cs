using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class TestCommand : ICommand
    {
        public char Activator { get => '!'; }
        public string CMD { get => "test"; }
        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public static void Init()
        {
            Program.CommandHandler.Register(new TestCommand() as ICommand);
            Coding.Methods.Log(typeof(TestCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                Coding.Methods.SendMessage(e.ChannelID, "success");
            }
            catch (Exception ex)
            {
                Coding.Methods.SendMessage(e.ChannelID, ex.ToString());
                Console.WriteLine(ex);
            }
        }
    }
}
