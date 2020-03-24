using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotPick
    {
        public int Id { get; set; }
        public string PickedBy { get; set; }
        public string Team { get; set; }
        public string Match { get; set; }
        public string Image { get; set; }
    }
}
