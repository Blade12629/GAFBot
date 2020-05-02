#if GAF
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public int? AveragePP { get; set; }
        public long? MedianPP { get; set; }
    }
}
#endif