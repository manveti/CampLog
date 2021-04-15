﻿using System;
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
        private int entry;
        private List<Entry> entries;
        private int previous_entry_idx = -1;
        private CampaignState state;
        public List<EntryAction> actions;
        public ICalendarControl timestamp_box;
        private ICalendarControl timestamp_diff_box;
        private CharacterListControl character_list;
        private InventoryListControl inventory_list;

        public EntryWindow(CampaignSave save_state, int entry = -1, List<EntryAction> actions = null) {
            this.valid = false;
            this.save_state = save_state;
            this.entry = entry;
            this.entries = new List<Entry>();
            for (int i = 0; i < save_state.domain.entries.Count; i++) {
                if (i != entry) { this.entries.Add(save_state.domain.entries[i]); }
            }
            this.state = save_state.domain.state.copy();
            Entry previous_entry = null, current_entry;
            if ((entry >= 0) && (entry < save_state.domain.entries.Count)) {
                current_entry = save_state.domain.entries[entry];
                if (entry > 0) {
                    this.previous_entry_idx = entry - 1;
                    previous_entry = this.entries[entry - 1];
                }
            }
            else {
                decimal timestamp;
                int session;
                if (save_state.domain.valid_entries > 0) {
                    this.previous_entry_idx = save_state.domain.valid_entries - 1;
                    previous_entry = this.entries[this.previous_entry_idx];
                    timestamp = previous_entry.timestamp + 1;
                }
                else {
                    timestamp = save_state.calendar.default_timestamp;
                }
                session = previous_entry?.session ?? 1;
                if ((previous_entry is not null) && ((DateTime.Now - previous_entry.created) >= SESSION_DOWNTIME_THRESHOLD)) { session += 1; }
                current_entry = new Entry(timestamp, DateTime.Now, "", session, actions);
                if (actions is not null) {
                    foreach (EntryAction action in actions) {
                        action.apply(this.state, current_entry);
                    }
                }
            }
            this.actions = new List<EntryAction>();
            this.add_actions(current_entry.actions, false);
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
            this.action_list.ItemsSource = this.actions;
            //TODO: events
            this.character_list = new CharacterListControl(this.entry_action_callback);
            this.character_list.set_char_sheet(this.save_state.character_sheet);
            this.character_list.set_state(this.state);
            this.character_group.Content = this.character_list;
            this.inventory_list = new InventoryListControl(this.entry_action_callback);
            this.inventory_list.set_state(this.state);
            this.inventory_group.Content = this.inventory_list;
            //TODO: topics, tasks
        }

        private void add_actions(List<EntryAction> actions, bool refresh = true) {
            foreach (EntryAction action in actions) {
                action.merge_to(this.actions);
            }
            if (refresh) {
                this.action_list.Items.Refresh();
            }
        }

        public DateTime get_created() {
            DateTime created = this.created_date_box.SelectedDate ?? DateTime.Today;
            created += new TimeSpan(((long)(this.created_time_box.Value)) * TimeSpan.TicksPerSecond);
            return created;
        }

        private void timestamp_changed() {
            decimal timestamp = this.timestamp_box.calendar_value, timestamp_diff;
            Entry ent = new Entry(timestamp, this.get_created(), "");
            int entry_idx = this.entries.BinarySearch(ent);
            if (entry_idx >= 0) {
                this.previous_entry_idx = entry_idx;
            }
            else {
                this.previous_entry_idx = ~entry_idx - 1;
            }
            if (this.previous_entry_idx >= 0) {
                timestamp_diff = timestamp - this.entries[this.previous_entry_idx].timestamp;
            }
            else {
                timestamp_diff = 0;
            }
            this.timestamp_diff_box.calendar_value = timestamp_diff;
            //TODO: update diff entry
        }

        private void timestamp_diff_changed() {
            decimal timestamp, timestamp_diff = this.timestamp_diff_box.calendar_value;
            if (this.previous_entry_idx < 0) {
                this.timestamp_diff_box.calendar_value = 0;
                return;
            }
            timestamp = this.entries[this.previous_entry_idx].timestamp + timestamp_diff;
            this.timestamp_box.calendar_value = timestamp;
            //TODO: update diff entry
        }

        private void action_list_sel_changed(object sender, RoutedEventArgs e) {
            int idx = this.action_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.actions.Count)) {
                this.act_rem_but.IsEnabled = false;
                this.act_edit_but.IsEnabled = false;
                return;
            }
            this.act_rem_but.IsEnabled = true;
            this.act_edit_but.IsEnabled = true;
        }

        private void entry_action_callback(List<EntryAction> actions) {
            if ((actions is null) || (actions.Count <= 0)) { return; }
            decimal timestamp = this.timestamp_box.calendar_value;
            DateTime created = this.get_created();
            string description = this.description_box.Text;
            int session = (int)(this.session_box.Value);
            this.add_actions(actions);
            Entry ent = new Entry(timestamp, created, description, session, new List<EntryAction>(this.actions));
            foreach (EntryAction action in actions) {
                action.apply(this.state, ent);
            }
            //TODO: update events list
            this.character_list.set_state(this.state);
            this.inventory_list.set_state(this.state);
            //TODO: update topics and tasks lists
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
