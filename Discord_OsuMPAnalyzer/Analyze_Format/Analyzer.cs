using Discord_OsuMPAnalyzer.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsuHistoryEndPoint;

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

        public class NewAnalyzer
        {
            public NewAnalyzer()
            {
            }

            public string[] Statistic { get; set; }

            public void CreateStatistic(HistoryJson.History history)
            {
                try
                {
                    List<string> Statistics = new List<string>();
                    HistoryJson.Game[] Games = GetData.GetMatches(history);

                    foreach(HistoryJson.Event ev in history.Events)
                    {
                        if (ev.Detail.Type == "other")
                        {
                            Statistics.Add("| " + ev.Detail.MatchName);
                            break;
                        }
                    }
                    //string matchName = GetData.GetMatchNames(history)[0];

                    List<Player> Players = new List<Player>();
                    HistoryJson.BeatMap highestScoreBeatMap = null;
                    KeyValuePair<int, HistoryJson.Score> HighestScorePlayer = new KeyValuePair<int, HistoryJson.Score>(-1, null);

                    #region GetPlayerScoresAndAvgAcc_And_GetHighestScorePlayer
                    List<HistoryJson.Score> _scores = new List<HistoryJson.Score>();
                    List<Player> _players = new List<Player>();

                    //foreach (HistoryJson.User user in history.Users)
                    //{
                    //    Player pl = new Player();
                    //    pl.UserId 
                    //}

                    //Get all scores
                    for (int i = 2; i < Games.Count(); i++)
                    {
                        HistoryJson.Game game = Games[i];

                        if (game.scores != null)
                        {
                            foreach (HistoryJson.Score score in game.scores)
                            {
                                Player curPlayer = Players.Find(ob => ob.UserId == score.user_id);

                                if (HighestScorePlayer.Key == -1)
                                {
                                    HighestScorePlayer = new KeyValuePair<int, HistoryJson.Score>(score.user_id.Value, score);
                                    highestScoreBeatMap = game.beatmap;
                                }

                                if (score.score.Value > HighestScorePlayer.Value.score.Value)
                                {
                                    HighestScorePlayer = new KeyValuePair<int, HistoryJson.Score>(score.user_id.Value, score);
                                    highestScoreBeatMap = game.beatmap;
                                }

                                if (curPlayer == null)
                                {
                                    curPlayer = new Player() { Scores = new HistoryJson.Score[] { } };
                                    curPlayer.UserName = GetData.GetUser(score.user_id.Value, history).Username;
                                    curPlayer.UserId = score.user_id.Value;
                                    curPlayer.Scores.Append(score);
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    foreach (Player pl in Players)
                    {
                        float acc = 0;

                        pl.Scores.ToList().ForEach(ob => acc += ob.accuracy.Value);
                        pl.AvgAcc = acc / pl.Scores.Count() * 100;
                        Console.WriteLine("PL {0} acc {1}", pl.UserId, pl.AvgAcc);
                    }
                    #endregion

                    #region SetPlayersInAvgAccOrder
                    List<Player> newPlayers = new List<Player>();

                    float[] indexVal = { -1, -1 };

                    for (int i = 0; i < Players.Count; i++)
                    {
                        if (Players.Count() == 0)
                            break;

                        if (indexVal[1] < Players[i].AvgAcc)
                            indexVal = new float[] { i, Players[i].AvgAcc };

                        if (i == Players.Count() - 1)
                        {
                            Player _player = Players[Convert.ToInt32(indexVal[0])];
                            newPlayers.Add(_player);
                            Players.Remove(_player);
                            indexVal = new float[] { -1, 0 };
                            i = 0;

                            switch (newPlayers.Count())
                            {
                                case 1:
                                    Statistics.Add(string.Format("First Place: {0} Acc: {1}", _player.UserName, _player.AvgAcc));
                                    break;
                                case 2:
                                    Statistics.Add(string.Format("Second Place: {0} Acc: {1}", _player.UserName, _player.AvgAcc));
                                    break;
                                case 3:
                                    Statistics.Add(string.Format("Third Place: {0} Acc: {1}", _player.UserName, _player.AvgAcc));
                                    break;
                                case 4:
                                    Statistics.Add(string.Format("Fourth Place: {0} Acc: {1}", _player.UserName, _player.AvgAcc));
                                    break;
                                default:
                                    Statistics.Add(string.Format("{0}. Place: {1} Acc: {2}", newPlayers.Count(), _player.UserName, _player.AvgAcc));
                                    break;
                            }
                        }
                    }
                    Players = newPlayers;
                    #endregion

                    string username = GetData.GetUser(HighestScorePlayer.Key, history).Username;
                    int x = 0;

                    Statistics.Add(string.Format("| The highest Score got {0} on the map {1} - {2} [{3}] with {4} Points and {5}% Accuracy!", username, highestScoreBeatMap.beatmapset.artist, highestScoreBeatMap.beatmapset.title, highestScoreBeatMap.version, HighestScorePlayer.Value.score.Value, HighestScorePlayer.Value.accuracy));

                    KeyValuePair<int, int> wins = new KeyValuePair<int, int>(0, 0);

                    foreach (HistoryJson.Game game in Games)
                    {
                        if (game.team_type != "team-vs" || game.beatmap == null)
                            continue;
                        
                        int BlueSC = 0;
                        int RedSC = 0;

                        foreach (HistoryJson.Score score in game.scores)
                        {
                            HistoryJson.Multiplayer multiplayer = score.multiplayer;

                            if (multiplayer.pass == 0)
                                continue;

                            switch (multiplayer.team)
                            {
                                case "red":
                                    RedSC += score.score.Value;
                                    break;
                                case "blue":
                                    BlueSC += score.score.Value;
                                    break;
                            }
                        }
                        if (BlueSC > RedSC)
                            wins = new KeyValuePair<int, int>(wins.Key, wins.Value + 1);
                        if (RedSC > BlueSC)
                            wins = new KeyValuePair<int, int>(wins.Key + 1, wins.Value);
                    }

                    string winner = null;
                    int point1 = 0;
                    int point2 = 0;
                    if (wins.Key > wins.Value)
                    {
                        winner = "Team red wins!";
                        point1 = wins.Key;
                        point2 = wins.Value;
                    }
                    else if (wins.Key < wins.Value)
                    {
                        winner = "Team blue wins!";
                        point1 = wins.Value;
                        point2 = wins.Key;
                    }
                    else if (wins.Key == wins.Value)
                    {
                        winner = "it's a draw";
                        point1 = wins.Key;
                        point2 = wins.Value;
                    }

                    Statistics.Add(string.Format("| {0} ({1} : {2})", winner, point1, point2));
                    Statistic = Statistics.ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.UtcNow + ": " + ex);
                }
            }

            public void CreateStatistic(string HistoryEndPoint)
            {
                HistoryJson.History history = CreateHistoryJSon(HistoryEndPoint);
                CreateStatistic(history);
            }

            public OsuHistoryEndPoint.HistoryJson.History CreateHistoryJSon(string HistoryEndPoint)
            {
                HistoryJson.History history = new HistoryJson.History();
                history = OsuHistoryEndPoint.GetData.ParseJsonFromUrl(HistoryEndPoint);
                
                return history;
            }

            public class Match
            {
                public HistoryJson.History MatchHistory { get; set; }
                public Player[] Players { get; set; }
                public Team.TeamColor Winner { get; set; }
                public Team[] Teams { get; set; }
            }

            public class Team
            {
                public enum TeamColor
                {
                    Red,
                    Blue
                }

                public TeamColor teamColor { get; set; }
                public int[] userIds { get; set; }
            }

            public class Player
            {
                public int UserId { get; set; }
                public string UserName { get; set; }
                public HistoryJson.Score[] Scores { get; set; }
                //{
                //    get { return Scores; }
                //    set
                //    {
                //        Scores = value;
                //        float[] Acc = new float[] { };

                //        Scores.ToList().ForEach(ob => Acc.Append(ob.accuracy.Value));
                //        float newAcc = CalculateAverageAccuracy(Acc);

                //        AvgAcc = newAcc;
                //    }
                //}
                public float AvgAcc { get; set; }

                public float CalculateAverageAccuracy(params float[] Accs)
                {
                    float AvAcc = 0;

                    Accs.ToList().ForEach(ob => AvAcc += ob);
                    AvAcc /= Accs.Count();

                    return AvAcc;
                }
            }
        }
    }
}
