using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.results
{
    public class AnalyzerQualifierResult
    {
        public int MatchId { get; set; }
        public string Stage { get; set; }
        public string MatchName { get; set; }
        public DateTime TimeStamp { get; set; }
        public QualifierTeam[] Teams { get; set; }

        public AnalyzerQualifierResult(int matchId, string stage, string matchName, QualifierTeam[] teams)
        {
            MatchId = matchId;
            Stage = stage;
            MatchName = matchName;
            Teams = teams;
        }
    }

    public class QualifierTeam
    {
        public string TeamName { get; set; }
        public QualifierPlayer[] Players { get; set; }

        public QualifierTeam(string teamName, QualifierPlayer[] players)
        {
            TeamName = teamName;
            Players = players;
        }
    }

    public class QualifierPlayer
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// Beatmapid, score
        /// </summary>
        public (long, HistoryJson.Score)[] Scores { get; set; }

        public QualifierPlayer(long userId, string userName, (long, HistoryJson.Score)[] scores)
        {
            UserId = userId;
            UserName = userName;
            Scores = scores;
        }
    }
}
