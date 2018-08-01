using System;
using System.ServiceProcess;
using AutomaticReminderCommon;

namespace AutomaticReminderService
{
    public class AutomaticReminderWinService : ServiceBase
    {
            /// <summary>
        /// Public Constructor for WindowsService.
        /// - Put all of your Initialization code here.
        /// </summary>
        public AutomaticReminderWinService()
        {
            ServiceName = CommonAutomaticReminder.ServiceName;
            EventLog.Log = "Application";
            
            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;
        }
        // The main entry point for the process
        public static void Main()
        {
            Run(new AutomaticReminderWinService());                
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ServiceName = CommonAutomaticReminder.ServiceName;
        }

        private AutomaticReminderService.AutomaticReminder _automaticReminder;

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            Logger.LogFormat("Service {0} is starting...", ServiceName);
            try
            {
                _automaticReminder = new AutomaticReminderService.AutomaticReminder();
                _automaticReminder.StartReminder();
            }
            catch (Exception e)
            {
                Logger.LogFormat("Cought an exception OnStart: {0}", e.Message);
            }

        }
        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            Logger.LogFormat("Service {0} is stopping...", ServiceName);
            if (_automaticReminder != null)
                _automaticReminder.StopReminder();
            Logger.Close();
        }
    }
}