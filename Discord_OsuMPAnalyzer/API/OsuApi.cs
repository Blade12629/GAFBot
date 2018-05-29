using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer.API
{
    public static class OsuApi
    {
        private static string API_Key = Program.OsuApiKey;
        private static string API_URL = "https://osu.ppy.sh/api/";

        public static Object GetMatch(int matchId)
        {
            if (matchId <= 0) return null;

            string JsonInput = Web.WebHandler.Download(string.Format("{0}get_match?k={1}&mp={2}", API_URL, API_Key, matchId));
            return Json.Controller.JsonReader(JsonInput, Json.Controller.JsonFormat.MultiMatch);
        }

        public static Object GetUser(int userId, int mode, string type, int event_days = 1)
        {

            return null;
        }
    }
}
