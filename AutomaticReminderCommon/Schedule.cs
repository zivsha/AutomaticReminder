using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomaticReminderCommon
{
    public delegate void ScheduleCallback();

    public class Schedule
    {
        public static readonly TimeSpan TimeSpanNoRepeat = TimeSpan.FromMilliseconds(Timeout.Infinite);

        private Timer _timer;
        private readonly ScheduleCallback _callback;
        private readonly DateTime _dutDate;
        private readonly TimeSpan _repeatTimeSpan;

        public Schedule(TimeOfDay dueTime, ScheduleCallback callback, bool isRepeated)
            : this(dueTime.AsTodayDateTime(), callback, isRepeated, TimeSpan.FromHours(24))
        {
        }


        public Schedule(DateTime dueDateTime, ScheduleCallback callback, bool isRepeated, TimeSpan repeatTimeSpan)
        {
            _dutDate = DateTime.Now <= dueDateTime ? dueDateTime : dueDateTime.Add(repeatTimeSpan);
            _callback = callback;
            _repeatTimeSpan = isRepeated ? TimeSpanNoRepeat : repeatTimeSpan;
        }

        public static Schedule OneTimeSchedule(DateTime dutDate, ScheduleCallback callback)
        {
            return new Schedule(dutDate, callback, false, TimeSpanNoRepeat);
        }

        public static Schedule OneTimeSchedule(TimeOfDay dutTime, ScheduleCallback callback)
        {
            return new Schedule(dutTime, callback, false);
        }

        public void Start()
        {
            if (DateTimeHelper.IsDateInPast(_dutDate))
                throw new ArgumentException("Due date is in the past");

            _timer = new Timer(state => _callback(), null, _dutDate - DateTime.Now, _repeatTimeSpan);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }


    }
}
