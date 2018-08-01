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
        private Task _pipeCommTask;
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken pipelineToken;
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
                Text = "Controller for " + UserConfiguration.ServiceDisplayName,
                Icon = Resources.AutomaticReminderIcon //Icon made by Maxim Basinski (https://www.flaticon.com/authors/maxim-basinski) from "Flaticon" (https://www.flaticon.com) is licensed by Creative Commons BY 3.0 http://creativecommons.org/licenses/by/3.0/
            };
            var menu = new ContextMenuStrip();
            _trayIcon.MouseClick += delegate(object sender, MouseEventArgs args)
            {
                if (OnMouseClick != null) OnMouseClick(sender, args);
            };

            var itemSendFakeReminder= new ToolStripMenuItem { Text = @"Send Fake Reminder", Image = Resources.FakeAlert };
            itemSendFakeReminder.ToolTipText = "Instruct service to test send a reminder";
            itemSendFakeReminder.Click +=
                (sender, args) =>
                    Task.Factory.StartNew(
                        () =>
                            SendMessageViaPipe(CommonAutomaticReminder.Client2ServerNamedPipeName,
                                CommonAutomaticReminder.TestReminderRemoteCommand)
                               );
                            
            menu.Items.Add(itemSendFakeReminder); 
            
            var itemStartReminder = new ToolStripMenuItem { Text = @"Trigger Reminder", Image = Resources.Alert };
            itemStartReminder.ToolTipText = "Instruct service to check for new reminders";
            itemStartReminder.Click +=
                (sender, args) =>
                    Task.Factory.StartNew(
                        () =>
                            SendMessageViaPipe(CommonAutomaticReminder.Client2ServerNamedPipeName,
                                CommonAutomaticReminder.StartReminderRemoteCommand));
            menu.Items.Add(itemStartReminder);

            var itemStop = new ToolStripMenuItem { Text = @"Stop Service",Image = Resources.Stop};
            itemStop.Enabled = false;
            itemStop.ToolTipText = $"Stop \"{UserConfiguration.ServiceName}\" (cannot reach service)";
            itemStop.Click += (sender, args) => ServiceAuxiliary.StopService(CommonAutomaticReminder.ServiceName, 1000);
            menu.Items.Add(itemStop);

            var itemStart = new ToolStripMenuItem { Text = @"Start Service", Image = Resources.Start};
            itemStart.Enabled = false;
            itemStart.ToolTipText = $"Start \"{UserConfiguration.ServiceName}\" (cannot reach service)";
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

            pipelineToken = source.Token;
            _pipeCommTask = Task.Run(()=> PipelineReaderProc());
        }

        private static void SendMessageViaPipe(string pipeName, string message)
        {
            using (var client = new NamedPipeClientStream(pipeName))
            {
                try
                {
                    client.Connect(5000);
                    var writer = new StreamWriter(client);
                    writer.WriteLine(message);
                    writer.Flush();
                    client.WaitForPipeDrain();
                }
                catch (TimeoutException e)
                {
                    Logger.LogFormat($"Timeout while trying to connect to service pipe {pipeName} ({e.Message})");
                }
                catch (IOException e)
                {
                    Logger.LogFormat($"IO Exception while trying to connect to service pipe {pipeName} ({e.Message})");
                }

                catch (Exception e)
                {
                    Logger.LogFormat($"Exception while trying to connect to service pipe {pipeName} ({e.Message})");
                }
            }
        }

        private void PipelineReaderProc()
        {
            Logger.LogFormat("Started Pipeline Reader Thread");
            while (!pipelineToken.IsCancellationRequested)
            {
                try
                {
                    using (var stream = new NamedPipeServerStream(CommonAutomaticReminder.Server2ClientNamedPipeName, 
                                                                  PipeDirection.InOut, 
                                                                  1,
                                                                  PipeTransmissionMode.Message,
                                                                  PipeOptions.Asynchronous))
                    {
                        Logger.LogFormat("Stream started waiting for connection");
                        var asyncResult = stream.BeginWaitForConnection(null, null);
                        if (asyncResult.AsyncWaitHandle.WaitOne(5000))
                        {
                            stream.EndWaitForConnection(asyncResult);
                            Logger.LogFormat("Stream finished waiting for connection");
                            using (var reader = new StreamReader(stream))
                            {
                                while (!pipelineToken.IsCancellationRequested)
                                {
                                    var lineReadTask = reader.ReadLineAsync();
                                    lineReadTask.Wait(pipelineToken);
                                    if (lineReadTask.IsCompleted)
                                    {
                                        var line = lineReadTask.Result;
                                        if (line != null && line.StartsWith(CommonAutomaticReminder.AutomaticReminderFinished))
                                        {
                                            ShowBallonTip(line);
                                            Logger.LogFormat("Received pipe message: {0}", line);
                                            stream.Disconnect();
                                            break;
                                        }
                                    }
                                }
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
                        BalloonTipManager.CreateBaloonTipInfo(line, "Automatic Reminder", 30, null)));
        }

        private void UpdateTrayIconTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            UpdateContextMenuItems();
        }

        private void UpdateContextMenuItems()
        {
            try
            {
                using (var sc = new ServiceController(CommonAutomaticReminder.ServiceName))
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.ContinuePending:
                        case ServiceControllerStatus.Running:
                        case ServiceControllerStatus.StartPending:
                            _trayIcon.ContextMenuStrip.Items[2].Enabled = true;
                            _trayIcon.ContextMenuStrip.Items[3].Enabled = false;
                            _trayIcon.ContextMenuStrip.Items[2].ToolTipText = $"Stop \"{UserConfiguration.ServiceName}\"";
                            _trayIcon.ContextMenuStrip.Items[3].ToolTipText = $"Start \"{UserConfiguration.ServiceName}\" (Already running)";
                            break;
                        case ServiceControllerStatus.StopPending:
                        case ServiceControllerStatus.PausePending:
                        case ServiceControllerStatus.Paused:
                        case ServiceControllerStatus.Stopped:
                            _trayIcon.ContextMenuStrip.Items[2].Enabled = false;
                            _trayIcon.ContextMenuStrip.Items[3].Enabled = true;
                            _trayIcon.ContextMenuStrip.Items[2].ToolTipText = $"Stop \"{UserConfiguration.ServiceName}\" (Already stopped)";
                            _trayIcon.ContextMenuStrip.Items[3].ToolTipText = $"Start \"{UserConfiguration.ServiceName}\"";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    _prevStatus = sc.Status;
                }
            }
            catch(Exception ex)
            {
                _trayIcon.ContextMenuStrip.Items[2].ToolTipText = $"Stop \"{UserConfiguration.ServiceName}\" ({ex.Message})";
                _trayIcon.ContextMenuStrip.Items[3].ToolTipText = $"Start \"{UserConfiguration.ServiceName}\" ({ex.Message})";
            }

        }
        public void Dispose()
        {
            if (_updateTrayIconTimer != null)
            {
                _updateTrayIconTimer.Stop();
            }

            source.Cancel();
            source.Dispose();

            if (_trayIcon != null)
            {
                _trayIcon.Dispose();
            }

            BalloonTipManager.ClearNotifications();
        }
    }
}
