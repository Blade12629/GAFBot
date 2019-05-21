using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.results
{
    public class User
    {
        public int userID { get; set; }
        public string userName { get; set; }
        public int AverageAccuracy { get; set; }
        public Score HighestPointsScore { get; set; }
        public int HighestPoints { get { return HighestPointsScore.UserScore.score ?? 0; } }
        /// <summary>
        /// found under "games"
        /// </summary>
        public string HighestPointsOnMap { get; set; }
    }
}
