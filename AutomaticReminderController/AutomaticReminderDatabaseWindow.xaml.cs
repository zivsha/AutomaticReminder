using System.ComponentModel;
using System.Windows;
using AutomaticReminderCommon;

namespace AutomaticReminderController
{
    /// <summary>
    /// Interaction logic for AutomaticReminderDatabaseWindow.xaml
    /// </summary>
    public partial class AutomaticReminderDatabaseWindow : Window
    {
        public AutomaticReminderDatabaseWindow()
        {
            InitializeComponent();
            AutomaticReminderDataBase.Instance.PropertyChanged += OnPropertyChanged;

            DataContext = AutomaticReminderDataBase.Instance;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            MessageBox.Show("Content changed: " + propertyChangedEventArgs.PropertyName);
        }
    }
}
