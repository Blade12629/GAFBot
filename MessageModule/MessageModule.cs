using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using GAFBot;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using GAFBot.Osu.results;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MessageModule
{
    public class MessageModule : IMessageHandler
    {
        private static readonly List<DiscordColor> _analyzerColors = new List<DiscordColor>()
        {
            DiscordColor.Aquamarine,
            DiscordColor.Gold,
            DiscordColor.Gray,
            DiscordColor.Green,
            DiscordColor.Magenta
        };

        /// <summary>
        /// Always enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return true;
            }
            set
            {

            }
        }

        public string ModuleName => "message";

        public void Initialize()
        {

        }

        public void Disable()
        {

        }

        public void Dispose()
        {

        }

        public void Enable()
        {

        }

        public void FakeTrigger(string teama, string teamb, string winningteam)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the access level for the user
        /// </summary>
        /// <param name="user">discord user id to find</param>
        /// <returns>user access level</returns>
        public AccessLevel GetAccessLevel(ulong user)
        {
            BotUsers buser;

            using (GAFContext context = new GAFContext())
                buser = context.BotUsers.FirstOrDefault(b => (ulong)b.DiscordId.Value == user);


            if (buser == null)
                return AccessLevel.User;

            return (AccessLevel)buser.AccessLevel;
        }

        /// <summary>
        /// Gets the info of a beatmap and sends it into the channel
        /// </summary>
        /// <param name="message">input message</param>
        /// <param name="channelId">output discord channel id</param>
        public void GetBeatmapInfo(string message, ulong channelId)
        {
            string content = message;
            int setid = -1;
            int id = -1;

            List<string> mods = new List<string>();

            int index;
            string modParams = "";

            if (content.Contains('+'))
            {
                index = content.IndexOf('+');
                modParams = content.Remove(0, index);
                content = content.Remove(index, modParams.Length);
            }

            while (modParams.Contains('+'))
            {
                modParams = modParams.Remove(0, 1);
                index = modParams.IndexOf('+');

                if (index < 0)
                {
                    mods.Add(modParams);
                    break;
                }

                mods.Add(modParams.Substring(0, index));
                modParams = modParams.Remove(0, index);
            }

            GAFBot.Osu.Api.Api.Mods modE = GAFBot.Osu.Api.Api.Mods.None;
            const string beatmapIdString = "https://osu.ppy.sh/b/";
            const string beatmapSetIdString = "https://osu.ppy.sh/beatmapsets/";

            foreach (string m in mods)
                if (Enum.TryParse(typeof(GAFBot.Osu.Api.Api.Mods), m, out object res))
                    modE |= (GAFBot.Osu.Api.Api.Mods)res;

            if (content.StartsWith(beatmapIdString, StringComparison.CurrentCultureIgnoreCase))
            {
                content = content.Remove(0, beatmapIdString.Length);
                content = content.TrimEnd('/');
                if (content.Contains('?'))
                {
                    index = content.IndexOf('=');
                    if (index > -1)
                        content = content.Remove(content.IndexOf('?'), 4);
                    else
                        return;
                }

                if (!int.TryParse(content, out id))
                    return;
            }
            else if (content.StartsWith(beatmapSetIdString, StringComparison.CurrentCultureIgnoreCase))
            {
                content = content.Remove(0, beatmapSetIdString.Length);

                if (content.Contains('#'))
                {
                    index = content.IndexOf('#');

                    if (!int.TryParse(content.Substring(0, index), out setid))
                        return;

                    content = content.Remove(0, index + "#osu/".Length).TrimEnd('/');

                    if (!int.TryParse(content.Substring(0, content.Length), out id))
                        return;
                }
                else
                {
                    Console.WriteLine(content);
                    if (!int.TryParse(content, out setid))
                        return;
                }
            }

            if (id <= 0 && setid <= 0)
            {
                Console.WriteLine("beatmapId and set is null");
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    GAFBot.Osu.Api.Json_Get_Beatmaps beatmaps = null;

                    if (id > 0)
                        beatmaps = GAFBot.Osu.Api.Api.GetBeatmaps(id: (int)id, mods: (int)modE);
                    else
                        beatmaps = GAFBot.Osu.Api.Api.GetBeatmaps(setid: (int)setid, mods: (int)modE);

                    if (beatmaps == null || beatmaps == default(GAFBot.Osu.Api.Json_Get_Beatmaps) || beatmaps.Beatmaps == null || beatmaps.Beatmaps.Length == 0)
                        return;

                    var beatmap = beatmaps.Beatmaps[0];

                    if (modE.HasFlag(GAFBot.Osu.Api.Api.Mods.HR))
                        ComputeHR(ref beatmap);

                    if (modE.HasFlag(GAFBot.Osu.Api.Api.Mods.DT))
                        ComputeDT(ref beatmap);
                    else if (modE.HasFlag(GAFBot.Osu.Api.Api.Mods.HT))
                        ComputeHT(ref beatmap);

                    if (modE.HasFlag(GAFBot.Osu.Api.Api.Mods.EZ))
                        ComputeEZ(ref beatmap);

                    List<DiscordColor> _colors = new List<DiscordColor>()
                        {
                            DiscordColor.Aquamarine,
                            DiscordColor.Gold,
                            DiscordColor.PhthaloGreen,
                            DiscordColor.NotQuiteBlack,
                            DiscordColor.MidnightBlue,
                            DiscordColor.IndianRed
                        };

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    {
                        Title = $"Mapinfo for {beatmap.artist} - {beatmap.title} [{beatmap.version}] ({beatmap.beatmapset_id}) + {modE.ToString()}",
                        Description = $"Mapped by {beatmap.creator}",
                        Color = _colors[Program.Rnd.Next(0, _colors.Count - 1)]
                    };

                    //Try convert difficulty
                    string difficultyRating = beatmap.difficultyrating.Replace('.', ',');

                    if (double.TryParse(difficultyRating, out double decResult))
                        difficultyRating = $"{decResult:0.00}";

                    difficultyRating = difficultyRating.Replace('.', ',');

                    if (difficultyRating.Equals("0.00"))
                        difficultyRating = beatmap.difficultyrating;

                    //Try convert approved
                    string approved = beatmap.approved;

                    if (int.TryParse(approved, out int iResult))
                        approved = ((GAFBot.Osu.ApprovedEnum)iResult).ToString();

                    builder.AddField("Difficulty", $"Difficulty: {difficultyRating}*{Environment.NewLine}Circle Size: {beatmap.diff_size}{Environment.NewLine}Approach Rate: {beatmap.diff_approach}{Environment.NewLine}Overall difficulty: {beatmap.diff_overall}{Environment.NewLine}Drain Rate: {beatmap.diff_drain}");
                    builder.AddField("Metadata", $"Hit/Total Length: {beatmap.hit_length}/{beatmap.total_length}{Environment.NewLine}Circles/Slider/Spinner: {beatmap.count_normal}/{beatmap.count_slider}/{beatmap.count_spinner}{Environment.NewLine}Max Combo: {beatmap.max_combo}{Environment.NewLine}Source: {(beatmap.source == null ? "null" : null)}{Environment.NewLine}Status: {approved}");
                    builder.AddField("Special Info", $"Download Unavailable: {beatmap.download_unavailable}{Environment.NewLine}Audio Unavailable: {beatmap.audio_unavailable}");
                    var embed = builder.Build();
                    var channel = Coding.Discord.GetChannel(channelId);
                    channel.SendMessageAsync(embed: embed).Wait();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.ToString());
                }
            }).Wait();

            void ComputeHR(ref GAFBot.Osu.Api.Json_Get_Beatmaps.Beatmap map)
            {
                double size = (double.Parse(map.diff_size.Replace('.', ',')) / 100.0 * 130.0);
                if (size > 10.0)
                    size = 10;

                double approach = (double.Parse(map.diff_approach.Replace('.', ',')) / 100.0 * 140.0);
                if (approach > 10.0)
                    approach = 10;

                double drain = (double.Parse(map.diff_drain.Replace('.', ',')) / 100.0 * 140.0);
                if (drain > 10.0)
                    drain = 10;

                double overall = (double.Parse(map.diff_overall.Replace('.', ',')) / 100.0 * 140.0);
                if (overall > 10.0)
                    overall = 10;

                map.diff_size = size.ToString().Replace(',', '.') + " (HR)";
                map.diff_approach = approach.ToString().Replace(',', '.') + " (HR)";
                map.diff_drain = drain.ToString().Replace(',', '.') + " (HR)";
                map.diff_overall = overall.ToString().Replace(',', '.') + " (HR)";
            }
            void ComputeDT(ref GAFBot.Osu.Api.Json_Get_Beatmaps.Beatmap map)
            {
                double bpm = ((double.Parse(map.bpm) / 100.0 * 150.0));
                double total_length = Math.Truncate(((double.Parse(map.total_length) / 100.0 * 67.0)));
                double hit_length = Math.Truncate(((double.Parse(map.hit_length) / 100.0 * 67.0)));

                map.bpm = bpm.ToString();
                map.total_length = total_length.ToString();
                map.hit_length = hit_length.ToString();
                map.diff_size += " (Non DT)";
                map.diff_approach += " (Non DT)";
                map.diff_drain += " (Non DT)";
                map.diff_overall += " (Non DT)";
            }
            void ComputeEZ(ref GAFBot.Osu.Api.Json_Get_Beatmaps.Beatmap map)
            {
                map.diff_size += " (Non EZ)";
                map.diff_approach += " (Non EZ)";
                map.diff_drain += " (Non EZ)";
                map.diff_overall += " (Non EZ)";
            }
            void ComputeHT(ref GAFBot.Osu.Api.Json_Get_Beatmaps.Beatmap map)
            {
                map.bpm = ((double.Parse(map.bpm) / 100.0 * 75.0)).ToString();
                map.total_length = Math.Truncate(((double.Parse(map.total_length) / 100.0 * 133.0))).ToString();
                map.hit_length = Math.Truncate(((double.Parse(map.hit_length) / 100.0 * 133.0))).ToString();
                map.diff_size += " (Non HT)";
                map.diff_approach += " (Non HT)";
                map.diff_drain += " (Non HT)";
                map.diff_overall += " (Non HT)";
            }
        }


        public void OnMemberRemoved(GuildMemberRemoveEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes newly recieved discord messages
        /// </summary>
        /// <param name="messageArgs">discord message</param>
        public void OnMessageRecieved(MessageCreateEventArgs messageArgs)
        {
            Task.Run(() =>
            {
                string channel = (messageArgs.Channel == null || string.IsNullOrEmpty(messageArgs.Channel.Name) ? "null" : messageArgs.Channel.Name);

                //Ignore web-changelog
                if (messageArgs.Channel.Id == 651838498868953099)
                    return;

                string logMsg = $"MessageHandler: New message: Channel: {channel}: User: {messageArgs.Author.Username}: {messageArgs.Message.Content}";

                Logger.Log(logMsg);

                string embed = "";
                if (messageArgs.Message.Embeds != null && messageArgs.Message.Embeds.Count > 0)
                {
                    for (int i = 0; i < messageArgs.Message.Embeds.Count; i++)
                    {
                        DiscordEmbed emb = messageArgs.Message.Embeds.ElementAt(i);

                        embed += $"ID: {i}: Title: {emb.Title ?? "null"}, Description: {emb.Description ?? "null"}";

                        if (emb.Fields != null)
                            foreach (var field in emb.Fields)
                                embed += Environment.NewLine + $"Title: {field.Name ?? "null"}, Description: {field.Value ?? "null"}, Inline: {field.Inline}";
                    }
                }

                if (!string.IsNullOrEmpty(embed))
                    Logger.Log("Embeds: " + embed);

                Register(messageArgs.Author);

                string message = messageArgs.Message.Content;

                if (messageArgs.Channel.Id == (ulong)Program.Config.AnalyzeChannel)
                    StartAnalyzer(messageArgs);

                BotUsers buser;

                using (GAFContext context = new GAFContext())
                    buser = context.BotUsers.FirstOrDefault(b => (ulong)b.DiscordId.Value == messageArgs.Author.Id);

                //Hoaq == 154605183714852864
                if (buser.DiscordId == 154605183714852864 && messageArgs.Message.Content.StartsWith("hiss~"))
                {
                    Coding.Discord.SendMessage(messageArgs.Channel.Id, "https://media.tenor.com/images/bebeb96736fc75a7e1b0bb1a1e9b0359/tenor.gif");
                    return;
                }

                if ((AccessLevel)buser.AccessLevel >= AccessLevel.Admin && messageArgs.Message.Content.StartsWith("d!", StringComparison.CurrentCultureIgnoreCase))
                {
                    StartAnalyzer(messageArgs, false);
                    return;
                }

                if (!char.IsLetterOrDigit(message[0]))
                    Task.Run(() => Program.CommandHandler.ActivateCommand(messageArgs.Message, (AccessLevel)buser.AccessLevel));
            });
        }

        /// <summary>
        /// Invoked when a user joins the guild
        /// </summary>
        public void OnUserJoinedGuild(GuildMemberAddEventArgs args)
        {
            Logger.Log($"MessageHandler: User joined guild: {args.Member.Id} {args.Member.DisplayName}");

            using (GAFContext context = new GAFContext())
            {
                var buser = context.BotUsers.FirstOrDefault(b => (ulong)b.DiscordId.Value == args.Member.Id);

                if (buser != null)
                {
                    if (buser.IsVerified)
                        Coding.Discord.AssignRole((ulong)buser.DiscordId, (ulong)Program.Config.DiscordGuildId, (ulong)Program.Config.VerifiedRoleId);
                }
            }

            if (!string.IsNullOrEmpty(Program.Config.WelcomeMessage) && Program.Config.WelcomeChannel != 0)
                WelcomeMessage((ulong)Program.Config.WelcomeChannel, Program.Config.WelcomeMessage, args.Member.Mention);

            DiscordUser user = Coding.Discord.GetUser(args.Member.Id);
            Register(user, args.Guild.Id);
        }

        /// <summary>
        /// registers a new user
        /// </summary>
        /// <param name="duser"></param>
        /// <param name="guildId"></param>
        public void Register(DiscordUser duser, ulong guildId = 0)
        {
            BotUsers buser;

            using (GAFContext context = new GAFContext())
                buser = context.BotUsers.FirstOrDefault(b => (ulong)b.DiscordId.Value == duser.Id);

            if (buser != null)
            {
                if (!buser.IsVerified || guildId == 0)
                    return;

                DiscordGuild guild = Program.Client.GetGuildAsync(guildId).Result;
                DiscordMember member = guild.GetMemberAsync(duser.Id).Result;
                DiscordRole role = member.Roles.FirstOrDefault(r => r.Id == (ulong)Program.Config.VerifiedRoleId);

                if (role == null)
                {
                    role = guild.GetRole((ulong)Program.Config.VerifiedRoleId);
                    member.GrantRoleAsync(role, "Already verified").Wait();
                }
                else if (role != null && !buser.IsVerified)
                {
                    buser.IsVerified = true;

                    using (GAFContext context = new GAFContext())
                    {
                        context.BotUsers.Update(buser);
                        context.SaveChanges();
                    }
                }
            }

            Logger.Log("MessageHandler: Registering new user " + duser.Username, LogLevel.Trace);

            bool autoVerify = false;

            if (guildId > 0)
            {
                DiscordGuild guild = Program.Client.GetGuildAsync(guildId).Result;
                DiscordMember member = guild.GetMemberAsync(duser.Id).Result;
                foreach (DiscordRole role in member.Roles)
                {
                    if (role.Id == (ulong)Program.Config.VerifiedRoleId)
                    {
                        autoVerify = true;
                        break;
                    }
                }
            }

            BotUsers user = new BotUsers()
            {
                DiscordId = (long)duser.Id,
                AccessLevel = 0,
                IsVerified = autoVerify,
                Points = 0,
                RegisteredOn = DateTime.UtcNow,
                OsuUsername = null
            };

            using (GAFContext context = new GAFContext())
            {
                context.BotUsers.Add(user);
                context.SaveChanges();
            }

            Logger.Log("MessageHandler: User registered", LogLevel.Trace);
        }

        /// <summary>
        /// starts the osu mp analyzer
        /// </summary>
        public void StartAnalyzer(MessageCreateEventArgs args, bool sendToApi = true, bool sendToDatabase = true)
        {
            Task.Run(() =>
            {
                try
                {
                    //<https://osu.ppy.sh/community/matches/53616778> 
                    //<https://osu.ppy.sh/mp/53616778> 

                    GAFBot.Osu.Analyzer analyzer = new GAFBot.Osu.Analyzer();
                    var matchData = analyzer.ParseMatch(args.Message.Content);

                    AnalyzerResult analyzerResult = analyzer.CreateStatistic(matchData.Item1, matchData.Item2);

                    if (analyzerResult == null)
                    {
                        Logger.Log("Failed to create result", LogLevel.ERROR);
                        return;
                    }

                    const string BAN_PATTERN = "bans from";

                    string[] lineSplit = args.Message.Content.Split(new char[] { '\r', '\n' });
                    lineSplit[0] = lineSplit[0].Replace("d!", "");

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
                        List<BotAnalyzerRank> branks = new List<BotAnalyzerRank>();

                        int mvpId = 0;
                        float mvpScore = 0;
                        foreach (Rank r in analyzerResult.Ranks)
                        {
                            branks.Add(new BotAnalyzerRank()
                            {
                                MatchId = analyzerResult.MatchId,
                                Place = r.Place,
                                PlayerOsuId = r.Player.UserId,
                                MvpScore = r.Player.MVPScore
                            });

                            if (mvpScore < r.Player.MVPScore)
                            {
                                mvpScore = r.Player.MVPScore;
                                mvpId = r.Player.UserId;
                            }
                        }

                        foreach (Rank r in analyzerResult.HighestScoresRanking)
                            branks.Find(b => r.Player.UserId == b.PlayerOsuId).PlaceScore = r.Place;

                        foreach (Rank r in analyzerResult.HighestAverageAccuracyRanking)
                            branks.Find(b => r.Player.UserId == b.PlayerOsuId).PlaceAccuracy = r.Place;

                        using (GAFContext context = new GAFContext())
                        {
                            context.BotAnalyzerRank.AddRange(branks);

                            foreach (BanInfo bi in analyzerResult.Bans)
                                context.BotAnalyzerBanInfo.Add(new BotAnalyzerBaninfo()
                                {
                                    MatchId = analyzerResult.MatchId,
                                    Artist = bi.Artist,
                                    Title = bi.Title,
                                    Version = bi.Version,
                                    BannedBy = bi.BannedBy
                                });

                            context.SaveChanges();
                        }

                        BotAnalyzerScore highAccScore = ConvertScore(analyzerResult.HighestAccuracyScore, analyzerResult.MatchId);
                        BotAnalyzerScore highScore = ConvertScore(analyzerResult.HighestScore, analyzerResult.MatchId);

                        EntityEntry<BotAnalyzerScore> highAccScoreEnt;
                        EntityEntry<BotAnalyzerScore> highScoreEnt;
                        using (GAFContext context = new GAFContext())
                        {
                            highAccScoreEnt = context.BotAnalyzerScore.Add(highAccScore);
                            highScoreEnt = context.BotAnalyzerScore.Add(highScore);

                            context.SaveChanges();
                        }

                        BotAnalyzerResult br = new BotAnalyzerResult()
                        {
                            MatchId = analyzerResult.MatchId,
                            Stage = analyzerResult.Stage,
                            MatchName = analyzerResult.MatchName,
                            WinningTeam = analyzerResult.WinningTeam,
                            WinningTeamWins = analyzerResult.WinningTeamWins,
                            WinningTeamColor = (int)analyzerResult.WinningTeamColor,
                            LosingTeam = analyzerResult.LosingTeam,
                            LosingTeamWins = analyzerResult.LosingTeamWins,
                            TimeStamp = analyzerResult.TimeStamp,
                            HighestScoreBeatmapId = (long)analyzerResult.HighestScoreBeatmap.id,
                            HighestScoreOsuId = analyzerResult.HighestScoreUser.UserId,
                            HighestAccuracyBeatmapId = (long)analyzerResult.HighestAccuracyBeatmap.id,
                            HighestAccuracyOsuId = analyzerResult.HighestAccuracyUser.UserId,
                            HighestAccuracyScoreId = highAccScore.Id,
                            HighestScoreId = highScore.Id,
                            MvpUserOsuId = mvpId,
                        };

                        using (GAFContext context = new GAFContext())
                        {
                            context.BotAnalyzerTourneyMatch.Add(new BotAnalyzerTourneyMatches()
                            {
                                Season = Program.Config.CurrentSeason,
                                MatchId = highAccScore.Id
                            });

                            context.BotAnalyzerTourneyMatch.Add(new BotAnalyzerTourneyMatches()
                            {
                                Season = Program.Config.CurrentSeason,
                                MatchId = highScore.Id
                            });

                            context.BotAnalyzerResult.Add(br);
                            context.SaveChanges();
                        }
                    }

                    DiscordColor statsColor = _analyzerColors[Program.Rnd.Next(0, _analyzerColors.Count - 1)];
                    DiscordEmbed statsEmbed = analyzer.CreateStatisticEmbed(analyzerResult, statsColor);

                    args.Channel.SendMessageAsync(embed: statsEmbed).Wait();

                    string winningTeam = analyzerResult.WinningTeam;
                    (string, string) teamNames = analyzerResult.TeamNames;

                    Logger.Log($"Executing OnMatchEnd {teamNames.Item1}, {teamNames.Item2}, {winningTeam}", LogLevel.Trace);
                    Task.Run(() =>
                    {
                        GAFBot.Gambling.Betting.IBettingHandler bettingHandler = GAFBot.Modules.ModuleHandler.Get("betting") as GAFBot.Gambling.Betting.IBettingHandler;
                        bettingHandler?.ResolveBets(teamNames.Item1, teamNames.Item2, winningTeam);
                    });
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.ToString(), LogLevel.ERROR);
                }
            });

            BotAnalyzerScore ConvertScore(OsuHistoryEndPoint.HistoryJson.Score score, int matchId)
            {
                string mods = "";

                if (score.mods != null && score.mods.Count > 0)
                {
                    mods = score.mods[0];

                    for (int i = 1; i < score.mods.Count; i++)
                        mods += "|" + score.mods[i];
                }

                return new BotAnalyzerScore()
                {
                    MatchId = matchId,
                    UserId = score.user_id ?? 0,
                    Accuracy = score.accuracy ?? 0,
                    Mods = mods,
                    Score = score.score ?? 0,
                    MaxCombo = score.max_combo ?? 0,
                    Perfect = score.perfect ?? 0,
                    PP = score.pp ?? 0,
                    Rank = score.rank ?? 0,
                    CreatedAt = score.created_at ?? DateTime.UtcNow,
                    Slot = score.multiplayer.slot ?? 0,
                    Team = score.multiplayer.team,
                    Pass = score.multiplayer.pass ?? 0,
                    Count50 = score.statistics.count_50 ?? 0,
                    Count100 = score.statistics.count_100 ?? 0,
                    Count300 = score.statistics.count_300 ?? 0,
                    CountGeki = score.statistics.count_geki ?? 0,
                    CountKatu = score.statistics.count_katu ?? 0,
                    CountMiss = score.statistics.count_miss ?? 0
                };
            }
        }

        public void WelcomeMessage(ulong channel, string welcomeMessage, string mentionString)
        {
            var welcomeChannel = Coding.Discord.GetChannel(channel);
            if (welcomeChannel == null || string.IsNullOrEmpty(Program.Config.WelcomeMessage))
                return;

            string welcomeFormat = Program.Config.WelcomeMessage;

            if (welcomeFormat.Contains("{mention}", StringComparison.CurrentCultureIgnoreCase))
                welcomeFormat = welcomeFormat.Replace("{mention}", mentionString, StringComparison.CurrentCultureIgnoreCase);

            welcomeChannel.SendMessageAsync(welcomeFormat).Wait();
        }
    }
}
