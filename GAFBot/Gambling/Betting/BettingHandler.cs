using GAFBot.MessageSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAFBot.Gambling.Betting
{
    public class BettingHandler
    {
        public static string BettingFile { get { return Program.CurrentPath + Program.Config.BettingFile; } }
        public static int CurrentReward { get { return Program.Config.CurrentBettingReward; } set { Program.Config.CurrentBettingReward = value; } }

        public List<Bet> ActiveBets { get; private set; }
        
        public BettingHandler()
        {
            Program.Logger.Log($"Betting: BettingHandler created", showConsole: Program.Config.Debug);

            Program.SaveEvent += () => Save();
            ActiveBets = new List<Bet>();
            MessageHandler.OnMatchEnd += (teamA, teamB, winningTeam) => ResolveBets(teamA, teamB, winningTeam);
        }

        /// <summary>
        /// Checks if there is a bet already active
        /// </summary>
        public bool ContainsBet(Bet b)
        {
            if (ActiveBets.Find(bt => bt.MatchId == b.MatchId && bt.DiscordUserId == b.DiscordUserId) != null)
                return true;
            return false;
        }

        /// <summary>
        /// Checks if there is a bet already active
        /// </summary>
        public bool ContainsBet(int matchId, ulong discordUserId)
            => ContainsBet(new Bet("null", matchId, discordUserId));

        /// <summary>
        /// Adds a bet to the active bets
        /// </summary>
        public void AddBet(string team, int matchId, ulong discordUserId, ulong channelId)
        {
            Program.Logger.Log($"Betting: Adding bet ({discordUserId}, {matchId}, {team ?? "null"}) (Channel {channelId})", showConsole: Program.Config.Debug);

            Bet b = new Bet(team, matchId, discordUserId);

            AddBet(b, channelId);
        }

        /// <summary>
        /// Adds a bet to the active bets
        /// </summary>
        public void AddBet(Bet b, ulong channelId)
        {
            Program.Logger.Log($"Betting: Adding bet ({b.DiscordUserId}, {b.MatchId}, {b.Team ?? "null"}) (Channel {channelId})", showConsole: Program.Config.Debug);
            if (!ContainsBet(b))
            {
                ActiveBets.Add(b);
                Program.Logger.Log($"Betting: Bet added", showConsole: Program.Config.Debug);
                Coding.Methods.SendMessage(channelId, $"Your bet has been created ({b.MatchId} : {b.Team})");
            }
            //else if (ActiveBets.FindAll(bt => bt.DiscordUserId == b.DiscordUserId).Count >= 2)
            //{
            //    Program.Logger.Log($"Betting: Could not create bet, user already have 2 bets running", showConsole: Program.Config.Debug);
            //    Coding.Methods.SendMessage(channelId, "You already have 2 bets running");
            //}
            else
            {
                Program.Logger.Log($"Betting: Could not create bet, user already betted on this game", showConsole: Program.Config.Debug);
                Coding.Methods.SendMessage(channelId, "You already betted on this game");
            }
        }

        /// <summary>
        /// Removes a bet from the active bets
        /// </summary>
        public void RemoveBets(ulong discordUserId)
        {
            Program.Logger.Log($"Betting: Removing all bets by user " + discordUserId, showConsole: Program.Config.Debug);
            ActiveBets.RemoveAll(b => b.DiscordUserId == discordUserId);
        }

        /// <summary>
        /// Removes a bet from the active bets
        /// </summary>
        public void RemoveBet(int matchId, ulong discordUserId)
        {
            Program.Logger.Log($"Betting: Removing all bets by matchId {matchId} and user {discordUserId} ", showConsole: Program.Config.Debug);
            ActiveBets.RemoveAll(b => b.DiscordUserId == discordUserId);
        }

        /// <summary>
        /// Resolves all bets that meet the team and winningTeam condition (challonge)
        /// </summary>
        public void ResolveBets(string teamA, string teamB, string winningTeam)
        {
            Program.Logger.Log($"Betting: Resolving bets {teamA}, {teamB}, {winningTeam}", showConsole: Program.Config.Debug);

            if (ActiveBets.Count == 0)
            {
                Program.Logger.Log("Betting: No bets to resolve found", showConsole: Program.Config.Debug);
                return;
            }

            List<(Challonge.Api.ChallongeHandler.MatchState, Challonge.Api.Match)> matches = null;
            Challonge.Api.Participant participantA = null;
            Challonge.Api.Participant participantB = null;

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
                Program.Logger.Log($"Betting: Null: ParticipantA {participantA == null}, ParticipantB {participantB == null}, Matches {matches == null}, Matches Count {matches.Count}", showConsole: Program.Config.Debug);
                return;
            }

            foreach (var match in matches)
            {
                if (match.Item2.suggested_play_order < 0)
                    continue;

                ResolveBets(winningTeam, match.Item2.suggested_play_order);
            }
        }

        /// <summary>
        /// Resolves all bets that meet the team and winningTeam condition (challonge)
        /// </summary>
        public void ResolveBets(string team, int matchId)
        {
            Program.Logger.Log($"Betting: Resolving Bets {team}, {matchId}", showConsole: Program.Config.Debug);

            List<Bet> bets = new List<Bet>();
            
            lock(ActiveBets)
            {
                for (int i = 0; i < ActiveBets.Count; i++)
                {
                    if (matchId != ActiveBets[i].MatchId)
                        continue;

                    if (ActiveBets[i].Team.Equals(team, StringComparison.CurrentCultureIgnoreCase))
                        BetWin(ActiveBets[i]);

                    ActiveBets.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Invoked if a bet is won
        /// </summary>
        private void BetWin(Bet b)
        {
            User user = null;
            lock (Program.MessageHandler.Users)
            {
                user = Program.MessageHandler.Users[b.DiscordUserId];
            }
            if (user == null)
                return;

            user.Points += CurrentReward;

            Program.Logger.Log($"Betting: User {user.DiscordID} won {CurrentReward} points for betting on {b.MatchId} {b.Team}", showConsole: Program.Config.Debug);

            var privChannel = Coding.Methods.GetPrivChannel(b.DiscordUserId);
            privChannel.SendMessageAsync($"You won your bet and recieved {CurrentReward} points (Team: {b.Team}, MatchId: {b.MatchId})").Wait();
        }

        /// <summary>
        /// Saves the bets
        /// </summary>
        public void Save()
        {
            Program.Logger.Log($"Betting: Saving bets", showConsole: Program.Config.Debug);

            if (ActiveBets == null || ActiveBets.Count <= 0)
            {
                ActiveBets = new List<Bet>();
                ActiveBets.Add(new Bet("default bet", -1, 0));
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(ActiveBets.ToArray());
            System.IO.File.WriteAllText(BettingFile, json);

            Program.Logger.Log($"Betting: Bets saved", showConsole: Program.Config.Debug);
        }

        /// <summary>
        /// Loads the bets
        /// </summary>
        public void Load()
        {
            Program.Logger.Log($"Betting: Loading bets", showConsole: Program.Config.Debug);

            if (!System.IO.File.Exists(BettingFile))
                return;

            string json = System.IO.File.ReadAllText(BettingFile);
            Bet[] bets = Newtonsoft.Json.JsonConvert.DeserializeObject<Bet[]>(json);
            ActiveBets = bets.ToList();
            Program.Logger.Log($"Betting: Bets Loaded", showConsole: Program.Config.Debug);

        }
    }

    public class Bet : IEquatable<Bet>
    {
        public string Team { get; set; }
        public int MatchId { get; set; }
        public ulong DiscordUserId { get; set; }

        public Bet(string team, int matchId, ulong discordUserId)
        {
            Team = team;
            MatchId = matchId;
            DiscordUserId = discordUserId;
        }

        public bool Equals(Bet bet)
        {
            return bet.Team == Team && bet.MatchId == MatchId;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Team, MatchId);
        }

        public static bool operator ==(Bet bet1, Bet bet2)
        {
            return EqualityComparer<Bet>.Default.Equals(bet1, bet2);
        }

        public static bool operator !=(Bet bet1, Bet bet2)
        {
            return !(bet1 == bet2);
        }
    }
}
