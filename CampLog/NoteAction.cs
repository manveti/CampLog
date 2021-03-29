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
        public string contents_from;
        public readonly string contents_to;
        public Dictionary<Guid, int> adjust_topics;

        public override string description { get => "Update note"; }

        public ActionNoteUpdate(Guid guid, string contents_from, string contents_to, Dictionary<Guid, int> adjust_topics) {
            if ((contents_from is null) != (contents_to is null)) { throw new ArgumentNullException(nameof(contents_from)); }
            if ((contents_to is null) && (adjust_topics is null)) { throw new ArgumentNullException(nameof(contents_to)); }
            if (adjust_topics is not null) {
                foreach (Guid topic in adjust_topics.Keys) {
                    if ((adjust_topics[topic] == 0) || (adjust_topics[topic] > 1)) {
                        throw new ArgumentOutOfRangeException(nameof(adjust_topics));
                    }
                }
            }
            this.guid = guid;
            this.contents_from = contents_from;
            this.contents_to = contents_to;
            this.adjust_topics = adjust_topics;
        }

        public override void rebase(CampaignState state) {
            if (!state.notes.notes.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Note note = state.notes.notes[this.guid];
            if (this.contents_from is not null) { this.contents_from = note.contents; }
            if (this.adjust_topics is not null) {
                foreach (Guid topic in this.adjust_topics.Keys) {
                    if (this.adjust_topics[topic] < 0) {
                        if (note.topics.Contains(topic)) {
                            this.adjust_topics[topic] = -note.topics.contents[topic];
                        }
                    }
                }
            }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.notes.notes.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Note note = state.notes.notes[this.guid];
            if (this.contents_to is not null) { note.contents = this.contents_to; }
            if (this.adjust_topics is not null) {
                foreach (Guid topic in this.adjust_topics.Keys) {
                    if (this.adjust_topics[topic] < 0) {
                        note.topics.Remove(topic);
                    }
                    else {
                        note.topics.Add(topic);
                    }
                }
            }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.notes.notes.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Note note = state.notes.notes[this.guid];
            if (this.contents_from is not null) { note.contents = this.contents_from; }
            if (this.adjust_topics is not null) {
                foreach (Guid topic in this.adjust_topics.Keys) {
                    if (this.adjust_topics[topic] < 0) {
                        note.topics.Add(topic, -this.adjust_topics[topic]);
                    }
                    else {
                        note.topics.RemoveRef(topic);
                    }
                }
            }
        }
    }
}