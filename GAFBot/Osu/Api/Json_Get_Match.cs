using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.Api
{
    public class Json_Get_Match
    {
#pragma warning disable IDE1006 // Naming Styles
        public Json_Match match { get; set; }
        public Json_Games[] games { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
