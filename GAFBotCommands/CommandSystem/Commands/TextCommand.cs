using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GAFBot.Database.Models;
using GAFBot.MessageSystem;

namespace GAFBot.Commands
{
    public class TextCommand : ICommand
    {
        public char Activator => '!';

        public string CMD => "t";

        public char ActivatorSpecial => default(char);

        public AccessLevel AccessLevel => AccessLevel.User;

        private object _imageLock = new object();

        public static void Init()
        {
            Program.CommandHandler.Register(new TextCommand() as ICommand);
            Coding.Methods.Log(typeof(TextCommand).Name + " Registered");
        }

        public TextCommand()
        {
        }
        
        public void Activate(CommandEventArg e)
        {
            string[] split = e.AfterCMD.Split(' ');

            if (split.Length == 0)
            {
                Coding.Methods.SendMessage(e.ChannelID, "No parameters found");
                return;
            }

            if (split[0].Equals("add", StringComparison.CurrentCultureIgnoreCase))
            {
                BotUsers user;

                using (Database.GAFContext context = new Database.GAFContext())
                    user = context.BotUsers.First(bu => (ulong)bu.DiscordId == e.DUserID);

                if (user == null)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, "Could not find user");
                    return;
                }
                else if ((AccessLevel)user.AccessLevel < AccessLevel.Admin)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, "You do not have sufficient permissions");
                    return;
                }

                //add genre
                if (split[1].Equals("category", StringComparison.CurrentCultureIgnoreCase) || split[1].Equals("c", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (split.Length < 4)
                    {
                        Coding.Methods.SendMessage(e.ChannelID, "Missing parameters (!t add c categoryName pattern");
                        return;
                    }
                    BotImages img;
                    using (Database.GAFContext context = new Database.GAFContext())
                        img = context.BotImages.First(i => i.Category.Equals(split[2], StringComparison.CurrentCultureIgnoreCase));

                    if (img != null)
                    {
                        Coding.Methods.ChannelMessage(e.ChannelID, $"Category {split[2]} already exists");
                        return;
                    }
                    
                    string pt = e.AfterCMD.Remove(0, split[0].Length + split[1].Length + split[2].Length + 3);
                    
                    BotPatterns bp = new BotPatterns()
                    {
                        Category = split[2],
                        Text = pt
                    };

                    using (Database.GAFContext context = new Database.GAFContext())
                    {
                        context.BotPatterns.Add(bp);
                        context.SaveChanges();
                    }

                    Coding.Methods.ChannelMessage(e.ChannelID, "Added category " + split[2] + " with pattern " + pt);
                    return;
                }

                if (split.Length < 3)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, "Not enough parameters (!t add categoryName imageUrl)");
                    return;
                }

                BotImages bimg = new BotImages()
                {
                    Category = split[1],
                    Url = split[2]
                };

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    context.BotImages.Add(bimg);
                    context.SaveChanges();
                }
                
                Coding.Methods.ChannelMessage(e.ChannelID, $"add image {split[2]} to category {split[1]}");
                return;
            }
            else if (split[0].Equals("delete", StringComparison.CurrentCultureIgnoreCase) || split[0].Equals("remove", StringComparison.CurrentCultureIgnoreCase))
            {
                using (Database.GAFContext context = new Database.GAFContext())
                {
                    BotUsers buser = context.BotUsers.First(b => (ulong)b.DiscordId == e.DUserID);

                    if (buser == null)
                    {
                        Coding.Methods.ChannelMessage(e.ChannelID, "Could not find user");
                        return;
                    }
                    else if ((AccessLevel)buser.AccessLevel < AccessLevel.Admin)
                    {
                        Coding.Methods.ChannelMessage(e.ChannelID, "You do not have sufficient permissions");
                        return;
                    }

                    if (split.Length < 2)
                    {
                        Coding.Methods.ChannelMessage(e.ChannelID, "Not enough parameters");
                        return;
                    }

                    int imgCounter = 0;

                    BotPatterns p = context.BotPatterns.First(p_ => p_.Category.Equals(split[1], StringComparison.CurrentCultureIgnoreCase));

                    if (p == null)
                        return;

                    context.BotPatterns.Remove(p);

                    foreach (BotImages img in context.BotImages.Where(i => i.Category.Equals(split[1], StringComparison.CurrentCultureIgnoreCase)))
                        context.BotImages.Remove(img);

                    context.SaveChanges();

                    if (imgCounter > 0)
                        Coding.Methods.ChannelMessage(e.ChannelID, $"Removed all images ({imgCounter})");
                    else
                        Coding.Methods.ChannelMessage(e.ChannelID, $"Could not find any image or category under " + split[1]);
                }
                return;
            }
            else if (split[0].Equals("set", StringComparison.CurrentCultureIgnoreCase))
            {
                BotUsers buser;
                using (Database.GAFContext context = new Database.GAFContext())
                    buser = context.BotUsers.First(b => (ulong)b.DiscordId == e.DUserID);

                if (buser == null)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, "Could not find user");
                    return;
                }
                else if ((AccessLevel)buser.AccessLevel < AccessLevel.Admin)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, "You do not have sufficient permissions");
                    return;
                }
                else if (split.Length < 3)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, "Missing parameters (!t set category newPattern");
                    return;
                }

                BotPatterns p;
                using (Database.GAFContext context = new Database.GAFContext())
                    p = context.BotPatterns.First(p_ => p_.Category.Equals(split[1], StringComparison.CurrentCultureIgnoreCase));

                if (p == null)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, "Could not find category " + split[1]);
                    return;
                }
                
                string oldPattern = split[1];
                string newPattern = e.AfterCMD.Remove(0, split[0].Length + split[1].Length + 2);
                p.Text = newPattern;

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    context.BotPatterns.Update(p);
                    context.SaveChanges();
                }

                Coding.Methods.ChannelMessage(e.ChannelID, $"Replaced pattern ({oldPattern}) ({newPattern})");
                return;
            }
            else if (split[0].Equals("list", StringComparison.CurrentCultureIgnoreCase))
            {
                string result = "";
                int nl = 0;

                IEnumerable<BotImages> images;
                using (Database.GAFContext context = new Database.GAFContext())
                    images = context.BotImages.AsEnumerable();

                for (int i = 0; i < images.Count(); i++ )
                {
                    var img = images.ElementAt(i);

                    result += img.Category;
                    nl++;

                    if (nl >= 6)
                        result += Environment.NewLine;
                    else if (i < images.Count() - 1)
                        result += ", ";
                }

                Coding.Methods.ChannelMessage(e.ChannelID, "Categories: " + Environment.NewLine + result);
                return;
            }

            List<string> imgs;
            BotPatterns pattern;
            using (Database.GAFContext context = new Database.GAFContext())
            {
                pattern = context.BotPatterns.First(p => p.Category.Equals(split[0], StringComparison.CurrentCultureIgnoreCase));

                if (pattern == null)
                {
                    Coding.Methods.ChannelMessage(e.ChannelID, $"Could not find category " + split[0]);
                    return;
                }

                imgs = context.BotImages.Where(i => i.Category.Equals(split[0], StringComparison.CurrentCultureIgnoreCase))
                                                             .Select(i => i.Url).ToList();
            }

            string uri = imgs[new Random().Next(0, imgs.Count - 1)];
            string output = ProcessPattern(pattern.Text, (split.Length > 1 ? split[1] : ""), e.DUserID) + Environment.NewLine + uri;

            Coding.Methods.ChannelMessage(e.ChannelID, output);
        }

        private string ProcessPattern(string pattern, string input, ulong user)
        {
            string result = string.Copy(pattern);

            if (string.IsNullOrEmpty(input))
            {
                if (result.Contains("{mention}"))
                    result = result.Replace("{mention}", "");

                if (result.Contains("{caller}"))
                    result = result.Replace("{caller}", $"<@!{user}>");

                return result;
            }

            int s, e, s2;
            // <@!140896783717892097>
            s = input.IndexOf("<@!");
            e = input.IndexOf(">");

            if (s >= 0 && e >= 0)
                if (result.Contains("{mention}"))
                    result = result.Replace("{mention}", input.Substring(s, e - s + 1));

            if (result.Contains("{caller}"))
                result = result.Replace("{caller}", "<@!" + user + ">");

            return result;
        }
    }

    public struct PicInfo : IEquatable<PicInfo>
    {
        public string Url { get; set; }
        public string Genre { get; set; }

        public PicInfo(string url, string genre) : this()
        {
            Url = url;
            Genre = genre;
        }

        public override bool Equals(object obj)
        {
            return obj is PicInfo && Equals((PicInfo)obj);
        }

        public bool Equals(PicInfo other)
        {
            return Url == other.Url &&
                   Genre == other.Genre;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Url, Genre);
        }

        public static bool operator ==(PicInfo info1, PicInfo info2)
        {
            return info1.Equals(info2);
        }

        public static bool operator !=(PicInfo info1, PicInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
