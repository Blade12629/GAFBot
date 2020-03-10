using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using GAFBot.Database.Models;
using GAFBot.Verification.Osu;

namespace GAFBot
{
    public class Program
    {
        static void Main(string[] args)
            => MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();

        public static readonly string CurrentPath = System.IO.Directory.GetCurrentDirectory();
        public static Assembly CommandAssembly { get; internal set; }
        public static Type CommandAssemblyType { get { return CommandAssembly.GetType("GAFBot.Commands.CommandHandler"); } }
        public static string CurrentCommandAssemblyName { get { return @"\GAFBotCommands.dll"; } }
        public static API.HTTP HTTPAPI { get; set; }


        public static Assembly ApiAssembly { get; set; }
        public static Type ApiAssemblyType { get { return CommandAssembly.GetType("GAFBot.Api.ApiHandler"); } }
        public static string ApiAssemblyName { get { return @"\GAFBotApi.dll"; } }

        public static Commands.ICommandHandler CommandHandler { get; internal set; }
        public static MessageSystem.MessageHandler MessageHandler { get; internal set; }
        public static Challonge.Api.ChallongeHandler ChallongeHandler { get; internal set; }
        public static Gambling.Betting.BettingHandler BettingHandler { get; internal set; }

        public static void RequestOsuUserStatus(string user, DiscordMessage message, string originalText)
        {
            IVerificationHandler verifyHandler = Modules.ModuleHandler.Get("verification") as IVerificationHandler;
            
            if (verifyHandler == null)
            {
                message.ModifyAsync(originalText + " failed to request status").Wait();
                return;
            }

            verifyHandler.GetUserStatus(user, message, originalText);
        }

        static System.Timers.Timer _saveTimer;

        /// <summary>
        /// Invoked by autosave (<see cref="StartSaveTimer"/>) and <see cref="ExitEvent"/>
        /// </summary>
        public static event Action SaveEvent;
        /// <summary>
        /// Invoked on program exit
        /// </summary>
        public static event Action ExitEvent;

        public static BotConfig Config
        {
            get
            {
                BotConfig config;
                using (Database.GAFContext context = new Database.GAFContext())
                {
                    int highest = context.BotConfig.Max(c => c.Id);
                    config = context.BotConfig.First(c => c.Id == highest);
                }

                return config;
            }
        }
        public static DiscordClient Client { get; set; }

        static EventWaitHandle _ewh;

        internal static Random Rnd { get; private set; }
        
        static Task _maintenanceTask;
        private static bool _maintenance;
        private static bool _checkForMaintenance;

        public static bool Maintenance => _maintenance;
        public static string MaintenanceNotification { get; private set; }

        private static void CheckForMaintenance()
        {
            while (_checkForMaintenance)
            {
                using (Database.GAFContext context = new Database.GAFContext())
                {
                    int highestId = context.BotMaintenance.Max(bm => bm.Id);
                    var maint = context.BotMaintenance.FirstOrDefault(bm => bm.Id == highestId);

                    if (maint == null)
                    {
                        if (!_checkForMaintenance)
                            break;

                        Task.Delay(10000).Wait();
                        continue;
                    }

                    if (Client.CurrentUser.Presence.Game == null || _lastStateMaint != maint.Enabled || _lastNotifyMaint == null || !_lastNotifyMaint.Equals(maint.Notification))
                    {
                        if (!maint.Enabled)
                        {
                            ToggleMaintenance(maint.Notification, false);
                            MaintenanceNotification = maint.Notification;
                        }
                        else if (maint.Enabled)
                        {
                            ToggleMaintenance(maint.Notification, true);
                            MaintenanceNotification = maint.Notification;
                            _maintenance = true;
                        }
                    }
                }

                if (!_checkForMaintenance)
                    break;

                Task.Delay(10000).Wait();
            }
        }

        private static async Task MainTask(string[] args)
        {
            Rnd = new Random();
            _checkForMaintenance = true;
            _maintenanceTask = new Task(CheckForMaintenance);

            SaveEvent += () => Logger.Log("Starting Save process");
            ExitEvent += () =>
            {
                _checkForMaintenance = false;

                while (_maintenanceTask.Status == TaskStatus.Running)
                    Task.Delay(5).Wait();

                Logger.Log("Exit Event: Invoking Save Event");
                SaveEvent?.Invoke();

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
            };

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            try
            {
                LoadEnviromentVariables();
                Logger.Initialize();
                
                Modules.ModuleHandler.Initialize();

                IVerificationHandler verifyHandler = Modules.ModuleHandler.Get("verification") as IVerificationHandler;
                Logger.Log("Verified: " + verifyHandler.IsUserVerified("Skyfly"));

                await Task.Delay(-1);

                //ToDo, remove soon
                Localization.Init(CurrentPath + @"\locales.cfg");

#if DEBUG
                Logger.Log("Running on DEBUG mode");
#else
                Logger.Log("Running on Release mode");
#endif

                //Loads the AutoInitAttribute
                InitializeAssembly(Assembly.GetEntryAssembly());


                Logger.Log("Program: Starting AutoSaveTimer");
                StartSaveTimer();

                Logger.Log("Program: Connecting discord client");
                await Client.ConnectAsync();

                Logger.Log("Program: GAF Bot initialized");
                _ewh = new EventWaitHandle(false, EventResetMode.AutoReset);

                //If you want to add custom code that should 
                //be able to activate after discord is ready
                //Enter it after this method call
                _ewh.WaitOne();

                _maintenanceTask.Start();

                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }

            await Task.Delay(-1);
        }

        #region load

        private static void InitializeAssembly(Assembly ass)
        {
            List<(MethodInfo, int)> methods = new List<(MethodInfo, int)>();

            AutoInitAttribute initAttrib;
            foreach (Type t in ass.GetTypes())
            {
                foreach (MethodInfo mInfo in t.GetMethods())
                {
                    if (!mInfo.IsStatic)
                        continue;

                    initAttrib = mInfo.GetCustomAttribute<AutoInitAttribute>();

                    if (initAttrib != null)
                        methods.Add((mInfo, initAttrib.Priority));
                }
            }

            methods.OrderByDescending(p => p.Item2);

            for (int i = 0; i < methods.Count; i++)
            {
                try
                {
                    Logger.Log("Initializing method: " + methods[i].Item1.Name, LogLevel.Trace);
                    methods[i].Item1.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Logger.Log("Could not initialize method: " + methods[i].Item1.Name + Environment.NewLine + ex, LogLevel.ERROR);
                }
            }
        }

        #region AutoInitialize

        /// <summary>
        /// Initializes Discord
        /// </summary>
        [AutoInit(0)]
        public static void LoadDiscord()
        {
            Logger.Log("Program: Initializing discord");
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = DecryptString(Config.DiscordClientSecretEncrypted),
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
                await Task.Run(() => MessageHandler.OnUserJoinedGuild(arg));
            };
            Client.GuildMemberRemoved += async (arg) =>
            {
                await Task.Run(() => MessageHandler.OnMemberRemoved(arg));
            };
            Logger.Log("Program: Discord client initialized");
        }
        
        [AutoInit(2)]
        public static void LoadApi()
        {
            HTTPAPI = new API.HTTP(Config.WebsiteHost);
            bool apiLogin = HTTPAPI.Auth(Config.WebsiteUser, DecryptString(Config.WebsitePassEncrypted)).Result;

            if (!apiLogin)
            {
                Logger.Log("Could not connect to api!", LogLevel.WARNING);
                return;
            }
        }

        /// <summary>
        /// Initializes the MessageSystem
        /// </summary>
        [AutoInit(1)]
        public static void LoadMessageSystem()
        {
            Logger.Log("Program: Initializing messagehandler");
            MessageHandler = new MessageSystem.MessageHandler();
        }
        /// <summary>
        /// Initializes the Commands
        /// </summary>
        [AutoInit(1)]
        public static void LoadCommands()
        {
            if (!File.Exists(CurrentPath + CurrentCommandAssemblyName))
                return;

            Logger.Log("Program: " + CurrentCommandAssemblyName);

            CommandHandler = null;

            GC.Collect();

            byte[] assemblyBytes = File.ReadAllBytes(CurrentPath + CurrentCommandAssemblyName);
            CommandAssembly = Assembly.Load(assemblyBytes);

            CommandHandler = Activator.CreateInstance(CommandAssemblyType) as Commands.ICommandHandler;
            CommandHandler.LoadCommands();
        }

        /// <summary>
        /// Initializes Challonge
        /// </summary>
        [AutoInit(2)]
        public static void LoadChallonge()
        {
            ChallongeHandler = new Challonge.Api.ChallongeHandler();
            ChallongeHandler.Update();
        }

        /// <summary>
        /// Initializes Betting
        /// </summary>
        [AutoInit(2)]
        public static void LoadBetting()
        {
            BettingHandler = new Gambling.Betting.BettingHandler();
        }

        #endregion

        #region ManualInitialize
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
                Interval = Config.AutoSaveTime.TotalMilliseconds
            };
            _saveTimer.Elapsed += SaveTimerTick;
            _saveTimer.Start();
        }

        /// <summary>
        /// Invokes <see cref="SaveEvent"/>
        /// </summary>
        public static void SaveTimerTick(object sender, ElapsedEventArgs arg)
            => SaveEvent();

        public static void LoadEnviromentVariables()
        {
            if (File.Exists(CurrentPath + @"\gaf"))
                Environment.SetEnvironmentVariable("GAF", "true", EnvironmentVariableTarget.Process);
            else
                Environment.SetEnvironmentVariable("GAF", "false", EnvironmentVariableTarget.Process);

            string dbFile = Path.Combine(CurrentPath, "dbconnection.string");
            Environment.SetEnvironmentVariable("DBConnectionString", File.ReadAllText(dbFile), EnvironmentVariableTarget.Process);
        }

        private static bool _lastStateMaint;
        private static string _lastNotifyMaint;

        public static void ToggleMaintenance(string notification, bool state)
        {
            _lastStateMaint = state;
            _lastNotifyMaint = notification;

            if (state)
            {
                Client.UpdateStatusAsync(new DiscordGame(string.IsNullOrEmpty(notification) ? "Maintenance" : notification), UserStatus.DoNotDisturb).Wait();
                return;
            }

            DiscordGame game = null;

            if (!string.IsNullOrEmpty(notification))
                game = new DiscordGame(notification);
            
            Client.UpdateStatusAsync(game, UserStatus.Online).Wait();
        }
        #endregion

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


        #region encryption
        private static byte[] _encK;
        private static byte[] _encryptionKey
        {
            get
            {
                if (_encK == null)
                    LoadEncryptionKey();

                return _encK;
            }
        }

        private static void LoadEncryptionKey()
        {
            if (!File.Exists("enc.key"))
                GenerateEncryptionKey();
            
            using (FileStream fstream = File.OpenRead("enc.key"))
            {
                _encK = new byte[fstream.Length];
                fstream.Read(_encK, 0, _encK.Length);
            }
        }
        
        private static void GenerateEncryptionKey()
        {
            Aes aes = Aes.Create();
            aes.IV = new byte[16];

            aes.GenerateKey();

            using (FileStream fstream = File.OpenWrite("enc.key"))
            {
                fstream.Write(aes.Key, 0, aes.Key.Length);
                fstream.Flush();
            }
        }

        public static string EncryptString(string plainInput)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainInput);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        #endregion
    }
}

