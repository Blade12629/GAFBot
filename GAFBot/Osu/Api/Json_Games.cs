using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.Api
{
    public class Json_Games
    {
#pragma warning disable IDE1006 // Naming Styles
        public int game_id { get; set; }
        public DateTime start_time { get; set; }
        public Object end_time { get; set; }
        public int beatmap_id { get; set; }
        // standard = 0, taiko = 1, ctb = 2, o!m = 3
        public int play_mode { get; set; }
        // couldn't find
        public int match_type { get; set; }
        public int scoring_type { get; set; }
        public int team_type { get; set; }
        public int mods { get; set; }

        public _scores[] scores { get; set; }
        public class _scores
        {
            public int slot { get; set; }
            public int team { get; set; }
            public int user_id { get; set; }
            public int score { get; set; }
            public int maxcombo { get; set; }
            public int rank { get; set; }
            public int count50 { get; set; }
            public int count100 { get; set; }
            public int count300 { get; set; }
            public int countmiss { get; set; }
            public int countgeki { get; set; }
            public int countkatu { get; set; }
            public int perfect { get; set; }
            public int pass { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
    }
}
