using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotSeasonPlayerCardCache
    {
        public long Id { get; set; }
        public long OsuUserId { get; set; }
        public string Username { get; set; }

        public string TeamName { get; set; }

        public double AverageAccuracy { get; set; }
        public double AverageScore { get; set; }
        public double AverageMisses { get; set; }
        public double AverageCombo { get; set; }

        public double AveragePerformance { get; set; }
        public double OverallRating { get; set; }

        public int MatchMvps { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
