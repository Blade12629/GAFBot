using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotUsers
    {
        public long Id { get; set; }
        public short? AccessLevel { get; set; }
        public long? DiscordId { get; set; }
        public string OsuUsername { get; set; }
        public long? Points { get; set; }
        public DateTime? RegisteredOn { get; set; }
        public bool IsVerified { get; set; }
    }
}
