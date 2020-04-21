using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotAnalyzerQualifierResult
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public string Stage { get; set; }
        public string MatchName { get; set; }
    }

    public class BotAnalyzerQualifierTeam
    {
        public int Id { get; set; }
        public int QualifierResultId { get; set; }
        public string TeamName { get; set; }
    }

    public class BotAnalyzerQualifierPlayer
    {
        public int Id { get; set; }
        public int QualifierTeamId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
    }
}
