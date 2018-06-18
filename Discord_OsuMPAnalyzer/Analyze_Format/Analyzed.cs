using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer.Analyze_Format
{
    public static class Analyzed
    {
        public class MultiMatch
        {
            public Json.Get_Match_Json.JsonFormat._match CurrentMatch { get; set; }
            public Json.Get_Match_Json.JsonFormat._games[] CurrentGames { get; set; }
            public Score HighestScore { get; set; }
            public int HighestScorePoints { get; set; }
            public Score[] HighestScores { get; set; }
            public Dictionary<int, float> BestAccuracies { get; set; }
            public List<string> AnalyzedData { get; set; }

            public int Team_Red_Wins { get; set; }
            public int Team_Blue_Wins { get; set; }
            public int Draws { get; set; }

            public User[] Users { get; set; }
        }

        public class Score
        {
            public int beatmapID { get; set; }
            public string beatmapName { get; set; }
            public string difficulty { get; set; }
            public float starRating { get; set; }
            public string userName { get; set; }
            public int user_id { get { return userScore.user_id; } }
            public float Acc { get; set; }
            public Json.Get_Match_Json.JsonFormat._games._scores userScore { get; set; }
        }

        public class User
        {
            public int userID { get; set; }
            public string userName { get; set; }
            public int AverageAccuracy { get; set; }
            public Json.Get_Match_Json.JsonFormat._games._scores HighestPointsScore { get; set; }
            public int HighestPoints { get { return (HighestPointsScore != null) ? HighestPointsScore.score : 0; } }
            /// <summary>
            /// found under "games"
            /// </summary>
            public string HighestPointsOnMap { get; set; }
        }
    }
}
