using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AutomaticReminderCommon;
using Timer = System.Timers.Timer;

namespace AutomaticReminderService
{
    public class AutomaticReminder
    {
        private readonly object sendLock = new object();
        private readonly Timer _checkTimeToSendTimer;
        private readonly Thread _pipeCommTask;

        public AutomaticReminder()
        {
            _checkTimeToSendTimer = new Timer(new TimeSpan(1, 0, 0).TotalMilliseconds);
            _checkTimeToSendTimer.Elapsed += OnCheckIfTimeToSendTimerElapsed;
            Logger.LogFormat("New Instance of AutomaticReminder class created");
            _pipeCommTask = new Thread(PipelineReaderProc) {IsBackground = true};
            _pipeCommTask.Start();
            Logger.LogFormat("Started new thread to run PipelineReaderProc");

        }

        private void OnCheckIfTimeToSendTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Logger.LogFormat("Timer Elapsed. Checking if email needs to be sent...");
            SendMailIfNeeded();
        }

        private void PipelineReaderProc()
        {
            Logger.LogFormat("Started Pipeline Reader Thread");
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(CommonAutomaticReminder.Client2ServerNamedPipeName))
                    {
                        Logger.LogFormat("server started waiting for connection");
                        server.WaitForConnection();
                        Logger.LogFormat("server finished waiting for connection");
                        var reader = new StreamReader(server);
                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (line != null && line.Equals(CommonAutomaticReminder.StartReminderRemoteCommand))
                            {
                                Logger.LogFormat("Received pipe message: {0}", line);
                                SendMailIfNeeded();
                                server.Disconnect();
                                break;
                            }
                            else if (line != null && line.Equals(CommonAutomaticReminder.TestReminderRemoteCommand))
                            {
                                Logger.LogFormat("Received pipe message: {0}", line);
                                TestSendMailIfNeeded();
                                server.Disconnect();
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogFormat("PipelineReaderProc error: {0}",e.Message);
                    Thread.Sleep(5000);
                }

            }
        }

        private void TestSendMailIfNeeded()
        {
            Logger.LogFormat("Sending Fake mail");
            if (MailHelper.SendAutomaticReminderEmail(UserConfiguration.FromName, AutomaticReminderMailMessage.FromEmail, "THIS IS A TEST", DateTime.Now))
            {
                OnAutomaticallyReminded(UserConfiguration.FromName, AutomaticReminderMailMessage.FromEmail, "THIS IS A TEST", DateTime.Now);
            }
            else
            {
                Logger.LogFormat("Failed to send FAKE reminder email");
            }
            SMSHelper.SendSms(UserConfiguration.FromName, UserConfiguration.TestPhoneNumber, "TEST", DateTime.Now);
        }

        public void StartReminder()
        {
             _checkTimeToSendTimer.Start();
             SendMailIfNeeded();
             Logger.LogFormat("Automatic Reminder has started and is running...");
        }
        public void StopReminder()
        {
            if (_checkTimeToSendTimer != null)
                _checkTimeToSendTimer.Stop();

            if (_pipeCommTask != null)
            {
                _pipeCommTask.Abort();
            }
            Logger.LogFormat("Automatic Reminder Stopped");
        }

        public void SendMailIfNeeded()
        {
            if (!DateTimeHelper.IsWorkingHours())
            {
                //Logger.LogFormat("It's after hours, will try again soon");
                return;
            }
            //Logger.LogFormat("It's working time!");

            lock (sendLock)
            {
                try
                {

                    Logger.LogFormat("Checking if a reminder needs to be sent...");

                    AutomaticReminderDataBase.Instance.Reload(); //Since it might have been changed 
                    foreach (var reminderEntry in AutomaticReminderDataBase.Instance.AutomaticReminderEvents)
                    {
                        string email = AutomaticReminderDataBase.Instance.Contacts[reminderEntry.Name].Email;
                        string phoneNumber = AutomaticReminderDataBase.Instance.Contacts[reminderEntry.Name].PhoneNumber;

                        if (DateTimeHelper.IsDateInPast(reminderEntry.DueDate))
                        {
                            reminderEntry.MarkAllAsSent();
                            continue;
                        }

                        if (DateTimeHelper.IsDueDateTommorow(reminderEntry.DueDate) 
                            && DateTimeHelper.IsTimeNowAfter(17)
                            && !String.IsNullOrEmpty(phoneNumber) 
                            && !reminderEntry.IsSmsReminderSent)
                        {
                            SMSHelper.SendSms(reminderEntry.Name, phoneNumber, "tomorrow", reminderEntry.DueDate);
                            reminderEntry.IsSmsReminderSent = true;
                        }

                        if (DateTimeHelper.IsDueDateToday(reminderEntry.DueDate) && !reminderEntry.IsTodayReminderSent)
                        {
                            Logger.LogFormat("Today is the day reminder to {0}, email = {1}", reminderEntry.Name, email);
                            if (MailHelper.SendAutomaticReminderEmail(reminderEntry.Name, email, "today", reminderEntry.DueDate))
                            {
                                reminderEntry.IsTodayReminderSent = true;
                                OnAutomaticallyReminded(reminderEntry.Name, email, "today", reminderEntry.DueDate);
                            }
                            else
                            {
                                Logger.LogFormat("Failed to send automatic reminder email to {0}", reminderEntry.Name);
                            }
                        }
                        else if (DateTimeHelper.IsDueDateTommorow(reminderEntry.DueDate) && !reminderEntry.Is1DayReminderSent)
                        {
                            Logger.LogFormat("1 day to go reminder to {0}, email = {1}", reminderEntry.Name, email);
                            if (MailHelper.SendAutomaticReminderEmail(reminderEntry.Name, email, "tomorrow", reminderEntry.DueDate))
                            {
                                reminderEntry.Is1DayReminderSent = true;
                                OnAutomaticallyReminded(reminderEntry.Name, email, "tomorrow", reminderEntry.DueDate);
                            }
                            else
                            {
                                Logger.LogFormat("Failed to send automatic reminder email to {0}", reminderEntry.Name);
                            }
                        }
                        else if (DateTimeHelper.IsDueDateIn1Week(reminderEntry.DueDate) && !reminderEntry.Is1WeekReminderSent)
                        {
                            Logger.LogFormat("1 week to go reminder to {0}, email = {1}", reminderEntry.Name, email);
                            if (MailHelper.SendAutomaticReminderEmail(reminderEntry.Name, email, "next week",
                                reminderEntry.DueDate))
                            {
                                reminderEntry.Is1WeekReminderSent = true;
                                OnAutomaticallyReminded(reminderEntry.Name, email, "next week", reminderEntry.DueDate);
                            }
                            else
                            {
                                Logger.LogFormat("Failed to send email reminder to {0}", reminderEntry.Name);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogFormat("Error while trying to remind: {0}", e.Message);
                }
                Logger.LogFormat("AutomaticReminder Ended");
            }
        }



        private void OnAutomaticallyReminded(string name, string email, string when, DateTime dueDate)
        {
            Task.Factory.StartNew(() =>
            {
                using (var client = new NamedPipeClientStream(CommonAutomaticReminder.Server2ClientNamedPipeName))
                {
                    try
                    {
                        client.Connect(5000);
                    }
                    catch (TimeoutException e)
                    {
                        Logger.LogFormat("Timeout while trying to connect to pipe {0} ({1})",
                            CommonAutomaticReminder.Server2ClientNamedPipeName, e.Message);
                    }
                    catch (IOException e)
                    {
                        Logger.LogFormat("IO Exception while trying to connect to pipe {0} ({1})",
                            CommonAutomaticReminder.Server2ClientNamedPipeName, e.Message);
                    }

                    catch (Exception e)
                    {
                        Logger.LogFormat("Exception while trying to connect to pipe {0} ({1})",
                            CommonAutomaticReminder.Server2ClientNamedPipeName, e.Message);
                    }
                    var writer = new StreamWriter(client);
                    writer.WriteLine(String.Format("{0} to {1}<{2}> that {3} is their time on {4}",
                        CommonAutomaticReminder.AutomaticReminderFinished, name, email, when, dueDate));

                    writer.Flush();
                    client.WaitForPipeDrain();
                }
            });
        }
    }
}