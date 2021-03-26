using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ActionNoteCreate : EntryAction {
        public readonly Guid guid;
        public readonly Note note;

        public override string description { get => "Add note"; }

        public ActionNoteCreate(Guid guid, Note note) {
            if (note is null) { throw new ArgumentNullException(nameof(note)); }
            this.guid = guid;
            this.note = note.copy();
        }

        public override void apply(CampaignState state, Entry ent) {
            state.notes.add_note(this.note.copy(), this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.notes.remove_note(this.guid);
            state.notes.notes.Remove(this.guid);
        }
    }


    [Serializable]
    public class ActionNoteRemove : EntryAction {
        public readonly Guid guid;

        public override string description { get => "Remove note"; }

        public ActionNoteRemove(Guid guid) {
            this.guid = guid;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.notes.remove_note(this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.notes.restore_note(this.guid);
        }
    }


    [Serializable]
    public class ActionNoteRestore : EntryAction {
        public readonly Guid guid;

        public override string description { get => "Restore note"; }

        public ActionNoteRestore(Guid guid) {
            this.guid = guid;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.notes.restore_note(this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.notes.remove_note(this.guid);
        }
    }


    [Serializable]
    public class ActionNoteUpdate : EntryAction {
        public readonly Guid guid;
        public readonly string contents_from;
        public readonly string contents_to;
        public readonly HashSet<Guid> remove_topics;
        public readonly HashSet<Guid> add_topics;

        public override string description { get => "Update note"; }

        public ActionNoteUpdate(Guid guid, string contents_from, string contents_to, HashSet<Guid> remove_topics, HashSet<Guid> add_topics) {
            if ((contents_from is null) != (contents_to is null)) { throw new ArgumentNullException(nameof(contents_from)); }
            if ((contents_to is null) && (remove_topics is null) && (add_topics is null)) { throw new ArgumentNullException(nameof(contents_to)); }
            this.guid = guid;
            this.contents_from = contents_from;
            this.contents_to = contents_to;
            this.remove_topics = remove_topics;
            this.add_topics = add_topics;
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.notes.notes.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Note note = state.notes.notes[this.guid];
            if (this.contents_to is not null) { note.contents = this.contents_to; }
            if (this.remove_topics is not null) { note.topics.ExceptWith(this.remove_topics); }
            if (this.add_topics is not null) { note.topics.UnionWith(this.add_topics); }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.notes.notes.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Note note = state.notes.notes[this.guid];
            if (this.contents_from is not null) { note.contents = this.contents_from; }
            if (this.add_topics is not null) { note.topics.ExceptWith(this.add_topics); }
            if (this.remove_topics is not null) { note.topics.UnionWith(this.remove_topics); }
        }
    }
}