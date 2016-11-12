using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
using System.Runtime.CompilerServices;

namespace AutomaticReminderCommon
{
    //TODO split contact information and events
    public class AutomaticReminderDataBase : INotifyPropertyChanged
    {
        public static AutomaticReminderDataBase Instance = new AutomaticReminderDataBase();

        public event PropertyChangedEventHandler PropertyChanged;
        private const string EndOfContactsLine = "----------END OF CONTACTS, DO NOT CHANGE THIS LINE---------";

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Dictionary<string, Contact> Contacts;
        public List<Contact> ContactsList { get; private set; }
        public List<AutomaticReminderEvent> AutomaticReminderEvents { get; private set; }

        private AutomaticReminderDataBase()
        {
            Reload();
        }

        private void GetEvents()
        {
            if (!File.Exists(CommonAutomaticReminder.DbPath))
            {
                Logger.LogFormat("ERROR: File {0} was not found", CommonAutomaticReminder.DbPath);
                return;
            }
            var fileLines = File.ReadAllLines(CommonAutomaticReminder.DbPath);

            foreach (var line in fileLines)
            {
                if (line.StartsWith("#") || String.IsNullOrWhiteSpace(line) ||
                    line.Equals(EndOfContactsLine))
                {
                    continue;
                }
                AutomaticReminderEvent automaticReminderEvent = AutomaticReminderEvent.Parse(line);
                if (automaticReminderEvent == null)
                {
                    Logger.LogFormat("Entry was not parsed correctly and is null. line = {0}", line);
                    continue;
                }
                AutomaticReminderEvents.Add(automaticReminderEvent);
            }
        }


        private void GetContacts()
        {
            if (!File.Exists(CommonAutomaticReminder.DbPath))
            {
                Logger.LogFormat("ERROR: File {0} was not found", CommonAutomaticReminder.DbPath);
                return;
            }
            var fileLines = File.ReadAllLines(CommonAutomaticReminder.DbPath);
            foreach (var line in fileLines)
            {
                if (line.Equals(EndOfContactsLine))
                {
                    break;
                }
                if (line.StartsWith("#"))
                {
                    string[] pair = line.Split(';');
                    if (pair.Length < 1)
                    {
                        continue;
                    }
                    string[] fullName = pair[0].Replace("#", "").Trim().Split(' ');
                    if (fullName.Length < 2)
                    {
                        continue;
                    }
                    string lastName = fullName[0];
                    string firstName = fullName[1];
                    string email = pair[1].Trim();
                    string phoneNumber = String.Empty;
                    if (pair.Length > 2)
                    {
                        phoneNumber = pair[2].Trim();                        
                    }
                    var contact = new Contact(firstName, lastName, email, phoneNumber);
                    ContactsList.Add(contact);
                    Contacts.Add(lastName + " " + firstName, contact);
                }
            }
        }

        public void Reload()
        {
            Logger.LogFormat("Reloading Database");
            Initialize();
            GetContacts();
            GetEvents();
        }

        private void Initialize()
        {
            Contacts = new Dictionary<string, Contact>();
            ContactsList = new List<Contact>();
            AutomaticReminderEvents = new List<AutomaticReminderEvent>();
        }
    }
}