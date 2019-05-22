using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GAFBot.Osu.Api
{
    public class Api
    {
        private static string API_Key { get { return Program.Config.OsuApiKey; } }
        private static string API_URL = "https://osu.ppy.sh/api/";

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

        // Parameters: k* api key, u* userid/username, m mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania) (default: 0), type* UseName/UseID (string, id) (preferred: UseID) , event_days (1-31)
        /// <summary>
        /// Gets a user from the api
        /// </summary>
        public static Json_Get_User GetUser(int user, int mode = 0, string type = "id", int event_days = 1)
        {
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
                foreach (string s in NJsonErrors) Console.WriteLine("{0}Error at WriteJson {1} -->> {2}{3}", Environment.NewLine, DateTime.UtcNow, s, Environment.NewLine);
                Console.WriteLine("{0} -->> {1}", DateTime.UtcNow, ex);
                Console.WriteLine("---------------------------------------------------");
                return null;
            }
        }
    }
}
