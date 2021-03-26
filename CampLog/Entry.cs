﻿using System;
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
    [Serializable]
    public abstract class EntryAction {
        public abstract string description { get; }

        public abstract void apply(CampaignState state, Entry ent);
        public abstract void revert(CampaignState state, Entry ent);
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

        public void apply(CampaignState state) {
            foreach (EntryAction action in this.actions) {
                action.apply(state, this);
            }
        }

        public void revert(CampaignState state) {
            for (int i = this.actions.Count - 1; i >= 0; i--) {
                this.actions[i].revert(state, this);
            }
        }
    }
}