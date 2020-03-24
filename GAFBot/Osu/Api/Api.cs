using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GAFBot.Osu.Api
{
    public class Api
    {
        private static string API_Key { get { return Program.DecryptString(Program.Config.OsuApiKeyEncrypted); } }
        private static string API_URL = "https://osu.ppy.sh/api/";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="setid"></param>
        /// <returns></returns>
        public static Json_Get_Beatmaps GetBeatmaps(int id = -1, int setid = -1, int mods = 0)
        {
            Console.WriteLine("API: Download initiated for id/setid/mods: " + id + "/" + setid + "/" + mods);

            if (id > 0 && setid > 0 || id < 1 && setid < 1)
                return null;
            
            string idstring = "";

            if (id > 0)
                idstring = "&b=" + id;
            else
                idstring = "&s=" + setid;
            
            string url = string.Format("{0}get_beatmaps?k={1}{2}&m=0&limit=30&mods={3}", API_URL, API_Key, idstring, mods);

            Console.WriteLine("API: Download from " + url);

            string jsonInput = null;

            using (WebClient webClient = new WebClient())
            {
                jsonInput = webClient.DownloadString(url);
            }

            if (string.IsNullOrEmpty(jsonInput))
                return null;

            return new Json_Get_Beatmaps() { Beatmaps = Newtonsoft.Json.JsonConvert.DeserializeObject<Json_Get_Beatmaps.Beatmap[]>(jsonInput) };
        }

        // Parameters: k* api key, mp* matchid | * = required
        /// <summary>
        /// Gets a match from the api
        /// </summary>
        public static Json_Get_Match GetMatch(int matchId)
        {
            if (matchId <= 0) return null;

            string jsonInput = "";

            using (WebClient webClient = new WebClient())
            {
                jsonInput = webClient.DownloadString(string.Format("{0}get_match?k={1}&mp={2}", API_URL, API_Key, matchId));
            }

            return WriteJson(jsonInput, new Json_Get_Match()) as Json_Get_Match;
        }
        
        /// <summary>
        /// Gets a user from the api
        /// </summary>
        /// <param name="user">string username for <paramref name="type"/> "name" || int userid for <paramref name="type"/> "id"  </param>
        /// <param name="mode">0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania</param>
        /// <param name="type">name == string || id == int</param>
        /// <param name="event_days">1 - 31</param>
        /// <returns>user json</returns>
        public static Json_Get_User GetUser(object user, int mode = 0, string type = "id", int event_days = 1)
        {
            if (type.Equals("id") && !(user is int))
                throw new ArgumentException("string type is 'id'" + Environment.NewLine +
                                            " - object user should be int but is instead: " + nameof(user));
            else if (type.Equals("name") && !(user is string))
                throw new ArgumentException("string type is 'name'" + Environment.NewLine +
                                            " - object user should be string but is instead: " + nameof(user));

            string jsonInput = "";

            using (WebClient webClient = new WebClient())
            {
                jsonInput = webClient.DownloadString(string.Format("{0}get_user?k={1}&u={2}&m={3}&t={4}&event_days={5}", API_URL, API_Key, user.ToString(), mode.ToString(), type, event_days));
            }
            jsonInput = jsonInput.Remove(0, 1);
            jsonInput = jsonInput.Remove(jsonInput.LastIndexOf(']'), 1);

            return WriteJson(jsonInput, new Json_Get_User()) as Json_Get_User;
        }

        /// <summary>
        /// Gets a beatmap from the api
        /// </summary>
        public static Json_Get_Beatmap Get_BeatMap(int bBeatmap_id = 0, GameModeEnum mMode = GameModeEnum.standard, int aConvertedMaps = 1, int limitSearchLimit = 30)
        {
            string jsonInput = "";

            using (WebClient webClient = new WebClient())
            {
                jsonInput = webClient.DownloadString(string.Format("{0}get_beatmaps?k={1}&b={2}&m={3}&a={4}&limit={5}", API_URL, API_Key, bBeatmap_id, (int)mMode, aConvertedMaps, limitSearchLimit));
            }
            jsonInput = jsonInput.Remove(0, 1);
            jsonInput = jsonInput.Remove(jsonInput.LastIndexOf(']'), 1);

            return WriteJson(jsonInput, new Json_Get_Beatmap()) as Json_Get_Beatmap;
        }

        public static string GetUserName(int user)
            => GetUser(user)?.username ?? "";

        /// <summary>
        /// writes a json to <see cref="T"/>
        /// </summary>
        private static Object WriteJson<T>(string JsonInput, T Format)
        {
            List<string> NJsonErrors = new List<string>();
            try
            {

                var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(JsonInput, Format, new Newtonsoft.Json.JsonSerializerSettings
                {
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        NJsonErrors.Add(args.ErrorContext.Error.Message);
                        NJsonErrors.Add(args.ErrorContext.Error.HelpLink);
                    }
                }
                );
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log("OsuApi: " + ex.ToString(), LogLevel.Trace);
                return null;
            }
        }

        [Flags]
        public enum Mods
        {
            None = 0,
            NF = 1,
            EZ = 2,
            TouchDevice = 4,
            HD = 8,
            HR = 16,
            SD = 32,
            DT = 64,
            RLX = 128,
            HT = 256,
            NC = 512, // Only set along with DoubleTime. i.e: NC only gives 576
            FL = 1024,
            AUTO = 2048,
            SO = 4096,
            AP = 8192,    // Autopilot
            PF = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
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
            LastMod = 1073741824,
            KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
            FreeModAllowed = NF | EZ | HD | HR | SD | FL | FadeIn | RLX | AP | SO | KeyMod,
            ScoreIncreaseMods = HD | HR | DT | FL | FadeIn
        }
    }
}
