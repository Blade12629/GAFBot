using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotAnalyzerBaninfo
    {
        public int Id { get; set; }
        public long? MatchId { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string BannedBy { get; set; }
    }
}
