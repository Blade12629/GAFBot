using DSharpPlus.Entities;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.Database.Readers;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class SessionCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "session"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Gets info about the current tourney session";

        /// <summary>
        /// 
        /// </summary>
        public string DescriptionUsage => "!session p osuUserId" + Environment.NewLine +
                                          "!session profile osuUserId" + Environment.NewLine /*+*/
                                          //"!session top10" + Environment.NewLine +
                                          /*"!session top pageX" + Environment.NewLine*/;

        public static void Init()
        {
            Program.CommandHandler.Register(new SessionCommand() as ICommand);
            Logger.Log(nameof(SessionCommand) + " Registered");
        }

        /*
         * ToDo:
         * remove qualifier tables
         * merge tables together
         * store gps for player in extra table called bot_analyzer_gps
         * create cache for player cards called bot_player_cards
         * update cache only when new result comes in
         * check if mplink has more than 100 events if yes merge them
         */
        public void Activate(CommandEventArg e)
        {
            if (string.IsNullOrEmpty(e.AfterCMD))
            {
                Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                return;
            }

            string userIdString = e.AfterCMD.ToLower();

            if (userIdString.StartsWith("profile", StringComparison.CurrentCultureIgnoreCase))
                userIdString = e.AfterCMD.Remove(0, "profile ".Length);
            else if (userIdString[0] == 'p')
                userIdString = e.AfterCMD.Remove(0, 2);
            else if (userIdString.StartsWith("top"))
            {
                string[] lsplit = userIdString.Split(' ');

                if (!int.TryParse(lsplit[1], out int topPage))
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Could not parse page: " + lsplit[1]);
                    return;
                }

                List<BotSeasonPlayerCardCache> top10List = new List<BotSeasonPlayerCardCache>();

                int indexStart = topPage - 10;
                int indexEnd = indexStart + 10;

                using (GAFContext context = new GAFContext())
                    top10List.AddRange(context.BotSeasonPlayerCardCache.OrderByDescending(cc => cc.OverallRating).ToList());

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = $"Top {topPage} Players",
                    Timestamp = DateTime.UtcNow
                };

                string list = "";

                for (int i = indexStart; i < top10List.Count && i <= indexEnd - 1; i++)
                {
                    var cc = top10List[i];
                    int place = i + 1;

                    string placeString = $"{place}.";

                    switch (place)
                    {
                        case 1:
                            placeString = "1st";
                            break;
                        case 2:
                            placeString = "2nd";
                            break;
                        case 3:
                            placeString = "3rd";
                            break;
                        case 4:
                            placeString = "4th";
                            break;
                    }

                    list += Environment.NewLine + $"{placeString} {cc.Username} (id: {cc.OsuUserId}) Rating: {cc.OverallRating}";
                }

                list = list.TrimStart(Environment.NewLine.ToCharArray());

                builder.AddField("Top " + topPage, list);

                Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: builder.Build()).Wait();
                return;
            }
            else if (userIdString.StartsWith("count"))
            {
                int count = 0;
                using (GAFContext context = new GAFContext())
                    count = context.BotSeasonPlayerCardCache.Count();

                Coding.Discord.SendMessage(e.ChannelID, "Players: " + count);
                return;
            }
            else if (userIdString.StartsWith("last"))
            {
                List<BotSeasonPlayerCardCache> top10List = new List<BotSeasonPlayerCardCache>();

                using (GAFContext context = new GAFContext())
                    top10List.AddRange(context.BotSeasonPlayerCardCache.OrderByDescending(cc => cc.OverallRating).ToList());

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = $"Last Players",
                    Timestamp = DateTime.UtcNow
                };

                string list = "";

                for (int i = top10List.Count - 10; i < top10List.Count; i++)
                {
                    var cc = top10List[i];
                    int place = i + 1;

                    list += Environment.NewLine + $"{place}. {cc.Username} (id: {cc.OsuUserId}) Rating: {cc.OverallRating}";
                }

                list = list.TrimStart(Environment.NewLine.ToCharArray());

                builder.AddField("Last of all", list);

                Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: builder.Build()).Wait();
                return;
            }
            else if (userIdString.StartsWith("updateall"))
            {
                using (GAFContext context = new GAFContext())
                {
                    BaseDBReader<BotUsers> userReader = new BaseDBReader<BotUsers>();
                    var user = userReader.Get(u => u.DiscordId == (long)e.DUserID);

                    userReader.Dispose();

                    if (user.AccessLevel < (int)AccessLevel.Admin)
                    {
                        Coding.Discord.SendMessage(e.ChannelID, "You do not have permissions to use this command!");
                        return;
                    }

                    Statistic.StatsHandler.ForceRefreshAllCaches(e.ChannelID, context);
                }
            }
            else
            {
                Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                return;
            }

            if (!long.TryParse(userIdString, out long userId))
            {
                Coding.Discord.SendMessage(e.ChannelID, "Could not parse userid: " + userIdString);
                return;
            }

            DiscordEmbed statistics = Statistic.StatsHandler.GetPlayerStatistics(userId);

            Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: statistics).Wait();

            //List<BotAnalyzerRank> ranks = new List<BotAnalyzerRank>();
            //List<BotAnalyzerScore> scores = new List<BotAnalyzerScore>();
            //Player player;
            //using (GAFContext context = new GAFContext())
            //{
            //    ranks.AddRange(context.BotAnalyzerRank.Where(r => r.PlayerOsuId == userId));
            //    scores.AddRange(context.BotAnalyzerScore.Where(s => s.UserId == userId));
            //    player = context.Player.FirstOrDefault(p => p.OsuId == userId);
            //}

            //PlayerCard pc = new PlayerCard();

            //foreach(BotAnalyzerScore score in scores)
            //{
            //    pc.AverageAccuracy += 100.0 * score.Accuracy;
            //    pc.AverageScore += score.Score;
            //    pc.AverageCombo += score.MaxCombo;
            //    pc.AverageMisses += score.CountMiss;

            //    BotAnalyzerRank rank = ranks.FirstOrDefault(r => r.MatchId == score.MatchId && r.PlayerOsuId == score.UserId);

            //    if (rank == null)
            //    {

            //        continue;
            //    }

            //    pc.AverageGPS += rank.MvpScore;
            //}

            //pc.AverageGPS /= scores.Count;
            //pc.AverageAccuracy /= scores.Count;
            //pc.AverageScore /= scores.Count;
            //pc.AverageCombo /= scores.Count;
            //pc.AverageMisses /= scores.Count;

            //if (player != null)
            //{
            //    pc.Username = player.Nickname;
            //    pc.CountryCode = player.Country;
            //}

            //pc.UserId = userId;
            //pc.AvatarUrl = "https://a.ppy.sh/" + userId;
            //pc.PlayCount = scores.Count;

            //Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: Build(pc)).Wait();
        }

        private DiscordEmbed Build(PlayerCard pc)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                ThumbnailUrl = pc.AvatarUrl,
                Timestamp = DateTime.UtcNow,
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = $"https://osu.ppy.sh/images/flags/{pc.CountryCode}.png",
                    Name = "Stats for: " + pc.Username ?? pc.UserId.ToString()
                },
            };

            builder.AddField("Average Accuracy", Math.Round(pc.AverageAccuracy, 2, MidpointRounding.AwayFromZero).ToString() + "%", true);
            builder.AddField("Average Misses", Math.Truncate(pc.AverageMisses).ToString(), true);
            builder.AddField("Average Score", string.Format("{0:n0}", Math.Truncate(pc.AverageScore)).ToString(), true);
            builder.AddField("Average Combo", Math.Truncate(pc.AverageCombo).ToString(), true);
            builder.AddField("Average GPS", Math.Round(pc.AverageGPS, 2, MidpointRounding.AwayFromZero).ToString(), true);
            builder.AddField("Play Count", pc.PlayCount.ToString(), true);

            builder.AddField("Overall Rating", pc.OverallRating.ToString(), true);

            return builder.Build();
        }

        private class PlayerCard
        {
            public double AverageAccuracy;
            public double AverageMisses;
            public double AverageScore;
            public double AverageCombo;
            public double AverageGPS;
            public double OverallRating;
            public string Username;
            public long UserId;

            public string CountryCode;
            public int PlayCount;
            public string AvatarUrl;
        }
    }

    public static class SessionCommandExtension
    {
        public static List<T> GetRange<T>(this List<T> input, int start, int count)
        {
            List<T> result = new List<T>();

            for (int i = start; i < input.Count && i <= start + count; i++)
                result.Add(input[i]);

            return result;
        }
    }
}
