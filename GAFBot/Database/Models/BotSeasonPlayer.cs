using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotSeasonPlayer
    {
        public long Id { get; set; }

        public long OsuUserId { get; set; }
        public string LastOsuUserName { get; set; }
        public long TeamId { get; set; }
    }
}
