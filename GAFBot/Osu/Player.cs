using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAFBot.Osu
{
    public class Player
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public HistoryJson.Score[] Scores { get; set; }

        public HistoryJson.Score HighestScore { get; set; }

        public float AverageAccuracy { get; set; }
        public float AverageAccuracyRounded
        {
            get { return (float)Math.Round(AverageAccuracy, 2, MidpointRounding.AwayFromZero); }
        }

        /// <summary>
        /// Empty player
        /// </summary>
        public Player()
        {

        }

        /// <summary>
        /// Creates the player and invokes <see cref="CalculateAverageAccuracy"/> and <see cref="GetHighestScore"/>
        /// </summary>
        /// <param name="scores"></param>
        public Player(params HistoryJson.Score[] scores)
        {
            Scores = scores;
            CalculateAverageAccuracy();
            GetHighestScore();
        }

        public void GetHighestScore()
        {
            foreach (HistoryJson.Score score in Scores)
                if (HighestScore == null || score.score.Value > HighestScore.score.Value)
                    HighestScore = score;
        }

        public void CalculateAverageAccuracy()
        {
            float AvgAcc = 0;

            Scores.ToList().ForEach(score => AvgAcc += score.accuracy.Value);
            AvgAcc /= Scores.Count();
            
            AvgAcc *= 100.0f;

            AverageAccuracy = AvgAcc;
        }
    }
}
