using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomaticReminderCommon
{
    public class AutomaticReminderMailMessage : MailMessage
    {
        public static string FromEmail = UserConfiguration.FromEmail;
        //TODO: make this configurable
        public AutomaticReminderMailMessage(string name, string email, string when, DateTime dueDate)
        {
            From = new MailAddress(FromEmail);
            Subject = "Automatic Happy-Hour reminder for " + name + ". You are in charge " + when;
            IsBodyHtml = true;
            Body = String.Format(
                "Hi {1},{0}" +
                "This is a friendly reminder that you are in charge of the next happy hour scheduled for <strong>{2} - {3}</strong>." +
                "{0}Please help your team, and you, have a happy hour :)" +
                "{0}{0}Thanks," +
                "{0}Happy Hour Reminder",
                "<br>" + Environment.NewLine, name.Split(' ')[1], when, dueDate.ToLongDateString());
            To.Add(email);
            CC.Add(FromEmail);

            string reminderBody = Body;
            Body = String.Format("{0}{1}{1}<strong>P.S: Attached a calendar reminder, please add it to your calendar</strong>", reminderBody,
                "<br>" + Environment.NewLine);
            
            DateTime exactTime = dueDate.Add(UserConfiguration.StartTime);
            string ical = File.ReadAllText(CommonAutomaticReminder.AutomaticReminderIcsTemplatePath);
            ical = ical.Replace("__TIME_NOW__", String.Format("{0:yyyyMMddTHHmmssZ}", DateTime.Now));
            ical = ical.Replace("__START_TIME__", String.Format("{0:yyyyMMddTHHmmssZ}", exactTime.ToUniversalTime()));
            ical = ical.Replace("__END_TIME__", String.Format("{0:yyyyMMddTHHmmssZ}", exactTime.AddHours(1).ToUniversalTime()));
            ical = ical.Replace("__MyLocation__", UserConfiguration.EventLocation);
            ical = ical.Replace("__MySubject__", String.Format("Buy goodies for the next happy hour.(I'm in charge at {0})", dueDate.ToLongDateString())); //TODO: make this configurable
            ical = ical.Replace("__MyBody__", reminderBody.Replace(Environment.NewLine, "<br>"));
            ical = ical.Replace("__GUID__", String.Format("{0}", Guid.NewGuid()));
            string tempFile = Path.GetTempFileName();
            tempFile = Path.Combine(Path.GetDirectoryName(tempFile), "AutomaticReminder.ics"); //TODO: make this configurable
            File.Delete(tempFile);
            File.WriteAllText(tempFile, ical);
            Attachments.Add(new Attachment(tempFile));
        }
    }
}