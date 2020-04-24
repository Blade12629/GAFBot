using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotTimer
    {
        public long Id { get; set; }
        public bool Enabled { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string PingMessage { get; set; }
        public long CreatedByDiscordId { get; set; }
        public long DiscordChannelId { get; set; }
        public bool IsPrivateChannel { get; set; }
        public bool Expired { get; set; }

        public BotTimer(bool enabled, DateTime startTime, DateTime endTime, long createdByDiscordId, bool expired, long discordChannelId, bool isPrivateChannel, string pingMessage)
        {
            Enabled = enabled;
            StartTime = startTime;
            EndTime = endTime;
            CreatedByDiscordId = createdByDiscordId;
            Expired = expired;
            DiscordChannelId = discordChannelId;
            IsPrivateChannel = isPrivateChannel;
            PingMessage = pingMessage;
        }

        public BotTimer()
        {
        }
    }
}
