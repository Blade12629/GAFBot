using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAFBot.API.Json
{
    public enum WinType
    {
        Red = 1,
        Blue = 2,
        Draw = 3,
    }
    public enum TeamColor
    {
        None,
        Red,
        Blue
    }
    public enum Mods
    {
        None = 0,
        NoFail = 1,
        Easy = 2,
        TouchDevice = 4,
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
        Flashlight = 1024,
        Autoplay = 2048,
        SpunOut = 4096,
        Relax2 = 8192,    // Autopilot
        Perfect = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
        Key4 = 32768,
        Key5 = 65536,
        Key6 = 131072,
        Key7 = 262144,
        Key8 = 524288,
        FadeIn = 1048576,
        Random = 2097152,
        Cinema = 4194304,
        Target = 8388608,
        Key9 = 16777216,
        KeyCoop = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456,
        ScoreV2 = 536870912,
        Mirror = 1073741824,
        KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod,
        ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
    }

    public class SendOsuResult
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("stage")]
        public string Stage { get; set; }
        [JsonProperty("matchName")]
        public string MatchName { get; set; }
        [JsonProperty("teams")]
        public TeamInfo[] Teams { get; set; }
        [JsonProperty("beatmaps")]
        public BeatmapInfo[] Beatmaps { get; set; }
        [JsonProperty("players")]
        public PlayerInfo[] Players { get; set; }

        [JsonProperty("bans")]
        public BanInfo[] Bans { get; set; }

        [JsonProperty("highestScore")]
        public HighestRankingInfo HighestScore { get; set; }
        [JsonProperty("highestAccuracy")]
        public HighestRankingInfo HighestAccuracy { get; set; }

        [JsonProperty("mvpPlayerId")]
        public int MVPPlayerID { get; set; }

        [JsonProperty("timeStamp")]
        public DateTime TimeStamp { get; set; }

        public SendOsuResult(int iD, string matchName, TeamInfo[] teams, BeatmapInfo[] beatmaps, PlayerInfo[] players, HighestRankingInfo highestScore, HighestRankingInfo highestAccuracy, int mVPPlayerID, DateTime timeStamp)
        {
            try
            {
                ID = iD;
                MatchName = matchName;
                Teams = teams;
                Beatmaps = beatmaps;
                Players = players;
                HighestScore = highestScore;
                HighestAccuracy = highestAccuracy;
                MVPPlayerID = mVPPlayerID;
                TimeStamp = timeStamp;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
                throw ex;
            }
        }

        public SendOsuResult()
        {
        }

        public static SendOsuResult CreateTestResult()
        {
            TeamInfo[] teams = new TeamInfo[]
            {
                new TeamInfo(3, (short)TeamColor.Blue, "blue"),
                new TeamInfo(2, (short)TeamColor.Red, "red")
            };

            BeatmapInfo[] maps = new BeatmapInfo[]
            {
                new BeatmapInfo(1, 10, 6.5, "Artist", "title", "version", "creator"),
                new BeatmapInfo(2, 11, 7.58, "Artist2", "title2", "version2", "creator2"),
            };

            PlayerInfo[] players = new PlayerInfo[]
            {
                new PlayerInfo(1, "user1", 30.05f, 99.34568f, 99.34f),
                new PlayerInfo(2, "user2", 30.05f, 99.34568f, 99.34f),
            };

            ScoreInfo[] scores = new ScoreInfo[]
            {
                new ScoreInfo(1, 1, (short)TeamColor.Blue, 99.0f, new string[] { "ez", "hr" }, 500, 5, 0, 50.0f, 1, DateTime.UtcNow, 30, 20, 10, 5),
                new ScoreInfo(2, 1, (short)TeamColor.Blue, 99.0f, new string[] { "ez", "hr" }, 500, 5, 0, 50.0f, 1, DateTime.UtcNow, 30, 20, 10, 5),
                new ScoreInfo(1, 1, (short)TeamColor.Blue, 99.0f, new string[] { "ez", "hr" }, 500, 5, 0, 50.0f, 1, DateTime.UtcNow, 30, 20, 10, 5),
                new ScoreInfo(1, 1, (short)TeamColor.Blue, 99.0f, new string[] { "ez", "hr" }, 500, 5, 0, 50.0f, 1, DateTime.UtcNow, 30, 20, 10, 5),
            };

            SendOsuResult r = new SendOsuResult(1, "GAF:(blue) vs (red)", teams, maps, players, new HighestRankingInfo(1, scores[0], scores[0].PlayerID), new HighestRankingInfo(2, scores[1], scores[1].PlayerID), 1, DateTime.UtcNow);
            
            return r;
        }
    }

    public class BanInfo : JsonFile<BanInfo>
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("bannedBy")]
        public string BannedBy { get; set; }

        public BanInfo()
        {

        }

        public BanInfo(string artist, string title, string version, string bannedBy)
        {
            Artist = artist;
            Title = title;
            Version = version;
            BannedBy = bannedBy;
        }
    }

    public class PlayerInfo : JsonFile<PlayerInfo>
    {
        [JsonProperty("osuId")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mvpScore")]
        public float MVPScore { get; set; }
        [JsonProperty("averageAccuracy")]
        public float AverageAccuracy { get; set; }
        [JsonProperty("averageAccuracyRounded")]
        public float AverageAccuracyRounded { get; set; }

        public PlayerInfo(int iD, string name, float mVPScore, float averageAccuracy, float averageAccuracyRounded)
        {
            ID = iD;
            Name = name;
            MVPScore = mVPScore;
            AverageAccuracy = averageAccuracy;
            AverageAccuracyRounded = averageAccuracyRounded;
        }
    }

    public class ScoreInfo : JsonFile<ScoreInfo>
    {
        [JsonProperty("playerId")]
        public int PlayerID { get; set; }
        [JsonProperty("mpSlot")]
        public short MPSlot { get; set; }
        [JsonProperty("mpTeam")]
        public short MPTeam { get; set; }
        [JsonProperty("accuracy")]
        public float Accuracy { get; set; }
        [JsonProperty("mods")]
        public string[] Mods { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("maxCombo")]
        public int MaxCombo { get; set; }
        [JsonProperty("perfect")]
        public short Perfect { get; set; }
        [JsonProperty("pp")]
        public float PP { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("count300")]
        public int Count300 { get; set; }
        [JsonProperty("count100")]
        public int Count100 { get; set; }
        [JsonProperty("count50")]
        public int Count50 { get; set; }
        [JsonProperty("countMiss")]
        public int CountMiss { get; set; }

        public ScoreInfo(int playerID, short mPSlot, short mPTeam, float accuracy, string[] mods, int score, int maxCombo, short perfect, float pP, int rank, DateTime createdAt, int count300, int count100, int count50, int countMiss)
        {
            PlayerID = playerID;
            MPSlot = mPSlot;
            MPTeam = mPTeam;
            Accuracy = accuracy;
            Mods = mods;
            Score = score;
            MaxCombo = maxCombo;
            Perfect = perfect;
            PP = pP;
            Rank = rank;
            CreatedAt = createdAt;
            Count300 = count300;
            Count100 = count100;
            Count50 = count50;
            CountMiss = countMiss;
        }

        public ScoreInfo()
        {
        }
    }

    public class HighestRankingInfo : JsonFile<HighestRankingInfo>
    {
        [JsonProperty("beatmapId")]
        public int MapID { get; set; }
        [JsonProperty("score")]
        public ScoreInfo Score { get; set; }
        [JsonProperty("playerId")]
        public int PlayerID { get; set; }

        public HighestRankingInfo(int mapID, ScoreInfo score, int playerID)
        {
            MapID = mapID;
            Score = score;
            PlayerID = playerID;
        }

        public HighestRankingInfo()
        {
        }
    }

    //public class RankingInfo : JsonFile<RankingInfo>
    //{
    //    [JsonProperty("")]
    //    public int PlayerID { get; set; }
    //    [JsonProperty("")]
    //    public short Place { get; set; }

    //    public RankingInfo(int playerID, short place)
    //    {
    //        PlayerID = playerID;
    //        Place = place;
    //    }

    //    public RankingInfo()
    //    {
    //    }
    //}

    public class BeatmapInfo : JsonFile<BeatmapInfo>
    {
        [JsonProperty("beatmapId")]
        public int ID { get; set; }
        [JsonProperty("mapsetId")]
        public int SetID { get; set; }
        [JsonProperty("difficulty")]
        public double Difficulty { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("creator")]
        public string Creator { get; set; }

        public BeatmapInfo(int iD, int setID, double difficulty, string artist, string title, string version, string creator)
        {
            ID = iD;
            SetID = setID;
            Difficulty = difficulty;
            Artist = artist;
            Title = title;
            Version = version;
            Creator = creator;
        }

        public BeatmapInfo()
        {
        }
    }

    public class TeamInfo : JsonFile<TeamInfo>
    {
        [JsonProperty("teamName")]
        public string TeamName { get; set; }
        [JsonProperty("wins")]
        public short Wins { get; set; }
        [JsonProperty("teamColor")]
        public short TeamColor { get; set; }

        public TeamInfo(short wins, short teamColor, string teamName)
        {
            TeamName = teamName;
            Wins = wins;
            TeamColor = teamColor;
        }

        public TeamInfo()
        {

        }
    }

    public static class JsonResultConverter
    {
        public static SendOsuResult ConvertSendOsuResult(Osu.results.AnalyzerResult ar)
        {
            try
            {
                BeatmapInfo[] maps = new BeatmapInfo[ar.Beatmaps.Length];

                for (int i = 0; i < ar.Beatmaps.Length; i++)
                    maps[i] = new BeatmapInfo(ar.Beatmaps[i].id ?? -1, ar.Beatmaps[i].beatmapset.id ?? -1, ar.Beatmaps[i].difficulty_rating,
                                              ar.Beatmaps[i].beatmapset.artist ?? "null", ar.Beatmaps[i].beatmapset.title ?? "null", ar.Beatmaps[i].version ?? "null", ar.Beatmaps[i].beatmapset.creator ?? "null");

                List<ScoreInfo> scoresL = new List<ScoreInfo>();
                PlayerInfo[] players = new PlayerInfo[ar.Ranks.Length];
                ScoreInfo hScore = new ScoreInfo()
                {
                    Score = -1
                };
                ScoreInfo hAcc = new ScoreInfo()
                {
                    Accuracy = -1.0f
                };
                PlayerInfo mvp = new PlayerInfo(0, "", -1, 0.0f, 0.0f);
                //RankingInfo[] rankings = new RankingInfo[ar.Ranks.Length];

                ScoreInfo sI;
                for (int i = 0; i < ar.Ranks.Length; i++)
                {
                    var rank = ar.Ranks[i];

                    players[i] = new PlayerInfo(rank.Player.UserId, rank.Player.UserName, rank.Player.MVPScore,
                                                rank.Player.AverageAccuracy, rank.Player.AverageAccuracyRounded);

                    if (players[i].MVPScore > mvp.MVPScore)
                        mvp = players[i];

                    //rankings[i] = new RankingInfo(rank.Player.UserId, (short)rank.Place);

                    for (int x = 0; x < rank.Player.Scores.Length; x++)
                    {
                        var score = rank.Player.Scores[x];
                        short slot = score.multiplayer == null ? (short)-1 : !score.multiplayer.slot.HasValue ? (short)-1 : (short)score.multiplayer.slot.Value;
                        short team = score.multiplayer.team.Equals("red", StringComparison.CurrentCultureIgnoreCase) ? (short)1 : (short)2;

                        sI = new ScoreInfo(score.user_id ?? 0, slot, team, (score.accuracy ?? 0.0f) * 100.0f, score.mods?.ToArray() ?? new string[] { }, score.score ?? 0,
                                           score.max_combo ?? 0, (short)(score.perfect ?? 0), score.pp ?? 0, score.rank ?? 0, score.created_at ?? DateTime.MinValue, score.statistics?.count_300 ?? 0,
                                           score.statistics?.count_100 ?? 0, score.statistics?.count_50 ?? 0, score.statistics?.count_miss ?? 0);

                        scoresL.Add(sI);

                        if (hScore.Score < sI.Score)
                            hScore = sI;

                        if (hAcc.Accuracy < sI.Accuracy)
                            hAcc = sI;
                    }
                }


                HighestRankingInfo highestScore = new HighestRankingInfo(ar.HighestScoreBeatmap.id.Value, hScore, ar.HighestScore.user_id.Value);

                HighestRankingInfo highestAccuracy = new HighestRankingInfo(ar.HighestAccuracyBeatmap.id.Value, hAcc, ar.HighestAccuracyScore.user_id.Value);

                ScoreInfo[] scores = new ScoreInfo[scoresL.Count];
                scoresL.CopyTo(scores);

                TeamInfo[] teamInfos = new TeamInfo[2];

                if (ar.WinningTeamColor == Osu.TeamColor.Red)
                {
                    teamInfos[0] = new TeamInfo((short)ar.WinningTeamWins, (short)TeamColor.Red, ar.WinningTeam);
                    teamInfos[1] = new TeamInfo((short)ar.LosingTeamWins, (short)TeamColor.Blue, ar.LosingTeam);
                }
                else
                {
                    teamInfos[0] = new TeamInfo((short)ar.LosingTeamWins, (short)TeamColor.Red, ar.LosingTeam);
                    teamInfos[1] = new TeamInfo((short)ar.WinningTeamWins, (short)TeamColor.Blue, ar.WinningTeam);
                }

                SendOsuResult result = new SendOsuResult(ar.MatchId, ar.MatchName, teamInfos, maps, players, highestScore,
                                                         highestAccuracy, mvp.ID, ar.TimeStamp);

                List<BanInfo> bans = new List<BanInfo>();

                foreach (Osu.results.BanInfo ban in ar.Bans)
                    bans.Add(new BanInfo(ban.Artist, ban.Title, ban.Version, ban.BannedBy));

                result.Bans = bans.ToArray();
                result.Stage = ar.Stage;

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
                throw ex;
            }
        }

        public static SendOsuResult ToSendOsuResult(this Osu.results.AnalyzerResult ar)
        {
            return ConvertSendOsuResult(ar);
        }
    }

    public abstract class JsonFile<T>
    {
        public virtual string GetJsonString(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public virtual T GetJsonObject(string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }
    }
}
