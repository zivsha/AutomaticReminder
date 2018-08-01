using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutomaticReminderCommon
{
    //TODO: refactor this logger, use a message queue and a single thread to write to file
    public static class Logger
    {
        class LogMessage
        {
            public string Message { get; set; }
            public DateTime Time { get; set; }
        }

        private static BlockingCollection<LogMessage> _logQueue = new BlockingCollection<LogMessage>();
        private static string LogPath = UserConfiguration.LogPath;
        private static Task _writerThread;
        private static CancellationTokenSource source = new CancellationTokenSource();
        private static CancellationToken token = source.Token;
        static Logger()
        {
            if (String.IsNullOrWhiteSpace(LogPath))
            {
                BalloonTipManager.CreateBaloonTipError("Failed to create logger. Invalid path to log file");
                _logQueue = null;
                return;
            }
            if (!File.Exists(LogPath))
            {
                File.WriteAllText(LogPath, String.Format("{0} Log File Created\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }

            _writerThread = Task.Run(() => FlushQueueToFile());
        }

        private static void FlushQueueToFile()
        {
            while (!token.IsCancellationRequested)
            {
                LogMessage log;
                while (_logQueue.TryTake(out log, 100))
                {
                    var timestamp = log.Time.ToString("dd/MM/yyyy HH:mm:ss");
                    File.AppendAllText(LogPath, $"{timestamp} {log.Message}{Environment.NewLine}");
                }
            }
        }

        public static void LogFormat(string format, params object[] args)
        {
            var timestamp = DateTime.Now;
            string msg = String.Format(format, args);
            Logger._logQueue?.Add(new LogMessage { Message = msg, Time = timestamp });
        }

        public static void Close()
        {
            source.Cancel();
            source.Dispose();
        }
    }
}