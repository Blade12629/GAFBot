using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NJSon = Newtonsoft.Json;

namespace Discord_OsuMPAnalyzer.Json
{
    public static class Controller
    {
        public enum JsonFormat
        {
            MultiMatch,
            GetPlayer
        }

        public static Object JsonReader(string JsonInput, JsonFormat Format)
        {
            switch (Format)
            {
                default:
                    return null;
                case JsonFormat.MultiMatch:
                    return WriteJson(JsonInput, new Get_Match_Json.JsonFormat());
                case JsonFormat.GetPlayer:
                    return WriteJson(JsonInput, new Get_User_Json.JsonFormat());
            }
        }

        public static void TestCase()
        {
            Get_Match_Json.JsonFormat JFormat = new Get_Match_Json.JsonFormat();

            JFormat = API.OsuApi.GetMatch(42788258) as Get_Match_Json.JsonFormat;

            Console.WriteLine("---------------------------");
            Console.WriteLine("{0} | {1}", JFormat.match.match_id, JFormat.match.name);
            foreach (Get_Match_Json.JsonFormat._games game in JFormat.games) Console.WriteLine("Game: {0} | {1}", game.beatmap_id, game.match_type);
            Console.WriteLine("---------------------------");


        }

        private static Object WriteJson<T>(string JsonInput, T Format)
        {
            var result = NJSon.JsonConvert.DeserializeAnonymousType(JsonInput, Format);

            return result;
        }

        private static void JsonWriter(string JsonInput)
        {
            throw new InvalidOperationException("ERROR: JsonWriter NOT implemented");
        }
    }
}
