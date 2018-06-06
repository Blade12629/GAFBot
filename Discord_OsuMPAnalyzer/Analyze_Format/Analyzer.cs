using Discord_OsuMPAnalyzer.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer.Analyze_Format
{
    public static class Analyzer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LowestOrHighest">Highest = true, Lowest = false</param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int GetLowestOrHighest(out int index, bool LowestOrHighest = true, params int[] values)
        {
            int highest = int.MinValue;
            int indexHighest = 0;

            int lowest = int.MaxValue;
            int indexLowest = 0;

            for (int i = 0; i < values.Count(); i++)
            {
                for (int y = values.Count(); y > i; y++)
                {
                    if (values[y - 1] > highest)
                    {
                        highest = values[y - 1];
                        indexHighest = y - 1;
                    }
                    if (values[y - 1] < lowest)
                    {
                        lowest = values[y - 1];
                        indexLowest = y - 1;
                    }
                }
            }

            if (LowestOrHighest)
            {
                index = indexHighest;
                return highest;
            }
            else
            {
                index = indexLowest;
                return lowest;
            }

        }

        public class MultiplayerMatch
        {
            public Get_Match_Json.JsonFormat MPJson { get; set; }

            private Analyzed.MultiMatch m_result;
            public Analyzed.MultiMatch Result { get { return m_result; } }

            public virtual Analyzed.MultiMatch Analyze(Get_Match_Json.JsonFormat mpJson)
            {
                try
                {
                    if (mpJson == null) return null;

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    MPJson = mpJson;

                    Analyzed.MultiMatch multiMatch = new Analyzed.MultiMatch();

                    multiMatch.CurrentGames = MPJson.games;
                    multiMatch.CurrentMatch = MPJson.match;

                    //beatmap_id, scores, highest score on each map
                    List<Analyzed.Score> Scores = new List<Analyzed.Score>();

                    //Set HighestScores
                    foreach (Get_Match_Json.JsonFormat._games game in mpJson.games)
                    {
                        int HighestPoints = 0;
                        int index = 0;

                        for (int i = 0; i < game.scores.Count(); i++)
                        {
                            if (HighestPoints == 0) HighestPoints = game.scores[i].score;
                            for (int y = game.scores.Count(); y > i; y--)
                            {
                                if (HighestPoints > game.scores[y - 1].score) continue;

                                HighestPoints = game.scores[y - 1].score;
                                index = y - 1;
                            }
                        }
                        Analyzed.Score score = new Analyzed.Score
                        {
                            userScore = game.scores[index],
                            beatmapID = game.beatmap_id
                        };
                        Scores.Add(score);
                    }
                    multiMatch.HighestScores = Scores.ToArray();

                    //Set HighestScore
                    int CurHighestScore = int.MinValue;
                    int _index = 0;


                    for (int i = 0; i < Scores.Count; i++)
                    {
                        for (int y = Scores.Count; y > i; y--)
                        {
                            if (CurHighestScore < Scores[y - 1].userScore.score)
                            {
                                _index = y - 1;
                                CurHighestScore = Scores[y - 1].userScore.score;
                            }
                        }
                    }
                    multiMatch.HighestScore = Scores[_index];
                    multiMatch.HighestScorePoints = CurHighestScore;


                    int TeamRedWins = 0;
                    int TeamBlueWins = 0;
                    int draws = 0;

                    foreach (Get_Match_Json.JsonFormat._games game in MPJson.games)
                    {
                        int PointsRed = 0;
                        int PointsBlue = 0;

                        foreach (Get_Match_Json.JsonFormat._games._scores score in game.scores)
                        {
                            if (score.pass == 0) continue;
                            switch (score.team)
                            {
                                default:
                                    break;
                                case 1:
                                    PointsBlue += score.score;
                                    break;
                                case 2:
                                    PointsRed += score.score;
                                    break;
                            }
                        }
                        if (PointsRed > PointsBlue) TeamRedWins++;
                        else if (PointsRed < PointsBlue) TeamBlueWins++;
                        else draws++;
                    }
                    multiMatch.Team_Blue_Wins = TeamBlueWins;
                    multiMatch.Team_Red_Wins = TeamRedWins;
                    multiMatch.Draws = draws;

                    Get_User_Json.JsonFormat UserJson = API.OsuApi.GetUser(multiMatch.HighestScore.user_id);
                    Get_Beatmaps.JsonFormat BeatmapsJson = API.OsuApi.Get_BeatMap(multiMatch.HighestScore.beatmapID);
                    multiMatch.HighestScore.beatmapName = string.Format("{0} - {1}", BeatmapsJson.artist, BeatmapsJson.title);
                    multiMatch.HighestScore.difficulty = BeatmapsJson.version;
                    multiMatch.HighestScore.starRating = BeatmapsJson.difficultyrating;
                    multiMatch.HighestScore.userName = UserJson.username;

                    sw.Stop();
                    Console.WriteLine("took {0} to analyze! (Download times included)", sw.ElapsedMilliseconds);

                    List<string> AnalyzeOutput = new List<string>();
                    AnalyzeOutput.Add(string.Format("| The highest Score got {0}  on the map {1} [{2}] ({3}) with {4} Points! |", multiMatch.HighestScore.userName, multiMatch.HighestScore.beatmapName, multiMatch.HighestScore.difficulty, multiMatch.HighestScore.starRating, multiMatch.HighestScore.userScore.score));
                    AnalyzeOutput.Add(string.Format("Team Red Wins: {0} {1} Team Blue Wins: {2}", multiMatch.Team_Red_Wins, Environment.NewLine, multiMatch.Team_Blue_Wins));
                    multiMatch.AnalyzedData = AnalyzeOutput;

                    return multiMatch;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return new Analyzed.MultiMatch();
                }
            }
        }
    }
}
