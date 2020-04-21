using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotSeasonMMR
    {
        public long Id { get; set; }

        public double MMR { get; set; }
        public long BotSeasonPlayerId { get; set; }
        public long BotSeasonResultId { get; set; }
    }
}
