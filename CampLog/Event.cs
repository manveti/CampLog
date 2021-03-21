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
    [Serializable]
    public abstract class EventAction {
        public abstract string description { get; }

        public abstract void apply(CampaignState state);
        public abstract void revert(CampaignState state);
    }


    [Serializable]
    public class Event : IComparable<Event> {
        public decimal timestamp;
        public DateTime created;
        public string description;
        public List<EventAction> actions;

        public Event(decimal timestamp, DateTime created, string description, List<EventAction> actions = null) {
            this.timestamp = timestamp;
            this.created = created;
            this.description = description;
            if (actions is null) {
                this.actions = new List<EventAction>();
            }
            else {
                this.actions = actions;
            }
        }

        public int CompareTo(Event other) {
            int result;

            if (other is null) { return 1; }

            result = this.timestamp.CompareTo(other.timestamp);
            if (result == 0) {
                result = this.created.CompareTo(other.created);
            }
            return result;
        }

        public void apply(CampaignState state) {
            foreach (EventAction action in this.actions) {
                action.apply(state);
            }
        }

        public void revert(CampaignState state) {
            for (int i = this.actions.Count - 1; i >= 0; i--) {
                this.actions[i].revert(state);
            }
        }
    }
}