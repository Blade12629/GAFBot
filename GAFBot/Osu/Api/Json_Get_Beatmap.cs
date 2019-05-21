using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAFBot.Osu.Api
{
#pragma warning disable IDE1006 //naming styles, lowerCaseWarning
    public class Json_Get_Beatmap
    {
        public ApprovedEnum approved { get; set; }
        public DateTime approved_date { get; set; }
        public DateTime last_update { get; set; }
        public string artist { get; set; }
        public int beatmap_id { get; set; }
        public int beatmapset_id { get; set; }
        public int bpm { get; set; }
        public string creator { get; set; }
        public float difficultyrating { get; set; }
        public float diff_size { get; set; }
        public float diff_overall { get; set; }
        public float diff_approach { get; set; }
        public float diff_drain { get; set; }
        public int hit_length { get; set; }
        public string source { get; set; }
        public GenreEnum genre_id { get; set; }
        public LanguageIDEnum language_id { get; set; }
        public string title { get; set; }
        public int total_length { get; set; }
        public string version { get; set; }
        public string file_md5 { get; set; }
        public GameModeEnum mode { get; set; }
        public string tags { get; set; }
        public int favourite_count { get; set; }
        public int playcount { get; set; }
        public int passcount { get; set; }
        public int max_combo { get; set; }
    }
#pragma warning restore IDE1006
}
