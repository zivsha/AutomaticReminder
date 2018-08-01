using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticReminderCommon
{
    public static class SMSHelper
    {
        public static void SendSms(string name, string phoneNumber, string when, DateTime dueDate)
        {
            string message = String.Format(
                "Hi {1},{0}" +
                "This is a friendly reminder that you are in charge of the next team happy hour scheduled for {2} - {3}." +
                "{0}RoboCookie",
                Environment.NewLine, name, when, dueDate.ToLongDateString());
            string gateway = UserConfiguration.SmsServerGatway;
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection
                {
                    ["Username"] = UserConfiguration.SmsServerUserName,
                    ["Password"] = UserConfiguration.SmsServerPassword,
                    ["Target"] = phoneNumber,
                    ["Source"] = "Automatic Reminder",
                    ["Validity"] = string.Empty,
                    ["Replace"] = string.Empty,
                    ["message"] = message
                };
                byte[] bytes = wb.UploadValues(new Uri(gateway), "POST", data);
                var response = System.Text.Encoding.Default.GetString(bytes);
                Logger.LogFormat("Sent SMS message to {0}: {1}. Response: {2}", name, phoneNumber, response);
            }
        }
    }
}
