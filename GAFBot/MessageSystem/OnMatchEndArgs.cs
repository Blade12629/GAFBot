using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.MessageSystem
{
    public class OnMatchEndArgs : EventArgs
    {
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public string WinningTeam { get; set; }

        public OnMatchEndArgs(string team1, string team2, string winningTeam)
        {
            Team1 = team1;
            Team2 = team2;
            WinningTeam = winningTeam;
        }
    }
}
