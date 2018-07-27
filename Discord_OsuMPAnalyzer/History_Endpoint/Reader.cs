using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_OsuMPAnalyzer.History_Endpoint
{
    public class Reader
    {
        public string Url { get; set; }

        public Reader(string Url)
        {
            this.Url = Url;
        }

        public void Read()
        {
            string history = Web.WebHandler.Download("https://osu.ppy.sh/community/matches/43024042/history");

            string ex2 = null;
            try
            {
                //Json = JsonConvert.DeserializeObject<History>(history);
                History HJson = JsonConvert.DeserializeObject<History>(history);

                Console.WriteLine("Events: {0}", HJson.EventCount);
                int countScores = 0;
                int countLeft = 0;
                int countJoin = 0;

                foreach (Event ev in HJson.Events)
                {
                    if (ev.Detail.Type == "player-joined")
                        countJoin++;
                    else if (ev.Detail.Type == "player-left")
                        countLeft++;
                    else if (ev.Detail.Type == "other")
                    {
                        countScores += ev.Game.scores.Count();
                        Console.WriteLine("DetailType: {0}, DetailText: {1} Scores: {2}", ev.Detail.Type, ev.Detail.MatchName, ev.Game.scores.Count());
                    }
                    else
                        Console.WriteLine("DetailType: {0}", ev.Detail.Type);
                }
                Console.WriteLine("{0}x player-joined", countJoin);
                Console.WriteLine("{0}x player-left", countLeft);
                Console.WriteLine("{0}x scores", countScores);
                
                using (StreamWriter sw = new StreamWriter("result.txt"))
                {
                    sw.Write(Environment.NewLine + history);
                    sw.WriteLine("------");
                    sw.Write(HJson);
                    sw.WriteLine("-------");
                    sw.Write(ex2);
                }
            }
            catch (Exception ex)
            {
                ex2 = ex.StackTrace;
                Console.WriteLine(ex);
            }

        }

        public string MakeJsonReadable(string Json)
        {
            try
            {
                string result3 = null;

                return result3.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
