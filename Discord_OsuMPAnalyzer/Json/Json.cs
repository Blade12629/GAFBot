using System;
using System.Collections.Generic;
using System.IO;
using NJSon = Newtonsoft.Json;

namespace Discord_OsuMPAnalyzer.Json
{
    public static class Controller
    {
        public enum JsonFormat
        {
            MultiMatch,
            GetPlayer,
            Get_BeatMaps
        }

        public enum ApprovedEnum
        {
            WIP = -1,
            pending = 0,
            ranked = 1,
            approved = 2,
            qualified = 3,
            loved = 4
        }

        public enum GenreEnum
        {
            any,
            unspecified,
            video_game,
            anime,
            rock,
            pop,
            other,
            novelty,
            hip_hop = 9,
            electronic = 10
        }

        public enum LanguageIDEnum
        {
            any,
            other,
            english,
            japanese,
            chinese,
            instrumental,
            korean,
            french,
            german,
            swedish,
            spanish,
            italian
        }

        public enum GameModeEnum
        {
            standard,
            taiko,
            catchthebeat,
            osumania
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
                case JsonFormat.Get_BeatMaps:
                    return WriteJson(JsonInput, new Get_Beatmaps.JsonFormat());
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

        public delegate string NJsonErrorDelegate();

        private static Object WriteJson<T>(string JsonInput, T Format)
        {
            List<string> NJsonErrors = new List<string>();
            try
            {

                var result = NJSon.JsonConvert.DeserializeAnonymousType(JsonInput, Format, new NJSon.JsonSerializerSettings
                {
                    Error = delegate(object sender, NJSon.Serialization.ErrorEventArgs args)
                    {
                        NJsonErrors.Add(args.ErrorContext.Error.Message);
                        NJsonErrors.Add(args.ErrorContext.Error.HelpLink);
                    }
                }
                );
                var result2 = NJSon.JsonConvert.DeserializeObject(JsonInput);
                var result3 = NJSon.JsonConvert.DeserializeAnonymousType(JsonInput, Format, new NJSon.JsonSerializerSettings() { NullValueHandling = NJSon.NullValueHandling.Ignore });
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

        private static void JsonWriter(string JsonInput)
        {
            throw new InvalidOperationException("ERROR: JsonWriter NOT implemented");
        }
    }
}
