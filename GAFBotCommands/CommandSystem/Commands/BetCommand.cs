using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;

namespace GAFBot.Commands
{
    public class BetCommand : ICommand
    {
        public char Activator { get => '!'; }
        public string CMD { get => "bet"; }
        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public static void Init()
        {
            Program.CommandHandler.Register(new BetCommand() as ICommand);
            Coding.Methods.Log(typeof(BetCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday || (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday && DateTime.UtcNow.Hour <= 7))
                {
                    Coding.Methods.SendMessage(e.ChannelID, "You can only bet from monday 8:00 am till friday 12:00 am (UTC)");
                    return;
                }

                Console.WriteLine(Program.ChallongeHandler.LastUpdate);
                Console.WriteLine(Program.ChallongeHandler.Matches.Count);
                Console.WriteLine(Program.ChallongeHandler.Participants.Count);
                Console.WriteLine(e.AfterCMD);
                //!bet matchId team
                string[] msgS = e.AfterCMD.Split(' ');
                int matchId = -1;
                DSharpPlus.Entities.DiscordEmbedBuilder embedBuilder = new DSharpPlus.Entities.DiscordEmbedBuilder();

                if (msgS == null || string.IsNullOrEmpty(e.AfterCMD))
                {
                    embedBuilder.Description = "Commands";
                    embedBuilder.AddField("!bet list", "Lists all your bets");
                    embedBuilder.AddField("!bet [matchID] [team name]", "Bets on team X on match Y");
                    embedBuilder.AddField("!bet remove [matchID]", "Removes your bet on match X");
                    embedBuilder.AddField("!bet remove all", "Removes all of your current bets");

                    var channel = Program.Client.GetChannelAsync(e.ChannelID).Result;
                    Program.Client.SendMessageAsync(channel, embed: embedBuilder.Build());
                    return;
                }
                else if (!string.IsNullOrEmpty(e.AfterCMD))
                {
                    if (e.AfterCMD.ToLower().StartsWith("list"))
                    {
                        List<Gambling.Betting.Bet> bets = Program.BettingHandler.ActiveBets.FindAll(b => b.DiscordUserId == e.DUserID);

                        if (bets == null || bets.Count == 0)
                        {
                            Coding.Methods.SendMessage(e.ChannelID, "No bets found");
                            return;
                        }
                        else
                            embedBuilder.Description = "Your current bets";

                        for (int i = 0; i < bets.Count; i++)
                            embedBuilder.AddField($"Bet {i + 1}", $"{bets[i].MatchId} : {bets[i].Team}");

                        var channel = Program.Client.GetChannelAsync(e.ChannelID).Result;
                        Program.Client.SendMessageAsync(channel, embed: embedBuilder.Build());
                        return;
                    }
                    else if (e.AfterCMD.ToLower().StartsWith("remove all"))
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Removing all of your current bets");
                        Program.BettingHandler.RemoveBets(e.DUserID);
                        return;
                    }
                    else if (e.AfterCMD.ToLower().StartsWith("remove"))
                    {
                        if (!int.TryParse(msgS[0], out matchId))
                        {
                            Coding.Methods.SendMessage(e.ChannelID, $"Could not parse {msgS[0]} to int");
                            return;
                        }
                        Coding.Methods.SendMessage(e.ChannelID, "Removing your bet on match " + matchId);

                        Program.BettingHandler.RemoveBet(matchId, e.DUserID);
                        return;
                    }
                    else if (e.AfterCMD.ToLower().StartsWith("_admin"))
                    {
                        if (Program.MessageHandler.Users[e.DUserID].AccessLevel < AccessLevel.Admin)
                            return;
                        //!bet _admin
                        string newmessage = e.AfterCMD.Remove(0, "_admin ".Length);

                        //teamA teamB winningTeam
                        if (newmessage.ToLower().StartsWith("faketrigger"))
                        {
                            newmessage = newmessage.Remove(0, "faketrigger ".Length);

                            Coding.Methods.SendMessage(e.ChannelID, $"Trying to fake trigger");
                            
                            string[] split = newmessage.Split('|');
                            
                            Program.MessageHandler.FakeTrigger(split[0].TrimStart(' ').TrimEnd(' '), split[1].TrimStart(' ').TrimEnd(' '), split[2].TrimStart(' ').TrimEnd(' '));

                            Coding.Methods.SendMessage(e.ChannelID, $"Fake triggered");
                        }
                        else if (newmessage.ToLower().StartsWith("points"))
                        {
                            if (newmessage.Length <= "points ".Length)
                                Coding.Methods.SendMessage(e.ChannelID, "Current reward points are: " + Gambling.Betting.BettingHandler.CurrentReward);
                            else if (int.TryParse(newmessage.Remove(0, "points ".Length), out int pointVal))
                            {
                                Gambling.Betting.BettingHandler.CurrentReward = pointVal;
                                Coding.Methods.SendMessage(e.ChannelID, "Current reward points set to: " + pointVal);
                            }
                            else
                                Coding.Methods.SendMessage(e.ChannelID, "Current reward points are: " + Gambling.Betting.BettingHandler.CurrentReward);

                        }

                        return;
                    }
                    else if (msgS.Length >= 2 && !int.TryParse(msgS[0], out matchId))
                    {
                        Coding.Methods.SendMessage(e.ChannelID, $"Could not parse {msgS[0]} to int");
                        return;
                    }
                    
                    string team = e.AfterCMD.Remove(0, msgS[0].Length + 1);

                    Program.BettingHandler.AddBet(team, matchId, e.DUserID, e.ChannelID);
                }
            }
            catch (Exception ex)
            {
                Coding.Methods.SendMessage(e.ChannelID, ex.ToString());
            }
        }
    }
}
