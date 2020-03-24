using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotApiRegisterCode
    {
        public int Id { get; set; }
        public long DiscordId { get; set; }
        public string Code { get; set; }
    }
}
