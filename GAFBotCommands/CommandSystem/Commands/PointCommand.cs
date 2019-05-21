using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class PointCommand : ICommand
    {
        public char Activator { get => '!'; }
        public string CMD { get => "points"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public static void Init()
        {
            Program.CommandHandler.Register(new PointCommand() as ICommand);
            Coding.Methods.Log(typeof(PointCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                if (e.ChannelID == 580438712475844608 || e.ChannelID == 578984727583784980)
                {
                    Program.Logger.Log("!points executed", showConsole: Program.Config.Debug);

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

                        List<User> sortedUsers = Program.MessageHandler.Users.Values.OrderByDescending(u => u.Points).ToList();
                        builder.Description = "Points ordered by descending";
                        string field = "";

                        for (int i = 0; i < 10; i++)
                            field += $"{i + 1}: {GetName(client, sortedUsers[i].DiscordID)} ({sortedUsers[i].DiscordID}): {sortedUsers[i].Points}{Environment.NewLine}";

                        field = field.Remove(builder.Description.Length - 1, 1);
                        builder.AddField("Top10", field);

                        client.SendMessageAsync(channel, embed: builder.Build()).Wait();
                    }
                    else if (e.AfterCMD.ToLower().StartsWith("toplast"))
                    {
                        builder = new DSharpPlus.Entities.DiscordEmbedBuilder();
                        builder.Title = "Top last points ranking";

                        List<User> sortedUsers = Program.MessageHandler.Users.Values.OrderBy(u => u.Points).ToList();
                        builder.Description = "Points ordered by ascending";
                        string field = "";

                        for (int i = 0; i < 10; i++)
                            field += $"{sortedUsers.Count - i - 1}: {GetName(client, sortedUsers[i].DiscordID)} ({sortedUsers[i].DiscordID}): {sortedUsers[i].Points}{Environment.NewLine}";

                        field = field.Remove(builder.Description.Length - 1, 1);
                        builder.AddField("Top last", field);

                        client.SendMessageAsync(channel, embed: builder.Build()).Wait();
                    }
                    else if (ulong.TryParse(e.AfterCMD, out ulong targetUser))
                        GetUserPoints(targetUser);
                    else if (e.AfterCMD.ToLower().StartsWith("_set"))
                    {
                        e.AfterCMD = e.AfterCMD.Remove(0, "_set ".Length);

                        string[] split = e.AfterCMD.Split(' ');

                        if (ulong.TryParse(split[0], out ulong userId))
                        {
                            if (split[1].ToLower().Equals("minvalue"))
                            {
                                Program.MessageHandler.Users[userId].Points = int.MinValue;
                                return;
                            }
                            else if (split[1].ToLower().Equals("maxvalue"))
                            {
                                Program.MessageHandler.Users[userId].Points = int.MaxValue;
                                return;
                            }

                            if (int.TryParse(split[1], out int newVal))
                                Program.MessageHandler.Users[userId].Points = newVal;
                        }
                    }
                    else if (e.AfterCMD.ToLower().StartsWith("_add"))
                    {
                        e.AfterCMD = e.AfterCMD.Remove(0, "_add ".Length);

                        string[] split = e.AfterCMD.Split(' ');

                        if (ulong.TryParse(split[0], out ulong userId))
                        {
                            if (int.TryParse(split[1], out int newVal))
                            {
                                Program.MessageHandler.Users[userId].Points += newVal;
                            }
                        }
                    }
                    else
                    {
                        var guild = client.Guilds.First(g => g.Key == e.GuildID);
                        var member = guild.Value.Members.ToList().Find(m => m.Mention.Equals(e.AfterCMD));

                        if (member == null)
                            return;

                        GetUserPoints(member.Id);
                    }

                    void GetUserPoints(ulong userId)
                    {
                        User user = Program.MessageHandler.Users[userId];

                        if (user == null)
                            return;

                        builder = new DSharpPlus.Entities.DiscordEmbedBuilder();
                        builder.Title = $"Point Information for user {GetName(client, userId)} ({userId})";
                        builder.Description = $"Points: {user.Points}";

                        channel.SendMessageAsync(embed: builder.Build()).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Log(ex.ToString(), showConsole: Program.Config.Debug);
            }
        }

        private string GetName(DSharpPlus.DiscordClient client, ulong userId)
        {
            var user = client.GetUserAsync(userId).Result;

            return user != null ? user.Username : "null";
        }
    }
}
