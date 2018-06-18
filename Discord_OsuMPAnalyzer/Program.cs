using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;

namespace Discord_OsuMPAnalyzer
{
    class Program
    {
        public static DiscordClient _DClient;

        //DO NOT SHARE
        public static void LogWriter(params string[] toWrite)
        {
            using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\log.log"))
            {
                foreach (string s in toWrite) sw.WriteLine("{0} --> {1}", DateTime.UtcNow, s);
            }
        }
        static void Main(string[] args)
        {
            MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }


        public static async Task MainTask(string[] args)
        {
            try
            {
                bool LoadedConfig = Config.LoadConfig();

                if (LoadedConfig)
                {
                    if (_DClient == null)
                    {
                        Console.WriteLine("{0}, {1}, {2}", LoadedConfig, Config.OsuApiKey, Config.DiscordClientSecret);
                        DiscordConfiguration dconfig = new DiscordConfiguration()
                        {
                            Token = Config.DiscordClientSecret,
                            TokenType = TokenType.Bot,
                        };

                        _DClient = new DiscordClient(dconfig);

                        _DClient.SetWebSocketClient<WebSocket4NetClient>();

                        await _DClient.ConnectAsync();

                        _DClient.Ready += async e =>
                        {
                            await Event_DClient_Ready(e);
                        };

                        _DClient.ClientErrored += async e =>
                        {
                            Console.WriteLine("{0} : {1}", e.EventName, e.Exception);
                        };

                        _DClient.ClientErrored += async e =>
                        {
                            Console.WriteLine("{0} : {1}", e.EventName, e.Exception);
                        };

                        _DClient.MessageCreated += async e =>
                        {
                            Console.WriteLine("{0}: {1} {2} {3}", DateTime.Now, e.Author, e.Author.Username, e.Message.Content);
                            if (e.Author.Id != _DClient.CurrentUser.Id)
                                await Task.Run(async () => { await OnMessage(e); });
                        };



                        //Analyze_Format.Analyzer.MultiplayerMatch mpmatch = new Analyze_Format.Analyzer.MultiplayerMatch();
                        //Analyze_Format.Analyzed.MultiMatch mpMatch = mpmatch.Analyze(API.OsuApi.GetMatch(42788258));

                        ////string toSend = "________________________";

                        ////foreach (string s in mpMatch.AnalyzedData)
                        ////{
                        ////    toSend += string.Format(Environment.NewLine + s);
                        ////}

                        ////toSend += "________________________";

                        ////#pragma warning restore

                        //Analyze_Format.Analyzed.MultiMatch ToAnalyze = mpMatch;

                        ////foreach (string s in ToAnalyze.AnalyzedData) Console.WriteLine(s);

                        //Console.WriteLine("Count: {0}", ToAnalyze.BestAccuracies.Count());

                        //using (StreamWriter sw = new StreamWriter("output.txt"))
                        //{
                        //    foreach (string s in ToAnalyze.AnalyzedData)
                        //        sw.WriteLine(s);
                        //}



                        await Task.Delay(-1);
                    }
                    else
                    {
                        Console.WriteLine("Failed to load the config!");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.UtcNow + " " + ex);
                int i = 0;
                while (i == 0)
                {
                    LogWriter(DateTime.UtcNow + " " + ex.ToString());
                    Console.ReadLine();
                }

            }
        }

        private static async Task OnMessage(MessageCreateEventArgs e)
        {
            MessageHandler MH = new MessageHandler();
            MH.NewMessage(e);
        }

        private static async Task Event_DClient_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            try
            {
                Console.WriteLine("DiscordClient is now ready...");
                List<string> toWrite = new List<string>();
                List<DiscordGuild> DGuilds = new List<DiscordGuild>();

                foreach (KeyValuePair<ulong, DiscordGuild> KvpGuild in _DClient.Guilds)
                {
                    DGuilds.Add(KvpGuild.Value);
                }

                foreach (DiscordGuild DGuild in DGuilds)
                {
                    toWrite.Add(string.Format("DGuild: {0} {1}", DGuild.Id, DGuild.Name));
                    foreach (DiscordChannel DChannel in DGuild.Channels)
                        toWrite.Add(string.Format("DChannel {1}", DChannel.Name, DChannel.Id));

                    foreach (DiscordMember DMember in DGuild.Members)
                        toWrite.Add(string.Format("DMember {0} {1} {2}", DMember.Id, DMember.DisplayName, DMember.Username));
                }

                foreach (string s in toWrite)
                {
                    Console.WriteLine(s);
                }

                using (StreamWriter sw = new StreamWriter("botinfo2.txt"))
                {
                    foreach (string s in toWrite)
                        sw.WriteLine(s);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
