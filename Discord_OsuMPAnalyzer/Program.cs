using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Net.WebSocket;

namespace Discord_OsuMPAnalyzer
{
    class Program
    {
        public static DiscordClient _DClient;

        //DO NOT SHARE
        private static string _DClientToken = "";
        public static string OsuApiKey = "";

        static void Main(string[] args)
        {
            try
            {
                if (_DClient == null)
                {
                    DiscordConfiguration dconfig = new DiscordConfiguration()
                    {
                        Token = _DClientToken,
                        TokenType = TokenType.Bot
                    };

                    _DClient = new DiscordClient(dconfig);

                    _DClient.SetWebSocketClient<WebSocket4NetClient>();
                }
                
                //Task t = Task.Run( () => Json.Controller.TestCase());
                //t.Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Task.Delay(-1);
            
            //_DClient.MessageCreated += async e =>
            //{
            //    if (e.Author = _DClient)
            //}
        }
    }
}
