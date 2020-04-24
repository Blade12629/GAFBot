using DSharpPlus.Entities;
using GAFBot;
using GAFBot.Commands;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.Database.Readers;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace GAFBot.Commands
{
    public class AnalyzerCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "analyzer"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public string Description => "Analyzer Management";

        public string DescriptionUsage => "```\n" +
                                          "!analyzer report messageLink matchLink report reason\n" +
                                          "!analyzer delete match*\n" +
                                          "!analyzer recalculate matchid [messageLink]\n" +
                                          "(*: match link or matchId)\n" +
                                          "```";

        const string HISTORY_URL = "https://osu.ppy.sh/community/matches/";
        const string HISTORY_URL_VARIANT = "https://osu.ppy.sh/mp/";

        //Skyfly
        const long REPORT_DEST_ID = 140896783717892097;

        public static void Init()
        {
            Program.CommandHandler.Register(new AnalyzerCommand());
            Logger.Log(nameof(AnalyzerCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                string message = e.AfterCMD.Replace("<", "").Replace(">", "");
                string[] paramSplit = message.Split(' ');

                if (paramSplit.Length < 2)
                {
                    Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                    return;
                }
                switch (paramSplit[0].ToLower())
                {
                    default:
                        Coding.Discord.SendMessage(e.ChannelID, $"Parameter {paramSplit[0]} not found!");
                        break;

                    case "report":
                        {
                            message = message.Remove(0, paramSplit[0].Length + 1);
                            paramSplit = paramSplit.Skip(1).ToArray();
                            string matchLink = paramSplit[1];
                            string[] dsplit = paramSplit[0].Split('/').Skip(4).ToArray();
                            
                            if (!ulong.TryParse(dsplit[1], out ulong dchannelId))
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "Could not parse channel id " + dsplit[1]);
                                return;
                            }
                            if (!ulong.TryParse(dsplit[2], out ulong dmsgId))
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "Could not parse msg id " + dsplit[1]);
                                return;
                            }

                            matchLink = matchLink.Replace(HISTORY_URL_VARIANT, HISTORY_URL);
                            string matchIdStr = matchLink.Remove(0, HISTORY_URL.Length);

                            if (!long.TryParse(matchIdStr, out long matchId))
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "Could not parse matchId " + matchIdStr);
                                return;
                            }

                            var dchannel = Coding.Discord.GetChannel(dchannelId);
                            var dmsg = dchannel.GetMessageAsync(dmsgId).Result;
                            DiscordEmbedBuilder placeholderBuilder = new DiscordEmbedBuilder()
                            {
                                Title = "Match Stats will be available at a later time"
                            };

                            dmsg.ModifyAsync(content: null, embed: placeholderBuilder.Build()).Wait();

                            Delete(matchId, e, null, true);

                            string reason = message.Remove(0, paramSplit[0].Length + paramSplit[1].Length + 2);

                            Coding.Discord.SendPrivateMessage(REPORT_DEST_ID, $"Message link: {paramSplit[0]}\nMatch stats reported: {matchLink}\nReason: {(string.IsNullOrEmpty(reason) ? "null" : reason)}");
                            Coding.Discord.SendMessage(e.ChannelID, "Report submitted");
                        }
                        break;

                    case "delete":
                        {
                            string matchLink;
                            bool partialDelete = false;

                            if (paramSplit.Length == 3)
                            {
                                matchLink = ParseMatchToHistoryUrl(paramSplit[2]);

                                bool.TryParse(paramSplit[1], out partialDelete);
                            }
                            else
                                matchLink = ParseMatchToHistoryUrl(paramSplit[1]);


                            if (string.IsNullOrEmpty(matchLink))
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "Could not parse match " + paramSplit[1]);
                                return;
                            }

                            Delete(matchLink, e, paramSplit.Skip(2).ToArray(), partialDelete);
                        }
                        break;

                    case "recalculate":
                        {
                            string matchLink = ParseMatchToHistoryUrl(paramSplit[1]);

                            if (string.IsNullOrEmpty(matchLink))
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "Could not parse match " + paramSplit[1]);
                                return;
                            }

                            Recalculate(matchLink, e, paramSplit.Skip(2).ToArray(), paramSplit[2]);
                        }
                        break;

                    case "post":
                        {
                            if (!long.TryParse(paramSplit[1], out long matchId))
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "Could not parse matchId " + paramSplit[1]);
                                return;
                            }

                            if (!ulong.TryParse(paramSplit[2], out ulong channelId))
                            {
                                Coding.Discord.SendMessage(e.ChannelID, "Could not parse channelId " + paramSplit[2]);
                                return;
                            }

                            DiscordEmbed embed;
                            using (GAFContext context = new GAFContext())
                                embed = Statistic.StatsHandler.GetMatchResultEmbed(matchId, context);

                            Coding.Discord.GetChannel(channelId).SendMessageAsync(embed: embed).Wait();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
            }
        }

        private string ParseMatchToHistoryUrl(string input)
        {
            if (long.TryParse(input, out long matchId))
                return HISTORY_URL + input;

            string inp = input;

            if (input.StartsWith(HISTORY_URL_VARIANT, StringComparison.CurrentCultureIgnoreCase))
                inp = input.Replace(HISTORY_URL_VARIANT, HISTORY_URL);
            else if (!input.StartsWith(HISTORY_URL, StringComparison.CurrentCultureIgnoreCase))
                return null;

            return inp;
        }

        private long MatchUrlToId(string matchUrl)
        {
            string matchIdStr = matchUrl.Remove(0, HISTORY_URL.Length);

            if (!long.TryParse(matchIdStr, out long matchId))
                matchId = -1;

            return matchId;
        }

        private void Delete(string matchUrl, CommandEventArg e, string[] parameters, bool partialDelete)
        {
            long matchId = MatchUrlToId(matchUrl);

            if (matchId == -1)
            {
                Coding.Discord.SendMessage(e.ChannelID, "Could not parse match " + matchUrl);
                return;
            }

            Delete(matchId, e, parameters, partialDelete);
        }

        private void Delete(long matchId, CommandEventArg e, string[] parameters, bool partialDelete)
        {
            using (GAFContext context = new GAFContext())
            {
                BaseDBReader<BotSeasonResult> resultReader = new BaseDBReader<BotSeasonResult>(context);
                BaseDBReader<BotSeasonBeatmapMod> modReader = new BaseDBReader<BotSeasonBeatmapMod>(context);
                SeasonPlayerReader playerReader = new SeasonPlayerReader(context);
                BaseDBReader<BotSeasonScore> scoreReader = new BaseDBReader<BotSeasonScore>(context);

                var result = resultReader.Get(r => r.MatchId == matchId);

                if (result == null)
                {
                    Coding.Discord.SendMessage(e.ChannelID, $"Match {matchId} not found");
                    return;
                }

                var scores = scoreReader.Where(s => s.BotSeasonResultId == result.Id);

                for (int i = 0; i < scores.Count; i++)
                {
                    modReader.RemoveAll(m => m.BotSeasonScoreId == scores[i].Id);
                    scoreReader.Remove(s => s.Id == scores[i].Id);
                }

                if (partialDelete)
                {
                    result.LosingTeam = "";
                    result.LosingTeamWins = 0;
                    result.WinningTeam = "";
                    result.WinningTeamColor = 0;
                    result.WinningTeamWins = 0;

                    resultReader.Update(result);
                }
                else
                    resultReader.Remove(r => r.Id == result.Id);

                context.SaveChanges();
            }

            Coding.Discord.SendMessage(e.ChannelID, "Deleted match " + matchId + " partial delete " + partialDelete);
        }

        private void Recalculate(string matchUrl, CommandEventArg e, string[] parameters, string msgStr)
        {
            long matchId = MatchUrlToId(matchUrl);

            if (matchId == -1)
            {
                Coding.Discord.SendMessage(e.ChannelID, "Could not parse match " + matchUrl);
                return;
            }

            Recalculate(matchId, e, parameters, msgStr);
        }

        private void Recalculate(long matchid, CommandEventArg e, string[] parameters, string msgStr)
        {
            string stage;
            using (BaseDBReader<BotSeasonResult> resultReader = new BaseDBReader<BotSeasonResult>())
            {
                BotSeasonResult result = resultReader.Get(r => r.MatchId == matchid);

                if (result == null)
                    stage = "Not found";
                else
                    stage = result.Stage;
            }


            Delete(matchid, e, null, true);

            string[] idSplit = parameters[0].Split('/').Skip(5).ToArray();

            ulong channelId = ulong.Parse(idSplit[0]);
            ulong msgId = ulong.Parse(idSplit[1]);

            var dChannel = Coding.Discord.GetChannel(channelId);
            var dMsg = dChannel.GetMessageAsync(msgId).Result;


            using (GAFContext context = new GAFContext())
            {
                if (Statistic.StatsHandler.UpdateSeasonStatistics(HISTORY_URL + matchid, stage, context))
                {

                    var resultEmbed = Statistic.StatsHandler.GetMatchResultEmbed(matchid, context);

                    dMsg.ModifyAsync(embed: resultEmbed).Wait();
                }
                else
                    Coding.Discord.SendMessage(e.ChannelID, "Failed");
            }

            Coding.Discord.SendMessage(e.ChannelID, "Recalculated match result " + matchid);
        }
    }
}
