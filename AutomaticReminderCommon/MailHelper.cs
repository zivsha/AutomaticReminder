using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace AutomaticReminderCommon
{
    public static class MailHelper
    {
        #region Private Members

        private const string SmtpServer = "10.253.24.26";
        private static readonly SmtpClient SmtpClient = new SmtpClient(SmtpServer);

        #endregion Private Members

        /// <summary>
        /// A function which sends an email to specific users.
        /// </summary>
        /// <param name="mailRecipients"> The users to whom the mail will be sent. </param>
        /// <param name="ccRecipients"> The list of CC recepients. </param>
        /// <param name="listOfAttachments"> The list of attachments. </param>
        /// <param name="senderAddress"> The address of the user sending the mail. </param>
        /// <param name="senderName"> The name of the user sending the mail.. </param>
        /// <param name="mailMessage"> The message which will be sent. </param>
        /// <param name="mailSubject"> The subject of the message. </param>
        public static void SendMailMessage(List<string> mailRecipients, List<string> ccRecipients, List<string> listOfAttachments, string senderAddress, string senderName, string mailMessage, string mailSubject)
        {
            try
            {
                var mailBody = mailMessage;

                var emailMessage = new MailMessage { From = new MailAddress(senderAddress, senderName) };

                foreach (var currentMember in mailRecipients)
                    emailMessage.To.Add(currentMember);

                if (ccRecipients != null)
                {
                    foreach (var currentMember in ccRecipients)
                        emailMessage.CC.Add(currentMember);
                }

                emailMessage.Subject = mailSubject;
                emailMessage.IsBodyHtml = true;
                emailMessage.Body = mailBody;

                if (listOfAttachments != null)
                {
                    foreach (var currentAttachment in listOfAttachments)
                        emailMessage.Attachments.Add(new Attachment(currentAttachment));
                }

                SmtpClient.Send(emailMessage);

                emailMessage.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public static bool SendAutomaticReminderEmail(string name, string email, string when, DateTime dueDate)
        {
            if (email.Split('@').Length < 2)
            {
                Logger.LogFormat("Invalid email: {0}", email);
                return false;
            }
            try
            {
                using (var message = new AutomaticReminderMailMessage(name, email, when, dueDate))
                {
                    SmtpClient.Send(message);
                    Logger.LogFormat("Sent email to {0} ({1}): {2}", email, name, message.Body);
                }
            }
            catch (Exception ex)
            {
                Logger.LogFormat("Exception caught in SendAutomaticReminderEmail(): {0}", ex.ToString());
                return false;
            }
            return true;
        }
    }
}