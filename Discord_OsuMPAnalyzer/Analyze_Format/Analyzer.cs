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
        public static float CalculateAccuracy(int count300, int count100, int count50, int countMiss)
        {
            return (count300 * 300 + count100 * 100 + count50 * 50) / (3 * (count300 + count100 + count50 + countMiss));
        }

        public static float GetAverage(params float[] numbers)
        {
            float result = 0;

            foreach (float f in numbers) result += f;

            return (result / numbers.Count());
        }

        public class MultiplayerMatch
        {
            public Get_Match_Json.JsonFormat MPJson { get; set; }

            private Analyzed.MultiMatch m_result;

            private class AvgScores
            {
                public ulong[] UserIDs { get; set; }
                public Score[] Scores { get; set; }
                public class Score
                {
                    public int beatmapid { get; set; }
                    public int count300 { get; set; }
                    public int count100 { get; set; }
                    public int count50 { get; set; }
                    public int countMiss { get; set; }
                    public float AvgAcc
                    {
                        get
                        {
                            return CalculateAccuracy(count300, count100, count50, countMiss);
                        }
                    }
                }
            }

            private Analyzed.MultiMatch CalculateScores(Analyzed.MultiMatch MPMatch)
            {
                try
                {
                    //Score, Game
                    Dictionary<Get_Match_Json.JsonFormat._games._scores, Get_Match_Json.JsonFormat._games> Scores = new Dictionary<Get_Match_Json.JsonFormat._games._scores, Get_Match_Json.JsonFormat._games>();

                    foreach (Get_Match_Json.JsonFormat._games CurGame in MPMatch.CurrentGames)
                        foreach (Get_Match_Json.JsonFormat._games._scores CurScore in CurGame.scores)
                            Scores.Add(CurScore, CurGame);

                    #region GetInformation
                    //HighestScore
                    int[] IVal = { 0, Scores.Keys.ElementAt(0).score };
                    //Average Acc for all players
                    Dictionary<int, float> AvgUserAcc = new Dictionary<int, float>();

                    for (int i = 0; i < Scores.Count; i++)
                    {
                        //Get highest score
                        if (IVal.ElementAt(1) < Scores.ElementAt(i).Key.score)
                            IVal = new int[] { i, Scores.ElementAt(i).Key.score };

                        //Get average acc
                        float AvgAcc = CalculateAccuracy(Scores.ElementAt(i).Key.count300, Scores.ElementAt(i).Key.count100, Scores.ElementAt(i).Key.count50, Scores.ElementAt(i).Key.countmiss);

                        if (AvgUserAcc.Count == 0 || !AvgUserAcc.ContainsKey(Scores.ElementAt(i).Key.user_id))
                        {
                            AvgUserAcc.Add(Scores.ElementAt(i).Key.user_id, AvgAcc);
                            continue;
                        }

                        AvgUserAcc[Scores.ElementAt(i).Key.user_id] = AvgAcc;
                    }
                    Get_Match_Json.JsonFormat._games._scores HighestScore = Scores.ElementAt(IVal[0]).Key;
                    Get_Match_Json.JsonFormat._games HighestScoreGame = Scores.ElementAt(IVal[0]).Value;

                    //Set Top 1-4 Players for acc ranking
                    int rankingMax = 4;

                    //Incase there aren't 4 players
                    while (rankingMax > AvgUserAcc.Count - 1 && rankingMax != 1)
                        rankingMax--;

                    Dictionary<int, float> BestAccs = new Dictionary<int, float>();
                    List<int> AccDone = new List<int>();
                    //Get highest average accuracies
                    for (int i = 0; i < rankingMax; i++)
                    {
                        float highest = AvgUserAcc.ElementAt(0).Value;

                        for (int x = 0; x < AvgUserAcc.Count; x++)
                        {
                            
                            if (BestAccs.Count == 0)
                            {
                                highest = AvgUserAcc.ElementAt(x).Value;
                                continue;
                            }

                            if (BestAccs.Count > 0)
                                if (AccDone.Contains(BestAccs.ElementAt(x).Key))
                                    continue;
                            
                            if (highest < AvgUserAcc.ElementAt(x).Value)
                                highest = AvgUserAcc.ElementAt(x).Value;

                            if (x == AvgUserAcc.Count - 1)
                            {
                                BestAccs.Add(AvgUserAcc.ElementAt(x).Key, highest);
                                AccDone.Add(BestAccs.ElementAt(x).Key);
                                highest = float.MinValue;
                            }
                        }
                    }
                    #endregion

                    MPMatch.HighestScore = new Analyzed.Score()
                    {
                        Acc = CalculateAccuracy(HighestScore.count300, HighestScore.count100, HighestScore.count50, HighestScore.countmiss),
                        beatmapID = HighestScoreGame.beatmap_id,
                        beatmapName = "",
                        difficulty = "",
                        starRating = 0,
                        userName = "",
                        userScore = HighestScore
                    };
                    MPMatch.HighestScorePoints = MPMatch.HighestScore.userScore.score;
                    MPMatch.BestAccuracies = BestAccs;
                }
                catch (Exception e)
                {
                    Console.WriteLine(DateTime.UtcNow + " : " + e);
                }
                return MPMatch;
            }

            public Analyzed.MultiMatch Result { get { return m_result; } }


            #region 1
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
                    multiMatch = CalculateScores(multiMatch);
                    #endregion

                    //beatmap_id, scores, highest score on each map
                    List<Analyzed.Score> Scores = new List<Analyzed.Score>();
                    
                    for (int i = 0; i < mpJson.games.Count(); i++)
                    {
                        int HighestPoints = int.MinValue;
                        int index = 0;

                        int PointsTeamBlue = 0;
                        int PointsTeamRed = 0;
                        
                        Get_Match_Json.JsonFormat._games CurGame = mpJson.games.ElementAt(i);
                        
                        for (int x = 0; x < CurGame.scores.Count(); x++)
                        {
                            int Score = CurGame.scores.ElementAt(x).score;
                            Get_Match_Json.JsonFormat._games._scores CurScore = CurGame.scores.ElementAt(x);

                            if (CurScore.pass != 0)
                            {
                                switch (CurScore.team)
                                {
                                    default:
                                        break;
                                    case 1:
                                        PointsTeamBlue += CurScore.score;
                                        break;
                                    case 2:
                                        PointsTeamRed += CurScore.score;
                                        break;
                                }
                            }

                            if (HighestPoints < Score)
                            {
                                index = x;
                                HighestPoints = Score;
                            }
                            Scores.Add(new Analyzed.Score() { userScore = CurGame.scores.ElementAt(index), beatmapID = CurGame.beatmap_id });
                        }

                        if (PointsTeamRed > PointsTeamBlue) multiMatch.Team_Red_Wins++;
                        else if (PointsTeamRed < PointsTeamBlue) multiMatch.Team_Blue_Wins++;
                        else multiMatch.Draws++;
                    }
                   
                    #region 3
                    Get_User_Json.JsonFormat UserJson = API.OsuApi.GetUser(multiMatch.HighestScore.user_id);
                    Get_Beatmaps.JsonFormat BeatmapsJson = API.OsuApi.Get_BeatMap(multiMatch.HighestScore.beatmapID);
                    multiMatch.HighestScore.beatmapName = string.Format("{0} - {1}", BeatmapsJson.artist, BeatmapsJson.title);
                    multiMatch.HighestScore.difficulty = BeatmapsJson.version;
                    multiMatch.HighestScore.starRating = BeatmapsJson.difficultyrating;
                    multiMatch.HighestScore.userName = UserJson.username;
                    multiMatch.HighestScore.Acc = CalculateAccuracy(multiMatch.HighestScore.userScore.count300, multiMatch.HighestScore.userScore.count100, multiMatch.HighestScore.userScore.count50, multiMatch.HighestScore.userScore.countmiss);

                    List<string> AnalyzeOutput = new List<string>();

                    for (int i = 0; i < multiMatch.BestAccuracies.Count; i++)
                    {
                        float Acc = multiMatch.BestAccuracies.ElementAt(i).Value;
                        string place = "";

                        Get_User_Json.JsonFormat userJson = API.OsuApi.GetUser(multiMatch.BestAccuracies.ElementAt(i).Key);
                        string name = userJson.username;

                        switch (i)
                        {
                            case 1:
                                place = "first";
                                break;
                            case 2:
                                place = "second";
                                break;
                            case 3:
                                place = "third";
                                break;
                            default:
                            case 4:
                                place = "fourth";
                                break;
                        }

                        AnalyzeOutput.Add(string.Format("| :{0}_place: Player: {1} Acc: {2}%", place, name, Acc));
                    }

                    AnalyzeOutput.Add(string.Format("| The highest Score got {0} on the map {1} [{2}] ({3}*) with {4} Points and {5}% Accuracy!", multiMatch.HighestScore.userName, multiMatch.HighestScore.beatmapName, multiMatch.HighestScore.difficulty, multiMatch.HighestScore.starRating, multiMatch.HighestScore.userScore.score, multiMatch.HighestScore.Acc));
                    AnalyzeOutput.Add(string.Format("| Team Red Wins: {0} {1}| Team Blue Wins: {2}", multiMatch.Team_Red_Wins, Environment.NewLine, multiMatch.Team_Blue_Wins));
                    multiMatch.AnalyzedData = AnalyzeOutput;

                    sw.Stop();
                    Console.WriteLine("took {0} to analyze! (Download times included)", sw.ElapsedMilliseconds);
                    #endregion
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
