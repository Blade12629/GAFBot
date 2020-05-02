#if GAF
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class Player
    {
        public long Id { get; set; }
        public string Image { get; set; }
        public string Nickname { get; set; }
        public long? OsuId { get; set; }
        public long? TeamId { get; set; }
        public long? Rank { get; set; }
        public long? PPRaw { get; set; }
        public long? PP { get; set; }
        public bool? IsInTeam { get; set; }
        public string Country { get; set; }
    }
}
#endif
