﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;

namespace CampLog {
    public delegate void ActionCallback(
        List<EntryAction> actions,
        Guid? entry_guid = null,
        Dictionary<Guid, Topic> topics = null,
        Dictionary<Guid, int> topic_refs = null,
        Dictionary<Guid, ExternalNote> notes = null
    );


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
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private decimal current_timestamp = 0;
        public ObservableCollection<EntryRow> entry_rows;
        private CharacterListControl character_list;
        private InventoryListControl inventory_list;
        private CalendarEventListControl calendar_event_list;
        private TaskListControl task_list;
        private TopicListControl topic_list;

        public MainWindow() {
            this.save_path = null;
            this.state = null;
            this.state_dirty = false;
            this.entry_rows = new ObservableCollection<EntryRow>();
            this.character_list = new CharacterListControl(this.entry_action_callback);
            this.inventory_list = new InventoryListControl(this.entry_action_callback);
            this.calendar_event_list = new CalendarEventListControl(this.entry_action_callback);
            this.task_list = new TaskListControl(this.entry_action_callback);
            this.topic_list = new TopicListControl(this.entry_action_callback);
            InitializeComponent();
            this.character_group.Content = this.character_list;
            this.inventory_group.Content = this.inventory_list;
            this.entries_list.ItemsSource = this.entry_rows;
            this.calendar_event_group.Content = this.calendar_event_list;
            this.task_group.Content = this.task_list;
            this.topic_group.Content = this.topic_list;
        }

        private void populate_campaign() {
            this.save_opt.IsEnabled = true;
            this.save_as_opt.IsEnabled = true;
            this.calendar_cfg_opt.IsEnabled = true;
            this.charsheet_cfg_opt.IsEnabled = true;
            this.item_library_opt.IsEnabled = true;
            this.character_list.set_char_sheet(this.state.character_sheet);
            this.character_list.set_state(this.state.domain.state);
            this.inventory_list.set_state(this.state, this.state.domain.state);
            int valid_idx = this.state.domain.valid_entries - 1;
            if (valid_idx < 0) { valid_idx = this.state.domain.entries.Count - 1; }
            if (valid_idx >= 0) {
                this.current_timestamp = this.state.domain.entries[valid_idx].timestamp;
                this.session_num_box.Content = (this.state.domain.entries[valid_idx].session ?? 0).ToString();
            }
            else {
                this.current_timestamp = this.state.calendar.default_timestamp;
                this.session_num_box.Content = "1";
            }
            this.current_timestamp_box.Content = this.state.calendar.format_timestamp(this.current_timestamp);
            this.entry_rows.Clear();
            for (int i = this.state.domain.entries.Count - 1; i >= 0; i--) {
                Entry ent = this.state.domain.entries[i];
                EntryRow row = new EntryRow(
                    this.state.calendar.format_timestamp(ent.timestamp),
                    i >= this.state.domain.valid_entries,
                    (ent.session ?? 0).ToString(),
                    ent.created.ToString("G"),
                    ent.description
                );
                this.entry_rows.Add(row);
            }
            this.ent_add_but.IsEnabled = true;
            this.calendar_event_list.show_past = this.state.show_past_events;
            this.calendar_event_list.set_calendar(this.state.calendar);
            this.calendar_event_list.set_state(this.state.domain.state, this.current_timestamp);
            this.task_list.show_inactive = this.state.show_inactive_tasks;
            this.task_list.set_calendar(this.state.calendar);
            this.task_list.set_state(this.state, this.state.domain.state, this.current_timestamp);
            this.topic_list.set_state(this.state, this.state.domain.state, this.current_timestamp);
        }

        private void new_campaign(object sender, RoutedEventArgs e) {
            if (this.state_dirty) {
                string msg = "Do you want to save changes to " + (this.save_path ?? "Untitled") + "?";
                MessageBoxResult result = MessageBox.Show(msg, "Campaign Log", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel) { return; }
                if (result != MessageBoxResult.No) {
                    this.save_campaign();
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
            this.populate_campaign();
        }

        private void open_campaign(object sender, RoutedEventArgs e) {
            if (this.state_dirty) {
                string msg = "Do you want to save changes to " + (this.save_path ?? "Untitled") + "?";
                MessageBoxResult result = MessageBox.Show(msg, "Campaign Log", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel) { return; }
                if (result != MessageBoxResult.No) {
                    this.save_campaign();
                }
            }

            OpenFileDialog open_dialog = new OpenFileDialog() {
                DefaultExt = ".cmp",
                Filter = "Campaign Files|*.cmp|All Files|*.*",
                ValidateNames = true,
            };
            if (this.save_path is not null) { open_dialog.FileName = this.save_path; }
            //TODO: if save directory defined: open_dialog.InitialDirectory = save directory
            if (open_dialog.ShowDialog() != true) { return; }
            this.save_path = open_dialog.FileName;
            //TODO: save directory = System.IO.Path.GetDirectoryName(this.save_path)

            DataContractSerializer serializer = new DataContractSerializer(typeof(CampaignSave));
            using (FileStream f = new FileStream(this.save_path, FileMode.Open)) {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(f, new XmlDictionaryReaderQuotas());
                CampaignSave save_state = (CampaignSave)(serializer.ReadObject(reader, true));
                this.state = save_state;
            }
            this.state_dirty = false;
            this.populate_campaign();
        }

        private void save_campaign(object sender = null, RoutedEventArgs e = null) {
            if (this.save_path is null) {
                this.save_campaign_as();
                return;
            }

            DataContractSerializer serializer = new DataContractSerializer(typeof(CampaignSave));
            using (FileStream f = new FileStream(this.save_path, FileMode.Create)) {
                serializer.WriteObject(f, this.state);
            }
            this.state_dirty = false;
        }

        private void save_campaign_as(object sender = null, RoutedEventArgs e = null) {
            SaveFileDialog save_dialog = new SaveFileDialog() {
                DefaultExt = ".cmp",
                Filter = "Campaign Files|*.cmp|All Files|*.*",
                ValidateNames = true,
            };
            //TODO: if save directory defined: save_dialog.InitialDirectory = save directory
            if (save_dialog.ShowDialog() != true) { return; }
            this.save_path = save_dialog.FileName;
            //TODO: save directory = System.IO.Path.GetDirectoryName(this.save_path)
            this.save_campaign();
        }

        //TODO: ...

        private void item_library(object sender, RoutedEventArgs e) {
            ItemLibraryWindow item_library_window = new ItemLibraryWindow(this.state) { Owner = this };
            item_library_window.ShowDialog();
            if (item_library_window.need_refresh) {
                this.inventory_list.set_state(this.state, this.state.domain.state);
            }
        }

        private void refresh_lists() {
            this.character_list.set_state(this.state.domain.state);
            this.inventory_list.set_state(this.state, this.state.domain.state);
            this.calendar_event_list.set_state(this.state.domain.state, this.current_timestamp);
            this.task_list.set_state(this.state, this.state.domain.state, this.current_timestamp);
            this.topic_list.set_state(this.state, this.state.domain.state, this.current_timestamp);
        }

        private void entry_action_callback(
            List<EntryAction> actions,
            Guid? entry_guid = null,
            Dictionary<Guid, Topic> topics = null,
            Dictionary<Guid, int> topic_refs = null,
            Dictionary<Guid, ExternalNote> notes = null
        ) {
            if (topics is not null) { this.state.domain.topics = topics; }
            if (topic_refs is not null) { this.state.topic_refs = topic_refs; }
            if (notes is not null) { this.state.domain.notes = notes; }

            if (actions is not null) {
                EntryWindow entry_window = new EntryWindow(this.state, actions: actions) { Owner = this };
                entry_window.ShowDialog();
                if (!entry_window.valid) { return; }

                if (entry_window.topics is not null) { this.state.domain.topics = entry_window.topics; }
                if (entry_window.topic_refs is not null) { this.state.topic_refs = entry_window.topic_refs; }
                if (entry_window.notes is not null) { this.state.domain.notes = entry_window.notes; }

                decimal timestamp = entry_window.timestamp_box.calendar_value;
                DateTime created = entry_window.get_created();
                string description = entry_window.description_box.Text;
                int session = (int)(entry_window.session_box.Value);
                Entry ent = new Entry(timestamp, created, description, session, new List<EntryAction>(entry_window.actions), entry_guid);
                this.state_dirty = true;
                int idx = this.state.domain.add_entry(ent);
                this.state.add_references(ent.actions);
                int valid_idx = this.state.domain.valid_entries - 1;
                if (valid_idx < 0) { valid_idx = this.state.domain.entries.Count - 1; }
                this.session_num_box.Content = (this.state.domain.entries[valid_idx].session ?? 0).ToString();
                this.current_timestamp = this.state.domain.entries[valid_idx].timestamp;
                this.current_timestamp_box.Content = this.state.calendar.format_timestamp(this.current_timestamp);
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

            this.refresh_lists();
        }

        private void entries_list_sel_changed(object sender, RoutedEventArgs e) {
            bool entry_selected = this.entries_list.SelectedIndex >= 0;
            this.ent_rem_but.IsEnabled = entry_selected;
            this.ent_view_but.IsEnabled = entry_selected;
        }

        private void add_entry(object sender, RoutedEventArgs e) {
            this.entry_action_callback(new List<EntryAction>());
        }

        private void remove_entry(object sender, RoutedEventArgs e) {
            int row_idx = this.entries_list.SelectedIndex, idx = this.state.domain.entries.Count - row_idx - 1;
            if ((idx < 0) || (idx >= this.state.domain.entries.Count)) { return; }
            this.state_dirty = true;
            this.state.remove_references(state.domain.entries[idx].actions);
            this.state.domain.remove_entry(idx);
            this.entry_rows.RemoveAt(row_idx);
            this.refresh_lists();
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
            this.state.remove_references(ent.actions);
            this.state.add_references(entry_window.actions);
            ent.timestamp = timestamp;
            ent.created = created;
            ent.description = description;
            ent.session = session;
            ent.actions = entry_window.actions;
            idx = this.state.domain.add_entry(ent);
            int valid_idx = this.state.domain.valid_entries - 1;
            if (valid_idx < 0) { valid_idx = this.state.domain.entries.Count - 1; }
            this.session_num_box.Content = (this.state.domain.entries[valid_idx].session ?? 0).ToString();
            this.current_timestamp = this.state.domain.entries[valid_idx].timestamp;
            this.current_timestamp_box.Content = this.state.calendar.format_timestamp(this.current_timestamp);
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
            this.refresh_lists();
        }

        //TODO: ...

        //TODO: remove
        private void do_test(object sender, RoutedEventArgs e) {
            CampaignSave state = new CampaignSave(new Calendar(), new CharacterSheet());
            Guid villain_id = Guid.NewGuid(), houserules_id = Guid.NewGuid();
            state.domain.topics[villain_id] = new Topic("Baddy McVillainface", "This is the BBEG of the whole campaign, and a jerk-face.");
            state.domain.topics[houserules_id] = new Topic("House Rules", "House rules we use for this campaign.");
            state.domain.notes[Guid.NewGuid()] = new ExternalNote(
                "Polymorph preserves base HP, but adjusts total HP based on new Con mod.", DateTime.Now, new HashSet<Guid>() { houserules_id }
            );
            TopicWindow tw = new TopicWindow(
                new Dictionary<Guid, List<EntityRow>>(), state.domain.topics, state, state.domain.state, 0, null, houserules_id
            ) { Owner = this };
            tw.ShowDialog();
#if false
            if (!iw.valid) { return; }
            List<string> action_types = new List<string>();
            foreach (EntryAction action in iw.get_actions()) { action_types.Add(action.GetType().ToString()); }
            MessageBox.Show(String.Join(", ", action_types), "Actions", MessageBoxButton.OK);
#endif
        }
        //TODO: /remove
    }
}
