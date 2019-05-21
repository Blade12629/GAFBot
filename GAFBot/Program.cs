using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public static MessageSystem.Logger Logger { get; private set; }

        static void Main(string[] args)
            => MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();

        public static readonly string CurrentPath = System.IO.Directory.GetCurrentDirectory();
        public static string UserFile { get { return CurrentPath + Config.UserFile; } }
        public static string LogFile { get { return CurrentPath + Config.LogFile; } }
        public static string ConfigFile { get { return CurrentPath + @"\gafbot.config"; } }
        public static string VerificationFile { get { return CurrentPath + Config.VerificationFile; } }
        public static MessageSystem.MessageHandler MessageHandler { get; private set; }
        public static Commands.ICommandHandler CommandHandler { get; private set; }
        public static Assembly CommandAssembly { get; private set; }
        public static Type CommandAssemblyType { get { return CommandAssembly.GetType("GAFBot.Commands.CommandHandler"); } }
        public static string CurrentCommandAssemblyName { get { return @"\GAFBotCommands.dll"; } }
        public static Verification.Osu.VerificationHandler VerificationHandler { get; private set; }


        public static Challonge.Api.ChallongeHandler ChallongeHandler { get; private set; }


        
        public static Gambling.Betting.BettingHandler BettingHandler { get; private set; }

        static System.Timers.Timer _saveTimer;

        public static event Action SaveEvent;
        public static event Action LoadEvent;
        public static event Action ExitEvent;

        public static Config Config { get; set; }
        public static DiscordClient Client { get; set; }

        static EventWaitHandle _ewh;

        public static async Task MainTask(string[] args)
        {
            ExitEvent += () =>
            {
                SaveEvent();

                if (Client != null)
                {
                    IReadOnlyList<DiscordConnection> connections = Client.GetConnectionsAsync().Result;
                    if (connections.Count > 0)
                    {
                        Client.DisconnectAsync().Wait();
                        Client.Dispose();
                    }
                }

                if (VerificationHandler != null && VerificationHandler.IsRunning)
                {
                    VerificationHandler.IrcStop();
                    VerificationHandler.Save(VerificationFile);
                }
            };
            LoadEvent += () =>
            {
                LoadDiscord();
                LoadMessageSystem();
                Logger.Log("Loading Commands");
                LoadCommands();
                LoadVerification();
                LoadChallonge();
                LoadBetting();
            };

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            
            try
            {
                LoadConfig();
                Config.ChallongeTournamentName = "GAF2vs2Edition2018";

                Logger = new MessageSystem.Logger(LogFile);
                Logger.Initialize();

                LoadEvent();

                Logger.Log("Starting AutoSaveTimer");
                StartSaveTimer();

                Logger.Log("Connecting discord client");
                await Client.ConnectAsync();

                Logger.Log("GAF Bot initialized");
                _ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
                _ewh.WaitOne();
            }
            catch (Exception ex)
            {
                if (Logger != null)
                    Logger.Log(ex.ToString(), showConsole: Config.Debug);
                else
                    Console.WriteLine(ex);
            }

            await Task.Delay(-1);
        }
        /// <summary>
        /// Initializes the Verification
        /// </summary>
        public static void LoadVerification()
        {
            Logger.Log("Initializing Osu irc");
            VerificationHandler = new Verification.Osu.VerificationHandler(Config.Debug);
            VerificationHandler.Setup(Config.IrcUser, Config.IrcPass);
            VerificationHandler.OnVerificationStart += (id, key, keyez) => Logger.Log($"Verification started with: {id} {key} {keyez}");
            VerificationHandler.OnVerified += (id, osu) => Logger.Log($"User verified: {id} {osu}");
            VerificationHandler.IrcStart();

            if (System.IO.File.Exists(VerificationFile))
                VerificationHandler.Load(VerificationFile);
        }
        /// <summary>
        /// Initializes the MessageSystem
        /// </summary>

        public static void LoadMessageSystem()
        {
            Logger.Log("Initializing messagehandler");
            MessageHandler = new MessageSystem.MessageHandler();

            Logger.Log("loading users");
            if (System.IO.File.Exists(UserFile))
                MessageHandler.LoadUsers(UserFile);
        }
        /// <summary>
        /// Initializes Discord
        /// </summary>

        public static void LoadDiscord()
        {
            Logger.Log("Initializing discord");
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = Config.DiscordClientSecret,
                TokenType = TokenType.Bot,
            });

            Client.ClientErrored += async (arg) =>
            {
                await Task.Run(() => Logger.Log(arg.Exception.ToString()));
            };
            Client.GuildAvailable += async (arg) =>
            {
                await Task.Run(() => Logger.Log($"New guild available: {arg.Guild.Id} {arg.Guild.Name}"));
                _ewh.Set();
            };
            Client.MessageCreated += async (arg) =>
            {
                if (arg.Author.Id != Client.CurrentUser.Id)
                    await Task.Run(() => MessageHandler.NewMessage(arg));
            };
            Client.Ready += async (arg) =>
            {
                await Task.Run(() => Logger.Log("Discord client is now ready"));
            };
            Client.GuildMemberAdded += async (arg) =>
            {
                await Task.Run(() =>  MessageHandler.OnUserJoinedGuild(arg));
            };
            Client.GuildMemberRemoved += async (arg) =>
            {
                await Task.Run(() =>  MessageHandler.OnMemberRemoved(arg));
            };
            Logger.Log("Discord client initialized");
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
            Console.WriteLine("Loading config");
            Config = Config.LoadConfig(ConfigFile);

            if (!reload)
            {
                if (!System.IO.File.Exists(ConfigFile))
                {
                    Console.WriteLine($"Did not find config file at {ConfigFile}");
                    //LoadConfig gives us a default config, now we save it
                    Config.SaveConfig(ConfigFile, Config);
                    Environment.Exit(0);
                }
            }

            Console.WriteLine("Loaded config");
        }

        /// <summary>
        /// Initializes the Commands
        /// </summary>
        public static void LoadCommands()
        {
            if (!File.Exists(CurrentPath + CurrentCommandAssemblyName))
                return;

            Logger.Log(CurrentCommandAssemblyName);

            //using (MemoryStream mstream = new MemoryStream())
            //{
            byte[] assemblyBytes = File.ReadAllBytes(CurrentPath + CurrentCommandAssemblyName);
            //mstream.Write(assemblyBytes, 0, assemblyBytes.Length);
            //mstream.Seek(0, SeekOrigin.Begin);
            CommandAssembly = Assembly.Load(assemblyBytes);
            //}

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
    }
}
