using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using GAFBot;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using GAFBot.Verification.Osu;
using NetIrc2;
using NetIrc2.Events;

namespace VerificationModule
{
    public class VerificationModule : IVerificationHandler, IDisposable
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsRunning { get; set; }
        /// <summary>
        /// Always enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return true;
            }
            set
            {

            }
        }

        private string _user;
        private string _password;
        private IrcClient _client;
        /// <summary>
        /// discordmessage, user, string to add result
        /// </summary>
        private List<(DSharpPlus.Entities.DiscordMessage, string, string)> _userStatusQueue;

        public string ModuleName => "verification";

        public void Enable()
        {
            try
            {
                Logger.Log("VerificationHandler: Starting irc", LogLevel.Trace);

                _userStatusQueue = new List<(DSharpPlus.Entities.DiscordMessage, string, string)>();

                BotConfig cfg;
                using (GAFContext context = new GAFContext())
                {
                    int cfgIndex = context.BotConfig.Max(c => c.Id);
                    cfg = context.BotConfig.First(c => c.Id == cfgIndex);
                }

                SetHost(cfg.OsuIrcHost, cfg.OsuIrcPort);
                SetAuthentication(cfg.OsuIrcUser, Program.DecryptString(cfg.OsuIrcPasswordEncrypted));

                Program.SaveEvent += () =>
                {
                    Stop();
                    Start();
                };

                Start();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }

        public void Disable()
        {
            Stop();
        }

        /// <summary>
        /// Client got a new message
        /// </summary>
        public void ClientGotMessage(object sender, ChatMessageEventArgs e)
        {
            try
            {
                string hostname = e.Sender.Hostname.ToString();
                string nickName = e.Sender.Nickname.ToString();
                string recipient = e.Recipient.ToString();
                string message = e.Message.ToString();

                Logger.Log($"IRC: New Message: from {nickName} {hostname} to {recipient}: {message}");
                //!verify 35483
                if (!recipient.Equals(_user))
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
                            Logger.Log(ex.ToString(), LogLevel.ERROR);
                        }
                    }
                }

                if (!message.StartsWith("!verify"))
                    return;

                if (message.Length <= "!verify ".Length)
                {
                    Logger.Log($"IRC: {nickName} - Verification id cannot be empty", LogLevel.Trace);

                    using (GAFContext context = new GAFContext())
                        ircMsg(context.BotLocalization.First(l => l.Code.Equals("verifyIDEmpty")).Code);

                    return;
                }
                string verifyStr = message.Remove(0, "!verify ".Length);
                //!verify 35483
                //35483

                Logger.Log($"VerificationHandler: Looking for userid for code: {verifyStr} full string: {message}", LogLevel.Trace);

                if (string.IsNullOrEmpty(verifyStr))
                {
                    Logger.Log($"IRC: {nickName} - Verification id cannot be empty", LogLevel.Trace);

                    using (GAFContext context = new GAFContext())
                        ircMsg(context.BotLocalization.First(l => l.Code.Equals("verifyIDEmpty")).Code);

                    return;
                }

                BotVerifications bver;

                using (GAFContext context = new GAFContext())
                    bver = context.BotVerifications.First(bv => bv.Code.Equals(verifyStr)); ;

                if (bver == null)
                {
                    Logger.Log($"IRC: {nickName} - Could not find your verification id |{bver.DiscordUserId}|{verifyStr}|", LogLevel.Trace);

                    using (GAFContext context = new GAFContext())
                        ircMsg($"{context.BotLocalization.First(l => l.Code.Equals("verifyIDNotFound")).Code}|{verifyStr}|");
                    
                    return;
                }

                Logger.Log($"IRC: {nickName} - Verifying", LogLevel.Trace);

                using (GAFContext context = new GAFContext())
                    ircMsg(context.BotLocalization.First(l => l.Code.Equals("verifyVerifying")).String);
                
                VerifyUser((ulong)bver.DiscordUserId, nickName, e.Sender.Nickname);

                void ircMsg(string msg)
                {
                    _client.Message(e.Sender.Nickname, new IrcString(msg));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"IRC: " + ex, LogLevel.Trace);
            }
        }

        /// <summary>
        /// Calls <see cref="Stop"/>
        /// </summary>
        public void Dispose()
        {
            Stop();
            _userStatusQueue?.Clear();
        }

        /// <summary>
        /// Initializes the module
        /// </summary>
        public void Initialize()
        {
            Enable();
        }

        /// <summary>
        /// Checks if user is verified
        /// </summary>
        /// <param name="duserid">discord user id</param>
        /// <returns>verified</returns>
        public bool IsUserVerified(ulong duserid)
        {
            BotUsers buser;

            using (GAFContext context = new GAFContext())
                buser = context.BotUsers.First(bm => (ulong)bm.DiscordId == duserid);

            if (buser == null)
                return false;

            return buser.IsVerified;
        }

        /// <summary>
        /// Checks if user is verified
        /// </summary>
        /// <param name="osuUser">discord user id</param>
        /// <returns>verified</returns>
        public bool IsUserVerified(string osuUser)
        {
            BotUsers buser;

            using (GAFContext context = new GAFContext())
                buser = context.BotUsers.First(bu => bu.OsuUsername.Equals(osuUser));

            if (buser == null)
                return false;

            return buser.IsVerified;
        }

        /// <summary>
        /// Sets the login data
        /// </summary>
        /// <param name="user">username</param>
        /// <param name="pass">password</param>
        public void SetAuthentication(string user, string pass)
        {
            _user = user;
            _password = pass;
        }

        /// <summary>
        /// Sets the host and port
        /// </summary>
        /// <param name="host">Hostname</param>
        /// <param name="port">Port</param>
        public void SetHost(string host, int port)
        {
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Connects and authenticates
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                return;

            _client = new IrcClient();
            _client.GotMessage += ClientGotMessage;
            _client.Connect(Host, Port);

            while (!_client.IsConnected)
                Task.Delay(5).Wait();

            IsRunning = true;

            _client.IrcCommand(new IrcString($"PASS"), new IrcString(_password));
            _client.IrcCommand(new IrcString($"NICK"), new IrcString(_user));

            Logger.Log("VerificationHandler: Sent Login data", LogLevel.Trace);
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Stop()
        {
            if (_client == null || !IsRunning)
                return;
            
            Logger.Log("VerificationHandler: Stopping irc");

            _client.Close();

            IsRunning = false;
        }
        
        /// <summary>
        /// Starts the verification process
        /// </summary>
        /// <param name="duserid">Discord user id</param>
        /// <returns>Verification code</returns>
        public string StartVerification(ulong duserid)
        {
            if (duserid == 0)
            {
                Logger.Log("Discord id cannot be 0");
                return null;
            }

            BotVerifications bver;
            using (GAFContext context = new GAFContext())
                bver = context.BotVerifications.FirstOrDefault(bv => (ulong)bv.DiscordUserId == duserid);

            if (bver != null)
            {
                string verifyAlreadyActiveLocale;
                using (GAFContext context = new GAFContext())
                    verifyAlreadyActiveLocale = context.BotLocalization.First(l => l.Code.Equals("verifyAlreadyActive")).String;

                Coding.Discord.SendPrivateMessage(duserid, $"{verifyAlreadyActiveLocale} {bver.Code}");
                return "active";
            }

            byte[] verificationKey = new byte[2];
            RandomNumberGenerator.Fill(verificationKey);

            string verificationStr = "";

            for (int i = 0; i < verificationKey.Length; i++)
                verificationStr += verificationKey[i].ToString();

            bver = new BotVerifications()
            {
                DiscordUserId = (long)duserid,
                Code = verificationStr,
            };

            using (GAFContext context = new GAFContext())
            {
                context.BotVerifications.Add(bver);
                context.SaveChanges();
            }

            Logger.Log("Verification started for " + duserid);

            return verificationStr;
        }
        
        /// <summary>
        /// Verifies the user
        /// </summary>
        /// <param name="duserid">Discord user id</param>
        /// <param name="osuUser">Osu username</param>
        /// <param name="senderOsu">Osu username to respond, null = no response in osu</param>
        public void VerifyUser(ulong duserid, string osuUser, string senderOsu)
        {
            Logger.Log("VerificationHandler: Verifying user " + duserid + " | " + osuUser, LogLevel.Trace);

            BotUsers buser;
            using (GAFContext context = new GAFContext())
                buser = context.BotUsers.First(bu => (ulong)bu.DiscordId == duserid);

            if (buser == null)
            {
                (GAFBot.Modules.ModuleHandler.Get("message") as IMessageHandler)?.Register(Coding.Discord.GetUser(duserid), (ulong)Program.Config.DiscordGuildId);

                using (GAFContext context = new GAFContext())
                    buser = context.BotUsers.First(bu => (ulong)bu.DiscordId == duserid);

                if (buser == null)
                {
                    string notFoundMessage;

                    using (GAFContext context = new GAFContext())
                        notFoundMessage = context.BotLocalization.First(l => l.Code.Equals("verifyUserNotFound")).String;

                    Logger.Log(notFoundMessage, LogLevel.Trace);

                    Coding.Discord.SendPrivateMessage(duserid, $"{notFoundMessage} {duserid}");

                    if (senderOsu != null)
                        ircMsg($"{notFoundMessage} {duserid}");

                    return;
                }
            }

            if (!string.IsNullOrEmpty(buser.OsuUsername) && buser.IsVerified)
            {
                Logger.Log("Account already linked: " + osuUser, LogLevel.Trace);

                string alreadyLinkedLocale;
                using (GAFContext context = new GAFContext())
                    alreadyLinkedLocale = context.BotLocalization.First(l => l.Code.Equals("verifyAccountAlreadyLinked")).String;

                Coding.Discord.SendPrivateMessage(duserid, alreadyLinkedLocale);

                if (senderOsu != null)
                    ircMsg(alreadyLinkedLocale);

                return;
            }

            using (GAFContext context = new GAFContext())
            {
                var bver = context.BotVerifications.First(bv => (ulong)bv.DiscordUserId == duserid);

                if (bver != null)
                    context.BotVerifications.Remove(bver);

                buser.IsVerified = true;
                buser.OsuUsername = osuUser;

                context.BotUsers.Update(buser);
                context.SaveChanges();
            }

            if (Program.Config.SetVerifiedRole)
                Coding.Discord.AssignRole(duserid, (ulong)Program.Config.DiscordGuildId, (ulong)Program.Config.VerifiedRoleId, "Verified, osu: " + osuUser);
            if (Program.Config.SetVerifiedName)
                Coding.Discord.SetUserName(duserid, (ulong)Program.Config.DiscordGuildId, osuUser.Replace("_", " "), "verified");
            
            var dclient = Coding.Discord.GetClient();
            var duser = dclient.GetUserAsync(duserid).Result;
            var dDmChat = dclient.CreateDmAsync(duser).Result;

            string localeLinked;
            string localeLinked2;

            using (GAFContext context = new GAFContext())
            {
                localeLinked = context.BotLocalization.First(l => l.Code.Equals("verifyAccountLinked")).String;
                localeLinked2 = context.BotLocalization.First(l => l.Code.Equals("verifyAccountLinked2")).String;
            }

            string message = $"{localeLinked} {duser.Username}#{duser.Discriminator} ({duser.Id}) {localeLinked2} {osuUser}";

            dDmChat.SendMessageAsync(message).Wait();
            ircMsg(message);

            Logger.Log(message);

            void ircMsg(string msg)
            {
                if (senderOsu == null)
                    return;

                _client.Message(senderOsu, new IrcString(msg));
            }
        }

        /// <summary>
        /// Requests a user status
        /// </summary>
        /// <param name="user">Osu username</param>
        /// <param name="message">discord message</param>
        /// <param name="originalText">original text</param>
        public void GetUserStatus(string user, DiscordMessage message, string originalText)
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
    }
}
