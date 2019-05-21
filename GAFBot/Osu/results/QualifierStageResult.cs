using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.results
{
    public class QualifierStageResult
    {
        public string MatchName { get; set; }
        public (string, string) TeamNames { get; set; }
        public HistoryJson.Score HighestScore { get; set; }
        public HistoryJson.BeatMap HighestScoreBeatmap { get; set; }
        public string HighestScoreUsername { get; set; }
        public Rank[] HighestScoresRanking { get; set; }
    }
}
