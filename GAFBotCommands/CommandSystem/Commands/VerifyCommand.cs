using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class VerifyCommand : ICommand
    {
        public char Activator { get => '!'; }
        public string CMD { get => "verify"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public Verification.Osu.VerificationHandler VerificationHandler { get { return Program.VerificationHandler; } }

        public static void Init()
        {
            Program.CommandHandler.Register(new VerifyCommand() as ICommand);
            Coding.Methods.Log(typeof(VerifyCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                var dclient = Coding.Methods.GetClient();
                var duser = dclient.GetUserAsync(e.DUserID).Result;
                var privChannel = dclient.CreateDmAsync(duser).Result;
                var dguild = dclient.Guilds.First(g => g.Key == Program.Config.DiscordGuildId).Value;
                var dchannel = dguild.GetChannel(e.ChannelID);
                var member = dguild.GetMemberAsync(e.DUserID).Result;

                User user = null;
                while (!Program.MessageHandler.Users.TryGetValue(e.DUserID, out user))
                    System.Threading.Tasks.Task.Delay(5).Wait();

                if (user != null && user.Verified)
                {
                    Coding.Methods.SendMessage(e.ChannelID, $"You already have been Verified (osu: {user.OsuUserName})");
                    return;
                }
                
                if (Program.Config.BypassVerification.Length > 0)
                {
                    foreach (var drole in member.Roles)
                    {
                        if (Program.Config.BypassVerification.Contains(drole.Id) && string.IsNullOrEmpty(e.AfterCMD))
                        {
                            Coding.Methods.SendMessage(e.ChannelID, "Due to bypass settings you only need to set your account, please type: !verify Your_Osu_Account_Name");
                            return;
                        }
                        else if (Program.Config.BypassVerification.Contains(drole.Id) && !string.IsNullOrEmpty(e.AfterCMD))
                        {
                            VerificationHandler.VerifyUser(e.DUserID, e.AfterCMD);
                            Coding.Methods.SendMessage(e.ChannelID, "Your osu account has been confirmed due to current bypass settings");
                            return;
                        }
                    }
                }
                
                (string, string) result = VerificationHandler.StartVerification(e.DUserID);

                if (result.Item1.Equals("v") && result.Item2 == "active")
                {
                    dchannel.SendMessageAsync($"Verification process is already running for you");
                    return;
                }
                else if (result.Item1.Equals("e"))
                {
                    dchannel.SendMessageAsync($"Verification failed. please contact skyfly with the following code: ''VerifyCommand.Activate.[{e.AfterCMD}|{e.GuildID}|{VerificationHandler == null}|{result.Item1 ?? "null"}|{result.Item2 ?? "null"}]''").Wait();
                    return;
                }
                
                privChannel.SendMessageAsync($"Please login to osu! and contact Skyfly (in osu!) with the following: !verify {result.Item1}").Wait();
            }
            catch (Exception ex)
            {
                Program.Logger.Log(ex.ToString(), showConsole: Program.Config.Debug);
            }
        }
    }
}
