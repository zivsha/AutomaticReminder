using System;
using System.Management;

namespace AutomaticReminderCommon
{
    public class AutomaticReminderServiceWatcher
    {
        public event Action OnServiceStatusChanged;

        private ManagementEventWatcher ProcessWatcher;
        public void StartMonitoring()
        {
            if (ProcessWatcher == null)
            {
                var wmiQuery =
                    new WqlEventQuery(
                        @"SELECT * FROM __InstanceModificationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Service'" +
                        " and TargetInstance.Name = 'AutomaticReminder'"); //TODO get service name from configurtaion
                ProcessWatcher = new ManagementEventWatcher(wmiQuery);
                ProcessWatcher.EventArrived += ProcessWatcher_EventArrived;
            }
            ProcessWatcher.Start();
        }

        private void ProcessWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (OnServiceStatusChanged != null)
            {
                OnServiceStatusChanged();
            }
        }

        public void StopMonitoring()
        {
            if (ProcessWatcher == null) return; // Stop not needed!

            ProcessWatcher.Stop();
        }

    }
}