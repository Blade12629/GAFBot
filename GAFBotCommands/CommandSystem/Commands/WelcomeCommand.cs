﻿using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class WelcomeCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "welcome"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public string Description => "Sets the welcome message";

        public string DescriptionUsage => "Usage: !welcome welcome message";

        public static void Init()
        {
            Program.CommandHandler.Register(new WelcomeCommand() as ICommand);
            Logger.Log(typeof(WelcomeCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            var dguild = Coding.Discord.GetGuild((ulong)Program.Config.DiscordGuildId);
            var dchannel = Coding.Discord.GetChannel((ulong)Program.Config.WelcomeChannel);
            
            string response = "```" + Environment.NewLine;

            foreach (var role in dguild.Roles)
                response += role.Name + ":" + role.Id + Environment.NewLine;

            response += "```";
            dchannel.SendMessageAsync(response).Wait();

            var duser = Coding.Discord.GetUser(e.DUserID);

            if (e.AfterCMD.StartsWith("_test", StringComparison.CurrentCultureIgnoreCase))
                (Modules.ModuleHandler.Get("message") as IMessageHandler)?.WelcomeMessage(e.ChannelID, Program.Config.WelcomeMessage, duser.Mention);

            else
            {
                Program.Config.WelcomeMessage = e.AfterCMD;
                Coding.Discord.SendMessage(e.ChannelID, "Updated Welcomemessage to " + e.AfterCMD);
            }
        }
    }
}
