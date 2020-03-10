using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GAFBot
{
    public static class Localization
    {
        public static ConcurrentDictionary<string, string> Localizations { get; private set; }

        public static string Get(string key)
        {
            if (!Localizations.ContainsKey(key))
                return "";

            return Localizations[key];
        }

        public static void Init(string file = null)
        {
            Localizations = new ConcurrentDictionary<string, string>();
            if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
            {
                using (System.IO.StreamReader sreader = new System.IO.StreamReader(file))
                {
                    string line;
                    string[] split;
                    while (!sreader.EndOfStream)
                    {
                        line = sreader.ReadLine();

                        if (line.StartsWith("//") || !line.Contains('='))
                            continue;

                        split = line.Split('=');

                        if (split.Length == 1)
                            continue;

                        if (Localizations.ContainsKey(split[0]))
                        {
                            Localizations[split[0]] = split[1];
                            return;
                        }

                        if (!Localizations.TryAdd(split[0], split[1]))
                            Logger.Log($"Could not add localization {split[0]} with value {split[1]}", LogLevel.WARNING);
                    }
                }
            }
            else
                CreateDefault(file);
        }

        private static void CreateDefault(string file)
        {
            Localizations.TryAdd("verifyIDEmpty", "Verification id cannot be empty");
            Localizations.TryAdd("verifyIDNotFound", "Could not find your verification id");
            Localizations.TryAdd("verifyVerifying", "Verifying...");
            Localizations.TryAdd("verifyUserIDNotFound", "Could not find your userId");
            Localizations.TryAdd("verifyAccountAlreadyLinked", "Your user account has already been linked to an discord account{/nl}If this is an error or is incorrect, please contact Skyfly on discord (??????#0284 (6*?)))");
            Localizations.TryAdd("verifyUserNotFound", "Could not find user");
            Localizations.TryAdd("verifyAccountLinked", "You have successfully linked your discord account to your osu! account");

            Localizations.TryAdd("analyzerTeam", "Team");
            Localizations.TryAdd("analyzerWon", "won!");
            Localizations.TryAdd("analyzerMVP", "Most Valuable Player");
            Localizations.TryAdd("analyzerHighestScore", "Highest Score");
            Localizations.TryAdd("analyzerHighestAcc", "Highest Accuracy");
            Localizations.TryAdd("analyzerFirst", "First Place");
            Localizations.TryAdd("analyzerSecond", "Second Place");
            Localizations.TryAdd("analyzerThird", "Third Place");
            Localizations.TryAdd("analyzerAverageAcc", "Average Acc");
            Localizations.TryAdd("analyzerAcc", "Accuracy");
            Localizations.TryAdd("analyzerTeamBlue", "Team Blue");
            Localizations.TryAdd("analyzerTeamRed", "Team Red");
            Localizations.TryAdd("analyzerOnMap", "on the map");
            Localizations.TryAdd("analyzerWith", "with");
            Localizations.TryAdd("analyzerWithPoints", "Points and");
            Localizations.TryAdd("analyzerMatchPlayed", "Match played at");
            Localizations.TryAdd("analyzerGeneratedPerformanceScore", "GPS");



            Localizations.TryAdd("notifyBugFound", "unused for now");
            Localizations.TryAdd("notifySuggestion", "unused for now");

            Save(file);
        }

        public static void Save(string file)
        {
            using (System.IO.StreamWriter swriter = new System.IO.StreamWriter(file, false))
            {
                foreach (var locale in Localizations)
                    swriter.WriteLine(locale.Key + "=" + locale.Value.Replace(Environment.NewLine, "{/nl}"));
            }
        }
    }
}
