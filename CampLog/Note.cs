using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class Note {
        public string contents;
        public Guid event_guid;
        public HashSet<Guid> topics;

        public Note(string contents, Guid event_guid, HashSet<Guid> topics = null) {
            this.contents = contents;
            this.event_guid = event_guid;
            if (topics is null) { this.topics = new HashSet<Guid>(); }
            else { this.topics = new HashSet<Guid>(topics); }
        }

        public Note copy() {
            return new Note(this.contents, this.event_guid, this.topics);
        }
    }


    [Serializable]
    public class NoteDomain : BaseDomain<Note> {
        public Dictionary<Guid, Note> notes {
            get => this.items;
            set => this.items = value;
        }
        public HashSet<Guid> active_notes {
            get => this.active_items;
            set => this.active_items = value;
        }

        public Guid add_note(Note note, Guid? guid = null) => this.add_item(note, guid);

        public void remove_note(Guid guid) => this.remove_item(guid);

        public void restore_note(Guid guid) => this.restore_item(guid);
    }
}