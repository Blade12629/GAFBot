using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.results
{
    public class MultiMatch
    {
        public HistoryJson HistoryJson { get; set; }
        public AnalyzerResult[] Results { get; set; }

        /// <summary>
        /// WinningTeam, draws, <see cref="TeamColor.Red"/> wins, <see cref="TeamColor.Blue"/> wins
        /// </summary>
        public Tuple<TeamColor, int, int, int> EndResult { get; set; }

        /// <summary>
        /// creates an empty <see cref="MultiMatch"/>
        /// </summary>
        public MultiMatch()
        {

        }
        
        /// <summary>
        /// invokes <see cref="CalculateEndResult"/>
        /// </summary>
        public MultiMatch(params AnalyzerResult[] results)
        {
            CalculateEndResult();
        }

        /// <summary>
        /// invokes <see cref="ReadHistoryJson"/> and then <see cref="CalculateEndResult"/>
        /// </summary>
        public MultiMatch(HistoryJson historyJson)
        {
            ReadHistoryJson();
            CalculateEndResult();
        }

        /// <summary>
        /// Reads <see cref="HistoryJson"/> into <see cref="Results"/>
        /// </summary>
        public void ReadHistoryJson()
        {

        }

        /// <summary>
        /// Calculates <see cref="EndResult"/> from <see cref="Results"/>
        /// </summary>
        public void CalculateEndResult()
        {
            int redWins = 0;
            int blueWins = 0;
            int draws = 0;
            
            foreach(AnalyzerResult result in Results)
            {
                switch (result.WinningTeamColor)
                {
                    default:
                    case TeamColor.None:
                        draws++;
                        break;
                    case TeamColor.Red:
                        redWins++;
                        break;
                    case TeamColor.Blue:
                        blueWins++;
                        break;
                }
            }

            EndResult = new Tuple<TeamColor, int, int, int>((redWins > blueWins ? TeamColor.Red : (redWins == blueWins ? TeamColor.None : TeamColor.Blue)),
                                                            draws, redWins, blueWins);
        }
    }
}
