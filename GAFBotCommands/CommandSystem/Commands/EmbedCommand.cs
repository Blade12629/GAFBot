//using GAFBot.MessageSystem;
//using System;

//namespace GAFBot.Commands
//{
//    public class EmbedCommand : ICommand
//    {
//        public char Activator { get => '!'; }
//        public char ActivatorSpecial { get => default(char); }
//        public string CMD { get => "compile"; }
//        public AccessLevel AccessLevel => AccessLevel.Moderator;

//        public static void Init()
//        {
//            Program.CommandHandler.Register(new EmbedCommand() as ICommand);
//            Coding.Methods.Log(typeof(EmbedCommand).Name + " Registered");
//        }

//        public void Activate(CommandEventArg e)
//        {
//            AccessLevel access = Program.MessageHandler.GetAccessLevel(e.DUserID);

//            if (access < AccessLevel.Moderator)
//                return;

//            string[] cmdSplit = e.AfterCMD.Split(' ');
            
//            if (cmdSplit[0].Equals("compile", StringComparison.CurrentCultureIgnoreCase))
//            {
//                if (cmdSplit.Length == 2)
//                {
//                    string compile = cmdSplit[2];
//                    var embed = BuildEmbed(compile);
//                    var channel = Coding.Methods.GetChannel(e.ChannelID);
//                }
//                else if (cmdSplit.Length )
//            }
//        }

//        private DSharpPlus.Entities.DiscordEmbed BuildEmbed(string code)
//        {
//            if (string.IsNullOrEmpty(code))
//                return null;

//            DSharpPlus.Entities.DiscordEmbedBuilder builder = new DSharpPlus.Entities.DiscordEmbedBuilder();
//            var mainInfo = Parse(code);
//            builder.Title = mainInfo.Item1;
//            builder.Description = mainInfo.Item2;
//            code = mainInfo.Item3;

//            while(!string.IsNullOrEmpty(code))
//            {
//                var result = Parse(code);
//                builder.AddField(result.Item1, result.Item2);
//                code = result.Item3;
//            }

//            return builder.Build();
            
//            //Title, Description, rest string
//            (string, string, string) Parse(string c)
//            {
//                c = c.Remove(0, c.IndexOf('"') + 1);
//                string title = c.Substring(0, c.IndexOf('"'));
//                c = c.Remove(0, c.IndexOf('>') + 2);
//                string description = c.Substring(0, c.IndexOf('"'));

//                int next = c.IndexOf('>');

//                if (next == -1)
//                    next = c.IndexOf('"');

//                c = c.Remove(0, next + 1);
                
//                return (title, description, c);
//            }
//        }
//    }
//}