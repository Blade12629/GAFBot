﻿using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class TestCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "test"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public static void Init()
        {
            Program.CommandHandler.Register(new TestCommand() as ICommand);
            Coding.Methods.Log(typeof(TestCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            try
            {
                var dguild = Coding.Methods.GetGuild(147255853341212672);
                var dchannel = Coding.Methods.GetChannel(578984727583784980);

                try
                {
                    string response = "```" + Environment.NewLine;

                    foreach (var role in dguild.Roles)
                        response += role.Name + ":" + role.Id + Environment.NewLine;

                    response += "```";
                    dchannel.SendMessageAsync(response).Wait();
                }
                catch (Exception ex)
                {
                    dchannel.SendMessageAsync(ex.ToString()).Wait();
                }
            }
            catch (Exception ex)
            {
                Coding.Methods.SendMessage(e.ChannelID, ex.ToString());
                Console.WriteLine(ex);
            }
        }
    }
}
