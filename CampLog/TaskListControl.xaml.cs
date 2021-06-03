using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public class TaskRow : IComparable {
        public Guid guid;
        public string _status;
        public string _name;
        public decimal? raw_timestamp;
        public string _timestamp;

        public TaskRow self { get => this; }
        public string status { get => this._status; }
        public string name { get => this._name; }
        public string timestamp { get => this._timestamp; }

        public TaskRow(Guid guid, string status, string name, decimal? raw_timestamp, string timestamp) {
            this.guid = guid;
            this._status = status;
            this._name = name;
            this.raw_timestamp = raw_timestamp;
            this._timestamp = timestamp;
        }

        public int CompareTo(object obj) {
            if (obj is null) { return 1; }
            TaskRow other = obj as TaskRow;
            if (other is null) { return 1; }
            if (this.raw_timestamp == other.raw_timestamp) { return this._name.CompareTo(other._name); }
            // sort null highest rather than lowest -- tasks without due date should be treated as due infinitely far in the future
            if (this.raw_timestamp is null) { return 1; }
            if (other.raw_timestamp is null) { return -1; }
            return this.raw_timestamp.Value.CompareTo(other.raw_timestamp.Value);
        }
    }


    public partial class TaskListControl : UserControl {
        private ActionCallback change_callback;
        private Calendar calendar;
        private Guid? entry_guid;
        private CampaignSave save_state;
        private CampaignState state;
        private decimal now;
        private List<TaskRow> task_rows;

        public bool show_inactive {
            get => this.show_inactive_box.IsChecked ?? false;
            set => this.show_inactive_box.IsChecked = value;
        }

        private void fix_listview_column_widths(ListView list_view) {
            GridView grid_view = list_view.View as GridView;
            if (grid_view is null) { return; }
            foreach (GridViewColumn col in grid_view.Columns) {
                col.Width = col.ActualWidth;
                col.Width = double.NaN;
            }
        }

        public TaskListControl(ActionCallback change_callback, Guid? entry_guid = null) {
            this.change_callback = change_callback;
            this.calendar = new Calendar();
            this.entry_guid = entry_guid;
            this.save_state = null;
            this.state = null;
            this.now = 0;
            this.task_rows = new List<TaskRow>();
            InitializeComponent();
            this.task_list.ItemsSource = this.task_rows;
        }

        private void populate_task_rows() {
            this.task_rows.Clear();
            if (this.state is null) {
                this.task_list.Items.Refresh();
                return;
            }
            foreach (Guid guid in this.state.tasks.active_tasks) {
                Task task = this.state.tasks.tasks[guid];
                decimal? completed_timestamp = null;
                if (task.completed_guid is not null) {
                    if (!this.show_inactive) { continue; }
                    if (task.completed_guid == this.entry_guid) {
                        completed_timestamp = this.now;
                    }
                    else {
                        foreach (Entry entry in this.save_state.domain.entries) {
                            if (entry.guid == task.completed_guid) {
                                completed_timestamp = entry.timestamp;
                                break;
                            }
                        }
                    }
                }
                decimal? raw_timestamp = task.due;
                string status = "", timestamp = "";
                if (completed_timestamp is not null) {
                    status = (task.failed ? "✗" : "✓");
                    raw_timestamp = completed_timestamp;
                }
                if (raw_timestamp is not null) {
                    if ((completed_timestamp is null) && (raw_timestamp < this.now)) { status = "⌛"; }
                    timestamp = this.calendar.format_timestamp(raw_timestamp.Value);
                }
                this.task_rows.Add(new TaskRow(guid, status, task.name, raw_timestamp, timestamp));
            }
            this.task_rows.Sort();
            this.task_list.Items.Refresh();
            this.fix_listview_column_widths(this.task_list);
        }

        public void set_calendar(Calendar calendar) {
            this.calendar = calendar;
            foreach (TaskRow row in this.task_rows) {
                if (row.raw_timestamp is not null) {
                    row._timestamp = calendar.format_timestamp(row.raw_timestamp.Value);
                }
            }
            this.task_list.Items.Refresh();
            this.fix_listview_column_widths(this.task_list);
        }

        public void set_state(CampaignSave save_state, CampaignState state, decimal timestamp) {
            this.save_state = save_state;
            this.state = state;
            this.now = timestamp;
            this.populate_task_rows();
            if (this.state is null) {
                this.add_but.IsEnabled = false;
                this.rem_but.IsEnabled = false;
                this.fail_but.IsEnabled = false;
                this.complete_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
            }
            else {
                this.add_but.IsEnabled = true;
            }
        }

        private void show_inactive_changed(object sender, RoutedEventArgs e) {
            this.populate_task_rows();
            if (this.task_rows.Count <= 0) { return; }
            TaskRow next_row = this.task_rows[^1];
            foreach (TaskRow row in this.task_rows) {
                if ((row.raw_timestamp is null) || (row.raw_timestamp >= this.now)) {
                    next_row = row;
                    break;
                }
            }
            this.task_list.ScrollIntoView(next_row);
        }

        private void list_sel_changed(object sender, RoutedEventArgs e) {
            TaskRow sel = this.task_list.SelectedValue as TaskRow;
            if ((sel is null) || (!this.state.tasks.active_tasks.Contains(sel.guid))) {
                this.rem_but.IsEnabled = false;
                this.fail_but.IsEnabled = false;
                this.complete_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
                return;
            }
            bool incomplete = (this.state.tasks.tasks[sel.guid].completed_guid is null);
            this.rem_but.IsEnabled = true;
            this.fail_but.IsEnabled = incomplete;
            this.complete_but.IsEnabled = incomplete;
            this.view_but.IsEnabled = true;
        }

        private void do_add(object sender, RoutedEventArgs e) {
            if (this.state is null) { return; }
            Guid ent_guid = this.entry_guid ?? Guid.NewGuid();
            TaskWindow dialog_window = new TaskWindow(this.save_state, this.state, this.calendar, this.now, ent_guid) { Owner = Window.GetWindow(this) };
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            List<EntryAction> actions = dialog_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions, ent_guid);
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            if ((this.change_callback is null) || (this.state is null)) { return; }
            TaskRow sel = this.task_list.SelectedValue as TaskRow;
            if ((sel is null) || (!this.state.tasks.tasks.ContainsKey(sel.guid))) { return; }
            List<EntryAction> actions = new List<EntryAction>() { new ActionTaskRemove(sel.guid) };
            this.change_callback(actions);
        }

        private void _do_resolve(bool failed) {
            if ((this.change_callback is null) || (this.state is null)) { return; }
            TaskRow sel = this.task_list.SelectedValue as TaskRow;
            if ((sel is null) || (!this.state.tasks.tasks.ContainsKey(sel.guid))) { return; }
            Guid ent_guid = this.entry_guid ?? Guid.NewGuid();
            Task from = this.state.tasks.tasks[sel.guid], to = new Task(from.entry_guid, "") { completed_guid = ent_guid, failed = failed };
            ActionTaskUpdate action = new ActionTaskUpdate(sel.guid, from, to, false, false, true, true, false);
            List<EntryAction> actions = new List<EntryAction>() { action };
            this.change_callback(actions, ent_guid);
        }

        private void do_fail(object sender, RoutedEventArgs e) {
            this._do_resolve(true);
        }

        private void do_complete(object sender, RoutedEventArgs e) {
            this._do_resolve(false);
        }

        private void do_view(object sender, RoutedEventArgs e) {
            if (this.state is null) { return; }
            TaskRow sel = this.task_list.SelectedValue as TaskRow;
            if ((sel is null) || (!this.state.tasks.tasks.ContainsKey(sel.guid))) { return; }
            Guid ent_guid = this.entry_guid ?? Guid.NewGuid();
            TaskWindow dialog_window = new TaskWindow(this.save_state, this.state, this.calendar, this.now, ent_guid, sel.guid) {
                Owner = Window.GetWindow(this)
            };
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            List<EntryAction> actions = dialog_window.get_actions();
            if ((actions is null) || (actions.Count <= 0)) { return; }
            this.change_callback(actions);
        }
    }
}
