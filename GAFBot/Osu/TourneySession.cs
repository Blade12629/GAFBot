using DSharpPlus.Entities;
using GAFBot.Database.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAFBot.Osu
{
    public static class OsuTourneySession
    {
        public static PlayerProfileDB GetPlayerProfileDB(long userId, string sessionName)
        {
            try
            {
                List<BotAnalyzerTourneyMatches> matches;
                List<BotAnalyzerRank> ranks = new List<BotAnalyzerRank>();
                List<BotAnalyzerScore> scores = new List<BotAnalyzerScore>();

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    matches = context.BotAnalyzerTourneyMatch.Where(m => m.ChallongeTournamentName.Equals(sessionName, StringComparison.CurrentCultureIgnoreCase)).ToList();

                    foreach (var match in matches)
                    {
                        ranks.Add(context.BotAnalyzerRank.FirstOrDefault(r => r.MatchId == match.Id && r.PlayerOsuId == userId));
                        scores = context.BotAnalyzerScore.Where(s => s.MatchId == match.Id && s.UserId == userId).ToList();
                    }
                }

                PlayerProfileDB result = new PlayerProfileDB();
                Dictionary<long, List<double>> accuracies = new Dictionary<long, List<double>>();

                foreach (var score in scores)
                {
                    result.AverageAccuracy += score.Accuracy;

                    if (!accuracies.ContainsKey(score.MatchId))
                        accuracies.Add(score.MatchId, new List<double>() { score.Accuracy });
                    else
                        accuracies[score.MatchId].Add(score.Accuracy);
                }

                result.AverageAccuracy /= scores.Count;

                Dictionary<long, double> avgGps = new Dictionary<long, double>();

                foreach (var rank in ranks)
                {
                    result.AverageGeneratedPerformanceScore += rank.MvpScore;

                    avgGps.Add(rank.MatchId, rank.MvpScore);
                }

                result.AverageGeneratedPerformanceScore /= ranks.Count;

                Dictionary<long, double> avgAccs = new Dictionary<long, double>();

                foreach (var pair in accuracies)
                {
                    double acc = 0;

                    pair.Value.ForEach(v => acc += v);
                    acc /= pair.Value.Count;

                    avgAccs.Add(pair.Key, acc);
                }

                List<(float, float)> dataForOR = new List<(float, float)>();

                foreach (var key in avgAccs.Keys)
                    dataForOR.Add(((float)avgAccs[key], (float)avgGps[key]));

                result.OverallRating = GetOverallPerformance(values: dataForOR.ToArray());
                result.PlayCount = scores.Count;

                result.UserId = (int)userId;
                result.UserName = Osu.Api.Api.GetUserName(result.UserId);

                return result;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rounded">If rounded to 2 digits or not</param>
        /// <param name="values">ACC, GPS</param>
        /// <returns></returns>
        public static float GetOverallPerformance(int rounded = 2, params (float, float)[] values)
        {
            List<float> accs = new List<float>();
            List<float> gpss = new List<float>();

            int n = values.Length;
            float x, y, acc, gps;

            float accMax = 0.0f;
            float gpsMax = 0.0f;

            foreach (var val in values)
            {
                x = val.Item1;
                y = val.Item2;

                acc = ((x + x) * x) / (x * 3.0f);
                gps = (y * y * y) / (y * 0.5f);

                accMax += acc;
                gpsMax += gps;

                accs.Add(acc);
                gpss.Add(gps);
            }

            float accAvg = accMax / n;
            float gpsAvg = gpsMax / n;

            float endResult = (accAvg * 1500.0f + gpsAvg * 5000.0f) * 2.0f / 20000000.0f * 3.0f;

            if (rounded < 0)
                return endResult;
            else
                return (float)Math.Round(endResult, rounded, MidpointRounding.AwayFromZero);
        }

        private static readonly List<DiscordColor> _colors = new List<DiscordColor>()
        {
            DiscordColor.Red,
            DiscordColor.Blue,
            DiscordColor.Orange,
            DiscordColor.Cyan,
            DiscordColor.Magenta,
            DiscordColor.HotPink
        };

        public static DiscordEmbed BuildProfile(PlayerProfileDB player)
        {
            DiscordEmbedBuilder builder;

            if (player == null)
            {
                builder = new DiscordEmbedBuilder()
                {
                    Title = "Could not fetch profile",
                };
                builder.AddField("Could not find player", "n/a");

                return builder.Build();
            }

            builder = new DiscordEmbedBuilder()
            {
                Title = "Profile (osu.ppy.sh)",
                Description = ".",
                Color = _colors[Program.Rnd.Next(0, _colors.Count - 1)],
                Url = $"https://osu.ppy.sh/users/{player.UserId}",
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Requested on"
                },
                ThumbnailUrl = $"https://a.ppy.sh/{player.UserId}",
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"Stats for {player.UserName}",
                    Url = $"https://osu.ppy.sh/users/{player.UserId}",
                    IconUrl = "https://cdn.discordapp.com/attachments/239737922595717121/621452111607234571/AYEorNAY.png"
                }
            };

            builder.AddField("Overall Rating", player.OverallRating.ToString(), true);
            builder.AddField("Average GPS", Math.Round(player.AverageGeneratedPerformanceScore, 2, MidpointRounding.AwayFromZero).ToString(), true);
            builder.AddField("Average Accuracy", Math.Round(player.AverageAccuracy, 2, MidpointRounding.AwayFromZero).ToString() + " %", true);
            builder.AddField("Playcount", player.PlayCount.ToString(), true);

            return builder.Build();
        }
    }


    public class PlayerProfileDB
    {
        public float OverallRating { get; set; }
        //{
        //    get
        //    {
        //        List<(float, float)> values = new List<(float, float)>();

        //        for (int i = 0; i < AverageGeneratedPerformanceScores.Length && i < AverageAccuracies.Length; i++)
        //            values.Add((AverageAccuracies[i], AverageGeneratedPerformanceScores[i]));

        //        return TourneySession.GetOverallPerformance(values: values.ToArray());
        //    }
        //}
        public int PlayCount { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// Always Up to date with <see cref="AverageGeneratedPerformanceScores"/>
        /// </summary>
        public float AverageGeneratedPerformanceScore { get; set; }
        /// <summary>
        /// Always Up to date with <see cref="AverageAccuracies"/>
        /// </summary>
        public float AverageAccuracy { get; set; }

        public PlayerProfileDB()
        {

        }

        public PlayerProfileDB(float[] gps, float[] avgAccuracies, int userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }

    }
}
