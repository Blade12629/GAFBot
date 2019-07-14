using GAFBot.MessageSystem;
using System;
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



                                                User user = GetUser(userId);

                                                if (user == null)
                                                {
                                                    Coding.Methods.SendMessage(e.ChannelID, $"Could not find user " + userId);
                                                    return;
                                                }

                                                user.OsuUserName = newName;
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

                                                User user = GetUser(userId);

                                                if (user == null)
                                                {
                                                    Coding.Methods.SendMessage(e.ChannelID, $"Could not find user " + userId);
                                                    return;
                                                }

                                                user.Verified = verified;
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
                Program.Logger.Log(ex.ToString(), showConsole: Program.Config.Debug);
            }
        }

        private User GetUser(ulong userid)
        {
            User user = null;

            if (!Program.MessageHandler.Users.ContainsKey(userid))
                return null;

            while (!Program.MessageHandler.Users.TryGetValue(userid, out user))
                Task.Delay(5).Wait();

            return user;
        }

        private enum VerificationEnum
        {
            UserName,
            Verified
        }
    }
}
