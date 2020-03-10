using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotBets
    {
        public int Id { get; set; }
        public string Team { get; set; }
        public long Matchid { get; set; }
        public long DiscordUserId { get; set; }
    }
}
