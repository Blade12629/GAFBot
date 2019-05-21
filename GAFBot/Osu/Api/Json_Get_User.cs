using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.Api
{/// <summary>
 /// Parameters: k* api key, u* userid/username, m mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania) (default: 0), type* UseName/UseID (string, id) (preferred: UseID) , event_days (1-31)
 /// </summary>
    public class Json_Get_User
    {
#pragma warning disable IDE1006 // Naming Styles
        public int user_id { get; set; }
        public string username { get; set; }
        public int count300 { get; set; }
        public int count100 { get; set; }
        public int count50 { get; set; }
        public int playcount { get; set; }
        public string ranked_score { get; set; }
        public string total_score { get; set; }
        public int pp_rank { get; set; }
        public float level { get; set; }
        public float pp_raw { get; set; }
        public float accuracy { get; set; }
        public int count_rank_ss { get; set; }
        public int count_rank_ssh { get; set; }
        public int count_rank_s { get; set; }
        public int count_rank_sh { get; set; }
        public int count_rank_a { get; set; }
        public string country { get; set; }
        public int pp_country_rank { get; set; }


        public Json_Events[] events { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
