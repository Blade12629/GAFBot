using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotImages
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }
    }
}
