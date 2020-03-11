using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAFBot.Commands
{
    public class MapInfoCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "m"; }
        public AccessLevel AccessLevel => AccessLevel.User;

        public static void Init()
        {
            Program.CommandHandler.Register(new MapInfoCommand() as ICommand);
            Coding.Methods.Log(typeof(MapInfoCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            if (!e.AfterCMD.StartsWith("https://osu.ppy.sh/b", StringComparison.CurrentCultureIgnoreCase))
                return;

            (Modules.ModuleHandler.Get("message") as IMessageHandler)?.GetBeatmapInfo(e.AfterCMD, e.ChannelID);
        }
    }
}
