using GAFBot.Database.Models;
using GAFBot.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Gambling.Betting
{
    public interface IBettingHandler : IModule
    {
        bool ContainsBet(long matchId = -1, string team = null, StringComparison comparer = StringComparison.CurrentCulture, ulong discordUserId = 0);
        bool ContainsBet(int matchId, ulong discordUserId);

        List<BotBets> GetBets(long matchId = -1, string team = null, StringComparison comparer = StringComparison.CurrentCulture, ulong discordUserId = 0);

        void AddBet(string team, int matchId, ulong discordUserId, ulong channelId);
        void AddBet(BotBets bet, ulong channelId);
        void RemoveBets(ulong discordUserId);
        void RemoveBet(int matchId, ulong discordUserId);
        void ResolveBets(string teamA, string teamB, string winningTeam);
        void ResolveBets(string winningTeam, string losingTeam, int matchId);
    }
}
