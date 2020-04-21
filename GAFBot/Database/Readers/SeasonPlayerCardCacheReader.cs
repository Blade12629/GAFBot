using System;
using System.Collections.Generic;
using System.Text;
using GAFBot.Database.Models;
using GAFBot.Database;

namespace GAFBot.Database.Readers
{
    public sealed class SeasonPlayerCardCacheReader : BaseDBReader<BotSeasonPlayerCardCache>
    {
        public SeasonPlayerCardCacheReader(GAFContext context = null) : base(context)
        {

        }

        public BotSeasonPlayerCardCache Get(long id = -1, long osuUserId = -1, string osuUsername = null,
                                            StringComparison userComparer = StringComparison.CurrentCultureIgnoreCase,
                                            string teamName = null, StringComparison teamComparer = StringComparison.CurrentCultureIgnoreCase)
        {
            DBSearchFunc<BotSeasonPlayerCardCache> searchFunc = new DBSearchFunc<BotSeasonPlayerCardCache>();

            if (id > -1)
                searchFunc += new Func<BotSeasonPlayerCardCache, bool>(cc => cc.Id == id);
            if (osuUserId > -1)
                searchFunc += new Func<BotSeasonPlayerCardCache, bool>(cc => cc.OsuUserId == osuUserId);
            if (osuUsername != null)
                searchFunc += new Func<BotSeasonPlayerCardCache, bool>(cc => cc.Username.Equals(osuUsername, userComparer));
            if (teamName != null)
                searchFunc += new Func<BotSeasonPlayerCardCache, bool>(cc => cc.TeamName.Equals(teamName, teamComparer));

            if (searchFunc.IsNull())
                return null;

            return base.Get(searchFunc.Get());
        }

        public bool Contains(long id = -1, long osuUserId = -1, string osuUsername = null, 
                             StringComparison userComparer = StringComparison.CurrentCultureIgnoreCase)
        {
            return Get(id, osuUserId, osuUsername, userComparer) != null;
        }
    }
}
