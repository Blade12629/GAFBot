using System;

namespace Discord_OsuMPAnalyzer.Json
{
#pragma warning disable IDE1006 // Naming Styles (lowerCaseWarning)
    public static class Get_User_Json
    {
        /// <summary>
        /// Parameters: k* api key, u* userid/username, m mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania) (default: 0), type* UseName/UseID (string, id) (preferred: UseID) , event_days (1-31)
        /// </summary>
        public class JsonFormat
        {
			public int user_id { get; set; }
			public string username { get; set; }
			public int count300 { get; set; }
			public int count100 { get; set; }
			public int count50 { get; set; }
			public int playcount { get; set; }
			public int ranked_score { get; set; }
			public int total_score { get; set; }
			public int pp_rank { get; set; }
			public float level { get; set; }
			public int pp_raw { get; set; }
			public float accuracy { get; set; }
			public int count_rank_ss { get; set; }
			public int count_rank_ssh { get; set; }
            public int count_rank_s { get; set; }
			public int count_rank_sh { get; set; }
			public int count_rank_a { get; set; }
			public string country { get; set; }
			public string pp_country_rank { get; set; }

			public _events[] events { get; set; } 
			public class _events
            {
				public string display_html { get; set; }
				public int beatmap_id { get; set; }
				public int beatmapset_id { get; set; }
				public DateTime date { get; set; }
				public int epicfactor { get; set; }
            }
        }
    }
}
