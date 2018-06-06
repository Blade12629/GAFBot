using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
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
#pragma warning disable
                        _DClient.MessageCreated += async e =>
                        {
                            if (e.Author == _DClient.CurrentUser) Task.Run(() =>
                            {
                                MessageHandler MH = new MessageHandler();
                                MH.NewMessage(e);
                            }
                            );
                        };
                        #pragma warning restore

                        //Analyze_Format.Analyzed.MultiMatch ToAnalyze = new Analyze_Format.Analyzed.MultiMatch();

                        //Analyze_Format.Analyzer.MultiplayerMatch MPMatch = new Analyze_Format.Analyzer.MultiplayerMatch();
                        //MPMatch.MPJson = API.OsuApi.GetMatch(42788258);
                        //ToAnalyze = MPMatch.Analyze(MPMatch.MPJson);

                        //foreach (string s in ToAnalyze.AnalyzedData) Console.WriteLine(s);


                        //List<string> toWrite = new List<string>();

                        //foreach (KeyValuePair<ulong, DiscordGuild> dguildKvp in _DClient.Guilds)
                        //{
                        //    DiscordGuild dguild = dguildKvp.Value;

                        //    string output = string.Format("Guild {0} : Owner: {1} Channels: {2} Members: {3}", dguild.Name, dguild.Owner, dguild.Channels, dguild.MemberCount);
                        //    Console.WriteLine(DateTime.UtcNow + " -->> " + output);
                        //    toWrite.Add(output);
                        //    foreach (DiscordChannel dchannel in dguild.Channels)
                        //    {
                        //        output = string.Format("------Channel {0}: Id: {1} GuildId: {2} channelType: {3}", dchannel.Name, dchannel.Id, dchannel.GuildId, dchannel.Type);
                        //        Console.WriteLine(DateTime.UtcNow + " -->> " + output);
                        //        toWrite.Add(output);
                        //    }
                        //    foreach (DiscordMember dmember in dguild.Members)
                        //    {
                        //        string roles = "";
                        //        output = string.Format("------------User {0}: DisplayName: {1} MemberId: {2} Roles: {3}", dmember.Username, dmember.DisplayName, dmember.Id, roles);
                        //        Console.WriteLine(DateTime.UtcNow + " -->> " + output);
                        //        foreach (DiscordRole drole in dmember.Roles) roles += string.Format(" | {0} id: {1}", drole.Name, drole.Id);
                        //        toWrite.Add(output);
                        //    }
                        //    foreach (DiscordRole drole in dguild.Roles)
                        //    {
                        //        output = string.Format("------------------Role {0} : {1}", drole.Name, drole.Id);
                        //        Console.WriteLine(output);
                        //        toWrite.Add(output);
                        //    }
                        //}
                        //Console.WriteLine("Saving output to {0}....", Directory.GetCurrentDirectory());

                        //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\botinfo.info", false))
                        //{
                        //    foreach (string s in toWrite) sw.WriteLine(s);
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

        private static async Task Event_DClient_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.WriteLine("DiscordClient is Ready");
        }
    }
}
