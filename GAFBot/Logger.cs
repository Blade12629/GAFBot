using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace GAFBot
{
    public static class Logger
    {
        //private static StreamWriter _logStream;
        private static object _logLock = new object();
        private static ConcurrentQueue<(string, LogLevel)> _queue;

        public static void Initialize()
        {
            _queue = new ConcurrentQueue<(string, LogLevel)>();
        }


        public static void Log(string message, LogLevel level = LogLevel.Info, bool console = true, [CallerMemberName()] string caller = "")
        {
            string toLog = $"{DateTime.UtcNow}: *{level}* {caller}: {message}";
            _queue.Enqueue((toLog, level));

            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(obj =>
            {
                lock(_logLock)
                {
                    if (!_queue.TryDequeue(out (string, LogLevel) p))
                        return;
                    
                    switch(p.Item2)
                    {
                        case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case LogLevel.WARNING:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case LogLevel.ERROR:
                        Console.ForegroundColor = ConsoleColor.Red;
                            break;
                    }
#if DEBUG
                    Console.WriteLine(toLog);
#else
                    if (level == LogLevel.Trace)
                        return;
                    else if (console)
                        Console.WriteLine(toLog);
#endif

                    if (level != LogLevel.Info)
                        Console.ForegroundColor = ConsoleColor.White;

                    if (level != LogLevel.Trace)
                    {
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            using (Database.GAFContext context = new Database.GAFContext())
                            {
                                context.BotLog.Add(new Database.Models.BotLog()
                                {
                                    Date = DateTime.UtcNow,
                                    Message = message,
                                    Type = level.ToString()
                                });

                                context.SaveChanges();
                            }
                        });
                    }
                }
            }));

        }

    }

    public enum LogLevel
    {
        Info,
        Trace,
        WARNING,
        ERROR
    }
}
