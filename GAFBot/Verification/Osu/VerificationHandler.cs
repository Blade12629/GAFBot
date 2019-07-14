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
        /// <summary>
        /// dUserId, osuUserName, code, codeEz
        /// </summary>
        public ConcurrentDictionary<ulong, (string, string)> ActiveVerifications { get; set; }

        string _host = Program.Config.IrcHost;
        int _port = Program.Config.IrcPort;
        string _user;
        string _password;

        IrcClient _client;

        public static MessageSystem.Logger Logger { get { return Program.Logger; } }

        public bool IsRunning { get; set; }
        public bool Debug { get; set; }

        public VerificationHandler(bool debug = false)
        {
            Debug = debug;
            ActiveVerifications = new ConcurrentDictionary<ulong, (string, string)>();
            Program.SaveEvent += () => Save(Program.VerificationFile);
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
            {
                verificationStr += verificationKey[i].ToString();

                if (i % 3 == 0)
                    verificationStrEz += " " + verificationKey[i].ToString();
                else
                    verificationStrEz += verificationKey[i];
            }

            while (!ActiveVerifications.TryAdd(duserid, (verificationStr, verificationStrEz)))
                Task.Delay(5);

            OnVerificationStart(duserid, verificationStr, verificationStrEz);

            return (verificationStr, verificationStrEz);
        }

        /// <summary>
        /// dUserId, osuUserName, code, codeEz
        /// </summary>
        public event Action<ulong, string, string> OnVerificationStart;
        public event Action<ulong, string> OnVerified;

        /// <summary>
        /// Setup the verification handler
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public void Setup(string user, string password)
        {
            if (Debug)
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
        /// Invoked if there is a new irc message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _client_GotMessage(object sender, NetIrc2.Events.ChatMessageEventArgs e)
        {
            string hostname = e.Sender.Hostname;
            string userName = e.Sender.Nickname;
            Logger.Log($"VerificationHandler: New Message: from {userName} {hostname} to {e.Recipient}: {e.Message}", showConsole: Debug);

            if (!e.Recipient.ToString().StartsWith("Skyfly") || !e.Message.ToString().StartsWith("!verify"))
                return;

            string verifyStr = e.Message.ToString().Remove(0, "!verify ".Length);
            ulong userId = 0;

            Logger.Log($"VerificationHandler: Looking for userid for code: {verifyStr} full string: {e.Message.ToString()}", showConsole: Debug);

            lock(ActiveVerifications)
            {
                int indexOf = ActiveVerifications.Values.ToList().FindIndex(ss => ss.Item1.Equals(verifyStr, StringComparison.CurrentCultureIgnoreCase) || ss.Item2.Equals(verifyStr, StringComparison.CurrentCultureIgnoreCase));
                userId = ActiveVerifications.Keys.ElementAt(indexOf);
            }

            Logger.Log("VerificationHandler: Found userid: " + userId, showConsole: Debug);

            if (userId > 0)
            {
                VerifyUser(userId, userName);
                Logger.Log($"VerificationHandler: Verified user {userId} {userName}", showConsole: Debug);
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
                        var privChannel = Coding.Methods.GetPrivChannel(duserId);
                        privChannel.SendMessageAsync("Your user account has already been linked to an discord account" + Environment.NewLine +
                            "If this is an error or is incorrect, please contact Skyfly on discord (??????#0284 (6*?)))").Wait();
                        return;
                    }
                }

                User user = null;

                while (!Program.MessageHandler.Users.TryGetValue(duserId, out user))
                    Task.Delay(5).Wait();

                if (ActiveVerifications.ContainsKey(duserId))
                    while (!ActiveVerifications.TryRemove(duserId, out (string, string) value))
                        Task.Delay(5).Wait();

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
            User user = Program.MessageHandler.Users[duserId];

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
            User user = Program.MessageHandler.Users.Values.ToList().Find(u => u.OsuUserName.Equals(osuUsername, StringComparison.CurrentCultureIgnoreCase));

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
                ActiveVerifications = Newtonsoft.Json.JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, (string, string)>>(json);
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
}
