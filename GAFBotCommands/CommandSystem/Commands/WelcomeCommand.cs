using GAFBot.MessageSystem;
using System;

namespace GAFBot.Commands
{
    public class WelcomeCommand : ICommand
    {
        public char Activator { get => '!'; }
        public char ActivatorSpecial { get => default(char); }
        public string CMD { get => "welcome"; }
        public AccessLevel AccessLevel => AccessLevel.Admin;

        public static void Init()
        {
            Program.CommandHandler.Register(new WelcomeCommand() as ICommand);
            Coding.Methods.Log(typeof(WelcomeCommand).Name + " Registered");
        }

        public void Activate(CommandEventArg e)
        {
            {
                var dguild = Coding.Methods.GetGuild((ulong)Program.Config.DiscordGuildId);
                var dchannel = Coding.Methods.GetChannel((ulong)Program.Config.WelcomeChannel);

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
            try
            {
                if (string.IsNullOrEmpty(e.AfterCMD))
                    return;

                var duser = Coding.Methods.GetUser(e.DUserID);

                if (e.AfterCMD.StartsWith("_test", StringComparison.CurrentCultureIgnoreCase))
                    (Modules.ModuleHandler.Get("message") as IMessageHandler)?.WelcomeMessage(e.ChannelID, Program.Config.WelcomeMessage, duser.Mention);

                else
                {
                    Program.Config.WelcomeMessage = e.AfterCMD;
                    Coding.Methods.SendMessage(e.ChannelID, "Updated Welcomemessage to " + e.AfterCMD);
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
