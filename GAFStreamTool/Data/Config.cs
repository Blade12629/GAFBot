using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAFStreamTool.Data
{
    public class Config
    {
        public string Host;
        public string Port;
        public int[] RGBText;
        public int[] RGBBackground;

        public Config(string host, string port)
        {
            Host = host;
            Port = port;
            RGBText = new int[3]
            {
                255,
                255,
                255
            };
            RGBBackground = new int[3]
            {
                255,
                255,
                255
            };
        }

        public bool Load()
        {
            if (!File.Exists("config.cfg"))
                return false;

            string[] lines = File.ReadAllLines("config.cfg");

            if (lines.Length < 4)
                return false;
        
            Host = lines[0].Trim(' ');
            Port = lines[1].Trim(' ');
            RGBText = SplitStringToInt(lines[2], ',');
            RGBBackground = SplitStringToInt(lines[3], ',');

            return true;
        }

        private int[] SplitStringToInt(string text, char splitter)
        {
            string[] split = text.Split(splitter);
            List<int> result = new List<int>();

            foreach (string s in split)
                if (int.TryParse(s, out int r))
                    result.Add(r);

            return result.ToArray();
        }

        private string IntArrayToString(int[] array, char splitter)
        {
            string result = array[0].ToString();

            for (int i = 1; i < array.Length; i++)
                result += "," + array[i];

            return result;
        }

        public void Save()
        {
            string[] lines = new string[]
            {
                Host,
                Port.ToString(),
                IntArrayToString(RGBText, ','),
                IntArrayToString(RGBBackground, ',')
            };

            if (File.Exists("config.cfg"))
                File.Delete("config.cfg");

            File.WriteAllLines("config.cfg", lines);
        }
    }
}
