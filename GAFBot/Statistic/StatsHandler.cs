using DSharpPlus.Entities;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.Database.Readers;
using GAFBot.Osu.results;
using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GAFBot.Statistic
{
    public static class StatsHandler
    {
        public static DiscordEmbed GetPlayerStatistics(long osuUserId, GAFContext context = null)
        {
            Logger.Log("Getting Player Statistics", LogLevel.Trace);

            bool dispose = context == null;

            BotSeasonPlayerCardCache cardCache;
            Player player;
            try
            {
                SeasonPlayerCardCacheReader cardCacheReader = new SeasonPlayerCardCacheReader(context);
                BaseDBReader<Player> playerReader = new BaseDBReader<Player>(context);

                cardCache = cardCacheReader.Get(cc => cc.OsuUserId == osuUserId);
                player = playerReader.Get(p => p.OsuId == osuUserId);
            }
            finally
            {
                if (dispose)
                    context?.Dispose();
            }

            if (cardCache == null)
            {
                DiscordEmbedBuilder errorBuilder = new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        Name = $"User {osuUserId} currently not in database cached"
                    }
                };

                return errorBuilder.Build();
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = "",
                    Name = $"Stats for {cardCache.Username}"
                },
                ThumbnailUrl = "https://a.ppy.sh/" + cardCache.OsuUserId,
                Title = "Team: " + cardCache.TeamName,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Last updated: " + cardCache.LastUpdated
                }
            };

            if (player != null)
                builder.Author.IconUrl = $"https://osu.ppy.sh/images/flags/{player.Country}.png";

            builder.AddField("Average Accuracy", Math.Round(cardCache.AverageAccuracy / 100.0, 2, MidpointRounding.AwayFromZero).ToString() + " %", true);
            int score = (int)cardCache.AverageScore;
            builder.AddField("Average Score", string.Format("{0:n0}", score), true);
            builder.AddField("Average Misses", Math.Truncate(cardCache.AverageMisses).ToString(), true);
            builder.AddField("Average Combo", Math.Truncate(cardCache.AverageCombo).ToString(), true);
            builder.AddField("Average GPS", Math.Truncate(cardCache.AveragePerformance).ToString(), true);
            builder.AddField("Match MVPs", cardCache.MatchMvps.ToString(), true);
            builder.AddField("Overall Rating", Math.Truncate(cardCache.OverallRating).ToString() + $"(+{cardCache.MatchMvps * 3.5})", true);

            return builder.Build();
        }

        public static DiscordEmbed GetMatchResultEmbed(long matchId, GAFContext context)
        {
            Logger.Log("Getting Match Result Embed", LogLevel.Trace);

            bool dispose = context == null;

            BaseDBReader<BotSeasonResult> seasonResultReader = new BaseDBReader<BotSeasonResult>(context);
            BaseDBReader<BotSeasonScore> seasonScoreReader = new BaseDBReader<BotSeasonScore>(context);
            BaseDBReader<BotSeasonBeatmap> seasonBeatmapReader = new BaseDBReader<BotSeasonBeatmap>(context);
            BaseDBReader<BotLocalization> localeReader = new BaseDBReader<BotLocalization>(context);
            SeasonPlayerReader seasonPlayerReader = new SeasonPlayerReader(context);
            string analyzerFirst, analyzerSecond, analyzerThird, analyzerAverageAcc;

            try
            {
                BotSeasonResult result = seasonResultReader.Get(r => r.MatchId == matchId);

                if (result == null)
                    return null;

                BotSeasonPlayer highestGpsWinningPlayer, highestGpsLosingPlayer;

                List<BotSeasonScore> scores = seasonScoreReader.Where(s => s.BotSeasonResultId == result.Id);

                BotSeasonScore highestAcc = scores.OrderByDescending(s => s.Accuracy).ElementAt(0);
                BotSeasonScore highestScore = scores.OrderByDescending(s => s.Score).ElementAt(0);

                BotSeasonBeatmap highestAccMap = seasonBeatmapReader.Get(b => b.BeatmapId == highestAcc.BeatmapId);
                BotSeasonBeatmap highestScoreMap = seasonBeatmapReader.Get(b => b.BeatmapId == highestScore.BeatmapId);

                BotSeasonPlayer highestAccPlayer = seasonPlayerReader.Get(id: highestAcc.BotSeasonPlayerId);
                BotSeasonPlayer highestScorePlayer = seasonPlayerReader.Get(id: highestScore.BotSeasonPlayerId);

                List<BotSeasonScore> highestGpsScores = scores.Where(s => s.HighestGPS).ToList();

                string winningTeamTrim = result.WinningTeam.Trim(' ', '_');
                string losingTeamTrim = result.LosingTeam.Trim(' ', '_');

                BotSeasonScore highestGpsWinningScore = highestGpsScores.FirstOrDefault(s => s.TeamName.Trim(' ', '_').Equals(winningTeamTrim, StringComparison.CurrentCultureIgnoreCase));
                BotSeasonScore highestGpsLosingScore = highestGpsScores.FirstOrDefault(s => s.TeamName.Trim(' ', '_').Equals(losingTeamTrim, StringComparison.CurrentCultureIgnoreCase));

                highestGpsWinningPlayer = seasonPlayerReader.Get(id: highestGpsWinningScore.BotSeasonPlayerId);
                highestGpsLosingPlayer = seasonPlayerReader.Get(id: highestGpsLosingScore.BotSeasonPlayerId);

                int rnd = Program.Rnd.Next(0, 8);
                DiscordColor color;
                switch (rnd)
                {
                    default:
                    case 0:
                        color = DiscordColor.Aquamarine;
                        break;
                    case 1:
                        color = DiscordColor.Red;
                        break;
                    case 2:
                        color = DiscordColor.Blurple;
                        break;
                    case 3:
                        color = DiscordColor.Green;
                        break;
                    case 4:
                        color = DiscordColor.Yellow;
                        break;
                    case 5:
                        color = DiscordColor.Orange;
                        break;
                    case 6:
                        color = DiscordColor.Black;
                        break;
                    case 7:
                        color = DiscordColor.Gold;
                        break;
                    case 8:
                        color = DiscordColor.CornflowerBlue;
                        break;
                }

                #region localizations
                string analyzerTeam = localeReader.Get(l => l.Code.Equals("analyzerTeam")).String;
                string analyzerWon = localeReader.Get(l => l.Code.Equals("analyzerWon")).String;
                string analyzerMatchPlayed = localeReader.Get(l => l.Code.Equals("analyzerMatchPlayed")).String;
                string analyzerMVP = localeReader.Get(l => l.Code.Equals("analyzerMVP")).String;
                string analyzerTeamBlue = localeReader.Get(l => l.Code.Equals("analyzerTeamBlue")).String;
                string analyzerGeneratedPerformanceScore = localeReader.Get(l => l.Code.Equals("analyzerGeneratedPerformanceScore")).String;
                string analyzerTeamRed = localeReader.Get(l => l.Code.Equals("analyzerTeamRed")).String;
                string analyzerHighestScore = localeReader.Get(l => l.Code.Equals("analyzerHighestScore")).String;
                string analyzerHighestAcc = localeReader.Get(l => l.Code.Equals("analyzerHighestAcc")).String;
                string analyzerOnMap = localeReader.Get(l => l.Code.Equals("analyzerOnMap")).String;
                string analyzerWith = localeReader.Get(l => l.Code.Equals("analyzerWith")).String;
                string analyzerWithPoints = localeReader.Get(l => l.Code.Equals("analyzerWithPoints")).String;
                string analyzerAcc = localeReader.Get(l => l.Code.Equals("analyzerAcc")).String;
                analyzerFirst = localeReader.Get(l => l.Code.Equals("analyzerFirst")).String;
                analyzerSecond = localeReader.Get(l => l.Code.Equals("analyzerSecond")).String;
                analyzerThird = localeReader.Get(l => l.Code.Equals("analyzerThird")).String;
                analyzerAverageAcc = localeReader.Get(l => l.Code.Equals("analyzerAverageAcc")).String;
                #endregion

                string description = string.Format("{0} {1} {2} ({3}:{4})", analyzerTeam, result.WinningTeam, analyzerWon, result.WinningTeamWins, result.LosingTeamWins);

                DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder()
                {
                    Title = result.MatchName,
                    Description = description,
                    Footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        Text = $"{analyzerMatchPlayed} {result.TimeStamp}",
                    },
                    Color = color,
                };

                Dictionary<long, List<(double, double)>> playerValues = new Dictionary<long, List<(double, double)>>();

                for (int i = 0; i < scores.Count; i++)
                {
                    if (!playerValues.ContainsKey(scores[i].BotSeasonPlayerId))
                    {
                        playerValues.Add(scores[i].BotSeasonPlayerId, new List<(double, double)>() { (scores[i].Accuracy, scores[i].GPS) });
                        continue;
                    }

                    playerValues[scores[i].BotSeasonPlayerId].Add((scores[i].Accuracy, scores[i].GPS));
                }

                Dictionary<long, (double, double)> playerAverage = new Dictionary<long, (double, double)>();

                foreach (var pair in playerValues)
                {
                    double avgAcc = pair.Value.Sum(s => s.Item1) / pair.Value.Count();
                    double avgGps = pair.Value.Sum(s => s.Item2) / pair.Value.Count();
                    playerAverage.Add(pair.Key, (avgAcc, avgGps));
                }

                var sortedAvgAcc = playerAverage.OrderByDescending(p => p.Value.Item1).ToList();
                var sortedAvgGps = playerAverage.OrderByDescending(p => p.Value.Item2).ToList();

                KeyValuePair<long, (double, double)> playerTeamAHighestAvgGps = sortedAvgGps.ElementAt(0);
                BotSeasonPlayer playerTeamAHighestGps = seasonPlayerReader.Get(p => p.Id == playerTeamAHighestAvgGps.Key);
                KeyValuePair<long, (double, double)> playerTeamBHighestAvgGps = default;
                BotSeasonPlayer playerTeamBHighestGps = null;

                for (int i = 1; i < sortedAvgGps.Count; i++)
                {
                    var pair = sortedAvgGps.ElementAt(i);
                    var player = seasonPlayerReader.Get(p => p.Id == pair.Key);

                    if (player.TeamId == playerTeamAHighestGps.TeamId)
                    {
                        if (playerTeamAHighestAvgGps.Value.Item2 >= pair.Value.Item2)
                            continue;

                        playerTeamAHighestAvgGps = pair;
                        playerTeamAHighestGps = player;
                    }
                    else
                    {
                        if (playerTeamBHighestAvgGps.Equals(default))
                        {
                            playerTeamBHighestAvgGps = pair;
                            playerTeamBHighestGps = player;

                            continue;
                        }

                        if (playerTeamBHighestAvgGps.Value.Item2 >= pair.Value.Item2)
                            continue;

                        playerTeamBHighestAvgGps = pair;
                        playerTeamBHighestGps = player;
                    }
                }

                //generated performance score = gps
                if (playerTeamAHighestGps.TeamId == highestGpsWinningPlayer.TeamId)
                {
                    discordEmbedBuilder.AddField(analyzerMVP, $"{result.WinningTeam}: {playerTeamAHighestGps.LastOsuUserName} ({Math.Round(playerTeamAHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {analyzerGeneratedPerformanceScore})\n" +
                                                              $"{result.LosingTeam}: {playerTeamBHighestGps.LastOsuUserName} ({Math.Round(playerTeamBHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {analyzerGeneratedPerformanceScore})");
                }
                else
                {
                    discordEmbedBuilder.AddField(analyzerMVP, $"{result.WinningTeam}: {playerTeamBHighestGps.LastOsuUserName} ({Math.Round(playerTeamBHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {analyzerGeneratedPerformanceScore})\n" +
                                                              $"{result.LosingTeam}: {playerTeamAHighestGps.LastOsuUserName} ({Math.Round(playerTeamAHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {analyzerGeneratedPerformanceScore})");
                }

                discordEmbedBuilder.AddField(analyzerHighestScore, string.Format("{0} on the map {1} - {2} [{3}] ({4}*) with {5:n0} Points and {6}% Accuracy!",
                    highestScorePlayer.LastOsuUserName,
                    highestScoreMap.Author,
                    highestScoreMap.Title,
                    highestScoreMap.Difficulty,
                    highestScoreMap.DifficultyRating,
                    string.Format("{0:n0}", highestScore.Score),
                    Math.Round(highestScore.Accuracy, 2, MidpointRounding.AwayFromZero)));

                discordEmbedBuilder.AddField(analyzerHighestAcc, string.Format("{0} {1} {2} - {3} [{4}] ({5}*) {6} {7:n0} {8} {9}% {10}!",
                    highestAccPlayer.LastOsuUserName,
                    analyzerOnMap,
                    highestAccMap.Author,
                    highestAccMap.Title,
                    highestAccMap.Difficulty,
                    highestAccMap.DifficultyRating,
                    analyzerWith,
                    string.Format("{0:n0}", highestAcc.Score),
                    analyzerWithPoints,
                    Math.Round(highestAcc.Accuracy, 2, MidpointRounding.AwayFromZero),
                    analyzerAcc));

                for (int i = 0; i < 4 && i < sortedAvgAcc.Count; i++)
                {
                    BotSeasonPlayer player = seasonPlayerReader.Get(p => p.Id == sortedAvgAcc[i].Key);

                    (string, string) placeString = GetPlaceString(sortedAvgAcc[i].Value.Item1, player, i + 1);
                    discordEmbedBuilder.AddField(placeString.Item1, placeString.Item2);
                }

                return discordEmbedBuilder.Build();
            }
            finally
            {
                if (dispose)
                    context.Dispose();
            }

            (string, string) GetPlaceString(double averageAcc, BotSeasonPlayer player_, int place)
            {
                switch (place)
                {
                    case 1:
                        return (analyzerFirst, $"{ player_.LastOsuUserName}: {analyzerAverageAcc}: { Math.Round(averageAcc, 2, MidpointRounding.AwayFromZero) }%");
                    case 2:
                        return (analyzerSecond, $"{ player_.LastOsuUserName}: {analyzerAverageAcc}: { Math.Round(averageAcc, 2, MidpointRounding.AwayFromZero) }%");
                    case 3:
                        return (analyzerThird, $"{ player_.LastOsuUserName}: {analyzerAverageAcc}: { Math.Round(averageAcc, 2, MidpointRounding.AwayFromZero) }%");
                    case 4:
                        return ("Fourth Place", $"{player_.LastOsuUserName}: {analyzerAverageAcc}: { Math.Round(averageAcc, 2, MidpointRounding.AwayFromZero) }%");
                    //Normally unused
                    default:
                        return ($"{place} Place", $"{ player_.LastOsuUserName}: {analyzerAverageAcc}: { Math.Round(averageAcc, 2, MidpointRounding.AwayFromZero) }%");
                }
            }
        }

        private static double GetAverageAccuracy(BotSeasonPlayer player, GAFContext context = null)
        {
            bool dispose = context == null;
            BaseDBReader<BotSeasonScore> scoreReader = new BaseDBReader<BotSeasonScore>();

            try
            {
                List<BotSeasonScore> scores = scoreReader.Where(s => s.BotSeasonPlayerId == player.Id);

                if (scores.Count == 0)
                    return 0;

                return scores.Sum(s => s.Accuracy) / scores.Count;
            }
            finally
            {
                if (dispose)
                    scoreReader.Dispose();
            }
        }

        private static double GetAverageAccuracy(BotSeasonPlayer player, BotSeasonResult result, GAFContext context)
        {
            BaseDBReader<BotSeasonScore> scoreReader = new BaseDBReader<BotSeasonScore>(context);

            List<BotSeasonScore> scores = scoreReader.Where(s => s.BotSeasonPlayerId == player.Id &&
                                                                 s.BotSeasonResultId == result.Id);

            if (scores.Count == 0)
                return 0;

            return scores.Sum(s => s.Accuracy) / scores.Count;
        }

        public static bool UpdateSeasonStatistics(string matchlink, string stage, GAFContext context)
        {
            Logger.Log($"Updating Player Statistics for {matchlink} ({stage})", LogLevel.Trace);

            Osu.Analyzer analyzer = new Osu.Analyzer();
            var matchData = analyzer.ParseMatch(matchlink);

            List<HistoryJson.History> historyJsons = new List<HistoryJson.History>()
            {
                matchData.Item1
            };

            HistoryJson.History history = new HistoryJson.History();

            if (historyJsons[0] == null || historyJsons[0].Events == null)
                return false;

            var firstEvent = historyJsons[0].Events[0];
            if (firstEvent.Detail == null || !firstEvent.Detail.Type.Equals("match-created"))
            {
                long before = firstEvent.EventId.Value;

                matchData = analyzer.ParseMatch(matchlink, "before=" + before);

                historyJsons.Add(matchData.Item1);

                List<HistoryJson.Event> events = new List<HistoryJson.Event>();
                List<HistoryJson.User> users = new List<HistoryJson.User>();

                foreach (var json in historyJsons)
                {
                    events.AddRange(json.Events);
                    users.AddRange(json.Users);
                }

                //set values via reflection
                SetProperty(history, "Events", events.ToArray());
                SetProperty(history, "Users", users.ToArray());
                SetProperty(history, "EventCount", events.Count);
            }
            else
                history = historyJsons[0];

            var otherEvent = history.Events.FirstOrDefault(ob => ob.Detail.Type == "other");

            string matchName = otherEvent.Detail.MatchName;
            HistoryJson.Game[] games = GetData.GetMatches(history);

            BotSeasonResult seasonResult = new BotSeasonResult()
            {
                Season = Program.Config.CurrentSeason,
                Stage = stage,
                MatchId = matchData.Item2,
                MatchName = matchName,
                TimeStamp = history.Events.Last().TimeStamp,

                LosingTeam = "null",
                WinningTeam = "null",
                LosingTeamWins = 0,
                WinningTeamWins = 0,
                WinningTeamColor = 0,
            };

            (string, string) teamNames = GetVersusTeamNames(matchName);

            bool dispose = context == null;

            if (context == null)
                context = new GAFContext();

            BaseDBReader<BotSeasonResult> seasonResultReader = new BaseDBReader<BotSeasonResult>(context);
            BaseDBReader<BotSeasonBeatmap> seasonBeatmapReader = new BaseDBReader<BotSeasonBeatmap>(context);
            BaseDBReader<Player> playerReader = new BaseDBReader<Player>(context);
            BaseDBReader<TeamPlayerList> tplReader = new BaseDBReader<TeamPlayerList>(context);
            BaseDBReader<Team> teamReader = new BaseDBReader<Team>(context);
            BaseDBReader<BotSeasonScore> seasonScoreReader = new BaseDBReader<BotSeasonScore>(context);
            BaseDBReader<BotSeasonBeatmapMod> seasonBeatmapModReader = new BaseDBReader<BotSeasonBeatmapMod>(context);
            SeasonPlayerReader seasonPlayerReader = new SeasonPlayerReader(context);

            try
            {
                BotSeasonResult r = seasonResultReader.Get(rr => rr.MatchName.Equals(matchName) && rr.TimeStamp.Equals(seasonResult.TimeStamp));

                if (r == null)
                {
                    seasonResult = seasonResultReader.Add(seasonResult);
                    seasonResultReader.Save();
                }
                else
                {
                    r.Season = Program.Config.CurrentSeason;
                    r.Stage = stage;
                    r.MatchId = matchData.Item2;
                    r.MatchName = matchName;
                    r.TimeStamp = history.Events.Last().TimeStamp;
                    r.LosingTeam = "null";
                    r.WinningTeam = "null";
                    r.LosingTeamWins = 0;
                    r.WinningTeamWins = 0;
                    r.WinningTeamColor = 0;

                    seasonResult = r;

                    seasonResultReader.Update(seasonResult);
                    seasonResultReader.Save();
                }


                List<long> osuIdsToUpdate = new List<long>();

                bool doubleAllScores;
                bool teamVs;
                int warmupCounter = 0;
                long blueTeamOsuUserId = 0;
                long redTeamOsuUserId = 0;
                int teamBlueWins = 0;
                int teamRedWins = 0;
                int warmupMatchCount = Program.Config.WarmupMatchCount;
                for (int i = 0; i < games.Length; i++)
                {
                    doubleAllScores = false;
                    var game = games[i];

                    if (game.scores == null || game.scores.Count == 0)
                        continue;

                    if (warmupMatchCount > 0 && warmupCounter < warmupMatchCount)
                    {
                        warmupCounter++;
                        continue;
                    }

                    if (game.team_type.Equals("team-vs", StringComparison.CurrentCultureIgnoreCase))
                        teamVs = true;
                    else
                        teamVs = false;

                    if (game.mods.Contains("ez", StringComparer.CurrentCultureIgnoreCase))
                        doubleAllScores = true;

                    BotSeasonBeatmap seasonBeatmap = seasonBeatmapReader.Get(bm => bm.BeatmapId == game.beatmap.id);

                    //Add beatmap if doesn't exist
                    if (seasonBeatmap == null)
                    {
                        seasonBeatmap = new BotSeasonBeatmap()
                        {
                            BeatmapId = game.beatmap.id ?? 0,
                            Author = game.beatmap.beatmapset.artist,
                            Difficulty = game.beatmap.version,
                            Title = game.beatmap.beatmapset.title,
                            DifficultyRating = game.beatmap.difficulty_rating
                        };

                        seasonBeatmapReader.Add(seasonBeatmap);
                        seasonBeatmapReader.Save();
                    }

                    long blueScore = 0;
                    long redScore = 0;

                    List<BotSeasonScore> scores = new List<BotSeasonScore>();

                    for (int x = 0; x < game.scores.Count; x++)
                    {
                        var score = game.scores[x];
                        bool ez = game.mods.Contains("ez", StringComparer.CurrentCultureIgnoreCase) || score.mods.Contains("ez", StringComparer.CurrentCultureIgnoreCase);

                        if (score.score == 0)
                            continue;

                        if ((score.multiplayer.pass ?? 0) == 0)
                            continue;

                        if (teamVs && !string.IsNullOrEmpty(score.multiplayer.team))
                        {
                            if (ez || doubleAllScores)
                            {
                                if (score.score.HasValue)
                                {
                                    Console.WriteLine("Old score: " + score.score);
                                    SetProperty(score, "score", new int?(score.score.Value * 2));
                                    Console.WriteLine("New score: " + score.score);
                                }
                            }

                            switch (score.multiplayer.team)
                            {
                                case "red":
                                    redScore += score.score.Value;
                                    redTeamOsuUserId = score.user_id ?? 0;
                                    break;
                                case "blue":
                                    blueScore += score.score.Value;
                                    blueTeamOsuUserId = score.user_id ?? 0;
                                    break;
                            }
                        }

                        BotSeasonPlayer player = seasonPlayerReader.Get(p => p.OsuUserId == score.user_id.Value);
                        Team t;

                        //Add player if doesn't exist
                        if (player == null)
                        {
                            Player pl_ = playerReader.Get(p => p.OsuId == score.user_id.Value);
                            TeamPlayerList tpl_ = tplReader.Get(p => p.PlayerListId == pl_.Id);
                            t = teamReader.Get(te => te.Id == tpl_.TeamId);

                            player = new BotSeasonPlayer()
                            {
                                LastOsuUserName = history.Users.FirstOrDefault(u => u.UserId == score.user_id.Value).Username,
                                OsuUserId = score.user_id ?? 0,
                                TeamId = tpl_.TeamId ?? 0
                            };

                            player = seasonPlayerReader.Add(player);
                            seasonPlayerReader.Save();
                        }
                        else
                        {
                            Player pl_ = playerReader.Get(p => p.OsuId == score.user_id.Value);
                            TeamPlayerList tpl_ = tplReader.Get(p => p.PlayerListId == pl_.Id);
                            t = teamReader.Get(te => te.Id == tpl_.TeamId);
                        }

                        if (!osuIdsToUpdate.Contains(player.OsuUserId))
                            osuIdsToUpdate.Add(player.OsuUserId);

                        BotSeasonScore bscore = new BotSeasonScore()
                        {
                            BeatmapId = game.beatmap.id ?? 0,
                            BotSeasonResultId = seasonResult.Id,
                            BotSeasonPlayerId = player.Id,
                            TeamName = t.Name,

                            TeamVs = teamVs,
                            PlayOrder = i,
                            Accuracy = score.accuracy.Value * 100.0f,
                            Score = score.score ?? 0,
                            MaxCombo = score.max_combo ?? 0,
                            Perfect = score.perfect ?? 0,
                            PlayedAt = score.created_at ?? DateTime.UtcNow,
                        };

                        if (ez || doubleAllScores)
                            bscore.Score *= 2;

                        if (doubleAllScores || (score.mods != null && score.mods.Contains("EZ", StringComparer.CurrentCultureIgnoreCase)))
                            bscore.Score = bscore.Score * 2;

                        if (score.multiplayer != null)
                        {
                            bscore.Pass = score.multiplayer.pass ?? 0;
                        }

                        if (score.statistics != null)
                        {
                            bscore.CountMiss = score.statistics.count_miss ?? 0;
                            bscore.CountKatu = score.statistics.count_katu ?? 0;
                            bscore.CountGeki = score.statistics.count_geki ?? 0;
                            bscore.Count50 = score.statistics.count_50 ?? 0;
                            bscore.Count300 = score.statistics.count_300 ?? 0;
                            bscore.Count100 = score.statistics.count_100 ?? 0;
                        }

                        bscore = seasonScoreReader.Add(bscore);
                        seasonScoreReader.Save();

                        scores.Add(bscore);

                        if (score.mods != null && score.mods.Count > 0)
                        {
                            foreach (string mod in score.mods)
                            {
                                if (mod.Equals("nf", StringComparison.CurrentCultureIgnoreCase))
                                    continue;

                                seasonBeatmapModReader.Add(new BotSeasonBeatmapMod()
                                {
                                    BotSeasonScoreId = bscore.Id,
                                    Mod = mod
                                });
                            }
                        }
                    }

                    Dictionary<long, double> gpsValues = CalculateGPS(ref scores);

                    long teamAId = -1;
                    long scoreAId = -1;
                    long teamBId = -1;
                    long scoreBId = -1;
                    foreach (var gpsPair in gpsValues)
                    {
                        BotSeasonScore score = scores.FirstOrDefault(s => s.BotSeasonPlayerId == gpsPair.Key);
                        score.GPS = gpsPair.Value;

                        Console.WriteLine($"Season player id: {score.BotSeasonPlayerId} Beatmap id: {score.BeatmapId} GPS: {score.GPS}");

                        if (score.TeamVs)
                        {
                            BotSeasonPlayer player = seasonPlayerReader.Get(p => p.Id == gpsPair.Key);

                            if (teamAId == -1)
                            {
                                teamAId = player.TeamId;
                                scoreAId = score.Id;
                            }
                            else if (teamBId == -1 && teamAId != player.TeamId)
                            {
                                teamBId = player.TeamId;
                                scoreBId = score.Id;
                            }
                            else
                            {
                                if (teamAId == player.TeamId && scores.FirstOrDefault(s => s.Id == scoreAId).GPS < score.GPS)
                                    scoreAId = score.Id;
                                else if (teamBId == player.TeamId && scores.FirstOrDefault(s => s.Id == scoreBId).GPS < score.GPS)
                                    scoreBId = score.Id;
                            }
                        }

                        seasonScoreReader.Update(score);
                    }

                    if (scoreAId != -1)
                    {
                        BotSeasonScore highestAScore = scores.FirstOrDefault(s => s.Id == scoreAId);
                        highestAScore.HighestGPS = true;

                        seasonScoreReader.Update(highestAScore);
                    }
                    if (scoreBId != -1)
                    {
                        BotSeasonScore highestBScore = scores.FirstOrDefault(s => s.Id == scoreBId);
                        highestBScore.HighestGPS = true;

                        seasonScoreReader.Update(highestBScore);
                    }

                    if (blueScore != 0 || redScore != 0)
                    {
                        if (blueScore > redScore)
                            teamBlueWins++;
                        else if (redScore > blueScore)
                            teamRedWins++;

                        Console.WriteLine(teamBlueWins + " vs " + teamRedWins);
                    }
                }

                seasonScoreReader.Save();

                foreach (long osuId in osuIdsToUpdate)
                    ForceRefreshCache(osuId, context);

                Team blueTeam = GetTeam(blueTeamOsuUserId, context);
                Team redTeam = GetTeam(redTeamOsuUserId, context);

                if (teamBlueWins > teamRedWins)
                {
                    seasonResult.WinningTeamColor = 0;
                    seasonResult.WinningTeamWins = teamBlueWins;
                    seasonResult.WinningTeam = blueTeam.Name;

                    seasonResult.LosingTeamWins = teamRedWins;
                    seasonResult.LosingTeam = redTeam.Name;
                }
                else
                {
                    seasonResult.WinningTeamColor = 1;
                    seasonResult.WinningTeamWins = teamRedWins;
                    seasonResult.WinningTeam = redTeam.Name;

                    seasonResult.LosingTeamWins = teamBlueWins;
                    seasonResult.LosingTeam = blueTeam.Name;
                }

                seasonResultReader.Update(seasonResult);
                seasonResultReader.Save();
            }
            finally
            {
                if (dispose)
                    context?.Dispose();
            }
            return true;
        }

        public static void ReadStatisticsDump(string url)
        {
            try
            {
                string dumpString;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                    dumpString = wc.DownloadString(url);

                List<string> lines = dumpString.Replace("\r\n", "\n").Split('\n').ToList();

                string curString = "";
                for (int i = 0; i < lines.Count; i++)
                {
                    if (string.IsNullOrEmpty(lines[i]))
                    {
                        Console.WriteLine("reading: " + curString);
                        ReadStatisticDumpRow(curString);
                        curString = "";

                        for (int x = 0; x <= i; x++)
                            lines.RemoveAt(0);

                        i = 0;

                        continue;
                    }

                    if (i == 0)
                    {
                        curString += lines[i];
                        continue;
                    }

                    curString += Environment.NewLine + lines[i];
                }
                Logger.Log("Done", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }

        private static void ReadStatisticDumpRow(string row, bool sendToApi = false, bool sendToDatabase = true)
        {
            try
            {
                //<https://osu.ppy.sh/community/matches/53616778> 
                //<https://osu.ppy.sh/mp/53616778> 
                using (GAFContext context = new GAFContext())
                    if (!UpdateSeasonStatistics(row, Program.Config.CurrentSeason, context))
                        throw new Exception("Could not update statistics for row, probably forfeited");


                Osu.Analyzer analyzer = new GAFBot.Osu.Analyzer();
                var matchData = analyzer.ParseMatch(row);

                const string BAN_PATTERN = "bans from";

                string[] lineSplit = row.Split(new char[] { '\r', '\n' });
                lineSplit[0] = lineSplit[0].Replace("d!", "");

                if (matchData.Item2 == -1 && matchData.Item1 == null)
                    return;

                AnalyzerResult analyzerResult = analyzer.CreateStatistic(matchData.Item1, matchData.Item2);

                //var embed = GAFBot.Statistic.StatsHandler.GetMatchResultEmbed(analyzerResult.MatchId);
                //Coding.Discord.GetChannel((ulong)Program.Config.AnalyzeChannel).SendMessageAsync(embed: embed).Wait();

                List<BanInfo> bans = new List<BanInfo>();
                string mline, bannedBy;
                string[] wSplit;
                foreach (string line in lineSplit)
                {
                    if (line.StartsWith("stage", StringComparison.CurrentCultureIgnoreCase))
                    {
                        wSplit = line.Split('-');
                        string stage = wSplit[1].TrimStart(' ').TrimEnd(' ');
                        analyzerResult.Stage = stage;

                        continue;
                    }
                    else if (!line.StartsWith(BAN_PATTERN, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    mline = line.Remove(0, BAN_PATTERN.Length + 1);
                    wSplit = mline.Split(':');

                    bannedBy = wSplit[0];

                    wSplit = wSplit[1].Split(',');

                    string title, artist, version;
                    string[] mSplit;
                    for (int i = 0; i < wSplit.Length; i++)
                    {
                        wSplit[i] = wSplit[i].TrimStart(' ').TrimEnd(' ');
                        int index = wSplit[i].IndexOf('-');
                        mSplit = new string[2];
                        mSplit[0] = wSplit[i].Substring(0, index);
                        mSplit[1] = wSplit[i].Remove(0, index + 1);

                        mSplit[0] = mSplit[0].TrimStart(' ').TrimEnd(' ');
                        mSplit[1] = mSplit[1].TrimStart(' ').TrimEnd(' ');

                        artist = mSplit[0].TrimStart(' ').TrimEnd(' ');

                        int versionStart = mSplit[1].IndexOf('[');

                        mSplit = mSplit.Where(s => !string.IsNullOrEmpty(s)).ToArray();


                        title = mSplit[1].Substring(0, versionStart - 1);
                        version = mSplit[1].Substring(versionStart + 1, mSplit[1].Length - versionStart - 2);

                        bans.Add(new BanInfo(artist, title, version, bannedBy));
                    }
                }

                if (analyzerResult == null)
                {
                    Logger.Log("Failed to create result", LogLevel.ERROR);
                    return;
                }
                analyzerResult.Bans = bans.ToArray();

                if (sendToApi && Program.HTTPAPI != null)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            Logger.Log("Api stats post result: " + Program.HTTPAPI.SendResults(analyzerResult).Result);
                        }
                        catch (Exception)
                        {
                            Logger.Log("Could not post result to api");
                        }
                    });
                }

                if (sendToDatabase)
                {
                    using (BaseDBReader<BotAnalyzerBaninfo> banReader = new BaseDBReader<BotAnalyzerBaninfo>())
                    {
                        foreach (BanInfo bi in analyzerResult.Bans)
                            banReader.Add(new BotAnalyzerBaninfo()
                            {
                                MatchId = analyzerResult.MatchId,
                                Artist = bi.Artist,
                                Title = bi.Title,
                                Version = bi.Version,
                                BannedBy = bi.BannedBy
                            });

                        banReader.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }

        private static Team GetTeam(long osuUserId, GAFContext context = null)
        {
            bool dispose = context == null;

            BaseDBReader<Player> playerReader = new BaseDBReader<Player>(context);
            BaseDBReader<TeamPlayerList> tplReader = new BaseDBReader<TeamPlayerList>(context);
            BaseDBReader<Team> teamReader = new BaseDBReader<Team>(context);

            try
            {
                Player player = playerReader.Get(p => p.OsuId == osuUserId);

                if (player == null)
                    return null;

                TeamPlayerList tpl = tplReader.Get(tp => tp.PlayerListId == player.Id);
                return teamReader.Get(t => t.Id == tpl.TeamId);
            }
            finally
            {
                if (dispose)
                    playerReader.Dispose();
            }
        }

        /// <summary>
        /// Map Scores
        /// </summary>
        /// <param name="scores"></param>
        /// <returns>bot_season_player_id, gps</returns>
        private static Dictionary<long, double> CalculateGPS(ref List<BotSeasonScore> scores)
        {
            Dictionary<long, double> result = new Dictionary<long, double>();

            List<BotSeasonScore> scoresByAcc = scores.OrderBy(s => s.Accuracy).ToList();
            List<BotSeasonScore> scoresByScore = scores.OrderBy(s => s.Score).ToList();
            List<BotSeasonScore> scoresByMisses = scores.OrderBy(s => s.CountMiss).ToList();
            List<BotSeasonScore> scoresByCombo = scores.OrderBy(s => s.MaxCombo).ToList();
            List<BotSeasonScore> scoresByHits300 = scores.OrderBy(s => s.Count300).ToList();

            const double SCORE_MULTI = 1.9;
            const double ACC_MULTI = 2.4;
            const double COMBO_MULTI = 2.0;
            const double HITS300_MULTI = 1.65;

            const double MISSES_MULTI = 1.15;
            const double MISSES_MULTI2 = 2.0;

            Dictionary<long, double> resultAcc = new Dictionary<long, double>();
            Dictionary<long, double> resultScore = new Dictionary<long, double>();
            Dictionary<long, double> resultMisses = new Dictionary<long, double>();
            Dictionary<long, double> resultCombo = new Dictionary<long, double>();
            Dictionary<long, double> resultHits300 = new Dictionary<long, double>();

            int x;
            for (int i = scoresByAcc.Count - 1; i >= 0; i--)
            {
                if (i < scoresByAcc.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByAcc.Count - 1 && scoresByAcc[i].Accuracy == scoresByAcc[x].Accuracy)
                        x++;

                    if (resultAcc.ContainsKey(scoresByAcc[i].BotSeasonPlayerId))
                        resultAcc[scoresByAcc[i].BotSeasonPlayerId] += x * ACC_MULTI;
                    else
                        resultAcc.Add(scoresByAcc[i].BotSeasonPlayerId, x * ACC_MULTI);

                    continue;
                }

                if (resultAcc.ContainsKey(scoresByAcc[i].BotSeasonPlayerId))
                    resultAcc[scoresByAcc[i].BotSeasonPlayerId] += i * ACC_MULTI;
                else
                    resultAcc.Add(scoresByAcc[i].BotSeasonPlayerId, i * ACC_MULTI);
            }

            for (int i = scoresByScore.Count - 1; i >= 0; i--)
            {
                if (i < scoresByScore.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByScore.Count - 1 && scoresByScore[i].Score == scoresByScore[x].Score)
                        x++;

                    if (resultScore.ContainsKey(scoresByScore[i].BotSeasonPlayerId))
                        resultScore[scoresByScore[i].BotSeasonPlayerId] += x * SCORE_MULTI;
                    else
                        resultScore.Add(scoresByScore[i].BotSeasonPlayerId, x * SCORE_MULTI);

                    continue;
                }

                if (resultScore.ContainsKey(scoresByScore[i].BotSeasonPlayerId))
                    resultScore[scoresByScore[i].BotSeasonPlayerId] += i * SCORE_MULTI;
                else
                    resultScore.Add(scoresByScore[i].BotSeasonPlayerId, i * SCORE_MULTI);
            }

            for (int i = scoresByMisses.Count - 1; i >= 0; i--)
            {
                if (i < scoresByMisses.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByMisses.Count - 1 && scoresByMisses[i].CountMiss == scoresByMisses[x].CountMiss)
                        x++;

                    if (resultMisses.ContainsKey(scoresByMisses[i].BotSeasonPlayerId))
                        resultMisses[scoresByMisses[i].BotSeasonPlayerId] += x * MISSES_MULTI * MISSES_MULTI2;
                    else
                        resultMisses.Add(scoresByMisses[i].BotSeasonPlayerId, x * MISSES_MULTI * MISSES_MULTI2);

                    continue;
                }

                if (resultMisses.ContainsKey(scoresByMisses[i].BotSeasonPlayerId))
                    resultMisses[scoresByMisses[i].BotSeasonPlayerId] += i * MISSES_MULTI * MISSES_MULTI2;
                else
                    resultMisses.Add(scoresByMisses[i].BotSeasonPlayerId, i * MISSES_MULTI * MISSES_MULTI2);
            }

            for (int i = scoresByCombo.Count - 1; i >= 0; i--)
            {
                if (i < scoresByCombo.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByCombo.Count - 1 && scoresByCombo[i].MaxCombo == scoresByCombo[x].MaxCombo)
                        x++;

                    if (resultCombo.ContainsKey(scoresByCombo[i].BotSeasonPlayerId))
                        resultCombo[scoresByCombo[i].BotSeasonPlayerId] += x * COMBO_MULTI;
                    else
                        resultCombo.Add(scoresByCombo[i].BotSeasonPlayerId, x * COMBO_MULTI);

                    continue;
                }

                if (resultCombo.ContainsKey(scoresByCombo[i].BotSeasonPlayerId))
                    resultCombo[scoresByCombo[i].BotSeasonPlayerId] += i * COMBO_MULTI;
                else
                    resultCombo.Add(scoresByCombo[i].BotSeasonPlayerId, i * COMBO_MULTI);

            }

            for (int i = scoresByHits300.Count - 1; i >= 0; i--)
            {
                if (i < scoresByHits300.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByHits300.Count - 1 && scoresByHits300[i].Count300 == scoresByHits300[x].Count300)
                        x++;

                    if (resultHits300.ContainsKey(scoresByHits300[i].BotSeasonPlayerId))
                        resultHits300[scoresByHits300[i].BotSeasonPlayerId] += x * HITS300_MULTI;
                    else
                        resultHits300.Add(scoresByHits300[i].BotSeasonPlayerId, x * HITS300_MULTI);

                    continue;
                }

                if (resultHits300.ContainsKey(scoresByHits300[i].BotSeasonPlayerId))
                    resultHits300[scoresByHits300[i].BotSeasonPlayerId] += i * HITS300_MULTI;
                else
                    resultHits300.Add(scoresByHits300[i].BotSeasonPlayerId, i * HITS300_MULTI);
            }

            for (int i = 0; i < resultAcc.Count; i++)
            {
                var pairAcc = resultAcc.ElementAt(i);
                var pairScore = resultScore.ElementAt(i);
                var pairCombo = resultCombo.ElementAt(i);
                var pairHits300 = resultHits300.ElementAt(i);
                var pairMisses = resultMisses.ElementAt(i);

                double missesVal = pairMisses.Value * 1.5;
                double gps = (2 * (pairAcc.Value + pairCombo.Value)) + (1.5 * (pairScore.Value +  + pairHits300.Value));
                gps -= missesVal;

                if (gps < 0 || double.IsNaN(gps))
                    gps = 0;
                else
                    gps *= 2;

                result.Add(pairAcc.Key, gps);
            }

            return result;
        }

        private static double GetOverallRating(long osuUserId, GAFContext context)
        {
            SeasonPlayerReader playerReader = new SeasonPlayerReader(context);
            BotSeasonPlayer player = playerReader.Get(osuUserId: osuUserId);

            if (player == null)
                return 0;

            return GetOverallRating(player, context);
        }

        private static double GetOverallRating(BotSeasonPlayer player, GAFContext context)
        {
            BaseDBReader<BotSeasonScore> scoreReader = new BaseDBReader<BotSeasonScore>(context);
            List<BotSeasonScore> scores = scoreReader.Where(s => s.BotSeasonPlayerId == player.Id);

            double result = 0;

            List<double> gpsValues = new List<double>();
            List<double> accValues = new List<double>();

            int n = scores.Count;
            float x, y, z, acc, gps, miss;
            double accMax = 0;
            double gpsMax = 0;

            for (int i = 0; i < scores.Count; i++)
            {
                BotSeasonScore score = scores[i];

                x = score.Accuracy;
                y = (float)score.GPS;
                z = (float)score.CountMiss;

                if (x <= 0 || y <= 0)
                {
                    continue;
                }

                acc = ((x + x) * x) / (x * 3.0f);
                gps = (y * y * y) / (y * 0.5f);
                miss = z * 10 / x * 3;

                accMax += acc - miss;
                gpsMax += gps - miss;

                accValues.Add(acc);
                gpsValues.Add(gps);
            }
            double accAvg = 0;
            double gpsAvg = 0;

            if (accMax > 0)
            {
                accAvg = accMax / n;
                gpsAvg = gpsMax / n;
            }

            if (accAvg != 0 || gpsAvg != 0)
            {
                double overallRating = ((gpsAvg * gpsAvg) * (accAvg * accAvg)) / (gpsAvg * accAvg) / 100 / 30 / 2.5;

                result = Math.Round(overallRating, 2, MidpointRounding.AwayFromZero);
            }

            return result;
        }

        private static (string, string) GetVersusTeamNames(string matchName)
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

        private static void SetProperty(object instance, string propertyName, object newValue, StringComparison nameComparer = StringComparison.CurrentCultureIgnoreCase)
        {
            try
            {
                Type instanceType = instance.GetType();
                PropertyInfo[] properties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (int i = 0; i < properties.Length; i++)
                {
                    if (!properties[i].Name.Equals(propertyName, nameComparer))
                        continue;

                    properties[i].SetValue(instance, newValue);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
                throw ex;
            }
        }

        private static BotSeasonPlayerCardCache ClearCache(long osuUserId, GAFContext context = null)
        {
            BotSeasonPlayerCardCache cardCache;

            bool dispose = context == null;
            SeasonPlayerCardCacheReader cardCacheReader = new SeasonPlayerCardCacheReader(context);

            try
            {
                cardCache = cardCacheReader.Get(osuUserId: osuUserId);

                if (cardCache == null)
                {

                    cardCache = cardCacheReader.Add(new BotSeasonPlayerCardCache()
                    {
                        OsuUserId = osuUserId,
                        AverageAccuracy = 0,
                        AverageCombo = 0,
                        AverageMisses = 0,
                        AveragePerformance = 0,
                        AverageScore = 0,
                        LastUpdated = DateTime.UtcNow,
                        MatchMvps = 0,
                        OverallRating = 0,
                        TeamName = "null",
                        Username = "null",
                    });

                    cardCacheReader.Save();
                    return cardCache;
                }

                cardCache.AverageAccuracy = 0;
                cardCache.AverageCombo = 0;
                cardCache.AverageMisses = 0;
                cardCache.AveragePerformance = 0;
                cardCache.AverageScore = 0;
                cardCache.LastUpdated = DateTime.UtcNow;
                cardCache.MatchMvps = 0;
                cardCache.OverallRating = 0;
                cardCache.TeamName = "null";
                cardCache.Username = "null";

                cardCache = cardCacheReader.Update(cardCache);
                cardCacheReader.Save();
            }
            finally
            {
                if (dispose)
                    context.Dispose();
            }

            return cardCache;
        }

        /// <summary>
        /// Force refreshes the cache for a specific user, automatically calls <see cref="ClearCache(long)"/>
        /// </summary>
        /// <param name="osuUserId"></param>
        /// <returns></returns>
        private static void ForceRefreshCache(long osuUserId, GAFContext context = null)
        {
            BotSeasonPlayerCardCache cardCache = ClearCache(osuUserId, context);

            bool dispose = context == null;

            SeasonPlayerReader playerReader = new SeasonPlayerReader(context);
            SeasonPlayerCardCacheReader cardCacheReader = new SeasonPlayerCardCacheReader(context);
            BaseDBReader<Team> teamReader = new BaseDBReader<Team>(context);
            BaseDBReader<BotSeasonScore> scoreReader = new BaseDBReader<BotSeasonScore>(context);

            try
            {
                BotSeasonPlayer player = playerReader.Get(p => p.OsuUserId == osuUserId);
                List<BotSeasonScore> scores = scoreReader.Where(s => s.BotSeasonPlayerId == player.Id);
                Team team = teamReader.Get(t => t.Id == player.TeamId);

                foreach (BotSeasonScore score in scores)
                {
                    cardCache.AverageAccuracy += score.Accuracy * 100.0;
                    cardCache.AverageCombo += score.MaxCombo;
                    cardCache.AverageMisses += score.CountMiss;
                    cardCache.AverageScore += score.Score;
                    cardCache.AveragePerformance += score.GPS;

                    if (score.HighestGPS)
                        cardCache.MatchMvps++;
                }

                cardCache.AverageAccuracy /= scores.Count;
                cardCache.AverageCombo /= scores.Count;
                cardCache.AverageMisses /= scores.Count;
                cardCache.AverageScore /= scores.Count;
                cardCache.AveragePerformance /= scores.Count;

                cardCache.OverallRating = GetOverallRating(player, context);

                cardCache.LastUpdated = DateTime.UtcNow;

                if (team == null)
                    cardCache.TeamName = "not found";
                else
                    cardCache.TeamName = team.Name;

                cardCache.Username = player.LastOsuUserName ?? "not found";

                cardCacheReader.Update(cardCache);
                cardCacheReader.Save();
            }
            finally
            {
                if (dispose)
                    cardCacheReader.Dispose();
            }
        }

        public static void ForceRefreshAllCaches(ulong discordChannelId, GAFContext context = null)
        {
            List<long> userIds = new List<long>();
            var dchannel = Coding.Discord.GetChannel(discordChannelId);

            bool dispose = context == null;
            SeasonPlayerCardCacheReader cardCacheReader = new SeasonPlayerCardCacheReader(context);
            SeasonPlayerReader playerReader = new SeasonPlayerReader(context);

            try
            {
                foreach (var p in playerReader.GetSet())
                    userIds.Add(p.OsuUserId);

                var dmessage = dchannel.SendMessageAsync("Updating cached card 0/" + userIds.Count).Result;
                for (int i = 0; i < userIds.Count; i++)
                {
                    dmessage.ModifyAsync($"Updating cached card {i + 1}/{userIds.Count}").Wait();
                    ForceRefreshCache(userIds[i], context);
                }

                dmessage.ModifyAsync($"Finished updating {userIds.Count} cards").Wait();
            }
            finally
            {
                if (dispose)
                    cardCacheReader.Dispose();
            }
        }
    }
}
