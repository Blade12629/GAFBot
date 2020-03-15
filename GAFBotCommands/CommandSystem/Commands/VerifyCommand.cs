using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class VerifyCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "verify"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public Verification.Osu.IVerificationHandler VerificationHandler { get { return Modules.ModuleHandler.Get("verification") as Verification.Osu.IVerificationHandler; } }

        public string Description => "Starts a verification or verifies a user";

        public string DescriptionUsage => "!verify" + Environment.NewLine + "!verify discordUserId";

        public static void Init()
        {
            Program.CommandHandler.Register(new VerifyCommand() as ICommand);
            Logger.Log(nameof(VerifyCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                Verification.Osu.IVerificationHandler verifyHandler = VerificationHandler;

                if (verifyHandler == null)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Disabled");
                    return;
                }

                var dclient = Coding.Discord.GetClient();
                var dchannel = Coding.Discord.GetChannel(e.ChannelID);

                if (e.AfterCMD.StartsWith("user", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!e.GuildID.HasValue)
                    {
                        Coding.Discord.SendMessage(e.ChannelID, "You can only use this in a guild chat!");
                        return;
                    }

                    string ulongStr = e.AfterCMD.Remove(0, 5);

                    if (!ulong.TryParse(ulongStr, out ulong duserId))
                    {
                        Coding.Discord.SendMessage(e.ChannelID, $"Could not parse {duserId} to ulong");
                        return;
                    }

                    var dmember = Coding.Discord.GetMember(duserId, e.GuildID.Value);

                    using (Database.GAFContext context = new Database.GAFContext())
                    {
                        BotUsers buser = context.BotUsers.FirstOrDefault(bu => (ulong)bu.DiscordId == dmember.Id);

                        if (buser == null)
                            (Modules.ModuleHandler.Get("message") as IMessageHandler)?.Register(Coding.Discord.GetUser(duserId), (ulong)e.GuildID);
                        
                        buser = context.BotUsers.FirstOrDefault(bu => (ulong)bu.DiscordId == dmember.Id);

                        buser.IsVerified = true;
                        context.BotUsers.Update(buser);

                        BotVerifications bver = context.BotVerifications.FirstOrDefault(bv => (ulong)bv.DiscordUserId == duserId);

                        if (bver != null)
                            context.BotVerifications.Remove(bver);

                        context.SaveChanges();
                    }

                    if (Program.Config.SetVerifiedRole)
                        Coding.Discord.AssignRole(duserId, (ulong)Program.Config.DiscordGuildId, (ulong)Program.Config.VerifiedRoleId, "Verified, bypass");


                    Coding.Discord.SendMessage(e.ChannelID, "Verified " + duserId);
                    return;
                }

                BotUsers user;

                using (Database.GAFContext context = new Database.GAFContext())
                    user = context.BotUsers.FirstOrDefault(u => (ulong)u.DiscordId == e.DUserID);

                if (user != null && user.IsVerified)
                {
                    Coding.Discord.SendMessage(e.ChannelID, $"You already have been Verified (osu: {user.OsuUsername})");
                    return;
                }
                
                string result = verifyHandler.StartVerification(e.DUserID);

                if (result.Equals("active"))
                {
                    dchannel.SendMessageAsync($"Verification process is already running for you");
                    return;
                }
                else if (result == null)
                {
                    dchannel.SendMessageAsync($"Verification failed. please contact skyfly with the following code: ''VerifyCommand.Activate.[{e.AfterCMD}|{e.GuildID}|{result ?? "null"}]''").Wait();
                    return;
                }
                
                Coding.Discord.SendPrivateMessage(e.DUserID, $"Please login to osu! and contact Skyfly (in osu!) with the following: !verify {result}");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Trace);
            }
        }
    }
}
