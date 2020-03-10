using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GAFBot.Commands
{
    public class SettingsCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "settings"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public static void Init()
        {
            Program.CommandHandler.Register(new SettingsCommand() as ICommand);
            Coding.Methods.Log(typeof(SettingsCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                Console.WriteLine(e.AfterCMD);
                string[] msgS = e.AfterCMD.Split(' ');
                ulong userId = 0;
                int currentIndex = 0;
                string merged;


                if (!ulong.TryParse(msgS[0], out userId))
                {
                    userId = e.DUserID;
                    currentIndex++;
                }
                
                switch (msgS[currentIndex].ToLower())
                {
                    case "v":
                    case "verification":
                        {
                            currentIndex++;
                            switch (msgS[currentIndex].ToLower())
                            {
                                default:
                                case "set":
                                    currentIndex++;
                                    switch (msgS[currentIndex].ToLower())
                                    {
                                        default:
                                        case "u":
                                        case "name":
                                        case "username":
                                            {
                                                currentIndex++;
                                                int paramLength = 0;

                                                for (int i = 0; i < currentIndex; i++)
                                                    paramLength += msgS[i].Length + 1;

                                                string newName = e.AfterCMD.Remove(0, paramLength);

                                                using (Database.GAFContext context = new Database.GAFContext())
                                                {
                                                    BotUsers user = context.BotUsers.First(u => (ulong)u.DiscordId == userId);

                                                    if (user == null)
                                                    {
                                                        Coding.Methods.SendMessage(e.ChannelID, $"Could not find user " + userId);
                                                        return;
                                                    }

                                                    user.OsuUsername = newName;

                                                    context.BotUsers.Update(user);
                                                    context.SaveChanges();

                                                }

                                                Coding.Methods.SendMessage(e.ChannelID, $"Set user {userId} osu name to {newName}");
                                                break;
                                            }
                                        case "v":
                                        case "verified":
                                            {
                                                bool verified = false;
                                                currentIndex++;

                                                if (!bool.TryParse(msgS[currentIndex].ToLower(), out verified))
                                                {
                                                    Coding.Methods.SendMessage(e.ChannelID, $"Could not parse {msgS[currentIndex]} as bool");
                                                    return;
                                                }

                                                using (Database.GAFContext context = new Database.GAFContext())
                                                {
                                                    BotUsers user = context.BotUsers.First(u => (ulong)u.DiscordId == userId);

                                                    if (user == null)
                                                    {
                                                        Coding.Methods.SendMessage(e.ChannelID, $"Could not find user " + userId);
                                                        return;
                                                    }

                                                    user.IsVerified = verified;

                                                    context.BotUsers.Update(user);
                                                    context.SaveChanges();
                                                }

                                                Coding.Methods.SendMessage(e.ChannelID, $"Set user {userId} verified to {verified}");
                                            }
                                            break;

                                    }
                                    break;
                                case "get":
                                    currentIndex++;


                                    break;
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Trace);
            }
        }

        private enum VerificationEnum
        {
            UserName,
            Verified
        }
    }
}
