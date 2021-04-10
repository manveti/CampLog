using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CampLog {
    public class EntryRow : INotifyPropertyChanged {
        public string _timestamp;
        public string _invalid;
        public string _session;
        public string _created;
        public string _description;

        public event PropertyChangedEventHandler PropertyChanged;

        public string timestamp { get => this._timestamp; }
        public string invalid { get => this._invalid; }
        public string session { get => this._session; }
        public string created { get => this._created; }
        public string description { get => this._description; }

        public EntryRow(string timestamp, bool invalid, string session, string created, string description) {
            this._timestamp = timestamp;
            this.set_invalid(invalid);
            this._session = session;
            this._created = created;
            this._description = description.Split("\n")[0];
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void set_invalid(bool invalid) {
            string new_invalid = (invalid ? "Invalid" : "");
            if (new_invalid == this._invalid) { return; }
            this._invalid = new_invalid;
            this.NotifyPropertyChanged();
        }
    }


    public partial class MainWindow : Window {
        private CampaignSave state;
        private bool state_dirty;
        private string save_path;
        public ObservableCollection<EntryRow> entry_rows;
        private CharacterListControl character_list;
        private InventoryListControl inventory_list;

        public MainWindow() {
            this.save_path = null;
            this.state = null;
            this.state_dirty = false;
            this.entry_rows = new ObservableCollection<EntryRow>();
            this.character_list = new CharacterListControl(this.entry_action_callback);
            this.inventory_list = new InventoryListControl(this.entry_action_callback);
            InitializeComponent();
            this.character_group.Content = this.character_list;
            this.inventory_group.Content = this.inventory_list;
            this.entries_list.ItemsSource = this.entry_rows;
        }

        private void new_campaign(object sender, RoutedEventArgs e) {
            if (this.state_dirty) {
                string msg = "Do you want to save changes to " + (this.save_path ?? "Untitled") + "?";
                MessageBoxResult result = MessageBox.Show(msg, "Campaign Log", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel) { return; }
                if (result != MessageBoxResult.No) {
                    //TODO: save this.state
                    this.state_dirty = false;
                }
            }

            CampaignPropsWindow props_window = new CampaignPropsWindow() { Owner = this };
            props_window.ShowDialog();
            if (!props_window.valid) { return; }

            Calendar cal;
            CharacterSheet char_sheet;
            try {
                cal = props_window.get_calendar();
                char_sheet = props_window.get_character_sheet();
            }
            catch (InvalidOperationException) { return; }

            this.state = new CampaignSave(cal, char_sheet);
            this.state_dirty = false;
            this.save_path = null;

            this.save_opt.IsEnabled = true;
            this.save_as_opt.IsEnabled = true;
            this.calendar_cfg_opt.IsEnabled = true;
            this.charsheet_cfg_opt.IsEnabled = true;
            this.item_library_opt.IsEnabled = true;
            this.revert_opt.IsEnabled = true;
            this.character_list.set_char_sheet(char_sheet);
            this.character_list.set_state(this.state.domain.state);
            this.inventory_list.set_state(this.state.domain.state);
            this.session_num_box.Content = "1";
            this.current_timestamp_box.Content = this.state.calendar.format_timestamp(this.state.calendar.default_timestamp);
            this.entry_rows.Clear();
            this.ent_add_but.IsEnabled = true;
            //TODO: ent_rem_but, ent_view_but
            this.show_past_events_box.IsChecked = this.state.show_past_events;
            //TODO: events_list, event_add_but, event_rem_but, event_view_but
            this.show_inactive_tasks_box.IsChecked = this.state.show_inactive_tasks;
            //TODO: tasks_list, task_add_but, task_rem_but, task_resolve_but, task_view_but
            //TODO: topics_list, topic_add_but, topic_rem_but, topic_view_but
        }

        //TODO: ...

        private void entry_action_callback(List<EntryAction> actions) {
            EntryWindow entry_window = new EntryWindow(this.state, actions: actions) { Owner = this };
            entry_window.ShowDialog();
            if (!entry_window.valid) { return; }

            decimal timestamp = entry_window.timestamp_box.calendar_value;
            DateTime created = entry_window.get_created();
            string description = entry_window.description_box.Text;
            int session = (int)(entry_window.session_box.Value);
            Entry ent = new Entry(timestamp, created, description, session, new List<EntryAction>(entry_window.actions));
            this.state_dirty = true;
            int idx = this.state.domain.add_entry(ent);
            int valid_idx = this.state.domain.valid_entries - 1;
            if (valid_idx < 0) { valid_idx = this.state.domain.entries.Count - 1; }
            this.session_num_box.Content = (this.state.domain.entries[valid_idx].session ?? 0).ToString();
            this.current_timestamp_box.Content = this.state.calendar.format_timestamp(this.state.domain.entries[valid_idx].timestamp);
            EntryRow row = new EntryRow(
                this.state.calendar.format_timestamp(ent.timestamp),
                idx >= this.state.domain.valid_entries,
                (ent.session ?? 0).ToString(),
                ent.created.ToString("G"),
                ent.description
            );
            this.entry_rows.Insert(this.entry_rows.Count - idx, row);
            for (int i = 0; i < this.entry_rows.Count; i++) {
                this.entry_rows[i].set_invalid(this.entry_rows.Count - i > this.state.domain.valid_entries);
            }
            this.character_list.set_state(this.state.domain.state);
            this.inventory_list.set_state(this.state.domain.state);
            //TODO: set_state for other lists
        }

        private void entries_list_sel_changed(object sender, RoutedEventArgs e) {
            bool entry_selected = this.entries_list.SelectedIndex >= 0;
            this.ent_rem_but.IsEnabled = entry_selected;
            this.ent_view_but.IsEnabled = entry_selected;
        }

        private void add_entry(object sender, RoutedEventArgs e) {
            this.entry_action_callback(null);
        }

        private void remove_entry(object sender, RoutedEventArgs e) {
            int row_idx = this.entries_list.SelectedIndex, idx = this.state.domain.entries.Count - row_idx - 1;
            if ((idx < 0) || (idx >= this.state.domain.entries.Count)) { return; }
            this.state_dirty = true;
            this.state.domain.remove_entry(idx);
            this.entry_rows.RemoveAt(row_idx);
        }

        private void view_entry(object sender, RoutedEventArgs e) {
            int row_idx = this.entries_list.SelectedIndex, idx = this.state.domain.entries.Count - row_idx - 1;
            if ((idx < 0) || (idx >= this.state.domain.entries.Count)) { return; }
            EntryWindow entry_window = new EntryWindow(this.state, idx) { Owner = this };
            entry_window.ShowDialog();
            if (!entry_window.valid) { return; }

            decimal timestamp = entry_window.timestamp_box.calendar_value;
            DateTime created = entry_window.get_created();
            string description = entry_window.description_box.Text;
            int session = (int)(entry_window.session_box.Value);
            Entry ent = this.state.domain.entries[idx];
            bool changed = (timestamp != ent.timestamp) || (created != ent.created) || (description != ent.description) || (session != ent.session);
            if ((!changed) && (entry_window.actions.Count != ent.actions.Count)) { changed = true; }
            if (!changed) {
                for (int i = 0; i < entry_window.actions.Count; i++) {
                    if (entry_window.actions[i] != ent.actions[i]) {
                        changed = true;
                        break;
                    }
                }
            }
            if (!changed) { return; }
            this.state_dirty = true;
            this.state.domain.remove_entry(idx, true);
            this.entry_rows.RemoveAt(row_idx);
            ent.timestamp = timestamp;
            ent.created = created;
            ent.description = description;
            ent.session = session;
            ent.actions = entry_window.actions;
            idx = this.state.domain.add_entry(ent);
            int valid_idx = this.state.domain.valid_entries - 1;
            if (valid_idx < 0) { valid_idx = this.state.domain.entries.Count - 1; }
            this.session_num_box.Content = (this.state.domain.entries[valid_idx].session ?? 0).ToString();
            this.current_timestamp_box.Content = this.state.calendar.format_timestamp(this.state.domain.entries[valid_idx].timestamp);
            EntryRow row = new EntryRow(
                this.state.calendar.format_timestamp(ent.timestamp),
                idx >= this.state.domain.valid_entries,
                (ent.session ?? 0).ToString(),
                ent.created.ToString("G"),
                ent.description
            );
            this.entry_rows.Insert(this.entry_rows.Count - idx, row);
            for (int i = 0; i < this.entry_rows.Count; i++) {
                this.entry_rows[i].set_invalid(this.entry_rows.Count - i > this.state.domain.valid_entries);
            }
        }

        //TODO: ...

        //TODO: remove
        private void do_test(object sender, RoutedEventArgs e) {
            CampaignSave state = new CampaignSave(new Calendar(), new CharacterSheet());
#if false
            Character chr = new Character("Bob");
            chr.set_property(new List<string>() { "Skills" }, new CharDictProperty());
            chr.set_property(new List<string>() { "Skills", "Jump" }, new CharNumProperty(7));
            chr.set_property(new List<string>() { "Skills", "Skull Smashing" }, new CharNumProperty(42));
            Guid guid = state.domain.state.characters.add_character(chr);
            //SimpleCharacterWindow cw = new SimpleCharacterWindow(state.domain.state, guid) { Owner = this };
            //cw.ShowDialog();
            this.character_list.set_state(state.domain.state);
#endif
            Guid guid = state.domain.state.inventories.new_inventory("Party Loot");
            this.inventory_list.set_state(state.domain.state);
#if false
            if (!cw.valid) { return; }
            List<string> action_types = new List<string>();
            foreach (EntryAction action in cw.actions) { action_types.Add(action.GetType().ToString()); }
            MessageBox.Show(String.Join(", ", action_types), "Actions", MessageBoxButton.OK);
#endif
        }
        //TODO: /remove
    }
}
