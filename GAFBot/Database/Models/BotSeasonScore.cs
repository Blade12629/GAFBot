using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotSeasonScore
    {
        public long Id { get; set; }

        public long BeatmapId { get; set; }
        public long BotSeasonPlayerId { get; set; }
        public long BotSeasonResultId { get; set; }
        public string TeamName { get; set; }

        public bool TeamVs { get; set; }
        /// <summary>
        /// 1. map = 1, 2. map = 2, etc.
        /// </summary>
        public int PlayOrder { get; set; }
        public double GPS { get; set; }
        public bool HighestGPS { get; set; }

        public float Accuracy { get; set; }

        public long Score { get; set; }
        public int MaxCombo { get; set; }
        public int Perfect { get; set; }
        public DateTime PlayedAt { get; set; }
        public int Pass { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountGeki { get; set; }
        public int CountKatu { get; set; }
        public int CountMiss { get; set; }
    }
}
