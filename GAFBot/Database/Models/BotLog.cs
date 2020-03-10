using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
