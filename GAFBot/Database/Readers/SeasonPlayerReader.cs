using GAFBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAFBot.Database.Readers
{
    public sealed class SeasonPlayerReader : BaseDBReader<BotSeasonPlayer>
    {
        public SeasonPlayerReader(GAFContext context = null) : base(context)
        {
            
        }

        public BotSeasonPlayer Add(string osuUsername, long osuUserId, long teamId)
        {
            return base.Add(new BotSeasonPlayer()
            {
                LastOsuUserName = osuUsername,
                OsuUserId = osuUserId,
                TeamId = teamId
            });
        }

        public BotSeasonPlayer Get(long id = -1, long osuUserId = -1, string username = null,
                                             StringComparison usernameComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            DBSearchFunc<BotSeasonPlayer> searchFunc = new DBSearchFunc<BotSeasonPlayer>();
            
            if (id >= 0)
                searchFunc += new Func<BotSeasonPlayer, bool>(p => p.Id == id);

            if (osuUserId >= 0) 
                searchFunc += new Func<BotSeasonPlayer, bool>(p => p.OsuUserId == id);

            if (username != null) 
                searchFunc += new Func<BotSeasonPlayer, bool>(p => p.LastOsuUserName.Equals(username, usernameComparison));

            if (searchFunc.IsNull())
                return null;

            return base.Get(searchFunc.Get());
        }

        public bool Contains(long osuUserId)
        {
            return base.Contains(func: new Func<BotSeasonPlayer, bool>(p => p.OsuUserId == osuUserId));
        }
    }
}
