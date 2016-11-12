﻿using System.ServiceProcess;

namespace AutomaticReminderService
{
    /// <summary>
    ///     Summary description for AutomaticReminderServiceInstaller.
    /// </summary>
    [System.ComponentModel.RunInstaller(true)]
    public class AutomaticReminderServiceInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        ///    Required designer variable.
        /// </summary>
        //private System.ComponentModel.Container components;
        private System.ServiceProcess.ServiceInstaller _serviceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller _serviceProcessInstaller;

        public AutomaticReminderServiceInstaller()
        {
            InitializeComponent();
        }

        /// <summary>
        ///    Required method for Designer support - do not modify
        ///    the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _serviceInstaller = new ServiceInstaller();
            _serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            _serviceInstaller.Description = "My Automatic Reminder";
            _serviceInstaller.DisplayName = "Automatic Reminder";
            _serviceInstaller.ServiceName = "AutomaticReminder"; //TODO: get from UserConfiguration
            _serviceInstaller.StartType = ServiceStartMode.Manual;

            Installers.Add(_serviceProcessInstaller);
            Installers.Add(_serviceInstaller);
        }
    }
}