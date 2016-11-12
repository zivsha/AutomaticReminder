using System;
using System.Globalization;
using System.IO;

namespace AutomaticReminderCommon
{
    public class AutomaticReminderEvent
    {
        private const string SentA1WeekToGoReminder = "Sent a 1-week-to-go reminder";
        private const string SentA2DaytToGoReminder = "Sent a 2-day-to-go reminder";
        private const string SentA1DaytToGoReminder = "Sent a 1-day-to-go reminder";
        private const string SentATodayReminder = "Sent a today reminder";
        private const string SentAnSmsReminder = "Sent an SMS reminder";
        private bool _is2DayReminderSent;
        private bool _is1DayReminderSent;
        private bool _is1WeekReminderSent;

        private bool _isTodayReminderSent;
        private bool _isSmsReminderSent;
        private readonly string _originalLine;
        private const int NUMBER_OF_INFO_FIELDS = 2;
        private const int NUMBER_OF_REMINDERS = 5;
        
        public static AutomaticReminderEvent Parse(string line)
        {
            if (String.IsNullOrWhiteSpace(line))
            {
                return null;
            }
            string[] splittedLine = line.Split(';');
            if (splittedLine.Length < 2)
            {
                Logger.LogFormat("ERROR: Could not parse line {0} by ';'", line);
                return null;
            }
            var instance = new AutomaticReminderEvent(line);
            string date = splittedLine[0].Trim();
            string name = splittedLine[1].Trim();
            DateTime dueDate; //Format: Thursday, June 22, 2015	|| Thursday, September 3, 2015	
            if (!DateTime.TryParseExact(date, new[] { "dddd, MMMM dd, yyyy", "dddd, MMMM d, yyyy" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dueDate))
            {
                Logger.LogFormat("Error parsing date {0}", date);
                return null;
            }
            instance.DueDate = dueDate;
            instance.Name = name;
            for (int i = 2; i < (NUMBER_OF_INFO_FIELDS + NUMBER_OF_REMINDERS); i++)
            {
                if (splittedLine.Length < (i + 1))
                {
                    return instance;
                }
                if (!instance._is1WeekReminderSent)
                    instance._is1WeekReminderSent = splittedLine[i].Trim().Equals(SentA1WeekToGoReminder);
                if (!instance._is2DayReminderSent)
                    instance._is2DayReminderSent = splittedLine[i].Trim().Equals(SentA2DaytToGoReminder);
                if (!instance._isSmsReminderSent)
                    instance._isSmsReminderSent = splittedLine[i].Trim().Equals(SentAnSmsReminder);
                if (!instance._is1DayReminderSent)
                    instance._is1DayReminderSent = splittedLine[i].Trim().Equals(SentA1DaytToGoReminder);
                if (!instance._isTodayReminderSent)
                    instance._isTodayReminderSent = splittedLine[i].Trim().Equals(SentATodayReminder);
            }
            return instance;
        }
        public DateTime DueDate { get; private set; }
        public string Name { get; private set; }

        public bool Is2DayReminderSent
        {
            get { return _is2DayReminderSent; }
            set
            {
                if (!_is2DayReminderSent && value == true)
                {
                    var fileText = File.ReadAllText(CommonAutomaticReminder.DbPath);
                    var fileTextWithReplacedLine = fileText.Replace(_originalLine, String.Format("{0} ; {1}", _originalLine, SentA2DaytToGoReminder));
                    File.WriteAllText(CommonAutomaticReminder.DbPath, fileTextWithReplacedLine);
                }
                _is2DayReminderSent = value;
            }
        }

        public bool Is1DayReminderSent
        {
            get { return _is1DayReminderSent; }
            set
            {
                if (!_is1DayReminderSent && value == true)
                {
                    var fileText = File.ReadAllText(CommonAutomaticReminder.DbPath);
                    var fileTextWithReplacedLine = fileText.Replace(_originalLine, String.Format("{0} ; {1}", _originalLine, SentA1DaytToGoReminder));
                    File.WriteAllText(CommonAutomaticReminder.DbPath, fileTextWithReplacedLine);
                }
                _is1DayReminderSent = value;
            }
        }

        public bool IsTodayReminderSent
        {
            get { return _isTodayReminderSent; }
            set
            {
                if (!_isTodayReminderSent && value == true)
                {
                    var fileText = File.ReadAllText(CommonAutomaticReminder.DbPath);
                    var fileTextWithReplacedLine = fileText.Replace(_originalLine, String.Format("{0} ; {1}", _originalLine, SentATodayReminder));
                    File.WriteAllText(CommonAutomaticReminder.DbPath, fileTextWithReplacedLine);
                }
                _isTodayReminderSent = value;
            }
        }

        public bool IsSmsReminderSent
        {
            get { return _isSmsReminderSent; }
            set
            {
                if (!_isSmsReminderSent && value == true)
                {
                    var fileText = File.ReadAllText(CommonAutomaticReminder.DbPath);
                    var fileTextWithReplacedLine = fileText.Replace(_originalLine, String.Format("{0} ; {1}", _originalLine, SentAnSmsReminder));
                    File.WriteAllText(CommonAutomaticReminder.DbPath, fileTextWithReplacedLine);
                }
                _isSmsReminderSent = value;
            }
        }

        public bool Is1WeekReminderSent
        {
            get { return _is1WeekReminderSent; }
            set
            {
                if (!_is1WeekReminderSent && value == true)
                {
                    var fileText = File.ReadAllText(CommonAutomaticReminder.DbPath);
                    var fileTextWithReplacedLine = fileText.Replace(_originalLine, String.Format("{0} ; {1}", _originalLine, SentA1WeekToGoReminder));
                    File.WriteAllText(CommonAutomaticReminder.DbPath, fileTextWithReplacedLine);
                }
                _is1WeekReminderSent = value;
            }
        }

        private AutomaticReminderEvent(string line)
        {
            _originalLine = line;
            _is1WeekReminderSent = false;
            _is2DayReminderSent = false;
            _is1DayReminderSent = false;
            _isTodayReminderSent = false;
            _isSmsReminderSent = false;
        }

        public AutomaticReminderEvent()
        {
            _originalLine = String.Empty;
            Name = String.Empty;
            DueDate = new DateTime();
            _is1WeekReminderSent = false;
            _is2DayReminderSent = false;
            _is1DayReminderSent = false;
            _isTodayReminderSent = false;
            _isSmsReminderSent = false;
        }

        public void MarkAllAsSent()
        {
            IsTodayReminderSent = true;
            Is1DayReminderSent = true;
            Is2DayReminderSent = true;
            Is1WeekReminderSent = true;
            IsSmsReminderSent = true;
        }
    }
}