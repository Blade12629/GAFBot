using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Net.WebSocket;

namespace Discord_OsuMPAnalyzer
{
    class Program
    {
        public static DiscordClient _DClient;

        //DO NOT SHARE

        static void Main(string[] args)
        {
            try
            {
                bool LoadedConfig = Config.LoadConfig();
                Console.WriteLine("{0}, {1}, {2}", LoadedConfig, Config.OsuApiKey, Config.DiscordClientSecret);

                if (LoadedConfig)
                {
                    //if (_DClient == null)
                    //{
                    //    DiscordConfiguration dconfig = new DiscordConfiguration()
                    //    {
                    //        Token = Config.DiscordClientSecret,
                    //        TokenType = TokenType.Bot
                    //    };

                    //    _DClient = new DiscordClient(dconfig);

                    //    _DClient.SetWebSocketClient<WebSocket4NetClient>();
                    //}
                    //_DClient.MessageCreated += async e =>
                    //{
                    //    if (e.Author = _DClient)
                    //}

                }
                else
                {
                    Console.WriteLine("Failed to load the config!");
                }

                Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                int i = 0;
                while (i == 0)
                {
                    Console.ReadLine();
                }
            }
        }
    }
}
