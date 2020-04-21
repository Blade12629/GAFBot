using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAFStreamTool.Data
{
    public class ApiKey
    {
        public string Key { get; set; }
        public string RegisterCode { get; set; }

        public ApiKey(string key, string registerCode)
        {
            Key = key;
            RegisterCode = registerCode;
        }

        public ApiKey(string key)
        {
            Key = key;
        }

        public ApiKey()
        {
        }

        public bool Read()
        {
            if (!File.Exists("api.key"))
                return false;

            string[] lines = File.ReadAllLines("api.key");

            if (lines.Length < 1)
                return false;

            Key = lines[0];

            if (lines.Length > 1)
                RegisterCode = lines[1];

            return true;
        }
    }
}
