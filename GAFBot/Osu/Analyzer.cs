using OsuHistoryEndPoint;
using System.Collections.Generic;
using System.Text;
using GAFBot.Osu.results;
using System.Linq;
using System;
using DSharpPlus.Entities;

namespace GAFBot.Osu
{
    public class Analyzer
    {
        private const float _accMulti = 0.8f;
        private const float _scoreMulti = 0.9f;
        private const float _missesMulti = 0.1f;
        private const float _comboMulti = 1.0f;
        private const float _300Multi = 1.0f;

        /// <summary>
        /// Creates a statistic for a osu mp match
        /// </summary>
        public AnalyzerResult CreateStatistic(HistoryJson.History history, int matchId)
        {
            AnalyzerResult result = null;
            try
            {
                string matchName = history.Events.FirstOrDefault(ob => ob.Detail.Type == "other").Detail.MatchName;
                HistoryJson.Game[] games = GetData.GetMatches(history);
                var HighestScoreRankingResult = CalculateHighestRankingAndPlayCount(games, history, true);
                
                (string, string) teamNames = GetVersusTeamNames(matchName);
                Tuple<int, int> wins = GetWins(games);
                TeamColor winningTeam = wins.Item1 > wins.Item2 ? TeamColor.Blue : TeamColor.Red;
                TeamColor losingTeam = wins.Item1 < wins.Item2 ? TeamColor.Blue : TeamColor.Red;

                result = new AnalyzerResult()
                {
                    MatchId = matchId,
                    MatchName = matchName,
                    HighestScore = HighestScoreRankingResult.Item1[0].Item1,
                    HighestScoreBeatmap = HighestScoreRankingResult.Item1[0].Item2,
                    HighestScoresRanking = HighestScoreRankingResult.Item1[0].Item3,

                    HighestAccuracyScore = HighestScoreRankingResult.Item1[1].Item1,
                    HighestAccuracyBeatmap = HighestScoreRankingResult.Item1[1].Item2,
                    HighestAverageAccuracyRanking = HighestScoreRankingResult.Item1[1].Item3,
                    
                    WinningTeam = winningTeam == TeamColor.Red ? teamNames.Item2 : teamNames.Item1,
                    WinningTeamColor = winningTeam,
                    WinningTeamWins = winningTeam == TeamColor.Red ? wins.Item2 : wins.Item1,

                    LosingTeam = losingTeam == TeamColor.Red ? teamNames.Item2 : teamNames.Item1,
                    LosingTeamWins = losingTeam == TeamColor.Red ? wins.Item2 : wins.Item1,

                    TimeStamp = history.Events.Last().TimeStamp
                };
                result.HighestScoreUser = HighestScoreRankingResult.Item1[0].Item3.FirstOrDefault(r => r.Player.UserId == result.HighestScore.user_id).Player;
                result.HighestAccuracyUser = HighestScoreRankingResult.Item1[1].Item3.FirstOrDefault(r => r.Player.UserId == result.HighestAccuracyScore.user_id).Player;
                result.Ranks = HighestScoreRankingResult.Item1[0].Item3;
                result.Beatmaps = HighestScoreRankingResult.Item2.Select(b => b.BeatMap).ToArray();
            }
            catch (Exception ex)
            {
                Logger.Log("Analyzer: " + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// parses a osu mp match
        /// </summary>
        public (HistoryJson.History, int) ParseMatch(string matchIdString)
        {
            const string historyUrl = "https://osu.ppy.sh/community/matches/";
            const string historyUrlVariant = "https://osu.ppy.sh/mp/";

            if (matchIdString.Contains(historyUrlVariant))
                matchIdString = matchIdString.Replace(historyUrlVariant, historyUrl);

            int matchId = -1;
            string[] multiLinkSplit = matchIdString.Split(new[] { "\r\n", "\r", "\n", Environment.NewLine }, StringSplitOptions.None);

            foreach (string s in multiLinkSplit)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
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
                return (null, 0);

            string endpointUrl = $"https://osu.ppy.sh/community/matches/{matchId}/history";
            HistoryJson.History historyJson = GetData.ParseJsonFromUrl(endpointUrl);

            return (historyJson, matchId);
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
        public Tuple<Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>[], BeatmapPlayCount[]> CalculateHighestRankingAndPlayCount(HistoryJson.Game[] games, HistoryJson.History history, bool calculateMVP = false)
        {
            HistoryJson.Score highestScore = null;
            HistoryJson.BeatMap highestScoreBeatmap = null;
            List<Player> highestScoreRanking = new List<Player>();
            List<Rank> sortedRanksScore = new List<Rank>();

            HistoryJson.Score highestAccuracy = null;
            HistoryJson.BeatMap highestAccuracyBeatmap = null;
            List<Rank> sortedRanksAccuracy = new List<Rank>();

            StringComparer curCultIgnore = StringComparer.CurrentCultureIgnoreCase;

            List<HistoryJson.Score> scores = new List<HistoryJson.Score>();
            List<BeatmapPlayCount> playCounts = new List<BeatmapPlayCount>();

            int warmupCounter = 0;
            List<HistoryJson.Score> hAcc;
            List<HistoryJson.Score> hScore;
            List<HistoryJson.Score> hMisses;
            List<HistoryJson.Score> hCombo;
            List<HistoryJson.Score> h300;

            bool doubleAllScores;
            foreach (HistoryJson.Game game in games)
            {
                doubleAllScores = false;
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

                if (game.mods.Contains("ez", curCultIgnore))
                    doubleAllScores = true;

                for (int i = 0; i < game.scores.Count; i++)
                {
                    HistoryJson.Score score = game.scores[i];
                    Player CurrentPlayer = highestScoreRanking.Find(player => player.UserId == score.user_id.Value);
                    
                    if (doubleAllScores || score.mods.Contains("EZ", curCultIgnore))
                        ModifyScore(ref score, score.score.Value * 2);

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

                if (calculateMVP)
                {

                    hAcc = game.scores.OrderBy(f => f.accuracy.Value).ToList();
                    hScore = game.scores.OrderBy(f => f.score.Value).ToList();
                    hMisses = game.scores.OrderByDescending(f => f.statistics.count_miss.Value).ToList();
                    hCombo = game.scores.OrderBy(f => f.max_combo.Value).ToList();
                    h300 = game.scores.OrderBy(f => f.statistics.count_300.Value).ToList();

                    int x;
                    for (int i = hAcc.Count - 1; i > 0; i--)
                    {
                        if (i < hAcc.Count - 1)
                        {
                            x = i + 1;
                            while(x < hAcc.Count - 1 && hAcc[i].accuracy.Value == hAcc[x].accuracy.Value)
                                x++;

                            highestScoreRanking.Find(p => p.UserId == hAcc[i].user_id.Value).MVPScore += x * _accMulti;
                            continue;
                        }

                        highestScoreRanking.Find(p => p.UserId == hAcc[i].user_id.Value).MVPScore += i * _accMulti;
                    }

                    for (int i = hScore.Count - 1; i > 0; i--)
                    {
                        if (i < hScore.Count - 1)
                        {
                            x = i + 1;
                            while (x < hScore.Count - 1 && hScore[i].score.Value == hScore[x].score.Value)
                                x++;

                            highestScoreRanking.Find(p => p.UserId == hScore[i].user_id.Value).MVPScore += x * _scoreMulti;
                            continue;
                        }

                        highestScoreRanking.Find(p => p.UserId == hScore[i].user_id.Value).MVPScore += i * _scoreMulti;
                    }

                    for (int i = hMisses.Count - 1; i > 0; i--)
                    {
                        if (i < hMisses.Count - 1)
                        {
                            x = i + 1;
                            while (x < hMisses.Count - 1 && hMisses[i].statistics.count_miss.Value == hMisses[x].statistics.count_miss.Value)
                                x++;

                            highestScoreRanking.Find(p => p.UserId == hMisses[i].user_id.Value).MVPScore -= x * _missesMulti;
                            continue;
                        }

                        highestScoreRanking.Find(p => p.UserId == hMisses[i].user_id.Value).MVPScore -= i * _missesMulti;
                    }

                    for (int i = hCombo.Count - 1; i > 0; i--)
                    {
                        if (i < hCombo.Count - 1)
                        {
                            x = i + 1;
                            while (x < hCombo.Count - 1 && hCombo[i].max_combo.Value == hCombo[x].max_combo.Value)
                                x++;

                            highestScoreRanking.Find(p => p.UserId == hCombo[i].user_id.Value).MVPScore += x * _comboMulti;
                            continue;
                        }

                        highestScoreRanking.Find(p => p.UserId == hCombo[i].user_id.Value).MVPScore += i * _comboMulti;
                    }

                    for (int i = h300.Count - 1; i > 0; i--)
                    {
                        if (i < h300.Count - 1)
                        {
                            x = i + 1;
                            while (x < h300.Count - 1 && h300[i].max_combo.Value == h300[x].max_combo.Value)
                                x++;

                            highestScoreRanking.Find(p => p.UserId == h300[i].user_id.Value).MVPScore += x * _300Multi;
                            continue;
                        }

                        highestScoreRanking.Find(p => p.UserId == h300[i].user_id.Value).MVPScore += i * _300Multi;
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

        public DiscordEmbed CreateStatisticEmbed(AnalyzerResult ar, DiscordColor embedColor)
        {
            string description = string.Format("{0} {1} {2} ({3}:{4})", Localization.Get("analyzerTeam"), ar.WinningTeam, Localization.Get("analyzerWon"), ar.WinningTeamWins, ar.LosingTeamWins);

            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder()
            {
                Title = ar.MatchName,
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"{Localization.Get("analyzerMatchPlayed")} {ar.TimeStamp}",
                },
                Color = embedColor,
            };

            var playersBlue = ar.HighestScoresRanking.Where(r => r.Player.Scores.ElementAt(r.Player.Scores.Length - 1).multiplayer.team.Trim(' ').Equals("blue", StringComparison.CurrentCultureIgnoreCase)).OrderByDescending(f => f.Player.MVPScore).ToList();
            var playersRed = ar.HighestScoresRanking.Where(r => r.Player.Scores.ElementAt(r.Player.Scores.Length - 1).multiplayer.team.Trim(' ').Equals("red", StringComparison.CurrentCultureIgnoreCase)).OrderByDescending(f => f.Player.MVPScore).ToList();

            var playerBlue = playersBlue.ElementAt(0).Player;
            var playerRed = playersRed.ElementAt(0).Player;
            //generated performance score = gps
            discordEmbedBuilder.AddField(Localization.Get("analyzerMVP"), $"{Localization.Get("analyzerTeamBlue")}: {playerBlue.UserName} ({playerBlue.MVPScore} {Localization.Get("analyzerGeneratedPerformanceScore")}){Environment.NewLine}{Localization.Get("analyzerTeamRed")}: {playerRed.UserName} ({playerRed.MVPScore} {Localization.Get("analyzerGeneratedPerformanceScore")})");

            discordEmbedBuilder.AddField(Localization.Get("analyzerHighestScore"), string.Format("{0} on the map {1} - {2} [{3}] ({4}*) with {5:n0} Points and {6}% Accuracy!",
                ar.HighestScoreUser.UserName, ar.HighestScoreBeatmap.beatmapset.artist,
                ar.HighestScoreBeatmap.beatmapset.title, ar.HighestScoreBeatmap.version,
                ar.HighestScoreBeatmap.difficulty_rating,
                string.Format("{0:n0}", ar.HighestScoreUser.HighestScore.score),
                Math.Round(ar.HighestScoreUser.HighestScore.accuracy.Value * 100.0f, 2, MidpointRounding.AwayFromZero)));

            discordEmbedBuilder.AddField(Localization.Get("analyzerHighestAcc"), string.Format("{0} {1} {2} - {3} [{4}] ({5}*) {6} {7:n0} {8} {9}% {10}!",
                ar.HighestAccuracyUser.UserName,
                Localization.Get("analyzerOnMap"),
                ar.HighestAccuracyBeatmap.beatmapset.artist,
                ar.HighestAccuracyBeatmap.beatmapset.title,
                ar.HighestAccuracyBeatmap.version,
                ar.HighestAccuracyBeatmap.difficulty_rating,
                Localization.Get("analyzerWith"),
                string.Format("{0:n0}", ar.HighestAccuracyScore.score),
                Localization.Get("analyzerWithPoints"),
                Math.Round(ar.HighestAccuracyScore.accuracy.Value * 100.0f, 2, MidpointRounding.AwayFromZero),
                Localization.Get("analyzerAcc")));

            for (int i = 1; i < 4; i++)
            {
                Rank place = ar.HighestAverageAccuracyRanking.Last(ob => ob.Place == i);
                (string, string) placeString = GetPlaceString(place);
                discordEmbedBuilder.AddField(placeString.Item1, placeString.Item2);
            }
            
            return discordEmbedBuilder.Build();

            (string, string) GetPlaceString(Rank place)
            {
                switch (place.Place)
                {
                    case 1:
                        return (Localization.Get("analyzerFirst"), $"{ place.Player.UserName}: {Localization.Get("analyzerAverageAcc")}: { place.Player.AverageAccuracyRounded}%");
                    case 2:
                        return (Localization.Get("analyzerSecond"), $"{ place.Player.UserName}: {Localization.Get("analyzerAverageAcc")}: { place.Player.AverageAccuracyRounded}%");
                    case 3:
                        return (Localization.Get("analyzerThird"), $"{place.Player.UserName}: {Localization.Get("analyzerAverageAcc")}: { place.Player.AverageAccuracyRounded}%");
                    //Normally unused
                    case 4:
                        return ("Fourth Place", $"{place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                    default:
                        return ($"{place.Place} Place", $"{ place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                }
            }
        }

        /// <summary>
        /// Do not use this lightly, this contains making changes 
        /// to the class via reflection and might be risky.
        /// 
        /// This is a temporary implementation since HistoryEndPoint 
        /// library need a setter update for this or a complete rewrite
        /// in order to update score values for ez mod (ez mod = doubled)
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        private void ModifyScore(ref HistoryJson.Score score, int newVal)
        {
            try
            {
                Type t = typeof(HistoryJson.Score);
                System.Reflection.PropertyInfo propInfo = t.GetProperties().FirstOrDefault(p => p.Name.Contains("score", StringComparison.CurrentCultureIgnoreCase));

                //If we cannot write this we just gonna get the private setter and use that
                if (!propInfo.CanWrite)
                {
                    propInfo = t.GetProperty("score", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    propInfo.SetValue(score, new int?(newVal));
                    return;
                }

                //just change value
                propInfo.SetValue(score, new int?(newVal));
            }
            catch (Exception ex)
            {
                Logger.Log("Exception at temp impl modifyScore in analyzer" + Environment.NewLine + ex.ToString());
            }
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
