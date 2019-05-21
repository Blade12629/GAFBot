using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.Api
{
    public class Json_Events
    {
#pragma warning disable IDE1006 // Naming Styles
        public string display_html { get; set; }
        public int beatmap_id { get; set; }
        public int beatmapset_id { get; set; }
        public DateTime date { get; set; }
        public int epicfactor { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
