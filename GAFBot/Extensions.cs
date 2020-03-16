using GAFBot.Database.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace GAFBot
{
    public static class Extensions
    {
        public static void Log(this Exception ex, [CallerMemberName] string caller = "")
            => Logger.Log(message: ex.ToString(), 
                          level: LogLevel.ERROR,
                          caller: caller);

        public static string ToString(this string[] array, int indexStart, int indexEnd, char splitter = ' ')
        {
            if (array == null || indexStart >= array.Length)
                return "";

            string result = array[indexStart];

            indexStart = Math.Max(indexStart, 0);
            indexEnd = Math.Min(indexEnd, array.Length - 1);

            for (int i = indexStart + 1; i <= indexEnd; i++)
                result += splitter + array[i];

            return result;
        }

        internal static string GetFromResources(this Assembly ass, string resourceName)
        {
            using (Stream stream = ass.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
