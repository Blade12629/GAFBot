using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GAFBot.Commands
{
    public class CommandHandler : ICommandHandler
    {
        public List<ICommand> ActiveCommands { get; private set; }
        
        public bool ActivateCommand(DSharpPlus.Entities.DiscordMessage message, AccessLevel access)
        {
            try
            {
                if (message == null || string.IsNullOrEmpty(message.Content) || char.IsLetterOrDigit(message.Content[0]))
                    return false;

                if (ActiveCommands == null)
                    ActiveCommands = new List<ICommand>();

                string[] cmdsplit = message.Content.Split(' ');

                if (cmdsplit == null || cmdsplit.Length == 0)
                    return false;

                ulong id = message.Author.Id;
                string name = message.Author.Username;
                var guild = message.Channel.Guild;
                var guildid = (ulong?)message.Channel.GuildId;
                ulong chid = message.Channel.Id;
                char activator = message.Content[0];
                string _cmd = cmdsplit[0].Substring(1, cmdsplit[0].Length - 1);
                string aftercmd = "";
                if (message.Content.Length > cmdsplit[0].Length + 1)
                    aftercmd = message.Content.Remove(0, cmdsplit[0].Length + 1);

                CommandEventArg arg = new CommandEventArg(id, name, (guild == null ? (ulong?)null : guildid), chid, activator, _cmd, aftercmd);


                ICommand cmd = null;

                //For now i hardcode this cause I'm lazy
                if (activator.Equals('>'))
                {
                    Console.WriteLine(">");
                    //TextCommand.OnTextCommand(arg);
                    return true;
                }

                cmd = ActiveCommands.Find(cmd_ => cmd_.Activator.Equals(arg.Activator) && cmd_.CMD.Equals(arg.CMD));

                if (cmd == null)
                    return false;

                if ((int)cmd.AccessLevel > (int)access)
                    return false;

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    int dbid = context.BotMaintenance.Max(m => m.Id);
                    var maint = context.BotMaintenance.FirstOrDefault(m => m.Id == dbid);

                    if (maint.Enabled && access < AccessLevel.Admin)
                    {
                        Coding.Methods.ChannelMessage(chid, "Bot is currently in maintenance, please try again later" + Environment.NewLine +
                                                            "Info: " + maint.Notification ?? "no information");

                        return false;
                    }
                }

                cmd.Activate(arg);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Trace);
                return false;
            }
        }

        public void LoadCommands()
        {
            try
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                Type[] types = ass.GetTypes();

                foreach (Type t in types)
                {
                    MethodInfo[] methods = t.GetMethods();
                    MethodInfo method = methods.ToList().Find(m => m.Name.Equals("Init", StringComparison.CurrentCulture));

                    if (method != null && method.IsStatic)
                        method.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public bool Register(ICommand command)
        {
            if (ActiveCommands == null)
                ActiveCommands = new List<ICommand>();

            try
            {
                ICommand cmd = null;
                foreach (ICommand cmd_ in ActiveCommands)
                    if (cmd_.Activator.Equals(command.Activator) && cmd_.CMD.Equals(command.CMD))
                    {
                        cmd = cmd_;
                        break;
                    }

                if (cmd != null)
                    return false;

                ActiveCommands.Add(command);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Trace);
                return false;
            }
        }

        public bool Unregister(ICommand command)
        {
            if (ActiveCommands == null)
                ActiveCommands = new List<ICommand>();

            ICommand cmd = ActiveCommands.Find(ic => ic.CMD.Equals(command.CMD));

            if (cmd == null)
                return false;

            lock(ActiveCommands)
            {
                ActiveCommands.RemoveAt(ActiveCommands.FindIndex(ic => ic.CMD.Equals(command.CMD)));
            }

            return true;
        }
    }
}
