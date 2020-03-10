using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotAnalyzerResult
    {
        public int Id { get; set; }
        public long MatchId { get; set; }
        public string Stage { get; set; }
        public string MatchName { get; set; }
        public string WinningTeam { get; set; }
        public int WinningTeamWins { get; set; }
        public int WinningTeamColor { get; set; }
        public string LosingTeam { get; set; }
        public int LosingTeamWins { get; set; }
        public DateTime TimeStamp { get; set; }
        public long HighestScoreBeatmapId { get; set; }
        public long HighestScoreOsuId { get; set; }
        public long HighestScoreId { get; set; }
        public long HighestAccuracyBeatmapId { get; set; }
        public long HighestAccuracyOsuId { get; set; }
        public long HighestAccuracyScoreId { get; set; }
        public long MvpUserOsuId { get; set; }
    }
}
