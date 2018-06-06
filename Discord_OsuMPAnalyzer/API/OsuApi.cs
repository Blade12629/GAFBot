using System;

namespace Discord_OsuMPAnalyzer.API
{
    public static class OsuApi
    {
        private static string API_Key { get { return Config.OsuApiKey; } }
        private static string API_URL = "https://osu.ppy.sh/api/";

        // Parameters: k* api key, mp* matchid | * = required
        public static Json.Get_Match_Json.JsonFormat GetMatch(int matchId)
        {
            if (matchId <= 0) return null;

            string JsonInput = Web.WebHandler.Download(string.Format("{0}get_match?k={1}&mp={2}", API_URL, API_Key, matchId));
            return Json.Controller.JsonReader(JsonInput, Json.Controller.JsonFormat.MultiMatch) as Json.Get_Match_Json.JsonFormat;
        }
        
        // Parameters: k* api key, u* userid/username, m mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania) (default: 0), type* UseName/UseID (string, id) (preferred: UseID) , event_days (1-31)
        public static Json.Get_User_Json.JsonFormat GetUser(int user, int mode = 0, string type = "id", int event_days = 1)
        {
            string JsonInput = Web.WebHandler.Download(string.Format("{0}get_user?k={1}&u={2}&m={3}&t={4}&event_days={5}", API_URL, API_Key, user.ToString(), mode.ToString(), type, event_days));
            JsonInput = JsonInput.Remove(0, 1);
            JsonInput = JsonInput.Remove(JsonInput.LastIndexOf(']'), 1);
            return Json.Controller.JsonReader(JsonInput, Json.Controller.JsonFormat.GetPlayer) as Json.Get_User_Json.JsonFormat;
        }

        public static Json.Get_Beatmaps.JsonFormat Get_BeatMap(int bBeatmap_id = 0, Json.Controller.GameModeEnum mMode = Json.Controller.GameModeEnum.standard, int aConvertedMaps = 1, int limitSearchLimit = 30)
        {
            string DownloadString = string.Format("{0}get_beatmaps?k={1}&b={2}&m={3}&a={4}&limit={5}", API_URL, API_Key, bBeatmap_id, (int)mMode, aConvertedMaps, limitSearchLimit);
            string JsonInput = Web.WebHandler.Download(DownloadString);
            JsonInput = JsonInput.Remove(0, 1);
            JsonInput = JsonInput.Remove(JsonInput.LastIndexOf(']'), 1);
            return Json.Controller.JsonReader(JsonInput, Json.Controller.JsonFormat.Get_BeatMaps) as Json.Get_Beatmaps.JsonFormat;
        }
    }
}
