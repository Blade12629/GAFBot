using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class PointCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "points"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Points for user info";

        public string DescriptionUsage => "see '!points' for more help" + Environment.NewLine + "!points _set discordUserId Points/MaxValue/MinValue";

        public static void Init()
        {
            Program.CommandHandler.Register(new PointCommand() as ICommand);
            Coding.Methods.Log(typeof(PointCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                ulong[] channels = new ulong[2]
                {
                    (ulong)Program.Config.BetChannel, //#bet_channel
                    (ulong)Program.Config.DevChannel //#skyfly-room
                };

                BotUsers user;

                using (Database.GAFContext context = new Database.GAFContext())
                    user = context.BotUsers.First(u => (ulong)u.DiscordId == e.DUserID);

                if (user == null)
                {
                    Coding.Methods.SendMessage(e.ChannelID, "Something went wrong, either retry or contact @??????#0284");
                    return;
                }

                if (!channels.Contains(e.ChannelID))
                {
                    if ((AccessLevel)user.AccessLevel != AccessLevel.Admin)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "You can only use this command in #bet_channel!");
                        return;
                    }
                    else
                        Coding.Methods.SendMessage(e.ChannelID, "Bypassed channel restriction due to admin status");
                }

                Logger.Log("!points executed", LogLevel.Trace);

                DSharpPlus.Entities.DiscordEmbedBuilder builder;
                var client = Coding.Methods.GetClient();
                var channel = Coding.Methods.GetChannel(e.ChannelID);

                if (string.IsNullOrEmpty(e.AfterCMD))
                {
                    builder = new DSharpPlus.Entities.DiscordEmbedBuilder();
                    builder.Title = "Points Command";

                    builder.AddField("!points top10", "Shows the top 10 users");
                    builder.AddField("!points toplast", "Shows the top last users");
                    builder.AddField("!points [userid]", "Shows points for user X");
                    builder.AddField("!points [@mention]", "Show points for mentioned user");

                    client.SendMessageAsync(channel, embed: builder.Build()).Wait();
                    return;
                }

                if (e.AfterCMD.ToLower().StartsWith("top10"))
                {
                    builder = new DSharpPlus.Entities.DiscordEmbedBuilder();
                    builder.Title = "Top 10 points ranking";

                    List<BotUsers> sortedUsers;
                    using (Database.GAFContext context = new Database.GAFContext())
                        sortedUsers = context.BotUsers.OrderByDescending(u => u.Points).ToList();

                    builder.Description = "Points ordered by descending";
                    string field = "";

                    for (int i = 0; i < 10; i++)
                        field += $"{i + 1}: {GetName(client, (ulong)sortedUsers[i].DiscordId)} ({sortedUsers[i].DiscordId}): {sortedUsers[i].Points}{Environment.NewLine}";

                    field = field.Remove(builder.Description.Length - 1, 1);
                    builder.AddField("Top10", field);

                    client.SendMessageAsync(channel, embed: builder.Build()).Wait();
                }
                else if (e.AfterCMD.ToLower().StartsWith("toplast"))
                {
                    builder = new DSharpPlus.Entities.DiscordEmbedBuilder();
                    builder.Title = "Top last points ranking";

                    List<BotUsers> sortedUsers;
                    using (Database.GAFContext context = new Database.GAFContext())
                        sortedUsers = context.BotUsers.OrderBy(u => u.Points).ToList();

                    builder.Description = "Points ordered by ascending";
                    string field = "";

                    for (int i = 0; i < 11; i++)
                        field += $"{sortedUsers.Count - i - 1}: {GetName(client, (ulong)sortedUsers[i].DiscordId)} ({sortedUsers[i].DiscordId}): {sortedUsers[i].Points}{Environment.NewLine}";

                    field = field.Remove(builder.Description.Length - 1, 1);
                    builder.AddField("Top last", field);

                    client.SendMessageAsync(channel, embed: builder.Build()).Wait();
                }
                else if (ulong.TryParse(e.AfterCMD, out ulong targetUser))
                    GetUserPointsById(targetUser);
                else if (e.AfterCMD.StartsWith("<@"))
                {
                    Logger.Log("Getting points by mention");
                    string mention = e.AfterCMD.TrimStart('<', '@', '!');
                    mention = mention.Remove(mention.ToList().FindIndex(c => c.Equals('>')), 1);

                    if (!ulong.TryParse(mention, out ulong result))
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Could not find mention, this might be a bug");
                        return;
                    }

                    Console.WriteLine("Getting points for user —" + result + "—");
                    GetUserPointsById(result);
                }
                else if ((AccessLevel)user.AccessLevel == AccessLevel.Admin)
                {
                    if (e.AfterCMD.ToLower().StartsWith("_set") && (AccessLevel)user.AccessLevel == AccessLevel.Admin)
                    {
                        e.AfterCMD = e.AfterCMD.Remove(0, "_set ".Length);

                        string[] split = e.AfterCMD.Split(' ');

                        if (ulong.TryParse(split[0], out ulong userId))
                        {
                            using (Database.GAFContext context = new Database.GAFContext())
                            {
                                if (split[1].ToLower().Equals("minvalue"))
                                {
                                    user.Points = int.MinValue;

                                    context.BotUsers.Update(user);
                                    context.SaveChanges();

                                    return;
                                }
                                else if (split[1].ToLower().Equals("maxvalue"))
                                {
                                    user.Points = int.MaxValue;

                                    context.BotUsers.Update(user);
                                    context.SaveChanges();

                                    return;
                                }

                                if (int.TryParse(split[1], out int newVal))
                                {
                                    user.Points = newVal;

                                    context.BotUsers.Update(user);
                                    context.SaveChanges();

                                    return;
                                }
                            }
                        }
                    }
                    else if (e.AfterCMD.ToLower().StartsWith("_add") && (AccessLevel)user.AccessLevel == AccessLevel.Admin)
                    {
                        e.AfterCMD = e.AfterCMD.Remove(0, "_add ".Length);

                        string[] split = e.AfterCMD.Split(' ');

                        if (ulong.TryParse(split[0], out ulong userId))
                        {
                            if (int.TryParse(split[1], out int newVal))
                            {
                                user.Points = newVal;

                                using (Database.GAFContext context = new Database.GAFContext())
                                {
                                    context.BotUsers.Update(user);
                                    context.SaveChanges();
                                }
                            }
                        }
                    }
                }
                else
                {
                    var guild = client.Guilds.First(g => g.Key == e.GuildID);
                    var member = guild.Value.Members.ToList().Find(m => m.Mention.Equals(e.AfterCMD));

                    if (member == null)
                        return;

                    GetUserPoints(user);
                }

                void GetUserPointsById(ulong targetUserId)
                {
                    BotUsers us;

                    using (Database.GAFContext context = new Database.GAFContext())
                        us = context.BotUsers.First(b => (ulong)b.DiscordId == targetUserId);

                    if (us != null)
                        GetUserPoints(us);
                }

                void GetUserPoints(BotUsers targetUser)
                {
                    if (targetUser == null)
                        return;

                    builder = new DSharpPlus.Entities.DiscordEmbedBuilder();
                    builder.Title = $"Point Information for user {GetName(client, (ulong)targetUser.DiscordId)} ({targetUser.DiscordId})";
                    builder.Description = $"Points: {targetUser.Points}";

                    channel.SendMessageAsync(embed: builder.Build()).Wait();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Trace);
            }
        }

        private string GetName(DSharpPlus.DiscordClient client, ulong userId)
        {
            var user = client.GetUserAsync(userId).Result;

            return user != null ? user.Username : "null";
        }
    }
}
