using GAFBot.Database;
using GAFBot.MessageSystem;
using System;
using System.Linq;

namespace GAFBot.Commands
{
    public class CompileCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "compile"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public string Description
        {
            get
            {
                using (GAFContext context = new GAFContext())
                    return context.BotLocalization.First(l => l.Code.Equals("cmdDescriptionCompile")).String;
            }
        }

        public string DescriptionUsage
        {
            get
            {
                using (GAFContext context = new GAFContext())
                    return context.BotLocalization.First(l => l.Code.Equals("cmdUsageCompile")).String;
            }
        }

        public static void Init()
        {
            Program.CommandHandler.Register(new CompileCommand() as ICommand);
            Logger.Log(nameof(CompileCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            var result = Coding.Compiler.Compile(e.AfterCMD, "", e.GuildID ?? 0, e.ChannelID, true);

            if (result.Key)
                Coding.Discord.SendMessage(e.ChannelID, "Compiled and ran successfull");
            else
            {
                if (result.Value == null)
                {
                    Coding.Discord.SendMessage(e.ChannelID, "Failed to compile/run: unkown reason");
                    return;
                }

                Exception ex = result.Value as Exception;

                if (ex == null)
                    Coding.Discord.SendMessage(e.ChannelID, "Failed to compile: " + (string)result.Value);
                else
                    Coding.Discord.SendMessage(e.ChannelID, "Failed to run: " + ex.ToString());
            }
        }
    }
}
