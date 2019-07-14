using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GAFBot
{
    public class Config
    {
        public string DiscordClientSecret { get; set; }
        public string OsuApiKey { get; set; }
        public int WarmupMatchCount { get; set; }
        public bool PickEmChallengeEnabled { get; set; }
        /*
         * ToDo: add command to set challonge tournament name
         */
        public string ChallongeTournamentName { get; set; }

        public string ChallongeApiKey { get; set; }

        public ulong AnalyzeChannel { get; set; }

        public string UserFile { get; set; }
        public string VerificationFile { get; set; }
        public string IrcHost { get; set; }
        public int IrcPort { get; set; }
        public string IrcUser { get; set; }
        public string IrcPass { get; set; }
        public bool Debug { get; set; }
        public string LogFile { get; set; }
        public double AutoSaveTime { get; set; }
        public ulong[] DefaultDiscordAdmins { get; set; }
        public ulong[] BypassVerification { get; set; }
        public ulong DiscordGuildId { get; set; }
        public ulong VerifiedRoleId { get; set; }
        public string BettingFile { get; set; }
        public string WelcomeMessage { get; set; }
        public ulong WelcomeChannel { get; set; }
        public int CurrentBettingReward { get; set; }
        public ulong BetChannel { get; set; }
        public ulong DevChannel { get; set; }
        public ulong RefereeRoleId { get; set; }

        /// <summary>
        /// Loads the config
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Config LoadConfig(string file)
        {
            Config config = GetDefault();

            FileInfo ConfigFile = new FileInfo(file);

            if (!ConfigFile.Exists)
                return config;

            string json = File.ReadAllText(ConfigFile.FullName);

            config = JsonConvert.DeserializeObject<Config>(json);

            return config;
        }

        /// <summary>
        /// Gets a default config file (not ready for use!). use this to save the config if you have none
        /// </summary>
        /// <returns></returns>
        public static Config GetDefault()
            => new Config()
                {
                    DiscordClientSecret = "https://discordapp.com/developers/applications/YourApplicationID/bots",
                    OsuApiKey = "https://osu.ppy.sh/p/api/",
                    WarmupMatchCount = 2,
                    PickEmChallengeEnabled = false,
                    IrcHost = "irc.ppy.sh",
                    AnalyzeChannel = 123456789,
                    ChallongeApiKey = "https://challonge.com/settings/developer",
                    ChallongeTournamentName = "TourneyName",
                    Debug = true,
                    IrcPass = "IrcPass",
                    IrcUser = "IrcUser",
                    IrcPort = 6667,
                    UserFile = @"\users.json",
                    VerificationFile = @"\verifications.json",
                    LogFile = @"\logs.txt",
                    AutoSaveTime = TimeSpan.FromMinutes(15).TotalMilliseconds,
                    DefaultDiscordAdmins = new ulong[] { 1234567890, 0123456789 },
                    BypassVerification = new ulong[] { 123456789 },
                    VerifiedRoleId = 0,
                    DiscordGuildId = 0,
                    BettingFile = @"\betting.json",
                    CurrentBettingReward = 16
                };

        /// <summary>
        /// Saves the config
        /// </summary>
        /// <param name="file"></param>
        /// <param name="config"></param>
        public static void SaveConfig(string file, Config config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file, json);
        }
    }
}
