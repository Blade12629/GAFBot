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

                    List<Analyzed.BestAccuracy> BestAccuracies = new List<Analyzed.BestAccuracy>();

                    foreach (KeyValuePair<int, float> kvpUser in AvgUserAcc)
                        BestAccuracies.Add(new Analyzed.BestAccuracy() { Accuracy = kvpUser.Value, userId = kvpUser.Key });

                    MPMatch.BestAccuracies = BestAccuracies.ToArray();
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
                        int indexx = 0;

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
                                indexx = x;
                                HighestPoints = Score;
                            }
                            Scores.Add(new Analyzed.Score() { userScore = CurGame.scores.ElementAt(indexx), beatmapID = CurGame.beatmap_id });
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

                    AnalyzeOutput.Add(string.Format("| Game: {0} ({1})", multiMatch.CurrentMatch.name, multiMatch.CurrentMatch.match_id));

                    //value, index
                    float[] index = { float.MinValue, 0 };
                    string n = "";
                    int place = 1;
                    
                    List<int> userDone = new List<int>();

                    for (int i = 0; i < multiMatch.BestAccuracies.Count(); i++)
                    {
                        float Acc = multiMatch.BestAccuracies.ElementAt(i).Accuracy;
                        int userid = multiMatch.BestAccuracies.ElementAt(i).userId;

                        if (userDone.Contains(userid))
                            continue;

                        Get_User_Json.JsonFormat user = API.OsuApi.GetUser(multiMatch.BestAccuracies.ElementAt(i).userId);
                        string name = user.username;


                        if (index[0] < Acc)
                        {
                            n = name;
                            index = new float[] { Acc, i };
                        }

                        if (i == multiMatch.BestAccuracies.Count() - 1)
                        {
                            i = 0;
                            int iindex = Convert.ToInt32(index[1]);
                            userDone.Add(multiMatch.BestAccuracies.ElementAt(iindex).userId);
                            float AAcc = multiMatch.BestAccuracies.ElementAt(iindex).Accuracy;
                            string nname = n;

                            index = new float[] { float.MinValue, 0 };

                            switch (place)
                            {
                                case 1:
                                    AnalyzeOutput.Add(string.Format("| First place: {0} Acc: {1}%", nname, AAcc));
                                    place++;
                                    break;
                                case 2:
                                    AnalyzeOutput.Add(string.Format("| Second place: {0} Acc: {1}%", nname, AAcc));
                                    place++;
                                    break;
                                case 3:
                                    AnalyzeOutput.Add(string.Format("| Third place: {0} Acc: {1}%", nname, AAcc));
                                    place++;
                                    break;
                                default:
                                    AnalyzeOutput.Add(string.Format("| {0}th place: {1} Acc: {2}%", place, nname, AAcc));
                                    place++;
                                    break;

                            }
                        }
                    }

                    AnalyzeOutput.Add(string.Format("| The highest Score got {0} on the map {1} [{2}] ({3}*) with {4} Points and {5}% Accuracy!", multiMatch.HighestScore.userName, multiMatch.HighestScore.beatmapName, multiMatch.HighestScore.difficulty, multiMatch.HighestScore.starRating, multiMatch.HighestScore.userScore.score, multiMatch.HighestScore.Acc));

                    string TeamWin = "| Team ";

                    if (multiMatch.Team_Blue_Wins > multiMatch.Team_Red_Wins)
                        TeamWin += "Blue wins";
                    else if (multiMatch.Team_Red_Wins > multiMatch.Team_Blue_Wins)
                        TeamWin += "Red wins";
                    else
                        TeamWin = "draw";

                    TeamWin += string.Format(" (Red: {0} | Blue: {1})", multiMatch.Team_Red_Wins, multiMatch.Team_Blue_Wins);

                    AnalyzeOutput.Add(TeamWin);
                    multiMatch.AnalyzedData = AnalyzeOutput;

                    sw.Stop();
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
