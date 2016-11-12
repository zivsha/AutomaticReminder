using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AutomaticReminderCommon
{
    public static class BalloonTipManager
    {
        static readonly List<NotifyIcon> m_ballonTips;
        private const uint MaxTrayIcons = 10;
        static BalloonTipManager()
        {
            m_ballonTips = new List<NotifyIcon>();
        }

        public static void ClearNotifications()
        {
            m_ballonTips.ForEach(ni =>
            {
                ni.Visible = false;
                ni.Dispose();
            });
            m_ballonTips.Clear();
        }
        public static void CreateBaloonTip(Icon trayIcon, string caption, string message, ToolTipIcon baloonTipIcon, int seconds, Action onBaloonTipClickedHandle)
        {
            var notifyIcon = new NotifyIcon
            {
                Icon = trayIcon,
                BalloonTipTitle = caption,
                BalloonTipText = message == "" ? "<EOM>" : message,
                BalloonTipIcon = baloonTipIcon,
                Visible = true,
                Text = caption
            };
            var cm = new ContextMenuStrip();
            var item = new ToolStripMenuItem {Text = "Remove Notification"};
            item.Click += delegate { CloseNotifyIcon(ref notifyIcon); };
            cm.Items.Add(item);
            notifyIcon.ContextMenuStrip = cm;
            notifyIcon.MouseClick += NotifyIconOnMouseClick;
            notifyIcon.BalloonTipClicked += (sender, args) =>
            {
                var nIcon = sender as NotifyIcon;
                if (nIcon == null || onBaloonTipClickedHandle == null)
                {
                    return;
                }
                onBaloonTipClickedHandle();
            };
            if (m_ballonTips.Count >= MaxTrayIcons)
            {
                var oldestNotifyIcon = m_ballonTips.First();
                CloseNotifyIcon(ref oldestNotifyIcon);
            }
            m_ballonTips.Add(notifyIcon);
            notifyIcon.ShowBalloonTip(seconds * 1000);
        }
        private static void NotifyIconOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.Button == MouseButtons.Left)
            {
                Logger.LogFormat("Click. Handled by process: {0}", Process.GetCurrentProcess().ProcessName);

                var notifyIcon = sender as NotifyIcon;
                if (notifyIcon != null)
                {
                    Logger.LogFormat("Notify Icon showing balloon: {0}", notifyIcon.BalloonTipText);
                    notifyIcon.ShowBalloonTip(5 * 1000);
                }

            }
        }
        private static void CloseNotifyIcon(ref NotifyIcon notifyIcon)
        {
            notifyIcon.Visible = false;
            m_ballonTips.Remove(notifyIcon);
            notifyIcon.Dispose();
        }
    }
}