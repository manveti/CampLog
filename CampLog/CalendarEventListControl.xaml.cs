using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public class CalendarEventRow {
        public Guid guid;
        public decimal raw_timestamp;
        public string _timestamp;
        public string _name;

        public CalendarEventRow self { get => this; }
        public string timestamp { get => this._timestamp; }
        public string name { get => this._name; }

        public CalendarEventRow(Guid guid, decimal raw_timestamp, string timestamp, string name) {
            this.guid = guid;
            this.raw_timestamp = raw_timestamp;
            this._timestamp = timestamp;
            this._name = name;
        }
    }


    public partial class CalendarEventListControl : UserControl {
        private ActionCallback change_callback;
        private Calendar calendar;
        private Guid? entry_guid;
        private CampaignState state;
        private decimal now;
        private List<CalendarEventRow> event_rows;

        public bool show_past {
            get => this.show_past_box.IsChecked ?? false;
            set => this.show_past_box.IsChecked = value;
        }

        private void fix_listview_column_widths(ListView list_view) {
            GridView grid_view = list_view.View as GridView;
            if (grid_view is null) { return; }
            foreach (GridViewColumn col in grid_view.Columns) {
                col.Width = col.ActualWidth;
                col.Width = double.NaN;
            }
        }

        public CalendarEventListControl(ActionCallback change_callback, Guid? entry_guid = null) {
            this.change_callback = change_callback;
            this.calendar = new Calendar();
            this.entry_guid = entry_guid;
            this.state = null;
            this.now = 0;
            this.event_rows = new List<CalendarEventRow>();
            InitializeComponent();
            this.event_list.ItemsSource = this.event_rows;
        }

        private void populate_event_rows() {
            this.event_rows.Clear();
            if (this.state is null) {
                this.event_list.Items.Refresh();
                return;
            }
            foreach (Guid guid in this.state.events.active_events) {
                CalendarEvent evt = this.state.events.events[guid];
                if ((evt.timestamp < this.now) && (evt.interval is null) && (!this.show_past)) { continue; }
                if (evt.interval is null) {
                    this.event_rows.Add(new CalendarEventRow(guid, evt.timestamp, this.calendar.format_timestamp(evt.timestamp), evt.name));
                    continue;
                }
                decimal timestamp = evt.timestamp;
                while ((timestamp < this.now) && (!this.show_past)) { timestamp += evt.interval.Value; }
                // add all instances of repeating event before now and two instances at or after now
                for (int i = 2; i > 0; timestamp += evt.interval.Value) {
                    if (timestamp >= this.now) { i -= 1; }
                    this.event_rows.Add(new CalendarEventRow(guid, timestamp, this.calendar.format_timestamp(timestamp), evt.name));
                }
            }
            this.event_rows.Sort((x, y) => x.raw_timestamp.CompareTo(y.raw_timestamp));
            this.event_list.Items.Refresh();
            this.fix_listview_column_widths(this.event_list);
        }

        public void set_calendar(Calendar calendar) {
            this.calendar = calendar;
            foreach (CalendarEventRow row in this.event_rows) {
                row._timestamp = calendar.format_timestamp(row.raw_timestamp);
            }
            this.event_list.Items.Refresh();
            this.fix_listview_column_widths(this.event_list);
        }

        public void set_state(CampaignState state, decimal timestamp) {
            this.state = state;
            this.now = timestamp;
            this.populate_event_rows();
            if (this.state is null) {
                this.add_but.IsEnabled = false;
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
            }
            else {
                this.add_but.IsEnabled = true;
            }
        }

        private void show_past_changed(object sender, RoutedEventArgs e) {
            this.populate_event_rows();
            CalendarEventRow next_row = this.event_rows[^1];
            foreach (CalendarEventRow row in this.event_rows) {
                if (row.raw_timestamp >= this.now) {
                    next_row = row;
                    break;
                }
            }
            this.event_list.ScrollIntoView(next_row);
        }

        private void list_sel_changed(object sender, RoutedEventArgs e) {
            CalendarEventRow sel = this.event_list.SelectedValue as CalendarEventRow;
            if ((sel is null) || (!this.state.events.active_events.Contains(sel.guid))) {
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
                return;
            }
            this.rem_but.IsEnabled = true;
            this.view_but.IsEnabled = true;
        }

        private void do_add(object sender, RoutedEventArgs e) {
            if (this.state is null) { return; }
            Guid ent_guid = this.entry_guid ?? Guid.NewGuid();
            CalendarEventWindow dialog_window = new CalendarEventWindow(this.state, this.calendar, this.now, ent_guid) { Owner = Window.GetWindow(this) };
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            List<EntryAction> actions = dialog_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions, ent_guid);
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            if ((this.change_callback is null) || (this.state is null)) { return; }
            CalendarEventRow sel = this.event_list.SelectedValue as CalendarEventRow;
            if ((sel is null) || (!this.state.events.events.ContainsKey(sel.guid))) { return; }
            List<EntryAction> actions = new List<EntryAction>() { new ActionCalendarEventRemove(sel.guid) };
            this.change_callback(actions);
        }

        private void do_view(object sender, RoutedEventArgs e) {
            if (this.state is null) { return; }
            CalendarEventRow sel = this.event_list.SelectedValue as CalendarEventRow;
            if ((sel is null) || (!this.state.events.events.ContainsKey(sel.guid))) { return; }
            CalendarEventWindow dialog_window = new CalendarEventWindow(this.state, this.calendar, this.now, guid: sel.guid) { Owner = Window.GetWindow(this) };
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            List<EntryAction> actions = dialog_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions);
        }
    }
}
