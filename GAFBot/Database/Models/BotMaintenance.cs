using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotMaintenance
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string Notification { get; set; }
    }
}
