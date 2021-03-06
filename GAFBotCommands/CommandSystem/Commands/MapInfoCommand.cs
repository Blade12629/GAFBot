﻿using GAFBot.MessageSystem;
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

        public string Description => "Gets info about a beatmap";

        public string DescriptionUsage => "!m maplink";

        public static void Init()
        {
            Program.CommandHandler.Register(new MapInfoCommand() as ICommand);
            Logger.Log(nameof(MapInfoCommand) + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            if (!e.AfterCMD.StartsWith("https://osu.ppy.sh/b", StringComparison.CurrentCultureIgnoreCase) &&
                !e.AfterCMD.StartsWith("https://osu.ppy.sh/beatmapsets", StringComparison.CurrentCultureIgnoreCase))
                return;

            (Modules.ModuleHandler.Get("message") as IMessageHandler)?.GetBeatmapInfo(e.AfterCMD, e.ChannelID);
        }
    }
}
