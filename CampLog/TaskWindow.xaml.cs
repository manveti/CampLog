using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public partial class TaskWindow : Window {
        public bool valid;
        private CampaignSave save_state;
        private CampaignState state;
        private decimal now;
        private Guid guid;
        private List<EntryAction> actions;
        private Task task;
        private ICalendarControl due_box = null;
        private ICalendarControl due_diff_box = null;

        public TaskWindow(CampaignSave save_state, CampaignState state, Calendar calendar, decimal now, Guid? entry_guid = null, Guid? guid = null) {
            this.valid = false;
            this.save_state = save_state;
            this.state = state.copy();
            this.now = now;
            this.actions = new List<EntryAction>();
            if (guid is null) {
                if (entry_guid is null) { throw new ArgumentNullException(nameof(entry_guid)); }
                this.guid = Guid.NewGuid();
                ActionTaskCreate add_action = new ActionTaskCreate(this.guid, new Task(entry_guid.Value, ""));
                this.actions.Add(add_action);
                this.task = add_action.task;
            }
            else {
                this.guid = guid.Value;
                this.task = this.state.tasks.tasks[this.guid];
            }
            InitializeComponent();
            this.name_box.Text = this.task.name;
            this.description_box.Text = this.task.description;
            this.current_timestamp_box.Text = calendar.format_timestamp(now);
            if (this.task.completed_guid is null) {
                FrameworkElement due_box = calendar.timestamp_control();
                Grid.SetRow(due_box, 2);
                Grid.SetColumn(due_box, 1);
                this.main_grid.Children.Add(due_box);
                this.due_box = due_box as ICalendarControl;
                if (this.due_box is null) { throw new InvalidOperationException(); }
                FrameworkElement due_diff_box = calendar.interval_control();
                Grid.SetRow(due_diff_box, 2);
                Grid.SetColumn(due_diff_box, 3);
                this.main_grid.Children.Add(due_diff_box);
                this.due_diff_box = due_diff_box as ICalendarControl;
                if (this.due_diff_box is null) { throw new InvalidOperationException(); }
                if (this.task.due is null) {
                    this.due_checkbox.IsChecked = false;
                    this.due_box.calendar_value = now;
                    this.due_box.IsReadOnly = true;
                    this.due_diff_box.calendar_value = 0;
                    this.due_diff_box.IsReadOnly = true;
                }
                else {
                    this.due_checkbox.IsChecked = true;
                    this.due_box.calendar_value = this.task.due.Value;
                    this.due_box.IsReadOnly = false;
                    decimal due_diff = this.task.due.Value - now;
                    if (due_diff < 0) {
                        this.timestamp_diff_label.Content = "before";
                        due_diff = -due_diff;
                    }
                    else {
                        this.timestamp_diff_label.Content = "after";
                    }
                    this.due_diff_box.calendar_value = due_diff;
                    this.due_diff_box.IsReadOnly = false;
                }
                this.due_box.value_changed = this.due_changed;
                this.due_diff_box.value_changed = this.due_diff_changed;
            }
            else {
                decimal? completed_timestamp = null;
                if (task.completed_guid == entry_guid) {
                    completed_timestamp = now;
                }
                else {
                    foreach (Entry entry in this.save_state.domain.entries) {
                        if (entry.guid == task.completed_guid) {
                            completed_timestamp = entry.timestamp;
                            break;
                        }
                    }
                }
                if (completed_timestamp is null) { throw new InvalidOperationException(); }
                if (this.task.failed) { this.status_label.Content = "Failed:"; }
                else { this.status_label.Content = "Completed:"; }
                this.status_label.Visibility = Visibility.Visible;
                this.due_checkbox.Visibility = Visibility.Collapsed;
                this.timestamp_box.Text = calendar.format_timestamp(completed_timestamp.Value);
                this.timestamp_box.Visibility = Visibility.Visible;
                decimal timestamp_diff = completed_timestamp.Value - now;
                if (timestamp_diff < 0) {
                    this.timestamp_diff_label.Content = "before";
                    timestamp_diff = -timestamp_diff;
                }
                else {
                    this.timestamp_diff_label.Content = "after";
                }
                this.timestamp_diff_box.Text = calendar.format_interval(timestamp_diff);
                this.timestamp_diff_box.Visibility = Visibility.Visible;
            }
        }

        private void due_checkbox_changed(object sender, RoutedEventArgs e) {
            bool is_read_only = !(this.due_checkbox.IsChecked ?? false);
            this.due_box.IsReadOnly = is_read_only;
            this.due_diff_box.IsReadOnly = is_read_only;
        }

        private void due_changed() {
            decimal due_diff = this.due_box.calendar_value - this.now;
            if (due_diff < 0) {
                this.timestamp_diff_label.Content = "before";
                due_diff = -due_diff;
            }
            else {
                this.timestamp_diff_label.Content = "after";
            }
            this.due_diff_box.calendar_value = due_diff;
        }

        private void due_diff_changed() {
            decimal due = this.due_box.calendar_value, due_diff = this.due_diff_box.calendar_value;
            if (due < this.now) {
                due_diff = -due_diff;
            }
            this.due_box.calendar_value = this.now + due_diff;
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            string name = this.name_box.Text, description = this.description_box.Text;
            decimal? due = null;
            bool set_name = (name != this.task.name), set_desc = (description != this.task.description), set_due = false;
            if (this.task.completed_guid is null) {
                due = (this.due_checkbox.IsChecked ?? false ? this.due_box.calendar_value : null);
                set_due = (due != this.task.due);
            }
            if ((set_name) || (set_desc) || (set_due)) {
                if (!set_name) { name = ""; }
                if (!set_desc) { description = ""; }
                if (!set_due) { due = null; }
                this.actions.Add(
                    new ActionTaskUpdate(
                        this.guid,
                        this.task,
                        new Task(Guid.NewGuid(), name, description, due),
                        set_name,
                        set_desc,
                        false,
                        false,
                        set_due
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
