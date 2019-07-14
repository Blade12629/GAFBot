using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class InfoCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "infou"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public static void Init()
        {
            Program.CommandHandler.Register(new InfoCommand() as ICommand);
            Coding.Methods.Log(typeof(InfoCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            ulong userid = 0;
            if (string.IsNullOrEmpty(e.AfterCMD))
            {
                Coding.Methods.SendMessage(e.ChannelID, "Usage: !infou discordUserId");
                return;
            }
            else if (e.AfterCMD.StartsWith("verifications"))
            {
                Coding.Methods.SendMessage(e.ChannelID, "Currently verified players: " + Program.MessageHandler.Users.Values.Count(u => u.Verified));
                return;
            }
            else if (!ulong.TryParse(e.AfterCMD, out userid))
            {
                //check if it's a mention
                string mention = e.AfterCMD.TrimStart('<', '@', '!');
                mention = mention.Remove(mention.ToList().FindIndex(c => c.Equals('>')), 1);

                if (!ulong.TryParse(mention, out userid))
                {
                    Coding.Methods.SendMessage(e.ChannelID, "Could not parse userid");
                    return;
                }
            }

            if (Program.MessageHandler.Users.ContainsKey(userid))
            {
                User user = null;

                while (!Program.MessageHandler.Users.TryGetValue(userid, out user))
                    System.Threading.Tasks.Task.Delay(5).Wait();

                if (user == null)
                {
                    Coding.Methods.SendMessage(e.ChannelID, "Could not get user");
                    return;
                }

                var duser = Coding.Methods.GetUser(userid);

                string responseStr = $"```{Environment.NewLine}User: {duser.Username} ({user.DiscordID}){Environment.NewLine}Access: {user.AccessLevel}{Environment.NewLine}Points:{user.Points}{Environment.NewLine}Registered:{user.RegisteredOn}{Environment.NewLine}Osu:{user.OsuUserName ?? "null"}{Environment.NewLine}Verified:{user.Verified}{Environment.NewLine}```";
                Coding.Methods.SendMessage(e.ChannelID, responseStr);
            }
        }
    }
}
