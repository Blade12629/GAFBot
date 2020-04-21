using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class Beatmap
    {
        public long Id { get; set; }
        public string Author { get; set; }
        public long BeatmapId { get; set; }
        public string Difficulty { get; set; }
        public string Image { get; set; }
        public long MapsetId { get; set; }
        public string Title { get; set; }
        public long ModId { get; set; }
        public long? MappoolId { get; set; }
        public bool IsInMappool { get; set; }
    }
}
