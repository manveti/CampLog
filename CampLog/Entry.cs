using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [KnownType(typeof(ActionCharacterSet))]
    [KnownType(typeof(ActionCharacterPropertySet))]
    [KnownType(typeof(ActionCharacterPropertyAdjust))]
    [KnownType(typeof(ActionCharacterSetInventory))]
    [KnownType(typeof(ActionInventoryCreate))]
    [KnownType(typeof(ActionInventoryRemove))]
    [KnownType(typeof(ActionInventoryRename))]
    [KnownType(typeof(ActionInventoryEntryAdd))]
    [KnownType(typeof(ActionInventoryEntryRemove))]
    [KnownType(typeof(ActionItemStackSet))]
    [KnownType(typeof(ActionItemStackAdjust))]
    [KnownType(typeof(ActionSingleItemSet))]
    [KnownType(typeof(ActionSingleItemAdjust))]
    [KnownType(typeof(ActionInventoryEntryMove))]
    [KnownType(typeof(ActionInventoryEntryMerge))]
    [KnownType(typeof(ActionInventoryEntryUnstack))]
    [KnownType(typeof(ActionInventoryEntrySplit))]
    [KnownType(typeof(ActionNoteCreate))]
    [KnownType(typeof(ActionNoteRemove))]
    [KnownType(typeof(ActionNoteRestore))]
    [KnownType(typeof(ActionNoteUpdate))]
    [KnownType(typeof(ActionTaskCreate))]
    [KnownType(typeof(ActionTaskRemove))]
    [KnownType(typeof(ActionTaskRestore))]
    [KnownType(typeof(ActionTaskUpdate))]
    [KnownType(typeof(ActionCalendarEventCreate))]
    [KnownType(typeof(ActionCalendarEventRemove))]
    [KnownType(typeof(ActionCalendarEventRestore))]
    [KnownType(typeof(ActionCalendarEventUpdate))]
    [Serializable]
    public abstract class EntryAction {
        public abstract string description { get; }

        public virtual void rebase(CampaignState state) { }
        public abstract void apply(CampaignState state, Entry ent);
        public abstract void revert(CampaignState state, Entry ent);
        public virtual void merge_to(List<EntryAction> actions) { actions.Add(this); }
    }


    [Serializable]
    public class Entry : IComparable<Entry> {
        public decimal timestamp;
        public DateTime created;
        public string description;
        public int? session;
        public List<EntryAction> actions;
        public Guid guid;

        public Entry(decimal timestamp, DateTime created, string description, int? session = null, List<EntryAction> actions = null, Guid? guid = null) {
            this.timestamp = timestamp;
            this.created = created;
            this.description = description;
            this.session = session;
            if (actions is null) {
                this.actions = new List<EntryAction>();
            }
            else {
                this.actions = actions;
            }
            this.guid = guid ?? Guid.NewGuid();
        }

        public int CompareTo(Entry other) {
            int result;

            if (other is null) { return 1; }

            result = this.timestamp.CompareTo(other.timestamp);
            if (result == 0) {
                result = this.created.CompareTo(other.created);
            }
            if (result == 0) {
                if (this.session is null) { result = -1; }
                else { result = this.session.Value.CompareTo(other.session); }
            }
            if (result == 0) {
                result = this.guid.CompareTo(other.guid);
            }
            return result;
        }

        public void rebase(CampaignState state) {
            foreach (EntryAction action in this.actions) {
                action.rebase(state);
            }
        }

        public void apply(CampaignState state, int start_index = 0) {
            for (int i = start_index; i < this.actions.Count; i++) {
                try {
                    this.actions[i].apply(state, this);
                }
                catch (ArgumentException e) {
                    e.Data["action_index"] = i;
                    throw;
                }
            }
        }

        public void revert(CampaignState state, int start_index = -1) {
            if (start_index < 0) { start_index = this.actions.Count - 1; }
            for (int i = start_index; i >= 0; i--) {
                try {
                    this.actions[i].revert(state, this);
                }
                catch (ArgumentException e) {
                    e.Data["action_index"] = i;
                    throw;
                }
            }
        }
    }
}