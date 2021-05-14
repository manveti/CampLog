using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public partial class CalendarEventWindow : Window {
        public bool valid;
        private CampaignState state;
        private Guid guid;
        private List<EntryAction> actions;
        private CalendarEvent evt;
        private ICalendarControl timestamp_box;
        private ICalendarControl interval_box;

        public CalendarEventWindow(CampaignState state, Calendar calendar, decimal now, Guid? event_guid = null, Guid? guid = null) {
            this.valid = false;
            this.state = state.copy();
            this.actions = new List<EntryAction>();
            if (guid is null) {
                if (event_guid is null) { throw new ArgumentNullException(nameof(event_guid)); }
                this.guid = Guid.NewGuid();
                ActionCalendarEventCreate add_action = new ActionCalendarEventCreate(this.guid, new CalendarEvent(event_guid.Value, now, ""));
                this.actions.Add(add_action);
                this.evt = add_action.evt;
            }
            else {
                this.guid = guid.Value;
                this.evt = this.state.events.events[this.guid];
            }
            InitializeComponent();
            FrameworkElement timestamp_box = calendar.timestamp_control();
            Grid.SetRow(timestamp_box, 0);
            Grid.SetColumn(timestamp_box, 1);
            this.main_grid.Children.Add(timestamp_box);
            this.timestamp_box = timestamp_box as ICalendarControl;
            if (this.timestamp_box is null) { throw new InvalidOperationException(); }
            this.timestamp_box.calendar_value = this.evt.timestamp;
            this.repeat_box.IsChecked = (this.evt.interval is not null);
            FrameworkElement interval_box = calendar.interval_control();
            Grid.SetRow(interval_box, 0);
            Grid.SetColumn(interval_box, 4);
            this.main_grid.Children.Add(interval_box);
            this.interval_box = interval_box as ICalendarControl;
            if (this.interval_box is null) { throw new InvalidOperationException(); }
            this.interval_box.calendar_value = (this.evt.interval ?? 0);
            this.name_box.Text = this.evt.name;
            this.description_box.Text = this.evt.description;
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            decimal timestamp = this.timestamp_box.calendar_value;
            string name = this.name_box.Text, description = this.description_box.Text;
            decimal? interval = (this.repeat_box.IsChecked ?? false ? this.interval_box.calendar_value : null);
            if ((timestamp != this.evt.timestamp) || (name != this.evt.name) || (description != this.evt.description) || (interval != this.evt.interval)) {
                bool set_timestamp = (timestamp != this.evt.timestamp), set_name = (name != this.evt.name),
                    set_description = (description != this.evt.description), set_interval = (interval != this.evt.interval);
                if (!set_name) { name = ""; }
                if (!set_description) { description = null; }
                if (!set_interval) { interval = null; }
                this.actions.Add(
                    new ActionCalendarEventUpdate(
                        this.guid,
                        this.evt,
                        new CalendarEvent(Guid.NewGuid(), timestamp, name, description, interval),
                        set_timestamp,
                        set_name,
                        set_description,
                        set_interval
                    )
                );
            }
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public List<EntryAction> get_actions() {
            if (!this.valid) { return null; }
            return this.actions;
        }
    }
}
