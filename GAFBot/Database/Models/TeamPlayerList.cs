#if GAF
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class TeamPlayerList
    {
        public long? TeamId { get; set; }
        public long? PlayerListId { get; set; }
    }
}
#endif