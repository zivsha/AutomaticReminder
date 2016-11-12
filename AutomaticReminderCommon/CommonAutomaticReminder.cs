using System;
using System.IO;
using System.Reflection;

namespace AutomaticReminderCommon
{
    public static class CommonAutomaticReminder
    {
        public static string DbPath { get; private set; }
        public static string AutomaticReminderIcsTemplatePath { get; private set; }
        static CommonAutomaticReminder()
        {
            DbPath = UserConfiguration.DatabasePath;
            AutomaticReminderIcsTemplatePath = UserConfiguration.AutomaticReminderIcsTemplatePath;
        }

        public const string ServiceName = @"AutomaticReminder";
        public const string Client2ServerNamedPipeName = @"AutomaticReminderPipeC2S";
        public const string Server2ClientNamedPipeName = @"AutomaticReminderPipeS2C";
        public const string TestReminderRemoteCommand = @"TestReminderRemoteCommand";
        public const string StartReminderRemoteCommand = @"StartAutomaticReminder";
        public const string AutomaticReminderFinished = @"Automatically Reminded";
    }
}
