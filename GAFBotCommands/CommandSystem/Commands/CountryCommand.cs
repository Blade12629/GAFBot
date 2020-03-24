using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;

namespace GAFBot.Commands
{
    public class CountryCommand : ICommand
    {
        public char Activator => '!';

        public string CMD => "country";

        public char ActivatorSpecial => char.MinValue;

        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public string Description => "Lists users and their countries";

        public string DescriptionUsage => "Usage: !country teamName";

        public static void Init()
        {
            Program.CommandHandler.Register(new CountryCommand());
            Logger.Log(nameof(CountryCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                Team team;
                using (Database.GAFContext context = new Database.GAFContext())
                    team = context.Team.FirstOrDefault(t => t.Name.Equals(e.AfterCMD, StringComparison.CurrentCultureIgnoreCase));

                if (team == null)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Could not find team " + e.AfterCMD);
                    return;
                }

                List<int> playerIds = new List<int>();

                using (Database.GAFContext context = new Database.GAFContext())
                    playerIds.AddRange(context.TeamPlayerList.Where(tpl => tpl.TeamId == team.Id).Select(tpl => (int)tpl.PlayerListId));

                List<Player> players = new List<Player>();

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    foreach (int playerId in playerIds)
                    {
                        Player player = context.Player.FirstOrDefault(p => p.Id == (long)playerId);

                        if (player == null)
                        {
                            Logger.Log("Could not find player: " + playerId, LogLevel.WARNING);
                            continue;
                        }

                        players.Add(player);
                    }
                }

                Dictionary<string, string> countryCodes = new Dictionary<string, string>();

                foreach (Player p in players)
                    if (countryCodes.Keys.FirstOrDefault(k => k.Equals(p.Country, StringComparison.CurrentCultureIgnoreCase)) == null)
                        countryCodes.Add(p.Country, null);

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    for (int i = 0; i < countryCodes.Keys.Count; i++)
                        countryCodes[countryCodes.Keys.ElementAt(i)] = context.BotCountryCode
                                                                              .FirstOrDefault(c => c.CountryCode.Equals(countryCodes.Keys.ElementAt(i),
                                                                                                   StringComparison.CurrentCultureIgnoreCase))
                                                                              .Country;
                }

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = "Country info for team: " + e.AfterCMD,
                    ThumbnailUrl = team.Image ?? null,
                    Description = "Average/Median PP: " + $"{team.AveragePP ?? '?'}/{team.MedianPP ?? '?'}",
                    Timestamp = DateTime.UtcNow
                };

                for (int i = 0; i < players.Count; i++)
                {
                    builder.AddField("Player", players[i].Nickname, true);
                    builder.AddField("Country", countryCodes[countryCodes.Keys.First(k => k.Equals(players[i].Country, StringComparison.CurrentCultureIgnoreCase))].Trim('"'), true);
                    builder.AddField("PP Raw", $"{players[i].PPRaw ?? '?'}", true);
                }

                DiscordEmbed embed = builder.Build();

                DiscordChannel channel = Coding.Discord.GetChannel(e.ChannelID);
                channel.SendMessageAsync(embed: embed).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
