using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class CampaignState {
        public CharacterDomain characters;
        public InventoryDomain inventories;
        public Dictionary<Guid, Guid> character_inventory;
        public NoteDomain notes;
        public TaskDomain tasks;
        public CalendarEventDomain events;

        public CampaignState() {
            this.characters = new CharacterDomain();
            this.inventories = new InventoryDomain();
            this.character_inventory = new Dictionary<Guid, Guid>();
            this.notes = new NoteDomain();
            this.tasks = new TaskDomain();
            this.events = new CalendarEventDomain();
        }

        public CampaignState copy() {
            return new CampaignState() {
                characters = this.characters.copy(),
                inventories = this.inventories.copy(),
                character_inventory = new Dictionary<Guid, Guid>(this.character_inventory),
                notes = this.notes.copy(),
                tasks = this.tasks.copy(),
                events = this.events.copy(),
            };
        }
    }


    [Serializable]
    public class CampaignDomain {
        public CampaignState state;
        public List<Entry> entries;
        public int valid_entries;
        public Dictionary<Guid, Topic> topics;
        public List<ExternalNote> notes;

        public CampaignDomain() {
            this.state = new CampaignState();
            this.entries = new List<Entry>();
            this.valid_entries = 0;
            this.topics = new Dictionary<Guid, Topic>();
            this.notes = new List<ExternalNote>();
        }

        public CampaignState get_entry_state(int idx) {
            if ((idx < -1) || (idx >= this.valid_entries)) { throw new ArgumentOutOfRangeException(nameof(idx)); }
            CampaignState result = this.state.copy();
            for (int i = this.valid_entries - 1; i > idx; i--) {
                this.entries[i].revert(result);
            }
            return result;
        }

        public int get_timestamp_index(decimal timestamp) {
            // search with maximum real-world timestamp so we get all entries for the given in-game timestamp
            // NOTE: not doing the same with the maximum Guid: we have bigger problems if we have entries at the maximum DateTime
            Entry ent = new Entry(timestamp, DateTime.MaxValue, "");
            int idx = this.entries.BinarySearch(ent);
            if (idx >= 0) { return idx; }
            // ~idx is insertion index, but we want index of last entry before timestamp, so subtract 1
            return ~idx - 1;
        }

        public CampaignState get_timestamp_state(decimal timestamp) => this.get_entry_state(this.get_timestamp_index(timestamp));

        private void revert_state(int idx) {
            for (; this.valid_entries - 1 > idx; this.valid_entries--) {
                this.entries[this.valid_entries - 1].revert(this.state);
            }
        }

        private void advance_state() {
            for (; this.valid_entries < this.entries.Count; this.valid_entries++) {
                try {
                    this.entries[this.valid_entries].rebase(this.state);
                }
                catch (ArgumentException) { break; }
                try {
                    this.entries[this.valid_entries].apply(this.state);
                }
                catch (ArgumentException e) {
                    this.entries[this.valid_entries].revert(this.state, (e.Data["action_index"] as int?) ?? 0 - 1);
                    break;
                }
            }
        }

        public void add_entry(Entry ent) {
            int idx = this.entries.BinarySearch(ent);
            if (idx >= 0) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            idx = ~idx;
            this.revert_state(idx - 1);
            this.entries.Insert(idx, ent);
            if (this.valid_entries < idx) { return; }
            this.advance_state();
        }

        public void remove_entry(int idx, bool invalidate = false) {
            if ((idx < 0) || (idx >= this.entries.Count)) { throw new ArgumentOutOfRangeException(nameof(idx)); }
            this.revert_state(idx - 1);
            this.entries.RemoveAt(idx);
            if (this.valid_entries < idx) { return; }
            if (invalidate) { return; }
            this.advance_state();
        }
    }
}