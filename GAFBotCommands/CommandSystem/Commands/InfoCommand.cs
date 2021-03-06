﻿using GAFBot.Database.Models;
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
        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public string Description => "Displays info about an discord user";

        public string DescriptionUsage => "Usage: !infou discordUserId";

        public static void Init()
        {
            Program.CommandHandler.Register(new InfoCommand() as ICommand);
            Logger.Log(nameof(InfoCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            ulong userid = 0;
            if (string.IsNullOrEmpty(e.AfterCMD))
            {
                Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                return;
            }
            else if (e.AfterCMD.StartsWith("verifications"))
            {
                using (Database.GAFContext context = new Database.GAFContext())
                    Coding.Discord.SendMessage(e.ChannelID, "Currently verified players: " + context.BotUsers.Where(bu => bu.IsVerified)?.Count());
                return;
            }
            else if (!ulong.TryParse(e.AfterCMD, out userid))
            {
                //check if it's a mention
                string mention = e.AfterCMD.TrimStart('<', '@', '!');
                mention = mention.Remove(mention.ToList().FindIndex(c => c.Equals('>')), 1);

                if (!ulong.TryParse(mention, out userid))
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Could not parse userid");
                    return;
                }
            }

            BotUsers user;

            using (Database.GAFContext context = new Database.GAFContext())
                user = context.BotUsers.FirstOrDefault(u => (ulong)u.DiscordId == userid);

            var duser = Coding.Discord.GetUser(userid);

            if (user == null)
            {
                string response = $"```{Environment.NewLine}User not registered in database{Environment.NewLine}User: {duser.Username} ({duser.Id}){Environment.NewLine}Access: {AccessLevel.User.ToString()}{Environment.NewLine}Points: 0{Environment.NewLine}Registered: {DateTime.MinValue}{Environment.NewLine}Osu: null{Environment.NewLine}Verified: false{Environment.NewLine}```";
                Coding.Discord.SendMessage(e.ChannelID, response);
                return;
            }

            string responseStr = $"```{Environment.NewLine}User: {duser.Username} ({user.DiscordId}){Environment.NewLine}Access: {user.AccessLevel}{Environment.NewLine}Points:{user.Points}{Environment.NewLine}Registered:{user.RegisteredOn}{Environment.NewLine}Osu:{user.OsuUsername ?? "null"}{Environment.NewLine}Verified:{user.IsVerified}{Environment.NewLine}```";
            Coding.Discord.SendMessage(e.ChannelID, responseStr);
        }
    }
}
