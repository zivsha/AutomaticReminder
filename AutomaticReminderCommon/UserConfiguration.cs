using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomaticReminderCommon
{
    public static class UserConfiguration
    {
        public static string DatabasePath { get; private set; }
        public static string FromEmail { get; private set; }
        public static string FromName { get; private set; }
        public static string TestPhoneNumber { get; private set; }
        
        public static string AutomaticReminderIcsTemplatePath { get; private set; }
        public static string EventLocation { get; private set; }
        public static TimeSpan StartTime { get; private set; }
        //TODO: move all SMS related configurations to different class\section
        public static string SmsServerUserName { get; set; }
        public static string SmsServerPassword { get; set; }
        public static string SmsServerGatway { get; set; }
        public static string LogPath { get; set; }
        public static string ServiceName { get; set; } = "AutomaticReminder";
        public static string ServiceDescription { get; set; } = "My Automatic Reminder";
        public static string ServiceDisplayName { get; set; } = "Automatic Reminder";
        public static ServiceStartMode ServiceStartType { get; set; } = ServiceStartMode.Manual;

        private const string filePath = @"UserConfigurations.txt"; //TODO: edit this
        static UserConfiguration()
        {
            if (!File.Exists(filePath))
            {
                BalloonTipManager.CreateBaloonTipError($"File path \"{filePath}\" doesn't exist");
                Logger.LogFormat("Error: Configuration file doesn't exist at: {0}", filePath);
                return;
            }
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }
                var keyValue = line.Split('=');
                if (keyValue.Length < 2)
                {
                    string msg = $"Error parsing line: \"{line}\" in configuration file";
                    BalloonTipManager.CreateBaloonTipError(msg);
                    Logger.LogFormat(msg);
                    continue;
                }
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();
                switch (key)
                {
                    case "DatabasePath": DatabasePath = value; break;
                    case "FromEmail": FromEmail = value; break;
                    case "FromName": FromName = value; break;
                    case "TestPhoneNumber": TestPhoneNumber = value; break;
                    case "AutomaticReminderIcsTemplatePath": AutomaticReminderIcsTemplatePath = value; break;
                    case "EventLocation": EventLocation = value; break;
                    case "StartTime":
                        var time = value.Split(':');
                        if (time.Length < 2)
                        {
                            Logger.LogFormat("Error: StartTime in configuration file ({0}) is in an invalid format.", value);
                            break;
                        }
                        var hours = time[0].Trim();
                        var minutes = time[1].Trim();
                        StartTime = new TimeSpan(0, Convert.ToInt32(hours), Convert.ToInt32(minutes),0);
                        break;
                    case "SmsServerUserName" : SmsServerUserName = value; break;
                    case "SmsServerPassword": SmsServerPassword = value; break;
                    case "SmsServerGatway": SmsServerUserName = value; break;
                    case "LogPath": LogPath = value.Replace("\"",""); break;
                    case "ServiceName": ServiceName = value; break;
                    case "ServiceDescription": ServiceDescription = value; break;
                    case "ServiceDisplayName": ServiceDisplayName = value; break;
                    case "ServiceStartType":
                        {
                            ServiceStartMode type;
                            if (Enum.TryParse(value, out type))
                            {
                                ServiceStartType = type;
                            }
                            break;
                        }
                    default:
                        BalloonTipManager.CreateBaloonTipError($"No such configuration: {key}");
                        Logger.LogFormat("Error: No such configuration: {0}" , key);
                        break;
                }
            }
        }

    }
}
