using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotVerifications
    {
        public int Id { get; set; }
        public long DiscordUserId { get; set; }
        public string Code { get; set; }
    }
}