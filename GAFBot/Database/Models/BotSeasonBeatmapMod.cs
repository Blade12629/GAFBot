using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotSeasonBeatmapMod
    {
        public long Id { get; set; }

        public long BotSeasonScoreId { get; set; }
        public string Mod { get; set; }
    }
}
