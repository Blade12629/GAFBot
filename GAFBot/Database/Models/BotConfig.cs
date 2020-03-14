using System;
using System.Collections.Generic;

namespace GAFBot.Database.Models
{
    public partial class BotConfig
    {
        public int Id { get; set; }
        public string CurrentSeason { get; set; }
        public string DiscordClientSecretEncrypted { get; set; }
        public string OsuApiKeyEncrypted { get; set; }
        public string OsuIrcHost { get; set; }
        public int OsuIrcPort { get; set; }
        public string OsuIrcUser { get; set; }
        public string OsuIrcPasswordEncrypted { get; set; }
        public string WebsiteUser { get; set; }
        public string WebsitePassEncrypted { get; set; }
        public string WebsiteHost { get; set; }
        public int WarmupMatchCount { get; set; }
        public long AnalyzeChannel { get; set; }
        public TimeSpan AutoSaveTime { get; set; }
        public long DiscordGuildId { get; set; }
        public long VerifiedRoleId { get; set; }
        public string WelcomeMessage { get; set; }
        public long WelcomeChannel { get; set; }
        public long RefereeRoleId { get; set; }
        public bool SetVerifiedRole { get; set; }
        public bool SetVerifiedName { get; set; }
    }
}
