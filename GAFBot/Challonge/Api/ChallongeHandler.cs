using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GAFBot.Challonge.Api
{
    public class ChallongeHandler
    {
        #region cache
        public List<(MatchState, Match)> Matches { get; private set; }
        public List<Participant> Participants { get; private set; }
        #endregion
        
        public DateTime LastUpdate { get; private set; }
        public DateTime NextUpdate { get; private set; }

        public ChallongeHandler()
        {
            Logger.Log($"Challonge: ChallongeHandler created", LogLevel.Trace);

            Matches = new List<(MatchState, Match)>();
            Participants = new List<Participant>();
        }

        /// <summary>
        /// Updates current cache
        /// </summary>
        public void Update()
        {
            Logger.Log($"Challonge: Checking for update", LogLevel.Trace);

            if (DateTime.UtcNow.Ticks < NextUpdate.Ticks)
            {
                Logger.Log($"Challonge: No updates found", LogLevel.Trace);
                return;
            }
            else
            {
                LastUpdate = DateTime.UtcNow;
                NextUpdate = LastUpdate.AddHours(8);

                Logger.Log($"Challonge: Updating, next update " + NextUpdate.ToString(), LogLevel.Trace);

                GetMatches();
                GetParticipants();
            }
        }

        /// <summary>
        /// Updates current match cache
        /// </summary>
        public void GetMatches()
        {
            Logger.Log("Challonge: Loading matches", LogLevel.Trace);
            string tourney = Program.Config.ChallongeTournamentName;

            string matchesStr = $"https://api.challonge.com/v1/tournaments/{tourney}/matches.json?api_key={Program.DecryptString(Program.Config.ChallongeApiKeyEncrypted)}";

            using (WebClient wc = new WebClient())
            {
                matchesStr = wc.DownloadString(matchesStr);
            }

            Match_Json[] matches = Newtonsoft.Json.JsonConvert.DeserializeObject<Match_Json[]>(matchesStr);

            if (Matches == null)
                Matches = new List<(MatchState, Match)>();
            else
                Matches.Clear();

            foreach(Match_Json matchj in matches)
            {
                Match match = matchj.match;
                if (match.round < 0 || !match.player1_id.HasValue || !match.player2_id.HasValue)
                    continue;

                if (match.state == "complete")
                    Matches.Add((MatchState.Closed, match));
                else
                    Matches.Add((MatchState.Open, match));
            }

            Logger.Log("Challonge: Loaded matches", LogLevel.Trace);
        }

        /// <summary>
        /// Updates current participant cache
        /// </summary>
        public void GetParticipants()
        {
            Logger.Log("Challonge: Loading participants", LogLevel.Trace);
            string tourney = Program.Config.ChallongeTournamentName;

            string participantsStr = "";

            using (WebClient wc = new WebClient())
            {
                participantsStr = wc.DownloadString($"https://api.challonge.com/v1/tournaments/{tourney}/participants.json?api_key={Program.DecryptString(Program.Config.ChallongeApiKeyEncrypted)}");
            }

            Participant_Json[] participants = Newtonsoft.Json.JsonConvert.DeserializeObject<Participant_Json[]>(participantsStr);
            if (Participants == null)
                Participants = new List<Participant>();
            else
                Participants.Clear();

            foreach (Participant_Json participantJson in participants)
                Participants.Add(participantJson.participant);
            Logger.Log("Challonge: Loaded participants", LogLevel.Trace);
        }

        /// <summary>
        /// Gets a match from the match cache
        /// </summary>
        public (MatchState, Match) GetMatch(int matchId, bool searchState = false, MatchState search = MatchState.Open)
            => Matches.Find(m => m.Item2.suggested_play_order == matchId);

        /// <summary>
        /// Gets a match from the match cache
        /// </summary>
        public (MatchState, Match, int) GetMatch(string teamA, string teamB, bool searchState = false, MatchState search = MatchState.Open)
        {
            (Participant, Participant) teams = (GetParticipant(teamA), GetParticipant(teamB));
            
            foreach((MatchState, Match) match in Matches)
            {
                if (!match.Item2.player1_id.HasValue || !match.Item2.player2_id.HasValue)
                    continue;

                if (match.Item2.player1_id == teams.Item1.id && match.Item2.player2_id == teams.Item2.id)
                {
                    if (searchState)
                    {
                        if (search == GetState(match.Item2))
                            return (match.Item1, match.Item2, 0);
                    }
                    else
                        return (match.Item1, match.Item2, 0);
                }
                else if (match.Item2.player1_id == teams.Item2.id && match.Item2.player2_id == teams.Item1.id)
                {
                    if (searchState)
                    {
                        if (search == GetState(match.Item2))
                            return (match.Item1, match.Item2, 1);
                    }
                    else
                        return (match.Item1, match.Item2, 1);
                }
            }

            return (MatchState.Open, null, 0);
        }

        /// <summary>
        /// Gets a participant from the participant cache
        /// </summary>
        public Participant GetParticipant(string team)
        {
            foreach (Participant participant in Participants)
            {
                if (participant.display_name != null && participant.display_name.Equals(team, StringComparison.CurrentCultureIgnoreCase))
                    return participant;
            }
            return null;
        }

        /// <summary>
        /// Gets a matchstate from the match cache for a match
        /// </summary>
        private MatchState GetState(Match match)
            => match.state.Equals("complete") ? MatchState.Closed : MatchState.Open;

        public enum MatchState
        {
            Open,
            Closed,
            TempClosed
        }
    }
}
