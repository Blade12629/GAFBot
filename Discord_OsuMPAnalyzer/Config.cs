using System;
using System.IO;

namespace Discord_OsuMPAnalyzer
{
    public static class Config
    {
        private static string m_DiscordClientSecret;
        public static string DiscordClientSecret { get { return m_DiscordClientSecret; } }

        private static string m_OsuApiKey;
        public static string OsuApiKey { get { return m_OsuApiKey; } }

        private static string m_ConfigFile = "OMPA.config";

        public static bool LoadConfig()
        {
            string FileAndPath = string.Format(@"{0}\{1}", Directory.GetCurrentDirectory(), m_ConfigFile);

            if (!CheckConfig(FileAndPath)) CreateConfig(FileAndPath);

            using (StreamReader sr = new StreamReader(FileAndPath))
            {
                int line = 0;
                string text;

                while ((text = sr.ReadLine()) != null)
                {
                    line++;
                    string sub = text.Substring(text.IndexOf('=') + 1);
                    switch (line)
                    {
                        case 1:
                            m_OsuApiKey = sub;
                            break;
                        case 2:
                            m_DiscordClientSecret = sub;
                            break;
                    }
                }

                if (m_OsuApiKey == null || m_OsuApiKey.Length < 1)
                {
                    Console.WriteLine("Osu API key not found!");
                    return false;
                }
                if (m_DiscordClientSecret == null || m_DiscordClientSecret.Length < 1)
                {
                    Console.WriteLine("DiscordClientSecret not found!");
                    return false;
                }

                Console.WriteLine("Successfully loaded the config");

                return true;
            }
        }
        

        private static bool CreateConfig(string FileAndPath)
        {
            try
            {
                string Config = string.Format("Osu_Api_Key={0}Discord_Client_Secret={1}", Environment.NewLine, Environment.NewLine);

                using (StreamWriter sw = new StreamWriter(FileAndPath))
                {
                    sw.Write(Config);
                }

                Console.WriteLine("Successfully created the config");
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public static bool CheckConfig(string FileAndPath)
        {
            if (!File.Exists(FileAndPath)) return false;

            return true;
        }

    }
}
