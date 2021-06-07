using System;
using System.Windows;

namespace CampLog {
    [Serializable]
    class CampaignDateCalendar : Calendar {
        public const long DAY_LENGTH = 24 * 60 * 60;

        public override decimal default_timestamp { get => CampaignDateCalendar.DAY_LENGTH; }

        public override string format_timestamp(decimal timestamp, bool verbose = false) {
            long days = (long)(timestamp / CampaignDateCalendar.DAY_LENGTH);
            timestamp %= CampaignDateCalendar.DAY_LENGTH;
            string pattern = (verbose ? "hh:mm:ss tt" : "hh:mm tt");
            string time = new DateTime(((long)timestamp) * TimeSpan.TicksPerSecond).ToString(pattern);
            string format = (verbose ? "Day {0}, {1}" : "D{0} {1}");
            return string.Format(format, days, time);
        }

        public override string format_interval(decimal timestamp, bool verbose = false) {
            long days = (long)(timestamp / CampaignDateCalendar.DAY_LENGTH);
            timestamp %= CampaignDateCalendar.DAY_LENGTH;
            string pattern = (verbose ? "HH:mm:ss" : "HH:mm");
            string time = new DateTime(((long)timestamp) * TimeSpan.TicksPerSecond).ToString(pattern);
            string format = (verbose ? "{0} days, {1}" : "{0}d {1}");
            return string.Format(format, days, time);
        }

        public override FrameworkElement timestamp_control() {
            CampaignDateCalendarControl result = new CampaignDateCalendarControl();
            result.setTimestampMode();
            return result;
        }

        public override FrameworkElement interval_control() {
            CampaignDateCalendarControl result = new CampaignDateCalendarControl();
            result.setIntervalMode();
            return result;
        }
    }


    public class CampaignDateCalendarFactory : CalendarFactory {
        public override CalendarParameters default_parameters() => null;
        public override Calendar get_calendar(CalendarParameters parameters) => new CampaignDateCalendar();
    }
}
