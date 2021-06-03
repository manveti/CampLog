using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CampLog {
    public class TopicRow {
        public Guid _guid;
        public string _name;

        public Guid guid { get => this._guid; }
        public string name { get => this._name; }

        public TopicRow(Guid guid, string name) {
            this._guid = guid;
            this._name = name;
        }
    }


    public partial class TopicListControl : UserControl {
        private ActionCallback change_callback;
        private Guid? entry_guid;
        private Dictionary<Guid, Topic> topics;
        private Dictionary<Guid, int> topic_refs;
        private Dictionary<Guid, ExternalNote> notes;
        private CampaignSave save_state;
        private CampaignState state;
        private decimal now;
        private List<TopicRow> topic_rows;
        private Dictionary<Guid, List<EntityRow>> topic_cache;

        public TopicListControl(ActionCallback change_callback, Guid? entry_guid = null) {
            this.change_callback = change_callback;
            this.entry_guid = entry_guid;
            this.topics = null;
            this.topic_refs = null;
            this.notes = null;
            this.save_state = null;
            this.state = null;
            this.now = 0;
            this.topic_rows = new List<TopicRow>();
            this.topic_cache = new Dictionary<Guid, List<EntityRow>>();
            InitializeComponent();
            this.topic_list.ItemsSource = this.topic_rows;
        }

        private void populate_topic_rows() {
            this.topic_rows.Clear();
            foreach (Guid guid in this.topics.Keys) {
                this.topic_rows.Add(new TopicRow(guid, this.topics[guid].name));
            }
            this.topic_rows.Sort((x, y) => x.name.CompareTo(y.name));
            if ((this.topic_refs.ContainsKey(Guid.Empty)) && (this.topic_refs[Guid.Empty] > 0)) {
                this.topic_rows.Add(new TopicRow(Guid.Empty, "<Uncategorized>"));
            }
            this.topic_list.Items.Refresh();
        }

        public void set_state(
            CampaignSave save_state,
            CampaignState state,
            decimal timestamp,
            Dictionary<Guid, Topic> topics = null,
            Dictionary<Guid, int> topic_refs = null,
            Dictionary<Guid, ExternalNote> notes = null
        ) {
            this.topic_cache.Clear();
            if (topics is not null) { this.topics = topics; }
            else { this.topics = new Dictionary<Guid, Topic>(save_state.domain.topics); }
            if (topic_refs is not null) { this.topic_refs = topic_refs; }
            else { this.topic_refs = new Dictionary<Guid, int>(save_state.topic_refs); }
            if (notes is not null) { this.notes = notes; }
            else { this.notes = new Dictionary<Guid, ExternalNote>(save_state.domain.notes); }
            this.save_state = save_state;
            this.state = state;
            this.now = timestamp;
            this.populate_topic_rows();
            if (this.state is null) {
                this.add_but.IsEnabled = false;
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = false;
                this.note_add_but.IsEnabled = false;
                this.external_note_add_but.IsEnabled = false;
            }
            else {
                this.add_but.IsEnabled = true;
                this.note_add_but.IsEnabled = true;
                this.external_note_add_but.IsEnabled = true;
            }
        }

        private void list_sel_changed(object sender, RoutedEventArgs e) {
            Guid? guid = this.topic_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.topics.ContainsKey(guid.Value))) {
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = (guid == Guid.Empty);
                return;
            }
            // can only remove topic if it's not referenced
            this.rem_but.IsEnabled = ((!this.topic_refs.ContainsKey(guid.Value)) || (this.topic_refs[guid.Value] <= 0));
            this.view_but.IsEnabled = true;
        }

        private void add_topics(Dictionary<Guid, Topic> topics, BaseNote note = null) {
            foreach (Guid guid in topics.Keys) {
                if (!this.topics.ContainsKey(guid)) {
                    this.topics[guid] = topics[guid];
                }
            }
            this.populate_topic_rows();
            if (note is not null) {
                if (note.topics.Count <= 0) {
                    if (!this.topic_refs.ContainsKey(Guid.Empty)) {
                        this.topic_refs[Guid.Empty] = 0;
                    }
                    this.topic_refs[Guid.Empty] += 1;
                }
                foreach (Guid guid in note.topics) {
                    if (!this.topic_refs.ContainsKey(guid)) {
                        this.topic_refs[guid] = 0;
                    }
                    this.topic_refs[guid] += 1;
                }
            }
        }

        private void do_add_view(Guid? topic_guid = null) {
            if ((this.topics is null) || (this.notes is null) || (this.save_state is null) || (this.state is null)) { return; }
            Guid ent_guid = this.entry_guid ?? Guid.NewGuid();
            TopicWindow dialog_window = new TopicWindow(
                this.topic_cache, this.topics, this.save_state, this.state, this.now, ent_guid, topic_guid
            ) { Owner = Window.GetWindow(this) };
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            Dictionary<Guid, Topic> topics = dialog_window.get_topics();
            Dictionary<Guid, int> topic_refs = dialog_window.get_topic_refs();
            Dictionary<Guid, ExternalNote> notes = dialog_window.get_notes();
            List<EntryAction> actions = dialog_window.get_actions();
            if (topics is not null) { this.topics = topics; }
            if (topic_refs is not null) {
                foreach (Guid guid in topic_refs.Keys) {
                    if (!this.topic_refs.ContainsKey(guid)) {
                        this.topic_refs[guid] = 0;
                    }
                    this.topic_refs[guid] += topic_refs[guid];
                }
            }
            if (notes is not null) { this.notes = notes; }
            this.change_callback(actions, ent_guid, this.topics, this.topic_refs, this.notes);
        }

        private void do_add(object sender, RoutedEventArgs e) {
            this.do_add_view();
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            if (this.topics is null) { return; }
            Guid? guid = this.topic_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.topics.ContainsKey(guid.Value))) { return; }
            if ((this.topic_refs.ContainsKey(guid.Value)) && (this.topic_refs[guid.Value] > 0)) { return; }
            this.topics.Remove(guid.Value);
            this.topic_refs.Remove(guid.Value);
            for (int i = 0; i < this.topic_rows.Count; i++) {
                if (this.topic_rows[i].guid == guid.Value) {
                    this.topic_rows.RemoveAt(i);
                    break;
                }
            }
            this.topic_list.Items.Refresh();
        }

        private void do_view(object sender, RoutedEventArgs e) {
            Guid? sel = this.topic_list.SelectedValue as Guid?;
            if ((sel is null) || (!this.topics.ContainsKey(sel.Value))) { return; }
            this.do_add_view(sel);
        }

        private void do_note_add(object sender, RoutedEventArgs e) {
            if (this.save_state is null) { return; }
            Guid ent_guid = this.entry_guid ?? Guid.NewGuid();
            Note note = new Note("", ent_guid);
            Guid? sel = this.topic_list.SelectedValue as Guid?;
            if ((sel is not null) && (sel.Value != Guid.Empty)) {
                note.topics.Add(sel.Value);
            }
            NoteWindow dialog_window = new NoteWindow(this.save_state, note, this.now) { Owner = Window.GetWindow(this) };
            dialog_window.ShowDialog();
            if (this.change_callback is null) { return; }
            Note new_note = dialog_window.get_note() as Note;
            Dictionary<Guid, Topic> topics = dialog_window.get_topics();
            if ((new_note is null) || (topics is null)) { return; }
            this.add_topics(topics, new_note);
            List<EntryAction> actions = new List<EntryAction>() { new ActionNoteCreate(Guid.NewGuid(), new_note) };
            this.change_callback(actions, ent_guid);
        }

        private void do_external_note_add(object sender, RoutedEventArgs e) {
            if ((this.topics is null) || (this.notes is null) || (this.save_state is null)) { return; }
            ExternalNote note = new ExternalNote("", DateTime.Now);
            Guid? sel = this.topic_list.SelectedValue as Guid?;
            if ((sel is not null) && (sel.Value != Guid.Empty)) {
                note.topics.Add(sel.Value);
            }
            NoteWindow dialog_window = new NoteWindow(this.save_state, note) { Owner = Window.GetWindow(this) };
            dialog_window.ShowDialog();
            ExternalNote new_note = dialog_window.get_note() as ExternalNote;
            Dictionary<Guid, Topic> topics = dialog_window.get_topics();
            if ((new_note is null) || (topics is null)) { return; }
            this.add_topics(topics, new_note);
            this.notes[Guid.NewGuid()] = new_note;
            this.topic_cache.Clear();
            this.change_callback(null, topics: this.topics, topic_refs: this.topic_refs, notes: this.notes);
        }
    }
}
