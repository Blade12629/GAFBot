using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class SessionCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "session"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public static void Init()
        {
            Program.CommandHandler.Register(new SessionCommand() as ICommand);
            Coding.Methods.Log(typeof(SessionCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            Coding.Methods.ChannelMessage(e.ChannelID, "Disabled for now");

            return;

            try
            {
                if (string.IsNullOrEmpty(e.AfterCMD))
                    return;

                string[] cmdSplit = e.AfterCMD.Split(' ');


                AccessLevel access;

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    BotUsers user = context.BotUsers.First(u => (ulong)u.DiscordId == e.DUserID);
                    access = (AccessLevel)user.AccessLevel;
                }

                if (cmdSplit[0].Equals("profile") || cmdSplit[0].Equals("p"))
                {
                    #region withSession
                    /*if (cmdSplit[1].StartsWith('"'))
                    //{
                    //    string sessionName = cmdSplit[1].TrimStart('"');
                    //    for (int i = 2; i < cmdSplit.Length; i++)
                    //    {
                    //        if (cmdSplit[i].EndsWith('"'))
                    //        {
                    //            sessionName += " " + cmdSplit[i].TrimEnd('"');
                    //            break;
                    //        }

                    //        sessionName += " " + cmdSplit[i];
                    //    }
                    }*/
                    #endregion

                    int userid = -1;

                    if (!int.TryParse(cmdSplit[1], out userid))
                        return;

                    var channel = Coding.Methods.GetChannel(e.ChannelID);
                    var profile = Osu.OsuTourneySession.GetPlayerProfileDB(userid, Program.Config.ChallongeTournamentName);
                    channel.SendMessageAsync(embed: Osu.OsuTourneySession.BuildProfile(profile));
                }
                else if (cmdSplit[0].Equals("top10", StringComparison.CurrentCultureIgnoreCase))
                {
                    GetAndSendTopList(e.ChannelID, 0);
                }
                else if (cmdSplit[0].Equals("top", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!int.TryParse(cmdSplit[1], out int offset))
                        return;

                    GetAndSendTopList(e.ChannelID, offset);
                }
            }
            catch (Exception ex)
            {
                Coding.Methods.SendMessage(e.ChannelID, ex.ToString());
                Console.WriteLine(ex);
            }
        }

        private void GetAndSendTopList(ulong channelId, int offset = 0)
        {
            //var playerProfiles = Osu.SessionHandler.CurrentSession.GetTopPlayers(offset);
            //var dchannel = Coding.Methods.GetChannel(channelId);

            //string topList = "";

            //for (int i = 0; i < playerProfiles.Count(); i++)
            //{
            //    var profile = playerProfiles.ElementAt(i);

            //    var embed = Osu.SessionHandler.BuildProfile(profile);
            //    topList += $"{Environment.NewLine}{offset * 10 + i + 1}: {profile.UserName} (OR: {profile.OverallRating}, userID: {profile.UserId})";
            //}

            //topList.TrimStart(Environment.NewLine.ToCharArray());

            //DSharpPlus.Entities.DiscordEmbedBuilder builder = new DSharpPlus.Entities.DiscordEmbedBuilder()
            //{
            //    Title = $"Top {(offset + 1) * 10} players",
            //    Description = topList,
            //    Author = new DSharpPlus.Entities.DiscordEmbedBuilder.EmbedAuthor()
            //    {
            //        IconUrl = "https://cdn.discordapp.com/attachments/239737922595717121/621452111607234571/AYEorNAY.png"
            //    }
            //};

            //dchannel.SendMessageAsync(embed: builder.Build()).Wait();
        }
    }
}
