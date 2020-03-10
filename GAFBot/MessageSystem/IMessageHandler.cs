using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.MessageSystem
{
    public interface IMessageHandler : Modules.IModule
    {
        void OnMessageRecieved(MessageCreateEventArgs e);
        void GetBeatmapInfo(string message, ulong channelId);
        void WelcomeMessage(ulong channel, string welcomeMessage, string mentionString);
        void OnMemberRemoved(GuildMemberRemoveEventArgs args);
        AccessLevel GetAccessLevel(ulong user);
        void FakeTrigger(string teama, string teamb, string winningteam);
        void StartAnalyzer(MessageCreateEventArgs args, bool sendToApi = true, bool sendToDatabase = true);
        void Register(DiscordUser duser, ulong guildId = 0);
        void OnUserJoinedGuild(GuildMemberAddEventArgs args);

        EventHandler OnMatchEnd { get; set; }
    }
}
