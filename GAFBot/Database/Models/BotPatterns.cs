using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotPatterns
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Text { get; set; }
    }
}
