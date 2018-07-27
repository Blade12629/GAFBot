using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer.Analyze_Format
{
    public static class OsuAnalyzer
    {
        public class HistoryReader
        {
            private Analyze_Format.Analyzer.NewAnalyzer m_analyzer;
            private bool m_didRead;
            private string[] m_Output;

            public bool didRead { get { return m_didRead; } }
            public Analyzer.NewAnalyzer Analyzer { get { return m_analyzer; } }
            public string[] Output { get { return m_Output; } }

            public void CreateAnalyzer(int matchID)
            {
                if (m_didRead)
                    return;

                m_analyzer = new Analyzer.NewAnalyzer();
                m_analyzer.CreateStatistic("https://osu.ppy.sh/community/matches/" + matchID + "/history");
                m_Output = m_analyzer.Statistic;
                m_didRead = true;
            }
        }

        public static HistoryReader ParseMatch(string Input)
        {
            int MatchId = -1;

            if (int.TryParse(Input, out int Presult))
                MatchId = Presult;
            
            if (Input.Contains("https://osu.ppy.sh/community/matches/"))
            {
                string[] split = Input.Split('/');

                foreach (string s in split)
                    if (int.TryParse(s, out int PresultSplit))
                    {
                        MatchId = PresultSplit;
                        break;
                    }
            }

            if (MatchId == -1)
                return null;

            HistoryReader hr = new HistoryReader();
            hr.CreateAnalyzer(MatchId);
            return hr;
        }
    }
}
