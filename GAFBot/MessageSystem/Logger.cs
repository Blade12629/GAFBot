using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GAFBot.MessageSystem
{
    public class Logger
    {
        public string File { get; private set; }

        public Logger(string file)
        {
            File = file;
            _threadQueue = new ConcurrentQueue<Action>();
        }

        public void Initialize()
        {
            Threadpublic = new Thread(new ThreadStart(() => Thread_Start(File)))
            {
                Name = "GAF.Logger",
                Priority = ThreadPriority.AboveNormal
            };

            Threadpublic.Start();

            Log("Started GAF.Logger thread");
        }

        public Thread Threadpublic { get; private set; }
        private ConcurrentQueue<Action> _threadQueue;
        /// <summary>
        /// If true signals the thread to finish the last action and then shut down
        /// </summary>
        private bool _threadShouldDispose;

        private void Thread_Start(string file)
        {
            try
            {
                while (!_threadShouldDispose)
                {
                    Action nextAction = next();
                    nextAction.Invoke();
                }

                if (_threadShouldDispose)
                    dispose();

                void dispose()
                {
                    _threadShouldDispose = false;

                    if (Threadpublic.IsAlive)
                        Threadpublic.Abort();
                }
                Action next()
                {
                    Action returnAction = null;

                    while (!_threadQueue.TryDequeue(out returnAction))
                        Task.Delay(5).Wait();

                    return returnAction;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        public void FileWriteLine(string file, string line)
        {
            System.IO.File.AppendAllText(file, line + Environment.NewLine);
        }

        public void Insert(string line)
        {
            _threadQueue.Enqueue(new Action(() => FileWriteLine(File, line)));
        }

        public void Log(string line, bool addDate = true, bool addNewLineAtEnd = true, bool showConsole = true, bool logToFile = true)
        {
            Task.Run(() =>
            {
                string newLine = "";

                if (addDate)
                    newLine += $"{DateTime.Now}: {line}";
                if (addNewLineAtEnd)
                    newLine += Environment.NewLine;

                if (showConsole)
                    Console.WriteLine(newLine);
                if (logToFile)
                    Insert(newLine);
            });
        }
    }
}
