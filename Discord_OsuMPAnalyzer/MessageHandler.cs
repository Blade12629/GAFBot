using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_OsuMPAnalyzer
{
    public static class MessageHandler
    {
        public static void NewMessage(MessageCreateEventArgs input)
        {
            if (input.Message.Author == Program._DClient.CurrentUser || input.Message == null) return;

            string Message = input.Message.Content;

            string MatchString = "https://osu.ppy.sh/community/matches/";
            if (Message.StartsWith(MatchString))
            {
                string sub = Message.Substring(MatchString.Length - 1);
                //ToDo
            }

        }

    }
}
