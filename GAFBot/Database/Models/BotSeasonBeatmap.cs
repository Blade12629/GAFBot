using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotSeasonBeatmap
    {
        public long Id { get; set; }

        public long BeatmapId { get; set; }
        public string Author { get; set; }
        public string Difficulty { get; set; }
        public double DifficultyRating { get; set; }
        public string Title { get; set; }
    }
}
