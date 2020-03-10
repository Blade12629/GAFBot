using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotAnalyzerScore
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public long MatchId { get; set; }
        public float Accuracy { get; set; }
        public string Mods { get; set; }
        public long Score { get; set; }
        public int MaxCombo { get; set; }
        public int Perfect { get; set; }
        public float PP { get; set; }
        public long Rank { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Slot { get; set; }
        public string Team { get; set; }
        public int Pass { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountGeki { get; set; }
        public int CountKatu { get; set; }
        public int CountMiss { get; set; }
    }
}
