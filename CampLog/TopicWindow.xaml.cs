using System;
using System.Collections.Generic;
using System.Windows;

namespace CampLog {
    public enum EntityType {
        ITEM,
        EVENT,
        TASK,
        NOTE,
        EXTERNAL_NOTE,
    }


    public class EntityRow : IComparable {
        public string _contents;
        public List<EntityRow> _children;
        public bool _is_expanded = true;
        public bool _is_selected = false;
        public EntityType? type;
        public Guid? guid;
        public string timestamp;
        public decimal? sort_order;
        public List<TopicRow> topics;

        public EntityRow self { get => this; }
        public string contents { get => this._contents; }
        public List<EntityRow> children { get => this._children; }
        public bool is_expanded { get => this._is_expanded; set => this._is_expanded = value; }
        public bool is_selected { get => this._is_selected; set => this._is_selected = value; }

        public EntityRow(
            string contents,
            EntityType? type = null,
            Guid? guid = null,
            string timestamp = null,
            decimal? sort_order = null,
            List<TopicRow> topics = null
        ) {
            this._contents = contents;
            this._children = new List<EntityRow>();
            this.type = type;
            this.guid = guid;
            this.timestamp = timestamp;
            this.sort_order = sort_order;
            this.topics = topics;
        }

        public int CompareTo(object obj) {
            if (obj is null) { return 1; }
            EntityRow other = obj as EntityRow;
            if (other is null) { return 1; }
            int result = 0;
            if (this.sort_order is null) {
                if (other.sort_order is not null) { result = -1; }
            }
            else if (other.sort_order is null) { result = 1; }
            else { result = this.sort_order.Value.CompareTo(other.sort_order.Value); }
            if (result != 0) { return result; }
            result = this._contents.CompareTo(other._contents);
            if (result != 0) { return result; }
            if (this.guid is null) { return -1; }
            if (other.guid is null) { return -1; }
            return this.guid.Value.CompareTo(other.guid.Value);
        }
    }


    public partial class TopicWindow : Window {
        public bool valid;
        private Dictionary<Guid, List<EntityRow>> topic_cache;
        private Dictionary<Guid, Topic> topics;
        private CampaignSave save_state;
        private CampaignState state;
        private decimal now;
        private Guid? entry_guid;
        private Guid guid;
        private List<EntryAction> actions;
        private Dictionary<Guid, ExternalNote> notes;
        private Dictionary<Guid, int> topic_refs;
        private Stack<Guid> history;

        private void _flush_type_cache(Dictionary<Guid, EntityRow> type_cache) {
            foreach (Guid guid in type_cache.Keys) {
                if (!this.topic_cache.ContainsKey(guid)) {
                    this.topic_cache[guid] = new List<EntityRow>();
                }
                type_cache[guid]._children.Sort();
                this.topic_cache[guid].Add(type_cache[guid]);
            }
            type_cache.Clear();
        }

        private void populate_topic_cache() {
            Dictionary<Guid, EntityRow> type_cache = new Dictionary<Guid, EntityRow>();
            //TODO: items, calendar events, tasks
            type_cache.Clear();
            foreach (Guid guid in this.state.notes.active_notes) {
                HashSet<Guid> topics = new HashSet<Guid>(this.state.notes.notes[guid].topics);
                List<TopicRow> topic_rows = new List<TopicRow>();
                foreach (Guid topic_guid in topics) {
                    if (this.topics.ContainsKey(topic_guid)) {
                        topic_rows.Add(new TopicRow(topic_guid, this.topics[topic_guid].name));
                    }
                }
                topic_rows.Sort((x, y) => x.name.CompareTo(y.name));
                if (topics.Count <= 0) { topics.Add(Guid.Empty); }
                Entry note_entry = null;
                foreach (Entry ent in this.save_state.domain.entries) {
                    if (ent.guid == this.state.notes.notes[guid].entry_guid) {
                        note_entry = ent;
                        break;
                    }
                }
                EntityRow row = new EntityRow(
                    this.state.notes.notes[guid].contents,
                    EntityType.NOTE,
                    guid,
                    (note_entry is null ? null : this.save_state.calendar.format_timestamp(note_entry.timestamp)),
                    note_entry?.timestamp,
                    topic_rows
                );
                foreach (Guid topic_guid in topics) {
                    if (!type_cache.ContainsKey(topic_guid)) {
                        type_cache[topic_guid] = new EntityRow("Notes");
                    }
                    type_cache[topic_guid]._children.Add(row);
                }
            }
            this._flush_type_cache(type_cache);
            foreach (Guid guid in this.notes.Keys) {
                HashSet<Guid> topics = new HashSet<Guid>(this.notes[guid].topics);
                List<TopicRow> topic_rows = new List<TopicRow>();
                foreach (Guid topic_guid in topics) {
                    if (this.topics.ContainsKey(topic_guid)) {
                        topic_rows.Add(new TopicRow(topic_guid, this.topics[topic_guid].name));
                    }
                }
                topic_rows.Sort((x, y) => x.name.CompareTo(y.name));
                if (topics.Count <= 0) { topics.Add(Guid.Empty); }
                EntityRow row = new EntityRow(
                    this.notes[guid].contents,
                    EntityType.EXTERNAL_NOTE,
                    guid,
                    this.notes[guid].timestamp.ToString(),
                    this.notes[guid].timestamp.Ticks,
                    topic_rows
                );
                foreach (Guid topic_guid in topics) {
                    if (!type_cache.ContainsKey(topic_guid)) {
                        type_cache[topic_guid] = new EntityRow("External Notes");
                    }
                    type_cache[topic_guid]._children.Add(row);
                }
            }
            this._flush_type_cache(type_cache);
        }

        public TopicWindow(
            Dictionary<Guid, List<EntityRow>> topic_cache,
            Dictionary<Guid, Topic> topics,
            CampaignSave save_state,
            CampaignState state,
            decimal now,
            Guid? entry_guid = null,
            Guid? guid = null
        ) {
            this.valid = false;
            this.topic_cache = topic_cache;
            this.topics = new Dictionary<Guid, Topic>(topics);
            this.save_state = save_state;
            this.state = state.copy();
            this.now = now;
            this.entry_guid = entry_guid;
            this.actions = new List<EntryAction>();
            this.notes = new Dictionary<Guid, ExternalNote>(save_state.domain.notes);
            this.topic_refs = new Dictionary<Guid, int>();
            this.history = new Stack<Guid>();
            if (guid is null) {
                if (entry_guid is null) { throw new ArgumentNullException(nameof(entry_guid)); }
                this.guid = Guid.NewGuid();
                this.topics[this.guid] = new Topic("");
            }
            else {
                this.guid = guid.Value;
            }
            if (topic_cache.Count <= 0) {
                this.populate_topic_cache();
            }
            InitializeComponent();
            this.set_topic(this.guid);
        }

        private void set_topic(Guid guid) {
            Topic topic = this.topics[guid];
            this.name_box.Text = topic.name;
            this.description_box.Text = topic.description;
            if (!this.topic_cache.ContainsKey(guid)) {
                this.topic_cache[guid] = new List<EntityRow>();
            }
            this.entity_list.ItemsSource = this.topic_cache[guid];
        }

        private void entity_list_sel_changed(object sender, RoutedEventArgs e) {
            EntityRow sel = this.entity_list.SelectedValue as EntityRow;
            if ((sel is null) || (sel.type is null)) {
                this.note_rem_but.Visibility = Visibility.Collapsed;
                this.entity_view_but.IsEnabled = false;
                this.sel_timestamp_box.Text = "";
                this.sel_contents_box.Text = "";
                this.sel_topic_list.ItemsSource = null;
                return;
            }

            if ((sel.type.Value == EntityType.NOTE) || (sel.type.Value == EntityType.EXTERNAL_NOTE)) {
                this.note_rem_but.Visibility = Visibility.Visible;
            }
            else {
                this.note_rem_but.Visibility = Visibility.Collapsed;
            }
            this.entity_view_but.IsEnabled = true;
            this.sel_timestamp_box.Text = sel.timestamp ?? "";
            this.sel_contents_box.Text = sel.contents;
            this.sel_topic_list.ItemsSource = sel.topics;
        }

        private void update_topic_refs(BaseNote note, int refcount = 1) {
            if (note is null) { return; }
            if (note.topics.Count <= 0) {
                if (!this.topic_refs.ContainsKey(Guid.Empty)) {
                    this.topic_refs[Guid.Empty] = 0;
                }
                this.topic_refs[Guid.Empty] += refcount;
            }
            foreach (Guid guid in note.topics) {
                if (!this.topic_refs.ContainsKey(guid)) {
                    this.topic_refs[guid] = 0;
                }
                this.topic_refs[guid] += refcount;
            }
        }

        private void add_topics(Dictionary<Guid, Topic> topics, BaseNote note = null) {
            foreach (Guid guid in topics.Keys) {
                if (!this.topics.ContainsKey(guid)) {
                    this.topics[guid] = topics[guid];
                }
            }
            this.update_topic_refs(note);
        }

        private void update_selected_topic() {
            if (!this.topics.ContainsKey(this.guid)) { return; }
            this.topics[this.guid].name = this.name_box.Text;
            this.topics[this.guid].description = this.description_box.Text;
        }

        private void do_note_add(object sender, RoutedEventArgs e) {
            this.update_selected_topic();
            Guid ent_guid = this.entry_guid ?? Guid.NewGuid();
            Note note = new Note("", ent_guid);
            note.topics.Add(this.guid);
            NoteWindow dialog_window = new NoteWindow(this.save_state, note, this.now, this.topics) { Owner = this };
            dialog_window.ShowDialog();
            Note new_note = dialog_window.get_note() as Note;
            Dictionary<Guid, Topic> topics = dialog_window.get_topics();
            if ((new_note is null) || (topics is null)) { return; }
            this.add_topics(topics, new_note);
            Guid guid = this.state.notes.add_note(new_note);
            this.actions.Add(new ActionNoteCreate(guid, new_note));
            this.topic_cache.Clear();
            this.populate_topic_cache();
            this.set_topic(this.guid);
        }

        private void do_ext_add(object sender, RoutedEventArgs e) {
            this.update_selected_topic();
            ExternalNote note = new ExternalNote("", DateTime.Now);
            note.topics.Add(this.guid);
            NoteWindow dialog_window = new NoteWindow(this.save_state, note, topics: this.topics) { Owner = this };
            dialog_window.ShowDialog();
            ExternalNote new_note = dialog_window.get_note() as ExternalNote;
            Dictionary<Guid, Topic> topics = dialog_window.get_topics();
            if ((new_note is null) || (topics is null)) { return; }
            this.add_topics(topics, new_note);
            this.notes[Guid.NewGuid()] = new_note;
            this.topic_cache.Clear();
            this.populate_topic_cache();
            this.set_topic(this.guid);
        }

        private void do_note_rem(object sender, RoutedEventArgs e) {
            EntityRow sel = this.entity_list.SelectedValue as EntityRow;
            if ((sel is null) || (sel.type is null) || (sel.guid is null)) { return; }
            BaseNote note = null;
            if (sel.type == EntityType.NOTE) {
                if (!this.state.notes.notes.ContainsKey(sel.guid.Value)) { return; }
                note = this.state.notes.notes[sel.guid.Value];
                this.state.notes.remove_note(sel.guid.Value);
                this.actions.Add(new ActionNoteRemove(sel.guid.Value));
            }
            else if (sel.type == EntityType.EXTERNAL_NOTE) {
                if (!this.notes.ContainsKey(sel.guid.Value)) { return; }
                note = this.notes[sel.guid.Value];
                this.notes.Remove(sel.guid.Value);
            }
            else { return; }
            this.update_topic_refs(note, refcount: -1);
            this.topic_cache.Clear();
            this.populate_topic_cache();
            this.set_topic(this.guid);
        }

        private void do_entity_view(object sender, RoutedEventArgs e) {
            EntityRow sel = this.entity_list.SelectedValue as EntityRow;
            if ((sel is null) || (sel.type is null) || (sel.guid is null)) { return; }

            switch (sel.type.Value) {
            //TODO: view selection (ITEM, EVENT, TASK)
            case EntityType.NOTE:
                {
                    if (!this.state.notes.notes.ContainsKey(sel.guid.Value)) { return; }
                    Note note = this.state.notes.notes[sel.guid.Value];
                    NoteWindow dialog_window = new NoteWindow(this.save_state, note, this.now, this.topics) { Owner = this };
                    dialog_window.ShowDialog();
                    Note new_note = dialog_window.get_note() as Note;
                    if (new_note is null) { return; }
                    Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>();
                    foreach (Guid topic_guid in note.topics) {
                        if (!new_note.topics.Contains(topic_guid)) {
                            adjust_topics[topic_guid] = -1;
                        }
                    }
                    foreach (Guid topic_guid in new_note.topics) {
                        if (!note.topics.Contains(topic_guid)) {
                            adjust_topics[topic_guid] = 1;
                        }
                    }
                    if ((new_note.contents != note.contents) || (adjust_topics.Count > 0)) {
                        string contents_from = null, contents_to = null;
                        if (new_note.contents != note.contents) {
                            contents_from = note.contents;
                            contents_to = new_note.contents;
                            note.contents = contents_to;
                        }
                        if (adjust_topics.Count > 0) {
                            foreach (Guid topic_guid in adjust_topics.Keys) {
                                if (adjust_topics[topic_guid] > 0) {
                                    note.topics.Add(topic_guid);
                                }
                                else {
                                    note.topics.Remove(topic_guid);
                                }
                            }
                        }
                        else {
                            adjust_topics = null;
                        }
                        this.actions.Add(new ActionNoteUpdate(sel.guid.Value, contents_from, contents_to, adjust_topics));
                        sel._contents = contents_to;
                        if (adjust_topics.Count > 0) {
                            sel.topics.Clear();
                            foreach (Guid topic_guid in note.topics) {
                                if (this.topics.ContainsKey(topic_guid)) {
                                    sel.topics.Add(new TopicRow(topic_guid, this.topics[topic_guid].name));
                                }
                            }
                            sel.topics.Sort((x, y) => x.name.CompareTo(y.name));
                        }
                        this.entity_list.Items.Refresh();
                    }
                }
                break;
            case EntityType.EXTERNAL_NOTE:
                {
                    if (!this.notes.ContainsKey(sel.guid.Value)) { return; }
                    ExternalNote note = this.notes[sel.guid.Value];
                    NoteWindow dialog_window = new NoteWindow(this.save_state, note, topics: this.topics) { Owner = this };
                    dialog_window.ShowDialog();
                    ExternalNote new_note = dialog_window.get_note() as ExternalNote;
                    if (new_note is null) { return; }
                    Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>();
                    foreach (Guid topic_guid in note.topics) {
                        if (!new_note.topics.Contains(topic_guid)) {
                            adjust_topics[topic_guid] = -1;
                        }
                    }
                    foreach (Guid topic_guid in new_note.topics) {
                        if (!note.topics.Contains(topic_guid)) {
                            adjust_topics[topic_guid] = 1;
                        }
                    }
                    if ((new_note.contents != note.contents) || (adjust_topics.Count > 0)) {
                        note.contents = new_note.contents;
                        sel._contents = note.contents;
                        if (adjust_topics.Count > 0) {
                            foreach (Guid topic_guid in adjust_topics.Keys) {
                                if (adjust_topics[topic_guid] > 0) {
                                    note.topics.Add(topic_guid);
                                }
                                else {
                                    note.topics.Remove(topic_guid);
                                }
                            }
                            sel.topics.Clear();
                            foreach (Guid topic_guid in note.topics) {
                                if (this.topics.ContainsKey(topic_guid)) {
                                    sel.topics.Add(new TopicRow(topic_guid, this.topics[topic_guid].name));
                                }
                            }
                            sel.topics.Sort((x, y) => x.name.CompareTo(y.name));
                        }
                        this.entity_list.Items.Refresh();
                    }
                }
                break;
            default: return;
            }
        }

        private void sel_topic_list_sel_changed(object sender, RoutedEventArgs e) {
            EntityRow sel = this.entity_list.SelectedValue as EntityRow;
            Guid? guid = this.sel_topic_list.SelectedValue as Guid?;
            if ((sel is null) || (sel.type is null) || (sel.guid is null) || (sel.topics is null) || (guid is null)) {
                this.topic_view_but.IsEnabled = false;
                return;
            }
            this.topic_view_but.IsEnabled = (guid.Value != this.guid);
        }

        private void do_topic_view(object sender, RoutedEventArgs e) {
            if (!this.topics.ContainsKey(this.guid)) { return; }
            Guid? guid = this.sel_topic_list.SelectedValue as Guid?;
            if ((guid is null) || (guid.Value == this.guid)) { return; }
            this.update_selected_topic();
            this.history.Push(this.guid);
            this.guid = guid.Value;
            this.back_but.IsEnabled = true;
            this.set_topic(this.guid);
        }

        private void do_back(object sender, RoutedEventArgs e) {
            if ((!this.topics.ContainsKey(this.guid)) || (this.history.Count <= 0)) { return; }
            this.update_selected_topic();
            this.guid = this.history.Pop();
            this.back_but.IsEnabled = (this.history.Count > 0);
            this.set_topic(this.guid);
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.update_selected_topic();
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public List<EntryAction> get_actions() {
            if (!this.valid) { return null; }
            return this.actions;
        }

        public Dictionary<Guid, Topic> get_topics() {
            if (!this.valid) { return null; }
            return this.topics;
        }

        public Dictionary<Guid, int> get_topic_refs() {
            if (!this.valid) { return null; }
            return this.topic_refs;
        }

        public Dictionary<Guid, ExternalNote> get_notes() {
            if (!this.valid) { return null; }
            return this.notes;
        }
    }
}
