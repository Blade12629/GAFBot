using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using GAFBot.Osu.results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAFBot.MessageSystem
{
    public class MessageHandler
    {

        public MessageHandler()
        {
            Users = new ConcurrentDictionary<ulong, User>();
            Program.SaveEvent += () => SaveUsers(Program.UserFile);
        }
        
        /// <summary>
        /// DiscordUserId, User
        /// </summary>
        public ConcurrentDictionary<ulong, User> Users { get; private set; }
        public Logger Logger { get { return Program.Logger; } }

        /// <summary>
        /// Invoked if there is a new message on discord
        /// </summary>
        /// <param name="messageArgs"></param>
        public void NewMessage(MessageCreateEventArgs messageArgs)
        {
            Task.Run(() =>
            {
                Logger.Log($"MessageHandler: New message: User: {messageArgs.Author.Username}: {messageArgs.Message.Content}");
                Register(messageArgs.Author);

                User user = Users[messageArgs.Author.Id];

                string message = messageArgs.Message.Content;
                
                if (messageArgs.Channel.Id == Program.Config.AnalyzeChannel)
                    StartAnalyzer(messageArgs);
                else if (Program.Config.DefaultDiscordAdmins.Contains(messageArgs.Author.Id) && messageArgs.Message.Content.StartsWith("d!", StringComparison.CurrentCultureIgnoreCase))
                    StartAnalyzer(messageArgs);

                if (!char.IsLetterOrDigit(message[0]))
                    Task.Run(() => Program.CommandHandler.ActivateCommand(messageArgs.Message, GetAccessLevel(messageArgs.Author.Id)));
            });
        }

        /// <summary>
        /// Invoked when a user joins the guild
        /// </summary>
        /// <param name="args"></param>
        public void OnUserJoinedGuild(GuildMemberAddEventArgs args)
        {
            Logger.Log($"MessageHandler: User joined guild: {args.Member.Id} {args.Member.DisplayName}");

            if (Users.ContainsKey(args.Member.Id))
            {
                User user = null;

                while (!Users.TryGetValue(args.Member.Id, out user))
                    Task.Delay(5).Wait();

                if (user.Verified)
                    Coding.Methods.AssignRole(user.DiscordID, Program.Config.DiscordGuildId, Program.Config.VerifiedRoleId);
            }
        }

        /// <summary>
        /// Invoked when a user leaves/get kicked/banned from the guild
        /// </summary>
        /// <param name="args"></param>
        public void OnMemberRemoved(GuildMemberRemoveEventArgs args)
        {
            Logger.Log($"MessageHandler: User removed from guild: {args.Member.Id} {args.Member.DisplayName}");
        }

        /// <summary>
        /// Gets the access level for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public AccessLevel GetAccessLevel(ulong user)
            => Users.ContainsKey(user) ? Users[user].AccessLevel : AccessLevel.User;

        public delegate void MatchEndDel(string teamA, string teamB, string winningTeam);
        public static event MatchEndDel OnMatchEnd;

        /// <summary>
        /// fake triggers a match end to the betting handler
        /// </summary>
        /// <param name="teama"></param>
        /// <param name="teamb"></param>
        /// <param name="winningteam"></param>
        public void FakeTrigger(string teama, string teamb, string winningteam)
        {
            OnMatchEnd(teama, teamb, winningteam);
        }

        /// <summary>
        /// starts the osu mp analyzer
        /// </summary>
        /// <param name="args"></param>
        public void StartAnalyzer(MessageCreateEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    string winningTeam = "";
                    (string, string) teamNames = ("", "");

                    Osu.Analyzer analyzer = new Osu.Analyzer();
                    DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder();

                    //Need to do - qualifier
                    if (Program.Config.QualifierStage)
                    {
                        QualifierStageResult qualifierResult = analyzer.CreateQualifierStatistics(analyzer.ParseMatch(args.Message.Content));
                        discordEmbedBuilder = new DiscordEmbedBuilder()
                        {
                            Title = qualifierResult.MatchName,
                            Footer = new DiscordEmbedBuilder.EmbedFooter()
                            {
                                Text = "match played at " + DateTime.Now.ToString()
                            }
                        };

                        discordEmbedBuilder.AddField("Highest Score", string.Format("{0} - {1} [{2}] ({3}*) with {4:n0} Points and {5}% Accuracy!", qualifierResult.HighestScoreBeatmap.beatmapset.artist,
                                                                                                                                                     qualifierResult.HighestScoreBeatmap.beatmapset.title, qualifierResult.HighestScoreBeatmap.version,
                                                                                                                                                     qualifierResult.HighestScoreBeatmap.difficulty_rating,
                                                                                                                                                     string.Format("{0:n0}", qualifierResult.HighestScore.score),
                                                                                                                                                     qualifierResult.HighestScore.accuracy.Value * 100.0f));
                        for (int i = 1; i < 4; i++)
                        {
                            Rank place = qualifierResult.HighestScoresRanking.First(ob => ob.Place == i);

                            switch (place.Place)
                            {
                                case 1:
                                    discordEmbedBuilder.AddField("First Place", string.Format("HighestScore: {0:n0} Acc: {1}%{2}", place.Player.UserName, place.Player.HighestScore.score, place.Player.AverageAccuracy));
                                    break;
                                case 2:
                                    discordEmbedBuilder.AddField("Second Place", string.Format("HighestScore: {0:n0} Acc: {1}%{2}", place.Player.UserName, place.Player.HighestScore.score, place.Player.AverageAccuracy));
                                    break;
                                case 3:
                                    discordEmbedBuilder.AddField("Third Place", string.Format("HighestScore: {0:n0} Acc: {1}%{2}", place.Player.UserName, place.Player.HighestScore.score, place.Player.AverageAccuracy));
                                    break;
                            }
                        }

                        teamNames = qualifierResult.TeamNames;
                    }
                    else
                    {
                        AnalyzerResult analyzerResult = analyzer.CreateStatistic(analyzer.ParseMatch(args.Message.Content));
                        string description = "";

                        if (analyzerResult.Win == Osu.WinType.Blue)
                            description = string.Format("Team {0} won! ({1}:{2})", analyzerResult.WinningTeam, analyzerResult.WinningTeamWins, analyzerResult.LosingTeamWins);
                        else if (analyzerResult.Win == Osu.WinType.Red)
                            description = string.Format("Team {0} won! ({1}:{2})", analyzerResult.WinningTeam, analyzerResult.WinningTeamWins, analyzerResult.LosingTeamWins);
                        else if (analyzerResult.Win == Osu.WinType.Draw)
                            description = string.Format("It's a draw! ({0} {1} : {2} {3})", analyzerResult.WinningTeam, analyzerResult.WinningTeamWins, analyzerResult.LosingTeamWins, analyzerResult.LosingTeam);

                        discordEmbedBuilder = new DiscordEmbedBuilder()
                        {
                            Title = analyzerResult.MatchName,
                            Description = description,
                            Footer = new DiscordEmbedBuilder.EmbedFooter()
                            {
                                Text = "Match played at " + analyzerResult.TimeStamp,
                            }
                        };

                        if (analyzerResult.MostPlayedBeatmap.Count > 1)
                        {
                            discordEmbedBuilder.AddField("Most Played Beatmap", string.Format("{0} - {1} [{2}] ({3}*) Playcount: {4}",
                                analyzerResult.MostPlayedBeatmap.BeatMap.beatmapset.artist, analyzerResult.MostPlayedBeatmap.BeatMap.beatmapset.title,
                                analyzerResult.MostPlayedBeatmap.BeatMap.version, analyzerResult.MostPlayedBeatmap.BeatMap.difficulty_rating,
                                analyzerResult.MostPlayedBeatmap.Count));
                        }

                        discordEmbedBuilder.AddField("Highest Score", string.Format("{0} on the map {1} - {2} [{3}] ({4}*) with {5:n0} Points and {6}% Accuracy!",
                            analyzerResult.HighestScoreUser.UserName, analyzerResult.HighestScoreBeatmap.beatmapset.artist,
                            analyzerResult.HighestScoreBeatmap.beatmapset.title, analyzerResult.HighestScoreBeatmap.version,
                            analyzerResult.HighestScoreBeatmap.difficulty_rating,
                            string.Format("{0:n0}", analyzerResult.HighestScoreUser.HighestScore.score),
                            Math.Round(analyzerResult.HighestScoreUser.HighestScore.accuracy.Value * 100.0f, 2, MidpointRounding.AwayFromZero)));

                        discordEmbedBuilder.AddField("Highest Acc", string.Format("{0} on the map {1} - {2} [{3}] ({4}*) with {5:n0} Points and {6}% Accuracy!",
                            analyzerResult.HighestAverageAccuracyUser.UserName, analyzerResult.HighestAccuracyBeatmap.beatmapset.artist,
                            analyzerResult.HighestAccuracyBeatmap.beatmapset.title, analyzerResult.HighestAccuracyBeatmap.version,
                            analyzerResult.HighestAccuracyBeatmap.difficulty_rating,
                            string.Format("{0:n0}", analyzerResult.HighestAccuracyScore.score),
                            Math.Round(analyzerResult.HighestAccuracyScore.accuracy.Value * 100.0f, 2, MidpointRounding.AwayFromZero)));

                        for (int i = 1; i < 4; i++)
                        {
                            Rank place = analyzerResult.HighestAverageAccuracyRanking.Last(ob => ob.Place == i);
                            (string, string) placeString = GetPlaceString(place);
                            discordEmbedBuilder.AddField(placeString.Item1, placeString.Item2);
                        }
                        teamNames = analyzerResult.TeamNames;
                        winningTeam = analyzerResult.WinningTeam;
                    }

                    args.Channel.SendMessageAsync(embed: discordEmbedBuilder.Build()).Wait();
                    
                    (string, string) GetPlaceString(Rank place)
                    {
                        switch (place.Place)
                        {
                            case 1:
                                return ("First Place", $"{ place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                            case 2:
                                return ("Second Place", $"{ place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                            case 3:
                                return ("Third Place", $"{place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                            case 4:
                                return ("Fourth Place", $"{place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                            default:
                                return ($"{place.Place} Place", $"{ place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                        }
                    }
                    if (string.IsNullOrEmpty(winningTeam) || string.IsNullOrEmpty(teamNames.Item1) || string.IsNullOrEmpty(teamNames.Item2))
                        return;

                   Task.Run(() => OnMatchEnd(teamNames.Item1, teamNames.Item2, winningTeam));
                }
                catch (Exception ex)
                {
                    Logger.Log("MessageHandler: " + ex.ToString());
                }
            });
        }

        /// <summary>
        /// registers a new user
        /// </summary>
        /// <param name="duser"></param>
        /// <param name="guildId"></param>
        public void Register(DiscordUser duser, ulong guildId = 0)
        {
            Logger.Log("MessageHandler: Trying to register new user " + duser.Username, showConsole: Program.Config.Debug);
            if (Users.ContainsKey(duser.Id))
            {
                Logger.Log("MessageHandler: User already registered " + duser.Username, showConsole: Program.Config.Debug);
                return;
            }
            bool autoVerify = false;

            if (guildId > 0)
            {
                DiscordGuild guild = Program.Client.GetGuildAsync(guildId).Result;
                DiscordMember member = guild.GetMemberAsync(duser.Id).Result;
                foreach (DiscordRole role in member.Roles)
                {
                    if (role.Id == Program.Config.VerifiedRoleId)
                    {
                        autoVerify = true;
                        break;
                    }

                }
            }

            User user = new User(duser.Id, 0, AccessLevel.User, DateTime.UtcNow, autoVerify);
            if (Program.Config.DefaultDiscordAdmins.Contains(duser.Id))
            {
                user.AccessLevel = AccessLevel.Admin;

            }
            
            while (!Users.TryAdd(duser.Id, user))
                Task.Delay(5);

            Logger.Log("MessageHandler: User registered", showConsole: Program.Config.Debug);
        }

        /// <summary>
        /// Loads users
        /// </summary>
        /// <param name="file"></param>
        public void LoadUsers(string file)
        {
            bool save = false;

            List<User> users = null;

            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                users = JsonConvert.DeserializeObject<List<User>>(json);
            }

            if (users == null || users.Count == 0)
            {
                users = new List<User>();
                users.Add(new User(140896783717892097, -50000, AccessLevel.Admin, DateTime.Now, true));
                save = true;
            }

            lock(Users)
            {
                users.ForEach(u =>
                {
                    while (!Users.TryAdd(u.DiscordID, u))
                        Task.Delay(5);
                });
            }

            if (save)
                SaveUsers(file);
        }

        /// <summary>
        /// Save users
        /// </summary>
        /// <param name="file"></param>

        public void SaveUsers(string file)
        {
            string json = "";
            
            lock(Users)
            {
                json = JsonConvert.SerializeObject(Users.Values.ToList());
            }

            File.WriteAllText(file, json);
        }
    }
}
