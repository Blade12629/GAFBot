using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.results
{
    public class BanInfo
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string BannedBy { get; set; }

        public BanInfo()
        {

        }

        public BanInfo(string artist, string title, string version, string bannedBy)
        {
            Artist = artist;
            Title = title;
            Version = version;
            BannedBy = bannedBy;
        }
    }
}
