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
    public class NoteDomain {
        public Dictionary<Guid, Note> notes;
        public HashSet<Guid> active_notes;

        public NoteDomain() {
            this.notes = new Dictionary<Guid, Note>();
            this.active_notes = new HashSet<Guid>();
        }

        public Guid add_note(Note note, Guid? guid = null) {
            if (note is null) { throw new ArgumentNullException(nameof(note)); }
            if ((guid is not null) && (this.notes.ContainsKey(guid.Value))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            if (this.notes.ContainsValue(note)) { throw new ArgumentOutOfRangeException(nameof(note)); }

            Guid note_guid = guid ?? Guid.NewGuid();
            this.notes[note_guid] = note;
            this.active_notes.Add(note_guid);
            return note_guid;
        }

        public void remove_note(Guid guid) {
            if ((!this.notes.ContainsKey(guid)) || (!this.active_notes.Contains(guid))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.active_notes.Remove(guid);
        }

        public void restore_note(Guid guid) {
            if ((!this.notes.ContainsKey(guid)) || (this.active_notes.Contains(guid))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.active_notes.Add(guid);
        }
    }
}