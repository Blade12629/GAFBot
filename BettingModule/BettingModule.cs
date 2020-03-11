using GAFBot;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.Gambling.Betting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BettingModule
{
    public class BettingModule : IBettingHandler
    {
        public bool Enabled { get; set; }

        public string ModuleName => "betting";

        /// <summary>
        /// Adds a bet to the active bets
        /// </summary>
        public void AddBet(string team, int matchId, ulong discordUserId, ulong channelId)
        {
            Logger.Log($"Betting: Adding bet ({discordUserId}, {matchId}, {team ?? "null"}) (Channel {channelId})", LogLevel.Trace);

            BotBets b = new BotBets()
            {
                Team = team,
                Matchid = matchId,
                DiscordUserId = (long)discordUserId
            };

            AddBet(b, channelId);
        }

        /// <summary>
        /// Adds a bet to the active bets
        /// </summary>
        public void AddBet(BotBets b, ulong channelId)
        {
            var bets = GetBets(matchId: b.Matchid, discordUserId: (ulong)b.DiscordUserId);

            if (bets.Count == 0)
            {
                Logger.Log($"Betting: Adding bet ({b.DiscordUserId}, {b.Matchid}, {b.Team ?? "null"}) (Channel {channelId})", LogLevel.Trace);

                using (GAFContext context = new GAFContext())
                {
                    context.BotBets.Add(b);
                    context.SaveChanges();
                }

                Logger.Log($"Betting: Bet added", LogLevel.Trace);
                Coding.Methods.SendMessage(channelId, $"Your bet has been created ({b.Matchid} : {b.Team})");

                return;
            }

            BotBets bet = bets[0];

            Logger.Log($"Betting: Could not create bet, user already betted on this game", LogLevel.Trace);
            Coding.Methods.SendMessage(channelId, "You already betted on this game");
        }

        /// <summary>
        /// Checks if a bet exists, pick atleast one option
        /// </summary>
        /// <param name="matchId">Osu match id</param>
        /// <param name="team">Team name</param>
        /// <param name="comparer">Team name comparer</param>
        /// <param name="discordUserId">Discord user id</param>
        /// <returns>bet exists</returns>
        public bool ContainsBet(long matchId = -1, string team = null, StringComparison comparer = StringComparison.CurrentCulture, ulong discordUserId = 0)
        {
            List<BotBets> bets = GetBets(matchId, team, comparer, discordUserId);

            return bets.Count != 0;
        }

        /// <summary>
        /// Checks if a bet exists, pick atleast one option
        /// </summary>
        /// <param name="matchId">Osu match id</param>
        /// <param name="discordUserId">Discord user id</param>
        /// <returns>bet exists</returns>
        public bool ContainsBet(int matchId, ulong discordUserId)
        {
            return ContainsBet(matchId: matchId, discordUserId: discordUserId);
        }

        public void Disable()
        {
            
        }

        public void Dispose()
        {
            
        }

        public void Enable()
        {
            
        }

        /// <summary>
        /// Gets a list of bets, pick atleast one option
        /// </summary>
        /// <param name="matchId">Osu match id</param>
        /// <param name="team">Team name</param>
        /// <param name="comparer">Team name comparer</param>
        /// <param name="discordUserId">Discord user id</param>
        /// <returns>Bets</returns>
        public List<BotBets> GetBets(long matchId = -1, string team = null, StringComparison comparer = StringComparison.CurrentCulture, ulong discordUserId = 0)
        {
            List<Func<BotBets, bool>> funcs = new List<Func<BotBets, bool>>();

            if (matchId > -1)
                funcs.Add(bb => bb.Matchid == matchId);
            if (!string.IsNullOrEmpty(team))
                funcs.Add(bb => bb.Team.Equals(team, comparer));
            if (discordUserId > 0)
                funcs.Add(bb => (ulong)bb.DiscordUserId == discordUserId);

            List<BotBets> bets;

            using (GAFContext context = new GAFContext())
                bets = context.BotBets.Where(funcs[0]).ToList();

            funcs.RemoveAt(0);

            foreach (var f in funcs)
                bets = bets.Where(f).ToList();

            return bets;
        }

        public void Initialize()
        {
            
        }

        /// <summary>
        /// Removes a bet from the active bets
        /// </summary>
        public void RemoveBet(int matchId, ulong discordUserId)
        {
            Logger.Log($"Betting: Removing all bets by matchId {matchId} and user {discordUserId} ", LogLevel.Trace);

            using (GAFContext context = new GAFContext())
            {
                foreach (BotBets bet in context.BotBets.Where(bb => (ulong)bb.DiscordUserId == discordUserId && bb.Matchid == matchId))
                    context.BotBets.Remove(bet);

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Removes a bet from the active bets
        /// </summary>
        public void RemoveBets(ulong discordUserId)
        {
            Logger.Log($"Betting: Removing all bets by user " + discordUserId, LogLevel.Trace);

            using (GAFContext context = new GAFContext())
            {
                foreach (BotBets bet in context.BotBets.Where(bb => (ulong)bb.DiscordUserId == discordUserId))
                    context.BotBets.Remove(bet);

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Resolves all bets that meet the team and winningTeam condition (challonge)
        /// </summary>
        public void ResolveBets(string teamA, string teamB, string winningTeam)
        {
            Logger.Log($"Betting: Resolving bets {teamA}, {teamB}, {winningTeam}", LogLevel.Trace);

            List<(GAFBot.Challonge.Api.ChallongeHandler.MatchState, GAFBot.Challonge.Api.Match)> matches = null;
            GAFBot.Challonge.Api.Participant participantA = null;
            GAFBot.Challonge.Api.Participant participantB = null;

            lock (Program.ChallongeHandler)
            {
                participantA = Program.ChallongeHandler.GetParticipant(teamA);
                if (participantA == null)
                    return;
                participantB = Program.ChallongeHandler.GetParticipant(teamB);
                if (participantB == null)
                    return;
                matches = Program.ChallongeHandler.Matches.FindAll(match => (match.Item2.player1_id == participantA.id && match.Item2.player2_id == participantB.id) || (match.Item2.player1_id == participantB.id && match.Item2.player2_id == participantA.id));
            }

            if (participantA == null || participantB == null || matches == null || matches.Count == 0)
            {
                Logger.Log($"Betting: Null: ParticipantA {participantA == null}, ParticipantB {participantB == null}, Matches {matches == null}, Matches Count {matches.Count}", LogLevel.Trace);
                return;
            }

            foreach (var match in matches)
            {
                if (match.Item2.suggested_play_order < 0)
                    continue;

                ResolveBets(winningTeam, (winningTeam.Equals(teamA) ? teamB : teamA), match.Item2.suggested_play_order);
            }
        }

        /// <summary>
        /// Resolves all bets that meet the team and winningTeam condition (challonge)
        /// </summary>
        public void ResolveBets(string winningTeam, string losingTeam, int matchId)
        {
            Logger.Log($"Betting: Resolving Bets {winningTeam}, {matchId}", LogLevel.Trace);

            List<BotBets> bets = GetBets(matchId, winningTeam);

            if (bets.Count == 0)
                return;

            int currentReward = Program.Config.CurrentBettingReward;

            using (GAFContext context = new GAFContext())
            {
                foreach (BotBets bet in bets)
                {
                    var privChannel = Coding.Methods.GetPrivChannel((ulong)bet.DiscordUserId);
                    privChannel.SendMessageAsync($"You won your bet and recieved {currentReward} points (Team: {bet.Team}, MatchId: {bet.Matchid})").Wait();

                    Logger.Log($"Betting: User {bet.DiscordUserId} won {currentReward} points for betting on {bet.Matchid} {bet.Team}", LogLevel.Trace);

                    context.Remove(bet);
                }

                bets.Clear();
                bets = GetBets(matchId, losingTeam);

                foreach (BotBets bet in bets)
                    context.Remove(bet);

                context.SaveChanges();
            }
        }
    }
}
