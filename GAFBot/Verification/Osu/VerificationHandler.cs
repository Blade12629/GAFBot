using NetIrc2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using GAFBot.MessageSystem;

namespace GAFBot.Verification.Osu
{
    public class VerificationHandler
    {

        string _host = Program.Config.IrcHost;
        int _port = Program.Config.IrcPort;
        string _user;
        string _password;
        IrcClient _client;

        public static MessageSystem.Logger Logger { get { return Program.Logger; } }

        public void GetUserStatus(string user, DSharpPlus.Entities.DiscordMessage message, string originalText)
        {
            try
            {
                _client.Message(new IrcString("BanchoBot"), new IrcString("!stats " + user));
                
                string userUnderscores = user.Replace(' ', '_');
                user = user.Replace('_', ' ');

                lock (_userStatusQueue)
                {
                    if (_userStatusQueue.FindIndex(p => p.Item2.Equals(userUnderscores, StringComparison.CurrentCultureIgnoreCase)) >= 0)
                        return;
                    
                    _userStatusQueue.Add((message, userUnderscores, originalText));
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Log(ex.ToString(), logToFile: false);
            }
        }

        
        /// <summary>
        /// dUserId, code, codeEz
        /// </summary>
        public MultiDict<ulong, string> ActiveVerifications { get; set; }
        public bool IsRunning { get; private set; }
        public bool Debug { get; set; }

        public VerificationHandler(bool debug = false)
        {
            Debug = debug;
            ActiveVerifications = new MultiDict<ulong, string>();
            _userStatusQueue = new List<(DSharpPlus.Entities.DiscordMessage, string, string)>();
            Program.LoadEvent += () => ActiveVerifications.Load(Program.VerificationFile);
            Program.SaveEvent += () =>
            {
                IrcStop();
                ActiveVerifications.Save(Program.VerificationFile);
                IrcStart();
            };
        }
        
        /// <summary>
        /// Starts the verification process
        /// </summary>
        /// <param name="duserid"></param>
        /// <returns></returns>
        public (string, string) StartVerification(ulong duserid)
        {
            if (duserid == 0)
                return ("e", "id=0");
            else if (ActiveVerifications.ContainsKey(duserid))
                return ("v", "active");

            byte[] verificationKey = new byte[2];
            RandomNumberGenerator.Fill(verificationKey);

            string verificationStr = "";
            string verificationStrEz = "";

            for (int i = 0; i < verificationKey.Length; i++)
                verificationStr += verificationKey[i].ToString();

            if (!ActiveVerifications.TryAdd(duserid, verificationStr))
                return ("v", "active");
            
            OnVerificationStart(duserid, verificationStr);

            return (verificationStr, verificationStrEz);
        }
        
        public event Action<ulong, string> OnVerificationStart;
        public event Action<ulong, string> OnVerified;

        /// <summary>
        /// Setup the verification handler
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public void Setup(string user, string password)
        {
            Logger.Log("VerificationHandler: Setting up irc client");

            _user = user;
            _password = password;

            _client = new IrcClient();
            _client.Connected += (sender, arg) => Logger.Log("VerificationHandler: IrcConnected");
            _client.GotMessage += _client_GotMessage;
            _client.GotIrcError += (sender, arg) => Logger.Log($"VerificationHandler: IrcError: {arg.Error}: {arg.Data}", showConsole: Debug);
            _client.GotWelcomeMessage += (sender, arg) => Logger.Log($"VerificationHandler: IrcGotWelcomeMessage: {arg.Message}");
            _client.Closed += (sender, arg) => Logger.Log("VerificationHandler: IrcConnection closed");
        }

        /// <summary>
        /// discordmessage, user, string to add result
        /// </summary>
        private List<(DSharpPlus.Entities.DiscordMessage, string, string)> _userStatusQueue;

        /// <summary>
        /// Invoked if there is a new irc message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _client_GotMessage(object sender, NetIrc2.Events.ChatMessageEventArgs e)
        {
            string hostname = e.Sender.Hostname.ToString();
            string nickName = e.Sender.Nickname.ToString();
            string recipient = e.Recipient.ToString();
            string message = e.Message.ToString();

            Logger.Log($"IRC: New Message: from {nickName} {hostname} to {recipient}: {message}");
            //!verify 35483
            if (!recipient.StartsWith("Skyfly"))
                return;

            //For getting the status
            if (nickName.Equals("BanchoBot") && _userStatusQueue.Count > 0)
            {
                if (message.StartsWith("Stats for (", StringComparison.CurrentCultureIgnoreCase))
                {
                    try
                    {
                        string user = message.Remove(0, message.IndexOf('(') + 1);
                        user = user.Substring(0, user.IndexOf(')'));
                        string underscoreUser = user.Replace(' ', '_');
                        user = user.Replace('_', ' ');
                        
                        //count 10, index 6, 10-6 = 4
                        int indexStatus = message.IndexOf(']') + 1;
                        string status = message.Remove(0, indexStatus + 4);
                        List<char> removeChars = new List<char>();
                        removeChars.AddRange(Environment.NewLine.ToList());
                        removeChars.Add(':');
                        removeChars.Add(' ');
                        char[] removeCharsA = removeChars.ToArray();
                        
                        status = status.TrimStart(removeCharsA).TrimEnd(removeCharsA);

                        DSharpPlus.Entities.DiscordMessage dmessage = null;
                        string username = "";
                        string originalText = "";

                        Console.WriteLine("user: " + user);
                        Console.WriteLine("underscore user: " + underscoreUser);
                        
                        lock (_userStatusQueue)
                        {
                            for (int i = 0; i < _userStatusQueue.Count; i++)
                            {
                                var pair = _userStatusQueue[i];

                                Console.WriteLine("pair: " + pair.Item2);

                                if (pair.Item2.Equals(user, StringComparison.CurrentCultureIgnoreCase) || pair.Item2.Equals(underscoreUser, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    dmessage = pair.Item1;
                                    username = pair.Item2;
                                    originalText = pair.Item3;
                                    _userStatusQueue.RemoveAt(i);
                                    break;
                                }
                            }
                        }

                        if (dmessage == null)
                            return;

                        dmessage.ModifyAsync(originalText + status + Environment.NewLine + "```").Wait();

                        return;
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Log(ex.ToString(), logToFile: false);
                    }
                }
            }

            if (!message.StartsWith("!verify"))
                return;

            if (message.Length <= "!verify ".Length)
            {
                Logger.Log($"IRC: {nickName} - Verification id cannot be empty", showConsole: Debug);
                ircMsg("Verification id cannot be empty");
                return;
            }
            string verifyStr = message.Remove(0, "!verify ".Length);
            //!verify 35483
            //35483
            ulong userId = 0;

            Logger.Log($"VerificationHandler: Looking for userid for code: {verifyStr} full string: {message}", showConsole: Debug);
            
            if (string.IsNullOrEmpty(verifyStr))
            {
                Logger.Log($"IRC: {nickName} - Verification id cannot be empty", showConsole: Debug);
                ircMsg("Verification id cannot be empty");
                return;
            }

            if (!ActiveVerifications.TryGetValue(verifyStr, out userId))
            {
                Logger.Log($"IRC: {nickName} - Could not find your verification id |{userId}|{verifyStr}|", showConsole: Debug);
                ircMsg($"Could not find your verification id |{userId}|{verifyStr}|");
                return;
            }

            Logger.Log("VerificationHandler: Found userid: " + userId, showConsole: Debug);

            if (userId > 0)
            {
                Logger.Log($"IRC: {nickName} - Verifying", showConsole: Debug);
                ircMsg("Verifying...");
                VerifyUser(userId, nickName);
            }
            else
            {
                Logger.Log($"IRC: {nickName} - Could not find your userId {userId}", showConsole: Debug);
                ircMsg("Could not find your userId " + userId);
            }

            void ircMsg(string msg)
            {
                _client.Message(e.Sender.Nickname, new IrcString(message));
            }
        }

        /// <summary>
        /// Invoked to verify the user after the verification process started or to instantly verify
        /// </summary>
        /// <param name="duserId"></param>
        /// <param name="osuUserName"></param>
        public void VerifyUser(ulong duserId, string osuUserName)
        {
            try
            {
                Logger.Log("VerificationHandler: Verifying user " + duserId + " | " + osuUserName, showConsole: Debug);

                List<User> currentUsers = null;

                lock(Program.MessageHandler.Users)
                {
                    currentUsers = Program.MessageHandler.Users.Values.ToList();
                }

                if (currentUsers != null && currentUsers.Count > 0)
                {
                    if (currentUsers.Find(u => !string.IsNullOrEmpty(u.OsuUserName) && u.OsuUserName.Equals(osuUserName, StringComparison.CurrentCultureIgnoreCase)) != null)
                    {
                        Program.Logger.Log("Account already linked: " + osuUserName, showConsole: Debug);
                        var privChannel = Coding.Methods.GetPrivChannel(duserId);
                        privChannel.SendMessageAsync("Your user account has already been linked to an discord account" + Environment.NewLine +
                            "If this is an error or is incorrect, please contact Skyfly on discord (??????#0284 (6*?)))").Wait();
                        return;
                    }
                }
                
                if (!Program.MessageHandler.Users.TryGetValue(duserId, out User user))
                {
                    Program.Logger.Log("Could not find userid: " + duserId, showConsole: Debug);
                    var privChannel = Coding.Methods.GetPrivChannel(duserId);
                    privChannel.SendMessageAsync("Could not find user " + duserId).Wait();
                    return;
                }

                ActiveVerifications.TryRemove(duserId, out string strVal);

                user.Verified = true;
                user.OsuUserName = osuUserName;

                Coding.Methods.AssignRole(duserId, Program.Config.DiscordGuildId, Program.Config.VerifiedRoleId, "Verified, osu: " + osuUserName);

                OnVerified(duserId, osuUserName);

                var dclient = Coding.Methods.GetClient();
                var duser = dclient.GetUserAsync(duserId).Result;
                var dDmChat = dclient.CreateDmAsync(duser).Result;
                dDmChat.SendMessageAsync("You have successfully linked your discord account to your osu! account " + osuUserName).Wait();
            }
            catch (Exception ex)
            {
                //Skyfly
                var priv = Coding.Methods.GetPrivChannel(Program.Config.DefaultDiscordAdmins[0]);
                priv.SendMessageAsync("Use caused exception" + Environment.NewLine + ex.ToString());
                Logger.Log("VerificationHandler: " + ex.ToString(), showConsole: Debug);
            }
        }

        /// <summary>
        /// Checks if the user is verified
        /// </summary>
        /// <param name="duserId"></param>
        /// <returns></returns>
        public bool IsUserVerified(ulong duserId)
        {
            User user = null;

            lock(Program.MessageHandler.Users)
            {
                user = Program.MessageHandler.Users[duserId];
            }

            if (user != null)
                return user.Verified;

            return false;
        }

        /// <summary>
        /// Checks if the user is verified
        /// </summary>
        /// <param name="duserId"></param>
        /// <returns></returns>
        public bool IsUserVerified(string osuUsername)
        {
            User user = null;

            lock (Program.MessageHandler.Users)
            {
                user = Program.MessageHandler.Users.Values.ToList().Find(u => u.OsuUserName.Equals(osuUsername, StringComparison.CurrentCultureIgnoreCase));
            }

            if (user != null)
                return user.Verified;

            return false;
        }

        /// <summary>
        /// Checks if the user can bypass the verification process
        /// </summary>
        /// <param name="duserId"></param>
        /// <returns></returns>
        public bool CanBypass(ulong duserId)
            => CanBypass(duserId, Program.Config.DiscordGuildId);

        /// <summary>
        /// Checks if the user can bypass the verification process
        /// </summary>
        /// <param name="duserId"></param>
        /// <returns></returns>
        public bool CanBypass(ulong duserId, ulong guildId)
        {
            var dmember = Coding.Methods.GetMember(duserId, guildId);

            if (Program.Config.DefaultDiscordAdmins != null && Program.Config.DefaultDiscordAdmins.Contains(duserId))
                return true;
            else
                foreach (var drole in dmember.Roles)
                    if (Program.Config.BypassVerification != null && Program.Config.BypassVerification.Contains(drole.Id))
                        return true;

            return false;
        }

        /// <summary>
        /// Starts the irc
        /// </summary>
        public void IrcStart()
        {
            if (IsRunning)
                return;
            
            Logger.Log("VerificationHandler: Starting irc", showConsole: Debug);

            IsRunning = true;

            _client.Connect(_host, _port);

            while (!_client.IsConnected)
                Task.Delay(5).Wait();
            _client.IrcCommand(new IrcString($"PASS"), new IrcString(_password));
            _client.IrcCommand(new IrcString($"NICK"), new IrcString(_user));
            Logger.Log("VerificationHandler: Sent Login data", showConsole: Debug);
        }

        /// <summary>
        /// Stops the irc
        /// </summary>
        public void IrcStop()
        {
            if (!IsRunning)
                return;

            if (Debug)
                Logger.Log("VerificationHandler: Stopping irc");

            _client.Close();

            IsRunning = false;
        }

        /// <summary>
        /// Loads active verifications
        /// </summary>
        /// <param name="file"></param>
        public void Load(string file)
        {
            string json = System.IO.File.ReadAllText(file);

            if (string.IsNullOrEmpty(json))
                return;
            lock(ActiveVerifications)
            {
                ActiveVerifications = Newtonsoft.Json.JsonConvert.DeserializeObject<MultiDict<ulong, string>>(json);
            }
        }

        /// <summary>
        /// Saves active verifications
        /// </summary>
        /// <param name="file"></param>
        public void Save(string file)
        {
            string json = "";

            lock (ActiveVerifications)
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(ActiveVerifications);
            }

            System.IO.File.WriteAllText(file, json);
        }
    }

    /// <summary>
    /// Thread-safe multi-dictionary for having 2 keys/values at the same time and easly accessing them
    /// </summary>
    public class MultiDict<T, A>
    {
        private ConcurrentDictionary<T, A> _valuesOne;
        private ConcurrentDictionary<A, T> _valuesTwo;

        public MultiDict()
        {
            _valuesOne = new ConcurrentDictionary<T, A>();
            _valuesTwo = new ConcurrentDictionary<A, T>();
        }

        public void Save(string file)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(_valuesOne, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(file, json);
        }

        public void Load(string file)
        {
            string json = System.IO.File.ReadAllText(file);

            lock (this)
            {
                _valuesOne = Newtonsoft.Json.JsonConvert.DeserializeObject<ConcurrentDictionary<T, A>>(json);
                _valuesTwo = new ConcurrentDictionary<A, T>();

                foreach (var pair in _valuesOne)
                    _valuesTwo.TryAdd(pair.Value, pair.Key);
            }
        }

        public bool ContainsKey(T key)
        {
            lock (this)
                return _valuesOne.ContainsKey(key);
        }
        
        public bool ContainsKey(A key)
        {
            lock (this)
                return _valuesTwo.ContainsKey(key);
        }
        
        public bool ContainsValue(T value)
            => ContainsKey(value);
        
        public bool ContainsValue(A value)
            => ContainsKey(value);

        public bool TryAdd(T key, A value)
        {
            if (!_valuesOne.TryAdd(key, value))
                return false;

            if(!_valuesTwo.TryAdd(value, key))
            {
                _valuesOne.TryRemove(key, out A newVal);
                return false;
            }

            return true;
        }

        public bool TryAdd(A key, T value)
        {
            if (!_valuesTwo.TryAdd(key, value))
                return false;

            if (_valuesOne.TryAdd(value, key))
            {
                _valuesTwo.TryRemove(key, out T newVal);
                return false;
            }

            return true;
        }

        public bool TryRemove(T key, out A value)
        {
            if (!_valuesOne.TryRemove(key, out value))
                return false;

            return _valuesTwo.TryRemove(value, out T result);
        }

        public bool TryRemove(A key, out T value)
        {
            if (!_valuesTwo.TryRemove(key, out value))
                return false;

            return _valuesOne.TryRemove(value, out A result);
        }

        public bool TryGetValue(T key, out A value)
        {
            return _valuesOne.TryGetValue(key, out value);
        }

        public bool TryGetValue(A key, out T value)
        {
            return _valuesTwo.TryGetValue(key, out value);
        }
    }
}
