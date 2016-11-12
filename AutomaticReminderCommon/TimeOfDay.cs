using System;

namespace AutomaticReminderCommon
{
    public class TimeOfDay
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }


        public TimeOfDay(int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {

            var datetime =
                DateTime.Today.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).AddMilliseconds(Milliseconds);
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
        }

        public string ToLongTimeString()
        {
            var dateTime = new DateTime(1, 1, 1, Hours, Minutes, Seconds, Milliseconds);
            return dateTime.ToLongTimeString();
        }

        public string ToShortTimeString()
        {
            var dateTime = new DateTime(1, 1, 1, Hours, Minutes, Seconds, Milliseconds);
            return dateTime.ToShortTimeString();
        }

        public override string ToString()
        {
            var dateTime = new DateTime(1, 1, 1, Hours, Minutes, Seconds, Milliseconds);
            return dateTime.ToString("HH:mm:ss,fffff");
        }

        public DateTime AsTodayDateTime()
        {
            return DateTime.Today.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds).AddMilliseconds(Milliseconds);
        }
    }
}