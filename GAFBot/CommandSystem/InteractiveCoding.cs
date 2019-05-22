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

                            Program.Logger.Log("Compiler: " + ErrorText.ToString(), showConsole: Program.Config.Debug);
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
                        "var dclient = Coding.Methods.GetClient();" + Environment.NewLine +
                    (currentGuild == 0 ? "" : $"var dguild = dclient.GetGuildAsync(326752025750667264).Result;") + Environment.NewLine +
                    $"var dchannel = dguild.GetChannel(331108527022276610);";
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
                            Program.Logger.Log("Compiler: Error at compiling", showConsole: Program.Config.Debug);
                            result.Diagnostics.Where(diag => diag.IsWarningAsError || diag.Severity == DiagnosticSeverity.Error).ToList().ForEach(diag => Program.Logger.Log("Compiler: " + diag.GetMessage(), showConsole: Program.Config.Debug)));
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
                    Program.Logger.Log("Compiler: " + E.ToString(), showConsole: Program.Config.Debug);
                    if (ShowErrors)
                        return new KeyValuePair<bool, object>(false, E);
                }

                return new KeyValuePair<bool, object>(false, null);
            }

        }
        public static class Methods
        {
            public static void ConsoleLine(string Message)                    
                => Program.Logger.Log(Message);


            public static void Log(string line, bool addDate = true, bool addNewLineAtEnd = true, bool showConsole = true, bool logToFile = true)
                => Program.Logger.Log("InteractiveCoding: " + line, addDate, addNewLineAtEnd, showConsole, logToFile);

            public static string GetProps(string beginsWith)
            {
                Type methods = typeof(Methods);
                List<MethodInfo> mi = methods.GetMethods().ToList();

                string resultReal = "";

                mi.ForEach(m =>
                {
                    List<ParameterInfo> param = m.GetParameters().ToList();
                    string result = $"{m.Name}(";
                    param.ForEach(p => result += $" {p.ParameterType.Name} {p.Name}");
                    result += ")";
                    resultReal += result + Environment.NewLine;
                });

                return resultReal;
            }

            #region discord
            public static void SendMessage(ulong ChannelID, string Message)
                => Program.Client.SendMessageAsync(GetChannel(ChannelID), Message).Wait();

            public static void CHMessage(ulong ChannelID, string Message)
            {
                try
                {
                    Program.Client.SendMessageAsync(Program.Client.GetChannelAsync(ChannelID).Result, Message).Wait();
                }
                catch (Exception ex)
                {
                    Program.Logger.Log("InteractiveCoding: " + ex.ToString(), showConsole: Program.Config.Debug);
                }
            }

            public static void ChannelMessage(ulong ChannelID, string Message)
                => CHMessage(ChannelID, Message);

            public static DSharpPlus.DiscordClient GetClient()
                => Program.Client;

            public static DSharpPlus.Entities.DiscordGuild GetGuild(ulong id)
                => GetClient().Guilds.ToList().Find(p => p.Key == id).Value;

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
                    Program.Logger.Log($"InteractiveCoding: Assigning role {id} : {guildid} : {roleid} : {reason}", showConsole: Program.Config.Debug);
                    var client = GetClient();
                    var guild = GetGuild(guildid);
                    var member = guild.GetMemberAsync(id).Result;
                    var role = guild.GetRole(roleid);

                    member.GrantRoleAsync(role, reason).Wait();
                    Program.Logger.Log("InteractiveCoding: Assigned role", showConsole: Program.Config.Debug);
                }
                catch (Exception ex)
                {
                    Program.Logger.Log("InteractiveCoding: " + ex.ToString(), showConsole: Program.Config.Debug);

                }
            }

            public static void abc()
            {
                var client = Coding.Methods.GetClient();
                var guild = client.GetGuildAsync(147255853341212672).Result;
                var member = guild.GetMemberAsync(140896783717892097).Result;
                var role = guild.GetRole(579618821518786570);
                guild.GrantRoleAsync(member, role).Wait();
                member.GrantRoleAsync(role).Wait();
            }

            public static bool CreateRole(DSharpPlus.DiscordClient client, ulong GuildID, string RoleName, int r, int g, int b)
            {
                DSharpPlus.Entities.DiscordGuild dguild = client.GetGuildAsync(GuildID).Result;

                dguild.CreateRoleAsync(RoleName, DSharpPlus.Permissions.None, new DSharpPlus.Entities.DiscordColor(r, g, b));
                return true;
            }

            #endregion

            public static void ReloadConfig(bool reload = false)
                => Program.LoadConfig(reload);

            public static Timer MuteTimer;
            public static List<MuteInfo> Mutes;

            public static Dictionary<ulong, DateTime> MuteList;

            public static bool Mute(int Duration, ulong UserID, ulong GuildID, string reason = "null")
            {
                if (Duration <= 0)
                    Duration = 1;

                if (MuteList == null)
                    MuteList = new Dictionary<ulong, DateTime>();

                if (MuteTimer == null)
                {
                    MuteTimer = new Timer()
                    {
                        AutoReset = true,
                        Interval = 500
                    };

                    MuteTimer.Elapsed += CheckMutes;

                    MuteTimer.Start();
                }

                var client = GetClient();
                var guild = GetGuild(GuildID);
                var member = guild.GetMemberAsync(UserID).Result;
                var muteRole = guild.Roles.FirstOrDefault(r => r.Id.Equals(528648021693431808));

                foreach (var role in member.Roles)
                {
                    if (role.Id.Equals(muteRole.Id) /* MUTED role*/)
                    {
                        guild.RevokeRoleAsync(member, muteRole, "reason: " + reason).Wait();
                        return true;
                    }
                }

                guild.GrantRoleAsync(member, muteRole, "reason: " + reason).Wait();

                return true;
            }

            public static void CheckMutes(object sender, ElapsedEventArgs arg)
            {
                var toUnmute = Mutes.Where(m => m.EndsOn.Ticks <= DateTime.UtcNow.Ticks);
                var client = GetClient();

                foreach (MuteInfo m in toUnmute)
                {
                    var guild = client.GetGuildAsync(m.GuildID).Result;
                    var member = guild.GetMemberAsync(m.UserID).Result;
                    var role = guild.Roles.FirstOrDefault(r => r.Id.Equals(528648021693431808)); //MUTED
                    guild.RevokeRoleAsync(member, role, "unmute 0").Wait();
                }
            }

            public class MuteInfo
            {
                public ulong UserID { get; set; }
                public DateTime EndsOn { get; set; }
                public ulong GuildID { get; set; }

                public MuteInfo(ulong userid, DateTime EndsOn, ulong guildID)
                {
                    UserID = userid;
                    this.EndsOn = EndsOn;
                    GuildID = guildID;
                }
            }
        }
    }
}
