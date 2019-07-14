using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;
using GAFBot.Osu.results;
using System.Linq;

namespace GAFBot.Osu
{
    public class Analyzer
    {
        /// <summary>
        /// Creates a qualifier statistic for a osu mp match
        /// </summary>
        [Obsolete("Use CreateStatistic", true)]
        public QualifierStageResult CreateQualifierStatistics(HistoryJson.History history)
        {
            var HighestScoreRankingResult = CalculateHighestRankingAndPlayCount(GetData.GetMatches(history), history);
            string matchName = history.Events.First(ob => ob.Detail.Type == "other").Detail.MatchName;
            (string, string) teamNames = GetVersusTeamNames(matchName);

            QualifierStageResult result = new QualifierStageResult()
            {
                MatchName = matchName,
                TeamNames = teamNames,
                HighestScore = HighestScoreRankingResult.Item1[0].Item1,
                HighestScoreBeatmap = HighestScoreRankingResult.Item1[0].Item2,
                HighestScoresRanking = HighestScoreRankingResult.Item1[0].Item3,
                HighestScoreUsername = HighestScoreRankingResult.Item1[0].Item3[0].Player.UserName,
            };

            return result;
        }

        /// <summary>
        /// Creates a statistic for a osu mp match
        /// </summary>
        public AnalyzerResult CreateStatistic(HistoryJson.History history)
        {
            AnalyzerResult result = null;
            try
            {
                string matchName = history.Events.First(ob => ob.Detail.Type == "other").Detail.MatchName;
                HistoryJson.Game[] games = GetData.GetMatches(history);
                var HighestScoreRankingResult = CalculateHighestRankingAndPlayCount(games, history);

                (string, string) teamNames = GetVersusTeamNames(matchName);
                Tuple<int, int> wins = GetWins(games);
                TeamColor winningTeam = wins.Item1 > wins.Item2 ? TeamColor.Red : TeamColor.Blue;
                TeamColor losingTeam = wins.Item1 < wins.Item2 ? TeamColor.Red : TeamColor.Blue;

                result = new AnalyzerResult()
                {
                    MatchName = matchName,
                    HighestScore = HighestScoreRankingResult.Item1[0].Item1,
                    HighestScoreBeatmap = HighestScoreRankingResult.Item1[0].Item2,
                    HighestScoresRanking = HighestScoreRankingResult.Item1[0].Item3,

                    HighestAccuracyScore = HighestScoreRankingResult.Item1[1].Item1,
                    HighestAccuracyBeatmap = HighestScoreRankingResult.Item1[1].Item2,
                    HighestAverageAccuracyRanking = HighestScoreRankingResult.Item1[1].Item3,

                    IsQualifier = false,

                    TeamNames = teamNames,
                    WinningTeam = winningTeam == TeamColor.Red ? teamNames.Item1 : teamNames.Item2,
                    Win = (WinType)(int)winningTeam,
                    WinningTeamColor = winningTeam,
                    WinningTeamWins = winningTeam == TeamColor.Red ? wins.Item1 : wins.Item2,

                    LosingTeam = losingTeam.ToString(),
                    LosingTeamWins = losingTeam == TeamColor.Red ? wins.Item1 : wins.Item2,

                    MostPlayedBeatmap = HighestScoreRankingResult.Item2.OrderBy(b => b.Count).ElementAt(0),

                    TimeStamp = history.Events.Last().TimeStamp
                };
                result.HighestScoreUser = HighestScoreRankingResult.Item1[0].Item3.First(r => r.Player.UserId == result.HighestScore.user_id).Player;
                result.HighestAccuracyUser = HighestScoreRankingResult.Item1[1].Item3.First(r => r.Player.UserId == result.HighestAccuracyScore.user_id).Player;
                result.Ranks = HighestScoreRankingResult.Item1[0].Item3;
                result.Beatmaps = HighestScoreRankingResult.Item2.Select(b => b.BeatMap).ToArray();
            }
            catch (Exception ex)
            {
                Program.Logger.Log("Analyzer: " + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// parses a osu mp match
        /// </summary>
        public HistoryJson.History ParseMatch(string matchIdString)
        {
            const string historyUrl = "https://osu.ppy.sh/community/matches/";
            int matchId = -1;
            string[] multiLinkSplit = matchIdString.Split(new[] { "\r\n", "\r", "\n", Environment.NewLine }, StringSplitOptions.None);

            foreach (string s in multiLinkSplit)
            {
                if (s.Contains(historyUrl))
                {
                    string[] split = s.Split('/');

                    foreach (string str in split)
                        if (int.TryParse(str, out int PresultSplit))
                        {
                            matchId = PresultSplit;
                            break;
                        }
                    if (matchId > 0)
                        break;

                    string numbers = null;
                    int indexOf = s.IndexOf(historyUrl) + historyUrl.Length;

                    string sub = s.Substring(indexOf, s.Length - indexOf);

                    foreach (char c in sub)
                    {
                        if (c.Equals(' '))
                            break;

                        if (int.TryParse(c.ToString(), out int result))
                            numbers += result;
                    }

                    if (int.TryParse(numbers, out int resultMP))
                        matchId = resultMP;
                }
            }

            if (matchId <= 0)
                return null;

            string endpointUrl = $"https://osu.ppy.sh/community/matches/{matchId}/history";
            HistoryJson.History historyJson = GetData.ParseJsonFromUrl(endpointUrl);

            return historyJson;
        }

        /// <summary>
        /// Gets osu mp match team names
        /// </summary>
        public (string, string) GetVersusTeamNames(string matchName)
        {
            string[] MatchNameSplit = matchName.Split(' ');
            string teamRed = MatchNameSplit[1].TrimStart('(');
            int teamVsIndex = MatchNameSplit.ToList().FindIndex(str => str.ToLower().Equals("vs"));
            
            for (int i = 2; i < teamVsIndex; i++)
                teamRed += string.Format(" {0}", MatchNameSplit[i]);

            string teamBlue = MatchNameSplit[teamVsIndex + 1].TrimStart('(');
            teamRed = teamRed.TrimEnd(')');

            for (int i = teamVsIndex + 2; i < MatchNameSplit.Count(); i++)
                teamBlue += string.Format(" {0}", MatchNameSplit[i]);

            teamBlue = teamBlue.TrimEnd(')');

            return (teamRed, teamBlue);
        }

        /// <summary>
        /// calculates the highest ranking players and beatmap play counts
        /// </summary>
        /// <param name="games"><see cref="GetData.GetMatches(HistoryJson.History)"/></param>
        /// <returns>Tuple { Tuple { HighestScore, HighestScoreBeatmap, HighestScoreRanking }[], BeatmapPlayCount } }</returns>
        public Tuple<Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>[], BeatmapPlayCount[]> CalculateHighestRankingAndPlayCount(HistoryJson.Game[] games, HistoryJson.History history)
        {
            HistoryJson.Score highestScore = null;
            HistoryJson.BeatMap highestScoreBeatmap = null;
            List<Player> highestScoreRanking = new List<Player>();
            List<Rank> sortedRanksScore = new List<Rank>();

            HistoryJson.Score highestAccuracy = null;
            HistoryJson.BeatMap highestAccuracyBeatmap = null;
            List<Rank> sortedRanksAccuracy = new List<Rank>();


            List<HistoryJson.Score> scores = new List<HistoryJson.Score>();
            List<BeatmapPlayCount> playCounts = new List<BeatmapPlayCount>();

            int warmupCounter = 0;

            foreach (HistoryJson.Game game in games)
            {
                List<HistoryJson.Score> gameScores = game.scores;

                if (gameScores == null)
                    continue;
                else if (Program.Config.WarmupMatchCount > 0 && warmupCounter < Program.Config.WarmupMatchCount)
                {
                    warmupCounter++;
                    continue;
                }

                int playCountIndex = playCounts.FindIndex(bpc => bpc.BeatMap.id.Value == game.beatmap.id.Value);

                if (playCountIndex > -1)
                    playCounts[playCountIndex].Count++;
                else
                    playCounts.Add(new BeatmapPlayCount()
                    {
                        BeatMap = game.beatmap,
                        Count = 1,
                    });

                foreach (HistoryJson.Score score in game.scores)
                {
                    Player CurrentPlayer = highestScoreRanking.Find(player => player.UserId == score.user_id.Value);

                    if (CurrentPlayer == null)
                    {
                        CurrentPlayer = new Player();
                        CurrentPlayer.UserId = score.user_id.Value;
                        CurrentPlayer.UserName = GetData.GetUser(score, history).Username;
                        CurrentPlayer.Scores = new HistoryJson.Score[] { score };
                        highestScoreRanking.Add(CurrentPlayer);
                    }
                    else
                    {
                        List<HistoryJson.Score> scoresPlayer = CurrentPlayer.Scores.ToList();
                        scoresPlayer.Add(score);
                        CurrentPlayer.Scores = scoresPlayer.ToArray();
                    }

                    if (highestScore == null || score.score.Value > highestScore.score.Value)
                    {
                        highestScore = score;
                        highestScoreBeatmap = game.beatmap;
                    }

                    if (highestAccuracy == null || highestAccuracy.accuracy.Value < score.accuracy.Value)
                    {
                        highestAccuracy = score;
                        highestAccuracyBeatmap = game.beatmap;
                    }
                }

            }

            highestScoreRanking.ForEach(ob =>
            {
                ob.CalculateAverageAccuracy();
                ob.GetHighestScore();
            });

            highestScoreRanking = highestScoreRanking.OrderByDescending(player => player.HighestScore.score.Value).ToList();
            
            for (int i = 0; i < highestScoreRanking.Count; i++)
            {
                Rank rank = new Rank()
                {
                    Player = highestScoreRanking[i],
                    Place = i + 1
                };
                
                sortedRanksScore.Add(rank);
            };
            
            sortedRanksAccuracy = sortedRanksScore.OrderByDescending(r => r.Player.AverageAccuracy).ToList();

            for (int i = 0; i < sortedRanksAccuracy.Count; i++)
                sortedRanksAccuracy[i].Place = i + 1;

            return new Tuple<Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>[], BeatmapPlayCount[]>(
                new Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>[]
            {
                new Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>(highestScore, highestScoreBeatmap, sortedRanksScore.ToArray()),
                new Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>(highestAccuracy, highestAccuracyBeatmap, sortedRanksAccuracy.ToArray()),
            },
            playCounts.ToArray());
        }

        /// <summary>
        /// Gets team wins
        /// </summary>
        /// <param name="games">games to analyze</param>
        /// <returns><see cref="Tuple{T1, T2}"/> [RedTeamWins, BlueTeamWins]</returns>
        public Tuple<int, int> GetWins(HistoryJson.Game[] games)
        {
            int red = 0;
            int blue = 0;
            int warmupCounter = 0;

            foreach (HistoryJson.Game game in games)
            {
                int redScore = 0;
                int blueScore = 0;

                if (warmupCounter < Program.Config.WarmupMatchCount)
                {
                    warmupCounter++;
                    continue;
                }

                foreach (HistoryJson.Score score in game.scores)
                {
                    HistoryJson.Multiplayer multiplayer = score.multiplayer;

                    if (multiplayer.pass == 0)
                        continue;

                    switch (multiplayer.team)
                    {
                        case "red":
                            redScore += score.score.Value;
                            break;
                        case "blue":
                            blueScore += score.score.Value;
                            break;
                    }
                }

                if (blueScore == redScore)
                    continue;
                else if (blueScore > redScore)
                    blue++;
                else if (redScore > blueScore)
                    red++;
            }
            
            return new Tuple<int, int>(red, blue);
        }
    }
}
