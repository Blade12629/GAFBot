using GAFBot;
using GAFBot.Database;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GAFBot.Commands
{   
    public class PickCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "pick"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public string Description => "Picks a team from a specific match";

        public string DescriptionUsage => $"Usage: !pick [-delete] match name/team name" + Environment.NewLine +
                                           "[delete] = optional" + Environment.NewLine + 
                                           "Example: !pick ABCdeF vs XYZ123/ABCdeF" + Environment.NewLine + 
                                           "!pick -delete";

        public static void Init()
        {
            Program.CommandHandler.Register(new PickCommand());
            Logger.Log(nameof(PickCommand) + " Registered");
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
                else if (!e.GuildID.HasValue)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "You can only use this command on a discord server!");
                    return;
                }
                var dmember = Coding.Discord.GetMember(e.DUserID, e.GuildID.Value);
                bool canUseCommand = false;
                foreach (var drole in dmember.Roles)
                {
                    if (drole.Name.Contains("commentator", StringComparison.CurrentCultureIgnoreCase) ||
                        drole.Name.Equals("commentator", StringComparison.CurrentCultureIgnoreCase) ||
                        drole.Name.Contains("streamer", StringComparison.CurrentCultureIgnoreCase) ||
                        drole.Name.Equals("streamer", StringComparison.CurrentCultureIgnoreCase))
                    {
                        canUseCommand = true;
                        break;
                    }
                }

                if (!canUseCommand)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "You need to have Commentator or Streamer role in order to use this command");
                    return;
                }

                string param = e.AfterCMD;
                bool delete = false;
                string[] split;
                string matchName = null;
                string teamName = null;
                if (e.AfterCMD.StartsWith("-register"))
                {
                    BotApiRegisterCode registerCode;
                    using (GAFContext context = new GAFContext())
                        registerCode = context.BotApiRegisterCode.FirstOrDefault(r => r.DiscordId == (long)e.DUserID);

                    if (registerCode != null)
                    {
                        Coding.Discord.SendPrivateMessage(e.DUserID, "You already started the registration, your code: " + registerCode.Code);
                        return;
                    }

                    string code = "";

                    byte[] codeBuffer = new byte[4];
                    RandomNumberGenerator.Fill(codeBuffer);

                    foreach (byte b in codeBuffer)
                        code += "" + b;

                    registerCode = new BotApiRegisterCode()
                    {
                        Code = code,
                        DiscordId = (long)e.DUserID
                    };

                    using (GAFContext context = new GAFContext())
                    {
                        context.BotApiRegisterCode.Add(registerCode);
                        context.SaveChanges();
                    }

                    Coding.Discord.SendPrivateMessage(e.DUserID, "Your Registration code is: " + code);
                    return;
                }
                else if (!e.AfterCMD.StartsWith("-delete"))
                {
                    split = param.Split('/');
                    matchName = split[0];
                    teamName = split[1];
                }
                else
                    delete = true;

                using (GAFContext context = new GAFContext())
                {
                    if (delete)
                    {
                        BotPick[] picks = context.BotPick.Where(pi => pi.PickedBy.Equals(dmember.DisplayName)).ToArray();

                        foreach (BotPick pi in picks)
                            context.Remove(pi);

                        context.SaveChanges();
                        Coding.Discord.SendMessage(e.ChannelID, "Removed all picks of you");
                        return;
                    }
                    
                    Team t = context.Team.FirstOrDefault(te => te.Name.Equals(teamName));

                    if (t == null)
                    {
                        Coding.Discord.SendMessage(e.ChannelID, $"Team {teamName} not found!");
                        return;
                    }
                    
                    BotPick p = context.BotPick.FirstOrDefault(pi => pi.PickedBy.Equals(dmember.DisplayName));

                    if (p == null)
                    {
                        context.BotPick.Add(new BotPick()
                        {
                            Image = t.Image,
                            Match = matchName,
                            PickedBy = dmember.DisplayName,
                            Team = teamName,
                        });
                    }
                    else
                    {
                        p.Image = t.Image;
                        p.Match = matchName;
                        p.PickedBy = dmember.DisplayName;
                        p.Team = teamName;

                        context.BotPick.Update(p);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
