using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_OsuMPAnalyzer.History_Endpoint
{
    public static class GetData
    {
        public static User GetUser(int userID, History history)
        {
            User result = null;

            foreach (User u in history.Users)
            {
                if (!u.UserId.HasValue)
                    continue;

                if (u.UserId.Value == userID)
                {
                    result = u;
                    break;
                }
            }

            return result;
        }

        public static User GetUser(Score score, History history)
        {
            if (!score.user_id.HasValue)
                return null;

            return GetUser(score.user_id.Value, history);
        }

        public static Game[] GetMatches(History history)
        {
            List<Game> games = new List<Game>();

            foreach (Event ev in history.Events)
            {
                if (ev.Detail.Type == "other")
                    games.Add(ev.Game);
            }

            return games.ToArray();
        }

        public static Event GetMatchCreatedEvent(History history)
            => history.Events[0];

        public static Event GetMatchDisbandedEvents(History history)
            => history.Events[history.Events.Count() - 1];

        public static int CountPlayerLeft(History history)
        {
            int Count = 0;

            foreach (Event ev in history.Events)
                if (ev.Detail.Type == "player-left")
                    Count++;

            return Count;
        }

        public static int CountPlayerJoined(History history)
        {
            int Count = 0;

            foreach (Event ev in history.Events)
                if (ev.Detail.Type == "player-joined")
                    Count++;

            return Count;
        }

        public static int CountPlayerScores(History history)
        {
            int Count = 0;

            foreach (Event ev in history.Events)
                if (ev.Detail.Type == "other")
                    Count++;

            return Count;
        }

        public static History ParseJsonFromString(string Json)
        {
            History history = JsonConvert.DeserializeObject<History>(Json);

            return history;
        }

        public static History ParseJsonFromUrl(string Url)
        {
            System.Net.WebClient WClient = new System.Net.WebClient();
            string dlstring = WClient.DownloadString(Url);

            return ParseJsonFromString(dlstring);
        }
    }

    public class History
    {
        [JsonProperty("events")]
        public Event[] Events { get; internal set; }

        [JsonProperty("users")]
        public History_Endpoint.User[] Users { get; internal set; }

        [JsonProperty("all_events_count")]
        public object EventCount { get; internal set; }
    }

    public class Event
    {
        [JsonProperty("id")]
        public int? EventId { get; internal set; }

        [JsonProperty("detail")]
        public Detail Detail { get; internal set; }

        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; internal set; }

        [JsonProperty("user_id")]
        public int? UserId { get; internal set; }
        [JsonProperty("game")]
        public Game Game { get; internal set; }
    }

    public class Game
    {
        [JsonProperty("start_time")]
        public DateTime start_time { get; internal set; }
        [JsonProperty("end_time")]
        public DateTime? end_time { get; internal set; }
        [JsonProperty("mode")]
        public string mode { get; internal set; }
        [JsonProperty("mode_int")]
        public int? mode_int { get; internal set; }
        [JsonProperty("scoring_type")]
        public string scoring_type { get; internal set; }
        [JsonProperty("team_type")]
        public string team_type { get; internal set; }
        [JsonProperty("mods")]
        public List<string> mods { get; internal set; }
        [JsonProperty("beatmap")]
        public BeatMap beatmap { get; internal set; }
        [JsonProperty("scores")]
        public List<Score> scores { get; internal set; }
    }

    public class Score
    {
        [JsonProperty("user_id")]
        public int? user_id { get; internal set; }
        [JsonProperty("accuracy")]
        public float? accuracy { get; internal set; }
        [JsonProperty("mods")]
        public List<string> mods { get; internal set; }
        [JsonProperty("score")]
        public int? score { get; internal set; }
        [JsonProperty("max_combo")]
        public int? max_combo { get; internal set; }
        [JsonProperty("perfect")]
        public int? perfect { get; internal set; }
        [JsonProperty("statistics")]
        public Statistics statistics { get; internal set; }
        [JsonProperty("pp")]
        public float? pp { get; internal set; }
        [JsonProperty("rank")]
        public int? rank { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime? created_at { get; internal set; }

    }

    public class Multiplayer
    {
        [JsonProperty("slot")]
        public int? slot { get; internal set; }
        [JsonProperty("team")]
        public int? team { get; internal set; }
        [JsonProperty("pass")]
        public int? pass { get; internal set; }
    }

    public class Statistics
    {
        [JsonProperty("count_50")]
        public int? count_50 { get; internal set; }
        [JsonProperty("count_100")]
        public int? count_100 { get; internal set; }
        [JsonProperty("count_300")]
        public int? count_300 { get; internal set; }
        [JsonProperty("count_geki")]
        public int? count_geki { get; internal set; }
        [JsonProperty("count_katu")]
        public int? count_katu { get; internal set; }
        [JsonProperty("count_miss")]
        public int? count_miss { get; internal set; }
    }

    public class BeatMap
    {
        public int? id { get; internal set; }
        public string mode { get; internal set; }
        public double difficulty_rating { get; internal set; }
        public string version { get; internal set; }
        public BeatMapSet beatmapset { get; internal set; }
    }

    public class BeatMapSet
    {
        [JsonProperty("id")]
        public int? id { get; internal set; }
        [JsonProperty("title")]
        public string title { get; internal set; }
        [JsonProperty("artist")]
        public string artist { get; internal set; }
        [JsonProperty("creator")]
        public string creator { get; internal set; }
        [JsonProperty("user_id")]
        public int? user_id { get; internal set; }
        [JsonProperty("covers")]
        public Covers covers { get; internal set; }
        [JsonProperty("favourite_count")]
        public int? favourite_count { get; internal set; }
        [JsonProperty("play_count")]
        public int? play_count { get; internal set; }
        [JsonProperty("preview_url")]
        public string preview_url { get; internal set; }
        [JsonProperty("video")]
        public bool video { get; internal set; }
        [JsonProperty("source")]
        public string source { get; internal set; }
        [JsonProperty("status")]
        public string status { get; internal set; }
    }

    public class Covers
    {
        [JsonProperty("cover")]
        public string cover { get; internal set; }
        [JsonProperty(@"cover@2x")]
        public string cover2x { get; internal set; }
        [JsonProperty("card")]
        public string card { get; internal set; }
        [JsonProperty("card@2x")]
        public string card2x { get; internal set; }
        [JsonProperty("list")]
        public string list { get; internal set; }
        [JsonProperty("list@2x")]
        public string list2x { get; internal set; }
        [JsonProperty("slimcover")]
        public string slimcover { get; internal set; }
        [JsonProperty("slimcover@2x")]
        public string slimcover2x { get; internal set; }
    }

    public class Detail
    {
        [JsonProperty("type")]
        public string Type { get; internal set; }

        [JsonProperty("text")]
        public string MatchName { get; internal set; }
    }

    public class User
    {
        [JsonProperty("id")]
        public int? UserId { get; internal set; }

        [JsonProperty("username")]
        public string Username { get; internal set; }

        [JsonProperty("profile_colour")]
        public object ProfileColour { get; internal set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; internal set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; internal set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; internal set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; internal set; }

        [JsonProperty("is_online")]
        public bool IsOnline { get; internal set; }

        [JsonProperty("is_supporter")]
        public bool IsSupporter { get; internal set; }

        [JsonProperty("country")]
        public Country Country { get; internal set; }
    }

    public class Country
    {
        [JsonProperty("code")]
        public string Code { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }
    }
}
