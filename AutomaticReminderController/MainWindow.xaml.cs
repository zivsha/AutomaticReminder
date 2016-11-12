using System;
using System.Windows;
using System.Windows.Forms;
using AutomaticReminderCommon;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace AutomaticReminderController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AutomaticReminderTrayIcon _trayIcon;
        private AutomaticReminderController.AutomaticReminderDatabaseWindow _dbWindow;

        public MainWindow()
        {
            InitializeComponent();
            _trayIcon = new AutomaticReminderTrayIcon(Show, Close);
            _trayIcon.OnMouseClick += TrayIconOnOnMouseClick;

            _dbWindow = new AutomaticReminderController.AutomaticReminderDatabaseWindow();
            Hide();
        }

        private void TrayIconOnOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            AutomaticReminderDataBase.Instance.Reload();
            if (mouseEventArgs.Button == MouseButtons.Left)
            {
                if (_dbWindow != null && _dbWindow.IsLoaded)
                {

                    if (_dbWindow.IsVisible)
                    {
                        if (_dbWindow.WindowState == WindowState.Minimized)
                        {
                            _dbWindow.WindowState = WindowState.Normal;
                        }
                        _dbWindow.Activate();
                        _dbWindow.Topmost = true;
                        _dbWindow.Topmost = false;
                        _dbWindow.Focus();
                    }
                    else
                    {
                        _dbWindow.Show();                        
                    }
                }
                else
                {
                    _dbWindow = new AutomaticReminderController.AutomaticReminderDatabaseWindow();
                    _dbWindow.Show();
                }

            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _trayIcon.Dispose();
        }
    }
}
