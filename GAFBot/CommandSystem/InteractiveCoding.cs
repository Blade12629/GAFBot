using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GAFBot
{
    public static class Coding
    {
        public static class Compiler
        {
            public static KeyValuePair<bool, object> Compile(string Code, string Parameter, ulong currentGuild, ulong currentChannel, bool ShowErrors = true)
            {
                if (Code == null || string.IsNullOrEmpty(Code))
                    return new KeyValuePair<bool, object>(false, null);

                return RunCSharpCode2(Code, currentGuild, currentChannel, ShowErrors, Parameter);
            }

            private static KeyValuePair<bool, object> RunCSharpCode(string CSharpCode, ulong currentGuild, ulong currentChannel, bool ShowErrors, string StringParameter)
            {
                try
                {
                    #region Encapsulate Code into a single Method
                    string Code =
                        "using System;" + Environment.NewLine +
                        "using System.IO;" + Environment.NewLine +
                        "using System.Text;" + Environment.NewLine +
                        "using System.Collections;" + Environment.NewLine +
                        "using System.Linq;" + Environment.NewLine +
                        "using System.Diagnostics;" + Environment.NewLine +
                        "using System.Xml;" + Environment.NewLine +
                        "using System.Reflection;" + Environment.NewLine +
                        "using System.Collections.Generic;" + Environment.NewLine +
                        "using System.Web;" + Environment.NewLine +
                        "using System.Threading.Tasks;" + Environment.NewLine +
                        "using Newtonsoft.Json;" + Environment.NewLine +
                        "using GafBot;" + Environment.NewLine +
                        "using GafBot.MessageSystem;" + Environment.NewLine +
                        "using GafBot.Osu;" + Environment.NewLine +
                        "using GafBot.Osu.Api;" + Environment.NewLine +

                        "public class UserClass" + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "public object UserMethod( string StringParameter )" + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "object Result = null;" + Environment.NewLine +
                        "var dclient = Coding.Methods.GetClient();" + Environment.NewLine +
                        Environment.NewLine +
                        Environment.NewLine +

                        CSharpCode +

                        Environment.NewLine +
                        Environment.NewLine +
                        "return Result;" + Environment.NewLine +
                        "}" + Environment.NewLine +
                        "}";
                    #endregion

                    #region Compile the Dll to Memory

                    #region Make Reference List
                    Assembly[] FullAssemblyList = AppDomain.CurrentDomain.GetAssemblies();

                    System.Collections.Specialized.StringCollection ReferencedAssemblies_sc = new System.Collections.Specialized.StringCollection();

                    foreach (Assembly ThisAssebly in FullAssemblyList)
                    {
                        try
                        {
                            if (ThisAssebly is System.Reflection.Emit.AssemblyBuilder)
                            {
                                // Skip dynamic assemblies
                                continue;
                            }

                            ReferencedAssemblies_sc.Add(ThisAssebly.Location);
                        }
                        catch (NotSupportedException)
                        {
                            // Skip other dynamic assemblies
                            continue;
                        }
                    }

                    string[] ReferencedAssemblies = new string[ReferencedAssemblies_sc.Count];
                    ReferencedAssemblies_sc.CopyTo(ReferencedAssemblies, 0);
                    #endregion

                    Microsoft.CSharp.CSharpCodeProvider codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
#pragma warning disable CS0618 // Type or member is obsolete
                    System.CodeDom.Compiler.ICodeCompiler CSharpCompiler = codeProvider.CreateCompiler();
#pragma warning restore CS0618 // Type or member is obsolete
                    System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters(ReferencedAssemblies)
                    {
                        GenerateExecutable = false,
                        GenerateInMemory = true,
                        IncludeDebugInformation = false,
                        OutputAssembly = "ScreenFunction"
                    };
                    System.CodeDom.Compiler.CompilerResults CompileResult = CSharpCompiler.CompileAssemblyFromSource(parameters, Code);
                    #endregion

                    if (CompileResult.Errors.HasErrors == false)
                    { // Successful Compile
                        #region Run "UserMethod" from "UserClass"
                        System.Type UserClass = CompileResult.CompiledAssembly.GetType("UserClass");
                        object Instance = Activator.CreateInstance(UserClass, false);
                        return new KeyValuePair<bool, object>(true, UserClass.GetMethod("UserMethod").Invoke(Instance, new object[] { StringParameter }));
                        #endregion
                    }
                    else // Failed Compile
                    {
                        if (ShowErrors)
                        {
                            #region Show Errors
                            StringBuilder ErrorText = new StringBuilder();

                            foreach (System.CodeDom.Compiler.CompilerError Error in CompileResult.Errors)
                            {
                                ErrorText.Append("Line " + (Error.Line - 1) +
                                    " (" + Error.ErrorText + ")" +
                                    Environment.NewLine);
                            }

                            Logger.Log("Compiler: " + ErrorText.ToString(), LogLevel.Trace);
                            #endregion

                        }
                    }
                }
                catch (Exception E)
                {
                    if (ShowErrors)
                        return new KeyValuePair<bool, object>(false, E);
                }

                return new KeyValuePair<bool, object>(false, null);
            }
            private static KeyValuePair<bool, object> RunCSharpCode2(string CSharpCode, ulong currentGuild, ulong currentChannel, bool ShowErrors, string StringParameter)
            {
                try
                {
                    #region Encapsulate Code into a single Method

                    string baseMethodCode =
                        "var dclient = Coding.Methods.GetClient();" + Environment.NewLine;

                    string code =
                        "using System;" + Environment.NewLine +
                        "using System.IO;" + Environment.NewLine +
                        "using System.Text;" + Environment.NewLine +
                        "using System.Collections;" + Environment.NewLine +
                        "using System.Linq;" + Environment.NewLine +
                        "using System.Diagnostics;" + Environment.NewLine +
                        "using System.Xml;" + Environment.NewLine +
                        "using System.Reflection;" + Environment.NewLine +
                        "using System.Collections.Generic;" + Environment.NewLine +
                        "using System.Web;" + Environment.NewLine +
                        "using System.Threading.Tasks;" + Environment.NewLine +
                        "using Newtonsoft.Json;" + Environment.NewLine +
                        "using GAFBot;" + Environment.NewLine +
                        "using GAFBot.MessageSystem;" + Environment.NewLine +
                        "using GAFBot.Osu;" + Environment.NewLine +
                        "using GAFBot.Osu.Api;" + Environment.NewLine +
                        "using DSharpPlus;" + Environment.NewLine +
                        "using DSharpPlus.Entities;" + Environment.NewLine +
                        "using DSharpPlus.EventArgs;" + Environment.NewLine +
                        "using DSharpPlus.Exceptions;" + Environment.NewLine +
                        Environment.NewLine +
                        "namespace GAFBot" + Environment.NewLine +
                        Environment.NewLine +
                        "{" + Environment.NewLine +
                        "public class UserClass" + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "public object UserMethod( string nl )" + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "object Result = null;" + Environment.NewLine +
                        baseMethodCode + Environment.NewLine +
                        Environment.NewLine +
                        Environment.NewLine +

                        CSharpCode +

                        Environment.NewLine +
                        Environment.NewLine +
                        "return Result;" + Environment.NewLine +
                        "}" + Environment.NewLine +
                        "}" + Environment.NewLine +
                        "}";
                    #endregion
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
                    string assemblyName = Path.GetRandomFileName();
                    MetadataReference[] references = new MetadataReference[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Xml.NamespaceHandling).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Web.HttpUtility).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.DateFormatHandling).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(GAFBot.Program).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(DSharpPlus.AsyncEvent).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Collections.Concurrent.Partitioner).Assembly.Location),
                        MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
                };

                    CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTrees: new[] { syntaxTree }, 
                                                                            references: references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                    Assembly ass = null;

                    using (MemoryStream mstream = new MemoryStream())
                    {
                        EmitResult result = compilation.Emit(mstream);

                        if (!result.Success)
                        {
                            Logger.Log("Compiler: Error at compiling", LogLevel.Trace);
                            result.Diagnostics.Where(diag => diag.IsWarningAsError || diag.Severity == DiagnosticSeverity.Error).ToList().ForEach(diag => Logger.Log("Compiler: " + diag.GetMessage(), LogLevel.Trace));
                            return new KeyValuePair<bool, object>(false, null);
                        }

                        mstream.Seek(0, SeekOrigin.Begin);
                        ass = AssemblyLoadContext.Default.LoadFromStream(mstream);
                    }

                    if (ass != null)
                    {
                        Type type = ass.GetType("GAFBot.UserClass");
                        object instance = Activator.CreateInstance(type);
                        MethodInfo method = type.GetMethod("UserMethod");
                        method.Invoke(instance, new[] { Environment.NewLine });
                    }

                }
                catch (Exception E)
                {
                    Logger.Log("Compiler: " + E.ToString(), LogLevel.Trace);
                    if (ShowErrors)
                        return new KeyValuePair<bool, object>(false, E);
                }

                return new KeyValuePair<bool, object>(false, null);
            }

        }

        public static class Discord
        {
            public static DSharpPlus.DiscordClient GetClient()
                => Program.Client;

            public static DSharpPlus.Entities.DiscordGuild GetGuild(ulong id)
                => GetClient().Guilds.ToList().Find(p => p.Key == id).Value;

            public static DSharpPlus.Entities.DiscordMessage SendMessage(ulong channelID, string message)
                => Program.Client.SendMessageAsync(GetChannel(channelID), message).Result;

            public static DSharpPlus.Entities.DiscordMessage SendPrivateMessage(ulong userID, string message)
                => GetPrivChannel(userID)?.SendMessageAsync(message).Result;

            public static void React(ulong channelId, ulong messageId, string emote)
            {
                var dclient = GetClient();
                var dchannel = GetChannel(channelId);
                if (dchannel == null)
                    return;

                var dmessage = dchannel.GetMessageAsync(messageId).Result;

                if (dmessage == null)
                    return;

                dmessage.CreateReactionAsync(DSharpPlus.Entities.DiscordEmoji.FromName(dclient, emote)).Wait();
            }

            public static void ConsoleLine(string Message)
                => Logger.Log(Message);

            public static void SetUserName(ulong userId, ulong guildId, string name, string reason = "null")
            {
                var member = GetMember(userId, guildId);
                member.ModifyAsync(nickname: name, reason: reason).Wait();
            }
            
            public static DSharpPlus.Entities.DiscordChannel GetChannel(ulong id)
                => GetClient().GetChannelAsync(id).Result;

            public static DSharpPlus.Entities.DiscordUser GetUser(ulong id)
                => GetClient().GetUserAsync(id).Result;

            public static DSharpPlus.Entities.DiscordMember GetMember(ulong id, ulong guildId)
            {
                var dguild = GetGuild(guildId);
                return dguild.GetMemberAsync(id).Result;
            }

            public static DSharpPlus.Entities.DiscordDmChannel GetPrivChannel(ulong dUserId)
            {
                var duser = GetUser(dUserId);
                return GetClient().CreateDmAsync(duser).Result;
            }

            public static DSharpPlus.Entities.DiscordRole GetRole(ulong guildId, ulong roleId)
            {
                var guild = GetGuild(guildId);
                return guild.Roles.ToList().Find(dr => dr.Id == roleId);
            }

            public static void AssignRole(ulong id, ulong guildid, ulong roleid, string reason = "null")
            {
                try
                {
                    Logger.Log($"InteractiveCoding: Assigning role {id} : {guildid} : {roleid} : {reason}", LogLevel.Trace);
                    var client = GetClient();
                    var guild = GetGuild(guildid);
                    var member = guild.GetMemberAsync(id).Result;
                    var role = guild.GetRole(roleid);

                    member.GrantRoleAsync(role, reason).Wait();
                    Logger.Log("InteractiveCoding: Assigned role", LogLevel.Trace);
                }
                catch (Exception ex)
                {
                    Logger.Log("InteractiveCoding: " + ex.ToString(), LogLevel.Trace);
                }
            }

            public static bool CreateRole(DSharpPlus.DiscordClient client, ulong GuildID, string RoleName, int r, int g, int b)
            {
                DSharpPlus.Entities.DiscordGuild dguild = client.GetGuildAsync(GuildID).Result;

                dguild.CreateRoleAsync(RoleName, DSharpPlus.Permissions.None, new DSharpPlus.Entities.DiscordColor(r, g, b));
                return true;
            }
        }

        public static class General
        {
            public static void SetAccessLevel(ulong id, MessageSystem.AccessLevel accessLevel = MessageSystem.AccessLevel.User)
            {
                using (Database.GAFContext context = new Database.GAFContext())
                {
                    var buser = context.BotUsers.First(bm => (ulong)bm.DiscordId == id);

                    if (buser == null)
                        return;

                    buser.AccessLevel = (short)accessLevel;

                    context.BotUsers.Update(buser);
                    context.SaveChanges();
                }
            }

            public static MessageSystem.AccessLevel GetAccessLevel(ulong id)
            {
                MessageSystem.AccessLevel access = MessageSystem.AccessLevel.User;

                using (Database.GAFContext context = new Database.GAFContext())
                {
                    var user = context.BotUsers.FirstOrDefault(u => u.Id == (long)id);

                    if (user == null)
                        return access;

                    return (MessageSystem.AccessLevel)user.AccessLevel;
                }
            }
            
        }
    }
}
