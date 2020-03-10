using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotAnalyzerRank
    {
        public int Id { get; set; }
        public long MatchId { get; set; }
        public long PlayerOsuId { get; set; }
        public int Place { get; set; }
        public int PlaceAccuracy { get; set; }
        public int PlaceScore { get; set; }
        public float MvpScore { get; set; }
    }
}
