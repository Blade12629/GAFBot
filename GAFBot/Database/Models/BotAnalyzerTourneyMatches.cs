using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotAnalyzerTourneyMatches
    {
        public int Id { get; set; }
        public string ChallongeTournamentName { get; set; }
        public long MatchId { get; set; }
    }
}
