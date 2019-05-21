using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.MessageSystem
{
    public class User : IEquatable<User>
    {
        public ulong DiscordID { get; set; }
        public long Points { get; set; }
        public DateTime RegisteredOn { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public bool Verified { get; set; }
        public string OsuUserName { get; set; }

        public User(ulong discordID, long points, AccessLevel accessLevel, DateTime registeredOn, bool verified)
        {
            DiscordID = discordID;
            Points = points;
            RegisteredOn = registeredOn;
            AccessLevel = accessLevel;
            Verified = verified;
        }

        public User()
        {

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User other)
        {
            return other != null &&
                   DiscordID == other.DiscordID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DiscordID);
        }

        public static bool operator ==(User user1, User user2)
        {
            return EqualityComparer<User>.Default.Equals(user1, user2);
        }

        public static bool operator !=(User user1, User user2)
        {
            return !(user1 == user2);
        }
    }
}
