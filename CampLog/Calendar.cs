using System;
using System.Collections.Generic;
using System.Windows;

namespace CampLog {
    public static class CalendarSpecs {
        public static readonly SortedDictionary<string, CalendarFactory> specs = new SortedDictionary<string, CalendarFactory>() {
            ["Campaign Date"] = new CampaignDateCalendarFactory(),
            ["None"] = new CalendarFactory(),
        };
    }


    [Serializable]
    public class Calendar {
        public virtual decimal default_timestamp { get => 0; }

        public virtual string format_timestamp(decimal timestamp, bool verbose = false) => string.Format("{0}", timestamp);
        public virtual string format_interval(decimal interval, bool verbose = false) => string.Format("{0}", interval);

        public virtual FrameworkElement timestamp_control() => new SimpleCalendarControl();
        public virtual FrameworkElement interval_control() => new SimpleCalendarControl();
    }


    public class CalendarFactory {
        public virtual CalendarParameters default_parameters() => null;
        public virtual Calendar get_calendar(CalendarParameters parameters) => new Calendar();
    }


    public abstract class CalendarParameters {
        public abstract bool configure_prompt();
    }
}