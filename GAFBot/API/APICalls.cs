using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API
{
    public static class APICalls
    {
        private static Dictionary<byte, APICall> _apiCalls;

        public static void Init()
        {
            _apiCalls = new Dictionary<byte, APICall>();
            CallDiscordMessage callDiscordMessage = new CallDiscordMessage();
            CallPing callPing = new CallPing();

            Register(callDiscordMessage);
            Register(callPing);
        }

        public static void Register(APICall apiCall)
        {
            if (apiCall == null || apiCall.Command == 0 || _apiCalls.ContainsKey(apiCall.Command))
                return;

            _apiCalls.Add(apiCall.Command, apiCall);
            Program.Logger.Log("API: Registered call " + apiCall.Command + ", " + apiCall.GetType().Name);
        }

        public static void UnRegister(byte id)
        {
            if (!_apiCalls.ContainsKey(id))
                return;
            _apiCalls.Remove(id);
        }

        public static void Invoke(byte command, BufferReader reader, long sender)
        {
            if (!_apiCalls.ContainsKey(command))
                return;

            _apiCalls[command].Invoke(reader, sender);
        }
    }

    public class ErrorReport : List<string>
    {

    }

    public class APICall
    {
        private object _errorReport;
        public virtual byte Command { get; private set; }
        
        public virtual void Invoke(BufferReader reader, long sender)
        {

        }

        /// <summary>
        /// after creating the ideal way would be to invoke <see cref="SendErrorReport(ulong)"/> in the overriden method
        /// </summary>
        /// <param name="writer">used so when overriden, overrider can write</param>
        public virtual void CreateErrorReport(long sender, BufferWriter writer = null, params string[] messages)
        {
            writer = new BufferWriter();
            writer.WriteString($"Exception occured on {DateTime.UtcNow} (UTC)");
        }

        public void SendReport(long id, ref BufferWriter writer)
        {
            Program.ApiServer.Write(id, writer.ToArray());
        }
    }

    public class CallPing : APICall
    {
        public override byte Command => 1;

        public override void Invoke(BufferReader reader, long sender)
        {
            BufferWriter writer = new BufferWriter();
            writer.WriteString("Pong!");
            SendReport(sender, ref writer);
        }
    }

    public class CallDiscordMessage : APICall
    {
        public override byte Command => 2;

        public override void Invoke(BufferReader reader, long sender)
        {
            base.Invoke(reader, sender);

            ulong channel = reader.ReadUlong();

            string message = reader.ReadString();
            Coding.Methods.SendMessage(channel, message);
        }
        
    }

    public class CallRequestLastOsuMPMatches : APICall
    {
        public override byte Command => 3;

        public override void Invoke(BufferReader reader, long sender)
        {
            base.Invoke(reader, sender);


        }
    }
}
