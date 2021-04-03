using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using GUIx;

namespace CampLog {
    public partial class EntryWindow : Window {
        private static readonly TimeSpan SESSION_DOWNTIME_THRESHOLD = new TimeSpan(12, 0, 0);

        public bool valid;
        private CampaignSave save_state;
        private CampaignState state;
        public List<EntryAction> actions;
        public ICalendarControl timestamp_box;
        private ICalendarControl timestamp_diff_box;

        public EntryWindow(CampaignSave save_state, int entry = -1) {
            this.valid = false;
            this.save_state = save_state;
            this.state = save_state.domain.state.copy();
            Entry previous_entry = null, current_entry;
            if ((entry >= 0) && (entry < save_state.domain.entries.Count)) {
                current_entry = save_state.domain.entries[entry];
                if (entry > 0) {
                    previous_entry = save_state.domain.entries[entry - 1];
                }
            }
            else {
                decimal timestamp;
                int session;
                if (save_state.domain.valid_entries > 0) {
                    previous_entry = save_state.domain.entries[save_state.domain.valid_entries - 1];
                    timestamp = previous_entry.timestamp + 1;
                }
                else {
                    timestamp = save_state.calendar.default_timestamp;
                }
                session = previous_entry?.session ?? 1;
                if ((previous_entry is not null) && ((DateTime.Now - previous_entry.created) >= SESSION_DOWNTIME_THRESHOLD)) { session += 1; }
                current_entry = new Entry(timestamp, DateTime.Now, "", session);
            }
            this.actions = new List<EntryAction>(current_entry.actions);
            InitializeComponent();
            this.session_box.textBox.VerticalContentAlignment = VerticalAlignment.Center;
            this.session_box.Value = current_entry.session ?? 0;
            this.created_time_box.textBox.VerticalContentAlignment = VerticalAlignment.Center;
            this.created_date_box.SelectedDate = current_entry.created.Date;
            this.created_time_box.Value = current_entry.created.TimeOfDay.TotalSeconds;
            FrameworkElement timestamp_box = save_state.calendar.timestamp_control();
            Grid.SetRow(timestamp_box, 0);
            Grid.SetColumn(timestamp_box, 6);
            this.header_grid.Children.Add(timestamp_box);
            this.timestamp_box = timestamp_box as ICalendarControl;
            if (this.timestamp_box is null) { throw new InvalidOperationException(); }
            this.timestamp_box.calendar_value = current_entry.timestamp;
            FrameworkElement timestamp_diff_box = save_state.calendar.interval_control();
            Grid.SetRow(timestamp_diff_box, 0);
            Grid.SetColumn(timestamp_diff_box, 8);
            this.header_grid.Children.Add(timestamp_diff_box);
            this.timestamp_diff_box = timestamp_diff_box as ICalendarControl;
            if (this.timestamp_diff_box is null) { throw new InvalidOperationException(); }
            this.timestamp_diff_box.calendar_value = (previous_entry is null ? 0 : current_entry.timestamp - previous_entry.timestamp);
            this.timestamp_box.value_changed = this.timestamp_changed;
            this.timestamp_diff_box.value_changed = this.timestamp_diff_changed;
            //TODO: previous entry
            this.description_box.Text = current_entry.description;
            //TODO: actions, events, characters, inventories, topics, tasks
        }

        private void timestamp_changed() {
            decimal timestamp = this.timestamp_box.calendar_value, timestamp_diff;
            int entry_idx = this.save_state.domain.get_timestamp_index(timestamp) + 1;
            timestamp_diff = (entry_idx > 0 ? timestamp - this.save_state.domain.entries[entry_idx - 1].timestamp : 0);
            this.timestamp_diff_box.calendar_value = timestamp_diff;
            //TODO: update diff entry
        }

        private void timestamp_diff_changed() {
            decimal timestamp = this.timestamp_box.calendar_value, timestamp_diff = this.timestamp_diff_box.calendar_value;
            int entry_idx = this.save_state.domain.get_timestamp_index(timestamp) + 1;
            if (entry_idx <= 0) {
                this.timestamp_diff_box.calendar_value = 0;
                return;
            }
            timestamp = this.save_state.domain.entries[entry_idx - 1].timestamp + timestamp_diff;
            this.timestamp_box.calendar_value = timestamp;
            //TODO: update diff entry
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
