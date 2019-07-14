using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class UserCommand : ICommand
    {
        public char Activator { get => '!'; }
        public string CMD { get => "user"; }
        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public static void Init()
        {
            Program.CommandHandler.Register(new UserCommand() as ICommand);
            Coding.Methods.Log(typeof(UserCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.AfterCMD))
                    return;

                string[] split = e.AfterCMD.Split(' ');

                ulong userId = 0;

                if (ulong.TryParse(split[0], out userId))
                    split = e.AfterCMD.Remove(0, split[0].Length + 1).Split(' ');
                else
                    userId = e.DUserID;

                ulong? duserId = null;

                switch (split[0].ToLower())
                {
                    case "access":
                    case "accesslevel":
                    case "acl":
                    case "al":

                        break;
                }
            }
            catch (Exception ex)
            {
                Coding.Methods.SendMessage(e.ChannelID, ex.ToString());
                Console.WriteLine(ex);
            }
        }
    }
}
