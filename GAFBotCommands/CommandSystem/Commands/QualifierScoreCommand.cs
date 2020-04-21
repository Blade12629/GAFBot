using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using GAFBot.MessageSystem;
using GAFBot.Database.Models;
using GAFBot.Database;

namespace GAFBot.Commands
{
    public class QualifierScoreCommand : ICommand
    {
        public char Activator => '!';

        public string CMD => "qscores";

        public char ActivatorSpecial => char.MinValue;

        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Displays all scores from a specific match id";

        public string DescriptionUsage => "Usage: !qscores matchid";

        public static void Init()
        {
            Program.CommandHandler.Register(new QualifierScoreCommand());
            Logger.Log(nameof(QualifierScoreCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            if (string.IsNullOrEmpty(e.AfterCMD))
            {
                Coding.Discord.SendMessage(e.ChannelID, DescriptionUsage);
                return;
            }
            if (!int.TryParse(e.AfterCMD.Trim(' '), out int matchId))
            {
                Coding.Discord.SendMessage(e.ChannelID, "Could not parse matchid " + matchId);
                return;
            }

            BotAnalyzerQualifierResult dbresult;
            List<(BotAnalyzerQualifierTeam, List<BotAnalyzerQualifierPlayer>)> dbteams = new List<(BotAnalyzerQualifierTeam, List<BotAnalyzerQualifierPlayer>)>();
            List<BotAnalyzerScore> dbscores = new List<BotAnalyzerScore>();
            List<Beatmap> dbmaps = new List<Beatmap>();

            using (GAFContext context = new GAFContext())
            {
                dbresult = context.BotAnalyzerQualifierResult.FirstOrDefault(r => r.MatchId == matchId);

                if (dbresult == null)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Could not find match: " + matchId);
                    return;
                }
                
                foreach (var result in context.BotAnalyzerQualifierTeam.Where(t => t.QualifierResultId == dbresult.Id))
                    dbteams.Add((result, new List<BotAnalyzerQualifierPlayer>()));
                
                for (int i = 0; i < dbteams.Count; i++)
                {
                    foreach(var player in context.BotAnalyzerQualifierPlayer.Where(p => p.QualifierTeamId == dbteams[i].Item1.Id))
                    {
                        dbteams[i].Item2.Add(player);

                        foreach(var score in context.BotAnalyzerScore.Where(s => s.MatchId == matchId && s.UserId == player.UserId))
                        {
                            dbscores.Add(score);

                            if (dbmaps.FirstOrDefault(m => m.BeatmapId == score.BeatmapId) != null)
                                continue;
                            
                            dbmaps.Add(context.Beatmap.FirstOrDefault(b => b.BeatmapId == score.BeatmapId));
                        }
                    }
                }
            }
            //beatmapid, (userid, score)
            Dictionary<long, List<(long, long)>> beatmapScores = new Dictionary<long, List<(long, long)>>();
            
            foreach(var teamPair in dbteams)
            {
                foreach(var user in teamPair.Item2)
                {
                    foreach(var score in dbscores.Where(s => s.UserId == user.UserId))
                    {
                        Beatmap map = dbmaps.FirstOrDefault(m => m.BeatmapId == score.BeatmapId);

                        if (beatmapScores.ContainsKey(map.BeatmapId))
                            beatmapScores[map.BeatmapId].Add((user.UserId, score.Score));
                        else
                            beatmapScores.Add(map.BeatmapId, new List<(long, long)>() { (user.UserId, score.Score) });
                    }
                }
            }

            string mapStats = "";

            for (int i = 0; i < beatmapScores.Count; i++)
            {
                //beatmapid, (userid, score)
                KeyValuePair<long, List<(long, long)>> beatmapScorePair = beatmapScores.ElementAt(i);
                Beatmap map = dbmaps.FirstOrDefault(m => m.BeatmapId == beatmapScorePair.Key);

                mapStats += $"Stats for map {map.Author} - {map.Title} [{map.Difficulty}] ({i + 1}/{beatmapScores.Count}): {Environment.NewLine}" +
                                  $"```{Environment.NewLine}";

                //teamid, (totalscore, Dictionary<(userid, score)>)
                Dictionary<int, (long, Dictionary<long, long>)> teamScores = new Dictionary<int, (long, Dictionary<long, long>)>();

                //(userid, score)
                foreach (var scorePair in beatmapScorePair.Value)
                {
                    //(team, player)
                    var team = dbteams.FirstOrDefault(p => p.Item2.FirstOrDefault(u => u.UserId == scorePair.Item1) != null);
                    
                    if (teamScores.ContainsKey(team.Item1.Id))
                    {
                        (long, Dictionary<long, long>) scores = teamScores[team.Item1.Id];

                        if (scores.Item2.ContainsKey(scorePair.Item1))
                            continue;

                        scores.Item1 += scorePair.Item2;
                        scores.Item2.Add(scorePair.Item1, scorePair.Item2);

                        teamScores[team.Item1.Id] = scores;
                    }
                    else
                        teamScores.Add(team.Item1.Id, (scorePair.Item2, new Dictionary<long, long>() { { scorePair.Item1, scorePair.Item2 } }));
                }

                //teamid, (totalscore, Dictionary<(userid, score)>)
                foreach (var teamScorePair in teamScores)
                {
                    var teamPair = dbteams.FirstOrDefault(t => t.Item1.Id == teamScorePair.Key);
                    mapStats += "Team " + teamPair.Item1.TeamName + ": " + string.Format("{0:n0}", teamScorePair.Value.Item1) + Environment.NewLine;

                    //Dictionary<(userid, score)>
                    foreach (var scorePair in teamScorePair.Value.Item2)
                    {
                        var user = teamPair.Item2.FirstOrDefault(p => p.UserId == scorePair.Key);
                        mapStats += $" - {user.UserName}: {string.Format("{0:n0}", scorePair.Value)}" + Environment.NewLine;
                    }

                    mapStats += Environment.NewLine;
                }

                mapStats += "```" + Environment.NewLine;

                //3 % 2 = 1
                int count = i + 1;
                if (count > 1 && count % 2 == 0)
                {
                    Coding.Discord.SendMessage(e.ChannelID, mapStats);
                    mapStats = "";
                }
            }
        }
    }
}
