﻿using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Linq;

namespace GAFBot.Commands
{
    public class IsOnlineCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "isonline"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Checks if a user is online ingame";

        public string DescriptionUsage => "!isonline Osu_User_Name";

        public static void Init()
        {
            Program.CommandHandler.Register(new IsOnlineCommand() as ICommand);
            Logger.Log(nameof(IsOnlineCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {

                AccessLevel access = (Modules.ModuleHandler.Get("message") as IMessageHandler)?.GetAccessLevel(e.DUserID) ?? AccessLevel.User;
                var dchannel = Coding.Discord.GetChannel(e.ChannelID);

                if ((int)access <= (int)AccessLevel.Moderator)
                {
                    if (!e.GuildID.HasValue)
                        return;

                    var dmember = Coding.Discord.GetMember(e.DUserID, e.GuildID.Value);
                    var roleList = dmember.Roles.Where(r => r.Id == (ulong)Program.Config.RefereeRoleId);

                    if (roleList.Count() <= 0)
                        return;
                }

                string discordUser = "";
                string discordStatus = "";
                ulong discordId = 0;

                BotUsers user;

                using (Database.GAFContext context = new Database.GAFContext())
                    user = context.BotUsers.First(u => u.OsuUsername.Equals(e.AfterCMD.Replace(' ', '_'), StringComparison.CurrentCultureIgnoreCase));

                if (user != null)
                {
                    var duser = Coding.Discord.GetUser((ulong)user.DiscordId);
                    discordUser = duser.Username;
                    discordId = duser.Id;

                    //Skyfly, test purposes
                    if (discordId == 140896783717892097)
                    {
                        var dmember = Coding.Discord.GetMember(duser.Id, 147255853341212672);
                        if (dmember.Presence != null)
                            discordStatus = dmember.Presence.Status.ToString();
                    }
                    else if (e.GuildID.HasValue)
                    {
                        var dmember = Coding.Discord.GetMember(duser.Id, e.GuildID.Value);
                        if (dmember.Presence != null)
                            discordStatus = dmember.Presence.Status.ToString();
                    }
                    else if (duser.Presence != null)
                        discordStatus = duser.Presence.Status.ToString();
                }

                string message = $"Online status for {e.AfterCMD}:{Environment.NewLine}```{Environment.NewLine}" +
                                                         $"Discord {(string.IsNullOrEmpty(discordUser) ? "not found" : discordUser)} ({discordId}): {(string.IsNullOrEmpty(discordStatus) ? "n/a" : discordStatus)}{Environment.NewLine}" +
                                                         $"Osu: ";
                
                var dmessage = dchannel.SendMessageAsync(message + "awaiting..." + Environment.NewLine + "```").Result;

                Program.RequestOsuUserStatus(e.AfterCMD, dmessage, message);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }
    }
}
