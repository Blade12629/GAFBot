using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer.Web
{
    public static class WebHandler
    {
        private static WebClient m_WebClient;
        private static bool CheckClient { get { return (m_WebClient == null) ? false : true; } }

        public static string Download(string url)
        {
            if (!CheckClient) m_WebClient = new WebClient();

            return m_WebClient.DownloadString(url);
        }
    }
}
