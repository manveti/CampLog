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
    public partial class NoteWindow : Window {
        public bool valid;
        private CampaignSave state;
        private BaseNote note;
        private List<TopicRow> topic_rows;
        private Dictionary<Guid, Topic> topics_by_guid;
        private Dictionary<string, Guid> topics_by_name;

        private void populate_topic_rows() {
            foreach (Guid guid in this.note.topics) {
                if (!this.state.domain.topics.ContainsKey(guid)) { continue; }
                this.topic_rows.Add(new TopicRow(guid, this.state.domain.topics[guid].name));
            }
            this.topic_rows.Sort((x, y) => x.name.CompareTo(y.name));
        }

        public NoteWindow(CampaignSave state, BaseNote note, decimal timestamp = 0) {
            this.valid = false;
            this.state = state;
            this.note = note.copy();
            this.topic_rows = new List<TopicRow>();
            this.populate_topic_rows();
            this.topics_by_guid = new Dictionary<Guid, Topic>();
            this.topics_by_name = new Dictionary<string, Guid>();
            foreach (Guid guid in state.domain.topics.Keys) {
                this.topics_by_guid[guid] = state.domain.topics[guid];
                this.topics_by_name[state.domain.topics[guid].name] = guid;
            }
            InitializeComponent();
            if (note is Note internal_note) {
                foreach (Entry entry in this.state.domain.entries) {
                    if (entry.guid == internal_note.entry_guid) {
                        timestamp = entry.timestamp;
                        break;
                    }
                }
                this.timestamp_box.Text = state.calendar.format_timestamp(timestamp);
            }
            if (note is ExternalNote external_note) {
                this.timestamp_box.Text = external_note.timestamp.ToString();
            }
            this.contents_box.Text = note.contents;
            this.topic_list.ItemsSource = this.topic_rows;
        }

        private void list_sel_changed(object sender, RoutedEventArgs e) {
            Guid? guid = this.topic_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.note.topics.Contains(guid.Value))) {
                this.rem_but.IsEnabled = false;
            }
            else {
                this.rem_but.IsEnabled = true;
            }
        }

        private void do_add(object sender, RoutedEventArgs e) {
            HashSet<string> topics = new HashSet<string>(this.topics_by_name.Keys);
            foreach (Guid guid in this.note.topics) {
                if (!this.topics_by_guid.ContainsKey(guid)) { continue; }
                if (topics.Contains(this.topics_by_guid[guid].name)) {
                    topics.Remove(this.topics_by_guid[guid].name);
                }
            }
            List<string> sorted_topics = topics.ToList();
            sorted_topics.Sort();
            string topic = SimpleDialog.askList("Add Topic", "Topic:", sorted_topics.ToArray(), canEdit: true, owner: this);
            if (topic is null) { return; }
            Guid topic_guid;
            if (this.topics_by_name.ContainsKey(topic)) {
                topic_guid = this.topics_by_name[topic];
            }
            else {
                topic_guid = Guid.NewGuid();
                this.topics_by_guid[topic_guid] = new Topic(topic);
                this.topics_by_name[topic] = topic_guid;
            }
            this.note.topics.Add(topic_guid);
            this.topic_rows.Add(new TopicRow(topic_guid, topic));
            this.topic_rows.Sort((x, y) => x.name.CompareTo(y.name));
            this.topic_list.Items.Refresh();
        }

        private void do_rem(object sender, RoutedEventArgs e) {
            Guid? guid = this.topic_list.SelectedValue as Guid?;
            if ((guid is null) || (!this.note.topics.Contains(guid.Value))) { return; }
            this.note.topics.Remove(guid.Value);
            for (int i = 0; i < this.topic_rows.Count; i++) {
                if (this.topic_rows[i].guid == guid.Value) {
                    this.topic_rows.RemoveAt(i);
                    break;
                }
            }
            this.topic_list.Items.Refresh();
        }

        private void do_ok(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void do_cancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public BaseNote get_note() {
            if (!this.valid) { return null; }
            return this.note;
        }

        public Dictionary<Guid, Topic> get_topics() {
            if (!this.valid) { return null; }
            return this.topics_by_guid;
        }
    }
}
