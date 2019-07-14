using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAFBot.Commands
{
    [Obsolete("Soon to be removed, only as short time solution implemented")]
    public class TextCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "text"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        private static List<TextInfo> _textInfos;
        private static System.IO.FileInfo _textInfoFile;

        public static void Init()
        {
            Program.CommandHandler.Register(new TextCommand() as ICommand);
            Coding.Methods.Log(typeof(TextCommand).Name + " Registered");

            _textInfos = new List<TextInfo>();
            System.IO.FileInfo usersFile = new System.IO.FileInfo(Program.CurrentPath + Program.Config.UserFile);
            System.IO.DirectoryInfo saveDir = usersFile.Directory;

            Program.SaveEvent += () => Save();
            Program.ExitEvent += () => Save();

            _textInfoFile = new System.IO.FileInfo(saveDir.FullName + @"\text.json");
            if (!_textInfoFile.Exists)
                return;
            try
            {
                string textJson = System.IO.File.ReadAllText(_textInfoFile.FullName);
                _textInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<TextInfo[]>(textJson).ToList();
            }
            catch (Exception ex)
            {
                Program.Logger.Log("TextCommand: Could not load textInfos");
            }
        }

        public static void Save()
        {
            Program.Logger.Log("TextCommand: Saving commands");
            if (_textInfos == null || _textInfos.Count == 0)
                return;

            string textJson = Newtonsoft.Json.JsonConvert.SerializeObject(_textInfos.ToArray(), Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(_textInfoFile.FullName, textJson);

            Program.Logger.Log("TextCommand: Commands saved ");
        }

        public void Activate(CommandEventArg e)
        {
            if (e.Activator.Equals('>'))
            {
                OnTextCommand(e);
                return;
            }

            if (!Program.MessageHandler.Users.TryGetValue(e.DUserID, out User user))
                return;

            string[] options = new string[]
            {
                "create",
                "delete"
            };
            string cmdString = null;

            try
            {
                if (string.IsNullOrEmpty(e.AfterCMD))
                    return;
                else if (e.AfterCMD.StartsWith(options[0], StringComparison.CurrentCultureIgnoreCase) && (int)user.AccessLevel >= (int)AccessLevel.Moderator)
                {
                    cmdString = e.AfterCMD.Remove(0, options[0].Length + 1);
                    string cmd = cmdString.Substring(0, cmdString.IndexOf(' '));
                    cmdString = cmdString.Remove(0, cmd.Length + 1);

                    TextInfo textInfo = new TextInfo()
                    {
                        CMD = cmd,
                        Output = cmdString
                    };

                    _textInfos.Add(textInfo);
                }
                else if (e.AfterCMD.StartsWith(options[1], StringComparison.CurrentCultureIgnoreCase) && (int)user.AccessLevel >= (int)AccessLevel.Moderator)
                {
                    if (_textInfos.Count == 0)
                        return;

                    cmdString = e.AfterCMD.Remove(0, options[0].Length + 1);
                    string cmd = cmdString.Substring(0, cmdString.IndexOf(' '));

                    _textInfos.RemoveAll(m => m.CMD.Equals(cmd, StringComparison.CurrentCultureIgnoreCase));
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        public static void OnTextCommand(CommandEventArg e)
        {
            try
            {
                if (_textInfos.Count == 0)
                    return;

                TextInfo tI = _textInfos.Find(t => t.CMD.Equals(e.CMD, StringComparison.CurrentCultureIgnoreCase));

                if (tI == null)
                    return;

                string output = tI.ParseOutput(e);

                if (string.IsNullOrEmpty(output))
                    return;

                Coding.Methods.SendMessage(e.ChannelID, output);
            }
            catch (Exception ex)
            {
            }
        }

        private class TextInfo
        {
            public string CMD { get; set; }
            public string Output { get; set; }

            [Newtonsoft.Json.JsonIgnore]
            static readonly string[] param = new string[]
            {
                    //single
                    "{caller}",

                    //multi
                    "{mention}",
                    //need to check userid on it's own
                    //single with multiParams
                    //usage example
                    //{caller} pats {mention} {rn%10{i%url1}{i%url2}{rn%50{i%url3}{i%url2}}
                    "n%", //n%5 for (int i = X; i < 5; i++)
                    "nl%", //new line
                    "i%", //i%url -> image
                    "r%", //r%5 = 1 in 5 chance
                    "rn%", //rn%5 = 1 in 5 chance, pick one
            };
            public string ParseOutput(CommandEventArg e)
            {
                string text = Output;

                bool success = true;

                paramSplit = e.AfterCMD.Split(' ');
                
                while (success != false)
                    success = ReplaceNextTag('{', '}', ref text, e);

                return text;
            }
            
            private string[] paramSplit;
            private bool ReplaceNextTag(char start, char end, ref string text, CommandEventArg e)
            {
                try
                {
                    int startI = text.IndexOf(start);
                    int endI = text.IndexOf(end);

                    if (startI == -1 || endI == -1)
                        return false;

                    string parameter = text.Substring(startI + 1, endI - 1 - startI);
                    string replaceText = start + parameter + end;
                    
                    string newParam = ParseParameter(e, replaceText);
                    text = text.Replace(replaceText, newParam);
                    return true;
                }
                catch (Exception ex)
                {

                }

                return false;
            }

            public string ParseParameter(CommandEventArg e, string parameter)
            {
                try
                {
                    var duser = Coding.Methods.GetUser(e.DUserID);
                    DSharpPlus.Entities.DiscordUser mduser = null;

                    if (parameter.Equals(param[0], StringComparison.CurrentCultureIgnoreCase))
                        return duser.Mention;
                    else if (parameter.Equals(param[1], StringComparison.CurrentCultureIgnoreCase))
                    {
                        return GetMention();
                    }
                    else if (ulong.TryParse(e.AfterCMD, out ulong id))
                        return GetMention(id);


                    string GetMention(ulong uid = 0)
                    {
                        string mention = e.AfterCMD.Remove(0, e.AfterCMD.IndexOf(' ') + 1).TrimStart('<', '@', '!');
                        mention = mention.Substring(0, mention.IndexOf('>')).TrimEnd('>');

                        ulong id = 0;

                        if (uid == 0 && !ulong.TryParse(mention, out id))
                            return "";
                        if (uid > 0)
                            id = uid;
                        if (id == 0)
                            return "";

                        mduser = Coding.Methods.GetUser(id);

                        if (mduser == null)
                            return "";

                        return mduser.Mention;
                    }
                }
                catch (Exception ex)
                {
                }

                return "";
            }

            public static string ParseOutput(CommandEventArg e, TextInfo textInfo)
                => textInfo.ParseOutput(e);
        }
    }
}
