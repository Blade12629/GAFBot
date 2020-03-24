using ServerModule.Network;
using System;
using System.Threading.Tasks;

namespace ServerTest
{
    class Program
    {
        public static Server Server;

        static void Main(string[] args)
            => MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();

        private static async Task MainTask(string[] args)
        {
            try
            {
                Console.WriteLine("Starting the server");

                Server = new Server(System.Net.IPAddress.Any, 40015);
                Server.Start();

                Console.WriteLine("Started the server");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            await Task.Delay(-1);
        }
    }
}
