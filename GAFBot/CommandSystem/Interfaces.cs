using GAFBot.CommandSystem;
using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Commands
{
    public interface ICommandHandler
    {
        bool ActivateCommand(DSharpPlus.Entities.DiscordMessage message, AccessLevel access);
        void LoadCommands();
        bool Register(ICommand command);
        bool Unregister(ICommand command);
    }

    public interface ICommand
    {
        char Activator { get; }
        string CMD { get; }
        AccessLevel AccessLevel { get; }
        void Activate(CommandEventArg e);
    }

    public interface ICommandEventArg
    {
        ulong DUserID { get; set; }
        string DUserName { get; set; }
        ulong? GuildID { get; set; }
        ulong ChannelID { get; set; }
        char Activator { get; set; }
        string CMD { get; set; }
        string AfterCMD { get; set; }
    }

    public class CommandEventArg : ICommandEventArg
    {
        public ulong DUserID { get; set; }
        public string DUserName { get; set; }
        public ulong? GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public char Activator { get; set; }
        public string CMD { get; set; }
        public string AfterCMD { get; set; }

        public CommandEventArg(ulong duserID, string duserName, ulong? guildID, ulong channelID, char activator, string cmd, string afterCmd)
        {
            DUserID = duserID;
            DUserName = duserName;
            GuildID = guildID;
            ChannelID = channelID;
            Activator = activator;
            CMD = cmd;
            AfterCMD = afterCmd;
        }
    }
}
