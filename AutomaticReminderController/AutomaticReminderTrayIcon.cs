using System;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using AutomaticReminderController.Properties;
using AutomaticReminderCommon;
using TimeoutException = System.TimeoutException;
using Timer = System.Timers.Timer;

namespace AutomaticReminderController
{
    public class AutomaticReminderTrayIcon
    {
        private readonly NotifyIcon _trayIcon;
        private ServiceControllerStatus _prevStatus;
        private readonly Timer _updateTrayIconTimer;
        public event MouseEventHandler OnMouseClick;
        private readonly Action _closeHandle;
        private readonly Action _showHandle;
        private Thread _pipeCommTask;
        //private Schedule _schedule;

        public AutomaticReminderTrayIcon(Action showHandle, Action closeHandle)
        {
            if (showHandle == null || closeHandle == null)
            {
                return;
            }
            _closeHandle = closeHandle;
            _showHandle = showHandle;
            _trayIcon = new NotifyIcon
            {
                Visible = true,
                Text = "Automatic Reminder",
                //Icon = Resources.AutomaticReminderIcon
            };
            var menu = new ContextMenuStrip();
            _trayIcon.MouseClick += delegate(object sender, MouseEventArgs args)
            {
                if (OnMouseClick != null) OnMouseClick(sender, args);
            };

            var itemSendFakeReminder= new ToolStripMenuItem { Text = @"Send Fake Reminder", Image = Resources.FakeAlert };
            itemSendFakeReminder.Click +=
                (sender, args) =>
                    Task.Factory.StartNew(
                        () =>
                            SendMessageViaPipe(CommonAutomaticReminder.Client2ServerNamedPipeName,
                                CommonAutomaticReminder.TestReminderRemoteCommand)
                               );
                            
            menu.Items.Add(itemSendFakeReminder); 
            
            var itemStartReminder = new ToolStripMenuItem { Text = @"Trigger Reminder", Image = Resources.Alert };
            itemStartReminder.Click +=
                (sender, args) =>
                    Task.Factory.StartNew(
                        () =>
                            SendMessageViaPipe(CommonAutomaticReminder.Client2ServerNamedPipeName,
                                CommonAutomaticReminder.StartReminderRemoteCommand));
            menu.Items.Add(itemStartReminder);

            var itemStop = new ToolStripMenuItem { Text = @"Stop Service",Image = Resources.Stop};
            itemStop.Click += (sender, args) => ServiceAuxiliary.StopService(CommonAutomaticReminder.ServiceName, 1000);
            menu.Items.Add(itemStop);

            var itemStart = new ToolStripMenuItem { Text = @"Start Service", Image = Resources.Start};
            itemStart.Click += (sender, args) => ServiceAuxiliary.StartService(CommonAutomaticReminder.ServiceName, 1000);
            menu.Items.Add(itemStart);

            var itemClearNotifications = new ToolStripMenuItem { Text = @"Clear Notifications", Image = Resources.Clear };
            itemClearNotifications.Click += (sender, args) => BalloonTipManager.ClearNotifications();
            menu.Items.Add(itemClearNotifications);

            var itemExit = new ToolStripMenuItem { Text = @"Exit", Image = Resources.Exit };
            itemExit.Click += (sender, args) =>
            {
                _closeHandle();
            };
            menu.Items.Add(itemExit);

            _trayIcon.ContextMenuStrip = menu;
            UpdateContextMenuItems();
            _updateTrayIconTimer = new Timer();
            _updateTrayIconTimer.Elapsed += UpdateTrayIconTimerOnElapsed;
            _updateTrayIconTimer.Interval = 500;
            _updateTrayIconTimer.Start();

            _pipeCommTask = new Thread(PipelineReaderProc) { IsBackground = true };
            _pipeCommTask.Start();
        }

        private static void SendMessageViaPipe(string pipeName, string message)
        {
            using (var client = new NamedPipeClientStream(pipeName))
            {
                try
                {
                    client.Connect(5000);
                }
                catch (TimeoutException e)
                {
                    Logger.LogFormat("Timeout while trying to connect to service pipe {1} ({0})", e.Message);
                }
                catch (IOException e)
                {
                    Logger.LogFormat("IO Exception while trying to connect to service pipe {1} ({0})", e.Message);
                }

                catch (Exception e)
                {
                    Logger.LogFormat("Exception while trying to connect to service pipe {1} ({0})", e.Message);
                }
                var writer = new StreamWriter(client);
                writer.WriteLine(message);
                writer.Flush();
                client.WaitForPipeDrain();
            }
        }

        private void PipelineReaderProc()
        {
            Logger.LogFormat("Started Pipeline Reader Thread");
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(CommonAutomaticReminder.Server2ClientNamedPipeName))
                    {
                        Logger.LogFormat("server started waiting for connection");
                        server.WaitForConnection();
                        Logger.LogFormat("server finished waiting for connection");
                        var reader = new StreamReader(server);
                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (line != null && line.StartsWith(CommonAutomaticReminder.AutomaticReminderFinished))
                            {
                                ShowBallonTip(line);
                                Logger.LogFormat("Received pipe message: {0}", line);
                                server.Disconnect();
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogFormat("PipelineReaderProc error: {0}", e.Message);
                }

            }
        }

        private void ShowBallonTip(string line)
        {
            _trayIcon.ContextMenuStrip.BeginInvoke(
                (Action)
                    (() =>
                        BalloonTipManager.CreateBaloonTip(SystemIcons.Information, "Automatic Reminder", line,
                            ToolTipIcon.Info, 30, null)));
        }

        private void UpdateTrayIconTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            UpdateContextMenuItems();
        }

        private void UpdateContextMenuItems()
        {
            try
            {
                var sc = new ServiceController(CommonAutomaticReminder.ServiceName);
                switch (sc.Status)
                {
                    case ServiceControllerStatus.ContinuePending:
                    case ServiceControllerStatus.Running:
                    case ServiceControllerStatus.StartPending:
                        _trayIcon.ContextMenuStrip.Items[1].Enabled = true;
                        _trayIcon.ContextMenuStrip.Items[2].Enabled = false;
                        break;
                    case ServiceControllerStatus.StopPending:
                    case ServiceControllerStatus.PausePending:
                    case ServiceControllerStatus.Paused:
                    case ServiceControllerStatus.Stopped:
                        _trayIcon.ContextMenuStrip.Items[1].Enabled = false;
                        _trayIcon.ContextMenuStrip.Items[2].Enabled = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _prevStatus = sc.Status;
            }
            catch
            {
            }

        }
        public void Dispose()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Dispose();
            }

            if (_updateTrayIconTimer != null)
            {
                _updateTrayIconTimer.Stop();
            }
            BalloonTipManager.ClearNotifications();
        }
    }
}
