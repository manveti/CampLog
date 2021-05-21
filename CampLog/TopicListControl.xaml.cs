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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private Action state_dirty_callback;
        private Guid? entry_guid;
        private CampaignSave save_state;
        private CampaignState state;
        private decimal now;
        private List<TopicRow> topic_rows;

        public TopicListControl(ActionCallback change_callback, Action state_dirty_callback, Guid? entry_guid = null) {
            this.change_callback = change_callback;
            this.state_dirty_callback = state_dirty_callback;
            this.entry_guid = entry_guid;
            this.save_state = null;
            this.state = null;
            this.now = 0;
            this.topic_rows = new List<TopicRow>();
            InitializeComponent();
            this.topic_list.ItemsSource = this.topic_rows;
        }

        private void populate_topic_rows() {
            this.topic_rows.Clear();
            foreach (Guid guid in this.save_state.domain.topics.Keys) {
                this.topic_rows.Add(new TopicRow(guid, this.save_state.domain.topics[guid].name));
            }
            this.topic_rows.Sort((x, y) => x.name.CompareTo(y.name));
            if ((this.save_state.topic_refs.ContainsKey(Guid.Empty)) && (this.save_state.topic_refs[Guid.Empty] > 0)) {
                this.topic_rows.Add(new TopicRow(Guid.Empty, "<Uncategorized>"));
            }
            this.topic_list.Items.Refresh();
        }

        public void set_state(CampaignSave save_state, CampaignState state, decimal timestamp) {
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
            if ((guid is null) || (!this.save_state.domain.topics.ContainsKey(guid.Value))) {
                this.rem_but.IsEnabled = false;
                this.view_but.IsEnabled = (guid == Guid.Empty);
                return;
            }
            // can only remove topic if it's not referenced
            this.rem_but.IsEnabled = ((!this.save_state.topic_refs.ContainsKey(guid.Value)) || (this.save_state.topic_refs[guid.Value] <= 0));
            this.view_but.IsEnabled = true;
        }

        private void add_topics(Dictionary<Guid, Topic> topics, BaseNote note = null) {
            foreach (Guid guid in topics.Keys) {
                if (!this.save_state.domain.topics.ContainsKey(guid)) {
                    this.state_dirty_callback();
                    this.save_state.domain.topics[guid] = topics[guid];
                }
            }
            this.populate_topic_rows();
            if (note is not null) {
                if (note.topics.Count <= 0) {
                    if (!this.save_state.topic_refs.ContainsKey(Guid.Empty)) {
                        this.save_state.topic_refs[Guid.Empty] = 0;
                    }
                    this.save_state.topic_refs[Guid.Empty] += 1;
                }
                foreach (Guid guid in note.topics) {
                    if (!this.save_state.topic_refs.ContainsKey(guid)) {
                        this.save_state.topic_refs[guid] = 0;
                    }
                    this.save_state.topic_refs[guid] += 1;
                }
            }
        }

        private void do_add(object sender, RoutedEventArgs e) {
            //TODO: add topic
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            if ((this.change_callback is null) || (this.state is null)) { return; }
            Guid? guid = this.topic_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.save_state.domain.topics.ContainsKey(guid.Value))) { return; }
            if ((this.save_state.topic_refs.ContainsKey(guid.Value)) && (this.save_state.topic_refs[guid.Value] > 0)) { return; }
            this.save_state.domain.topics.Remove(guid.Value);
            this.save_state.topic_refs.Remove(guid.Value);
            for (int i = 0; i < this.topic_rows.Count; i++) {
                if (this.topic_rows[i].guid == guid.Value) {
                    this.topic_rows.RemoveAt(i);
                    break;
                }
            }
            this.topic_list.Items.Refresh();
        }

        private void do_view(object sender, RoutedEventArgs e) {
            //TODO: view topic
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
            if (this.save_state is null) { return; }
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
            this.state_dirty_callback();
            this.save_state.domain.notes[Guid.NewGuid()] = new_note;
        }
    }
}
