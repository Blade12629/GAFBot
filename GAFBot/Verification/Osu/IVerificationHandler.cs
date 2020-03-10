using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Verification.Osu
{
    public interface IVerificationHandler : Modules.IModule
    {
        string Host { get; set; }
        int Port { get; set; }

        void SetAuthentication(string user, string pass);
        void SetHost(string host, int port);
        void ClientGotMessage(object sender, NetIrc2.Events.ChatMessageEventArgs e);
        void VerifyUser(ulong duserid, string osuUser, string senderOsu);
        void Start();
        void Stop();
        void GetUserStatus(string user, DSharpPlus.Entities.DiscordMessage message, string originalText);

        bool IsUserVerified(ulong duserid);
        bool IsUserVerified(string osuUser);
        
        string StartVerification(ulong duserid);
    }
}
