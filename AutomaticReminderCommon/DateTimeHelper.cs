using System;

namespace AutomaticReminderCommon
{
    public class DateTimeHelper
    {
        public static bool IsWorkingHours()
        {
            return IsTimeNowPast8Am() && IsTimeNowBefore(18);
        }

        public static bool IsTimeNowBefore5Pm()
        {
            var exactly5PmmToday = DateTime.Today.AddHours(17);
            //(17:00:00 - 16:59:59).TotalMilliseconds = 1000 > 0
            //(17:00:00 - 17:00:01).TotalMilliseconds = -1000 < 0
            return exactly5PmmToday.Subtract(DateTime.Now).TotalMilliseconds > 0;
        }

        public static bool IsTimeNowBefore(int hour24)
        {
            var timeOclock = DateTime.Today.AddHours(hour24);
            //(17:00:00 - 16:59:59).TotalMilliseconds = 1000 > 0
            //(17:00:00 - 17:00:01).TotalMilliseconds = -1000 < 0
            return timeOclock.Subtract(DateTime.Now).TotalMilliseconds > 0;
        }

        public static bool IsTimeNowPast8Am()
        {
            //(08:00:01 - 08:00:00).TotalMilliseconds = 1000 > 0
            //(07:59:59 - 08:00:00).TotalMilliseconds = -1000 < 0
            var exactly8AmToday = DateTime.Today.AddHours(8);
            return DateTime.Now.Subtract(exactly8AmToday).TotalMilliseconds > 0;
        }

        public static bool IsDateInPast(DateTime dueDate)
        {
            return DateTime.Now.Subtract(dueDate).TotalDays > 1;
        }

        public static bool IsDueDateToday(DateTime day)
        {
            return DateTime.Now.Subtract(day).TotalDays < 1 && DateTime.Now.Subtract(day).TotalDays > 0;
        }

        public static bool IsDueDateIn2Days(DateTime day)
        {
            return day.Subtract(DateTime.Now).TotalDays < 2 && day.Subtract(DateTime.Now).TotalDays > 0;
        }

        public static bool IsDueDateTommorow(DateTime day)
        {
            return day.Subtract(DateTime.Now).TotalDays < 1 && day.Subtract(DateTime.Now).TotalDays >= 0;
        }

        public static bool IsTimeNowAfter(int hour24)
        {
            //(08:00:01 - 08:00:00).TotalMilliseconds = 1000 > 0
            //(07:59:59 - 08:00:00).TotalMilliseconds = -1000 < 0
            var timeOclock = DateTime.Today.AddHours(hour24);
            return DateTime.Now.Subtract(timeOclock).TotalMilliseconds > 0;
        }

        public static bool IsDueDateIn1Week(DateTime dueDate)
        {
            return dueDate.Subtract(DateTime.Now).TotalDays < 7 && dueDate.Subtract(DateTime.Now).TotalDays > 0;
        }
    }
}