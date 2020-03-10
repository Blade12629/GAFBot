using System;
using System.Collections.Generic;
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
    }
}
