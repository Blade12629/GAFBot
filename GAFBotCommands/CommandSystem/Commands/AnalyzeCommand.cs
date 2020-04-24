using GAFBot;
using GAFBot.Commands;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace GAFBot.Commands
{
    public class AnalyzeCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "analyze"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public string Description => "Options for the analyzer";

        public string DescriptionUsage => "!analyze" + Environment.NewLine +
                                          "!analyze <-qualifier:true/[false]> <-db:true/[false]> <-api:true/[false]> mplink" + Environment.NewLine + 
                                          "example:" + Environment.NewLine +
                                          "!analyze -qualifier:true -db:true mplink" + Environment.NewLine +
                                          "<> = optional, [] = default";

        public static void Init()
        {
            Program.CommandHandler.Register(new AnalyzeCommand());
            Logger.Log(nameof(AnalyzeCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.AfterCMD))
                {
                    Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                    return;
                }

                bool db = false;
                bool api = false;
                bool qualifierStage = false;

                string parameters = e.AfterCMD;
                (string, bool) paramParsed;
                while ((paramParsed = ParseSpecial(ref parameters)).Item1 != null)
                {
                    switch (paramParsed.Item1)
                    {
                        case "db":
                            db = paramParsed.Item2;
                            break;
                        case "api":
                            api = paramParsed.Item2;
                            break;
                        case "qualifier":
                            qualifierStage = paramParsed.Item2;
                            break;
                    }
                }

                IMessageHandler messageHandler = Modules.ModuleHandler.Get("message") as IMessageHandler;

                if (qualifierStage)
                    messageHandler.StartQualifierAnalyzer(parameters, e.ChannelID, db);
                else
                {
                    const string historyUrl = "https://osu.ppy.sh/community/matches/";
                    const string historyUrlVariant = "https://osu.ppy.sh/mp/";

                    if (parameters.Contains(historyUrlVariant))
                        parameters = parameters.Replace(historyUrlVariant, historyUrl);

                    string matchIdString = parameters.Remove(0, historyUrl.Length).Trim('>', '<').Trim('/');

                    if (!int.TryParse(matchIdString, out int matchId))
                    {
                        Coding.Discord.SendMessage(e.ChannelID, "Could not parse matchid: " + matchIdString);

                        return;
                    }


                    using (GAFContext context = new GAFContext())
                    {
                        if (Statistic.StatsHandler.UpdateSeasonStatistics(parameters.Trim('>', '<'), Program.Config.CurrentSeason, context))
                        {
                            var matchEmbed = Statistic.StatsHandler.GetMatchResultEmbed(matchId, context);
                            Coding.Discord.GetChannel(e.ChannelID).SendMessageAsync(embed: matchEmbed);
                        }
                        else
                            Coding.Discord.SendMessage(e.ChannelID, "Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Trace);
            }
                //messageHandler.StartAnalyzer(parameters, e.ChannelID, api, db);
        }

        private (string, bool) ParseSpecial(ref string input)
        {
            int index = input.IndexOf('-');

            if (index < 0)
                return (null, false);

            input = input.Substring(0, index + 1).TrimStart('-');
            index = input.IndexOf(' ');

            string parsed = "";

            if (index > -1)
            {
                parsed = input.Substring(0, index);
                input = input.Remove(0, index + 1);
            }
            else
            {
                parsed = input;
                input = "";
            }

            string[] parsedSplit = parsed.Split(':');
            string prop = parsedSplit[0];
            bool result = false;

            bool.TryParse(parsedSplit[1], out result);

            return (prop.ToLower(), result);
        }
    }
}
