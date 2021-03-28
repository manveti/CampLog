using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class BaseNote {
        public string contents;
        public RefcountSet<Guid> topics;

        public BaseNote(string contents, HashSet<Guid> topics = null) {
            this.contents = contents;
            if (topics is null) { this.topics = new RefcountSet<Guid>(); }
            else { this.topics = new RefcountSet<Guid>(topics); }
        }
    }


    [Serializable]
    public class Note : BaseNote {
        public Guid entry_guid;

        public Note(string contents, Guid entry_guid, HashSet<Guid> topics = null) : base(contents, topics) {
            this.entry_guid = entry_guid;
        }

        public Note copy() {
            return new Note(this.contents, this.entry_guid) { topics = new RefcountSet<Guid>(this.topics) };
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

        public NoteDomain copy() {
            NoteDomain result = new NoteDomain() {
                active_items = new HashSet<Guid>(this.active_items)
            };
            foreach (Guid guid in this.items.Keys) {
                result.items[guid] = this.items[guid].copy();
            }
            return result;
        }

        public Guid add_note(Note note, Guid? guid = null) => this.add_item(note, guid);

        public void remove_note(Guid guid) => this.remove_item(guid);

        public void restore_note(Guid guid) => this.restore_item(guid);
    }


    [Serializable]
    public class ExternalNote : BaseNote {
        public DateTime timestamp;

        public ExternalNote(string contents, DateTime timestamp, HashSet<Guid> topics = null) : base(contents, topics) {
            this.timestamp = timestamp;
        }
    }
}