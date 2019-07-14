using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.results
{
    public class AnalyzerResult
    {
        public string MatchName { get; set; }
        /// <summary>
        /// TeamA, TeamB, Winning
        /// </summary>
        public (string, string) TeamNames { get; set; }
        public string WinningTeam { get; set; }
        public int WinningTeamWins { get; set; }
        public string LosingTeam { get; set; }
        public int LosingTeamWins { get; set; }
        public WinType Win { get; set; }
        public TeamColor WinningTeamColor { get; set; }
        public bool IsQualifier { get; set; }
        public Rank[] Ranks { get; set; }
        public HistoryJson.BeatMap[] Beatmaps { get; set; }
        public DateTime TimeStamp { get; set; }

        public BeatmapPlayCount MostPlayedBeatmap { get; set; }
        public HistoryJson.BeatMap HighestScoreBeatmap { get; set; }

        public HistoryJson.Score HighestScore { get; set; }
        public Player HighestScoreUser { get; set; }

        public HistoryJson.BeatMap HighestAccuracyBeatmap { get; set; }
        public Player HighestAverageAccuracyUser { get; set; }
        public HistoryJson.Score HighestAccuracyScore { get; set; }

        public Rank[] HighestAverageAccuracyRanking { get; set; }
        public Rank[] HighestScoresRanking { get; set; }
    }
}
