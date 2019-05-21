using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.Api
{
    public class Json_Match
    {
#pragma warning disable IDE1006 // Naming Styles
        public int match_id { get; set; }
        public string name { get; set; }
        public DateTime start_time { get; set; }
        /// <summary>
        /// not supported yet - always null
        /// </summary>
        public Object end_time { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
