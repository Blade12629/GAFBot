#define PUBLICRELEASE
#define BETARELEASE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

namespace GAFBot
{
    public class Program
    {
        static void Main(string[] args)
            => MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();

        public static readonly string CurrentPath = System.IO.Directory.GetCurrentDirectory();
        public static string UserFile { get { return CurrentPath + Config.UserFile; } }
        public static string LogFile { get { return CurrentPath + Config.LogFile; } }
        public static string ConfigFile { get { return CurrentPath + @"\gafbot.config"; } }
        public static string VerificationFile { get { return CurrentPath + Config.VerificationFile; } }
        public static Assembly CommandAssembly { get; private set; }
        public static Type CommandAssemblyType { get { return CommandAssembly.GetType("GAFBot.Commands.CommandHandler"); } }
        public static string CurrentCommandAssemblyName { get { return @"\GAFBotCommands.dll"; } }

        public static Assembly ApiAssembly { get; set; }
        public static Type ApiAssemblyType { get { return CommandAssembly.GetType("GAFBot.Api.ApiHandler"); } }
        public static string ApiAssemblyName { get { return @"\GAFBotApi.dll"; } }

        public static Commands.ICommandHandler CommandHandler { get; private set; }
        public static MessageSystem.MessageHandler MessageHandler { get; private set; }
        public static Verification.Osu.VerificationHandler VerificationHandler { get; private set; }
        public static Challonge.Api.ChallongeHandler ChallongeHandler { get; private set; }
        public static Gambling.Betting.BettingHandler BettingHandler { get; private set; }
        public static MessageSystem.Logger Logger { get; private set; }

        public static void RequestOsuUserStatus(string user, DiscordMessage message, string originalText)
        {
            if (VerificationHandler == null)
            {
                message.ModifyAsync(originalText + "failed to request status").Wait();
                return;
            }

            VerificationHandler.GetUserStatus(user, message, originalText);
        }

        static System.Timers.Timer _saveTimer;

        /// <summary>
        /// Invoked by autosave (<see cref="StartSaveTimer"/>) and <see cref="ExitEvent"/>
        /// </summary>
        public static event Action SaveEvent;
        /// <summary>
        /// Invoked on program start
        /// </summary>
        public static event Action LoadEvent;
        /// <summary>
        /// Invoked on program exit
        /// </summary>
        public static event Action ExitEvent;

        public static Config Config { get; set; }
        public static DiscordClient Client { get; set; }

        static EventWaitHandle _ewh;


        private static API.APIServer _apiServer;
        internal static API.APIServer ApiServer { get { return _apiServer; } }


        private static async Task MainTask(string[] args)
        {
            SaveEvent += () => Config.SaveConfig(ConfigFile, Config);
            ExitEvent += () =>
            {
                Logger.Log("Exit Event: Invoking Save Event");
                SaveEvent();

                if (Client != null)
                {
                    Logger.Log("Exit Event: Closing Discord connections");
                    IReadOnlyList<DiscordConnection> connections = Client.GetConnectionsAsync().Result;
                    if (connections.Count > 0)
                    {
                        Client.DisconnectAsync().Wait();
                        Client.Dispose();
                    }
                }

                if (VerificationHandler != null && VerificationHandler.IsRunning)
                {
                    Logger.Log("Exit Event: Closing Verification Handler");
                    VerificationHandler.IrcStop();
                    VerificationHandler.Save(VerificationFile);
                }
            };
            LoadEvent += () =>
            {
                Logger.Log("LoadEvent: Loading Discord");
                LoadDiscord();
                Logger.Log("LoadEvent: Loading Message System");
                LoadMessageSystem();
                Logger.Log("LoadEvent: Loading Commands");
                LoadCommands();
                Logger.Log("LoadEvent: Loading Verification");
                LoadVerification();
#if (BETARELEASE)
                Logger.Log("LoadEvent: Loading Challonge");
                LoadChallonge();
                Logger.Log("LoadEvent: Loading Betting");
                LoadBetting();
#endif
#if (!PUBLICRELEASE)
                Logger.Log("LoadEvent: Loading API");
                LoadAPI();
#endif
            };

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            
            try
            {
                LoadConfig();

                Logger = new MessageSystem.Logger(LogFile);
                Logger.Initialize();

                LoadEvent();
                
                Logger.Log("Program: Starting AutoSaveTimer");
                StartSaveTimer();

                Logger.Log("Program: Connecting discord client");
                await Client.ConnectAsync();
                
                Logger.Log("Program: GAF Bot initialized");
                _ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
                _ewh.WaitOne();
            }
            catch (Exception ex)
            {
                if (Logger != null)
                    Logger.Log("Program: " + ex.ToString(), showConsole: Config.Debug);
                else
                    Console.WriteLine(ex);
            }

            await Task.Delay(-1);
        }

#region load

        /// <summary>
        /// Initializes the Verification
        /// </summary>
        public static void LoadVerification()
        {
            Logger.Log("Program: Initializing Osu irc");
            VerificationHandler = new Verification.Osu.VerificationHandler(Config.Debug);
            VerificationHandler.Setup(Config.IrcUser, Config.IrcPass);
            VerificationHandler.OnVerificationStart += (id, key) => Logger.Log($"Program: Verification started with: {id} {key}");
            VerificationHandler.OnVerified += (id, osu) => Logger.Log($"Program: User verified: {id} {osu}");
            VerificationHandler.IrcStart();
        }

        /// <summary>
        /// Initializes the MessageSystem
        /// </summary>
        public static void LoadMessageSystem()
        {
            Logger.Log("Program: Initializing messagehandler");
            MessageHandler = new MessageSystem.MessageHandler();

            Logger.Log("Program: loading users");
            if (System.IO.File.Exists(UserFile))
                MessageHandler.LoadUsers(UserFile);
        }

        /// <summary>
        /// Initializes Discord
        /// </summary>
        public static void LoadDiscord()
        {
            Logger.Log("Program: Initializing discord");
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = Config.DiscordClientSecret,
                TokenType = TokenType.Bot,
            });

            Client.ClientErrored += async (arg) =>
            {
                await Task.Run(() => Logger.Log("Program: " + arg.Exception.ToString()));
            };
            Client.GuildAvailable += async (arg) =>
            {
                await Task.Run(() => Logger.Log($"Program: New guild available: {arg.Guild.Id} {arg.Guild.Name}"));
                _ewh.Set();
            };
            Client.MessageCreated += async (arg) =>
            {
                if (arg.Author.Id != Client.CurrentUser.Id)
                    await Task.Run(() => MessageHandler.NewMessage(arg));
            };
            Client.Ready += async (arg) =>
            {
                await Task.Run(() => Logger.Log("Program: Discord client is now ready"));
            };
            Client.GuildMemberAdded += async (arg) =>
            {
                await Task.Run(() =>  MessageHandler.OnUserJoinedGuild(arg));
            };
            Client.GuildMemberRemoved += async (arg) =>
            {
                await Task.Run(() =>  MessageHandler.OnMemberRemoved(arg));
            };
            Logger.Log("Program: Discord client initialized");
        }

        /// <summary>
        /// Initializes the SaveTimer
        /// </summary>
        public static void StartSaveTimer()
        {
            if (_saveTimer != null)
                return;

            _saveTimer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = Config.AutoSaveTime
            };
            _saveTimer.Elapsed += SaveTimerTick;
            _saveTimer.Start();
        }

        /// <summary>
        /// Invokes <see cref="SaveEvent"/>
        /// </summary>
        public static void SaveTimerTick(object sender, ElapsedEventArgs arg)
            => SaveEvent();

        /// <summary>
        /// Initializes the Config
        /// </summary>
        public static void LoadConfig(bool reload = false)
        {
            Console.WriteLine("Program: Loading config");
            Config = Config.LoadConfig(ConfigFile);

            if (!reload)
            {
                if (!System.IO.File.Exists(ConfigFile))
                {
                    Console.WriteLine($"Program: Did not find config file at {ConfigFile}");
                    //LoadConfig gives us a default config, now we save it
                    Config.SaveConfig(ConfigFile, Config);
                    Environment.Exit(0);
                }
            }

            Console.WriteLine("Program: Loaded config");
        }

        /// <summary>
        /// Initializes the Commands
        /// </summary>
        public static void LoadCommands()
        {
            if (!File.Exists(CurrentPath + CurrentCommandAssemblyName))
                return;

            Logger.Log("Program: " + CurrentCommandAssemblyName);
            
            byte[] assemblyBytes = File.ReadAllBytes(CurrentPath + CurrentCommandAssemblyName);
            CommandAssembly = Assembly.Load(assemblyBytes);

            CommandHandler = Activator.CreateInstance(CommandAssemblyType) as Commands.ICommandHandler;
            CommandHandler.LoadCommands();
            GC.Collect();
        }

        /// <summary>
        /// Initializes Challonge
        /// </summary>
        public static void LoadChallonge()
        {
            ChallongeHandler = new Challonge.Api.ChallongeHandler();
            ChallongeHandler.Update();
        }

        /// <summary>
        /// Initializes Betting
        /// </summary>
        public static void LoadBetting()
        {
            BettingHandler = new Gambling.Betting.BettingHandler();
            BettingHandler.Load();
        }

        public static void LoadAPI()
        {
            try
            {
                API.APICalls.Init();
                _apiServer = new API.APIServer(40003, IPAddress.Any);
                _apiServer.OnClientConnected += (id) => Console.WriteLine("New connection with session id " + id);
                _apiServer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("API EXCEPTION: " + ex.ToString());
            }
        }

#endregion

#region consoleEvents

        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        /// <summary>
        /// Invoked on console exit
        /// </summary>
        /// <param name="sig"></param>
        /// <returns></returns>
        static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    ExitEvent();
                    return false;
            }
        }

#endregion
    }
}
