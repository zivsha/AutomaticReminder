using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutomaticReminderCommon
{
    //TODO: refactor this logger, use a message queue and a single thread to write to file
    public static class Logger
    {
        private static readonly object _writeLock = new object();
        private const string LogPath = @"AutomaticReminderLog.txt"; //TODO: get from configuration file

        static Logger()
        {
            if (!File.Exists(LogPath))
            {
                File.WriteAllText(LogPath, String.Format("{0} Log File Created\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }
        }

        public static void LogFormat(string format, params object[] args)
        {
            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            string msg = String.Format(format, args);
            
            lock (_writeLock)
            {
                try
                {
                    File.AppendAllText(LogPath, String.Format("{0} {1}{2}", timestamp, msg, Environment.NewLine));
                }
                catch (IOException)
                {
                    ExponentialTryAppendToFile(LogPath, timestamp, msg);
                }
            }
        }

        private static void ExponentialTryAppendToFile(string path, string timestamp, string line)
        {
            Task.Factory.StartNew(() =>
            {
                var random = new Random(Guid.NewGuid().GetHashCode());
                for (int attempts = 0; attempts < 8; attempts++)
                {
                    var sleepTime = Convert.ToInt32(Math.Pow(2, attempts) * 100) + random.Next(-100,100);
                    try
                    {
                        File.AppendAllText(path, String.Format("{0} ({1} attempts) {2}{3}", timestamp, attempts, line,Environment.NewLine));
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(sleepTime);
                    }
                }
            });
        }
    }
}