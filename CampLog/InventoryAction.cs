using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ActionInventoryCreate : EventAction {
        public readonly Guid guid;
        public readonly string name;

        public override string description {
            get => string.Format("Create inventory {0}", this.name ?? this.guid.ToString());
        }

        public ActionInventoryCreate(Guid guid, string name) {
            this.guid = guid;
            this.name = name;
        }

        public override void apply(CampaignState state) {
            state.inventories.new_inventory(this.name, this.guid);
        }

        public override void revert(CampaignState state) {
            state.inventories.remove_inventory(this.guid);
        }
    }


    [Serializable]
    public class ActionInventoryRemove : EventAction {
        public readonly Guid guid;
        public readonly string name;

        public override string description {
            get => string.Format("Remove inventory {0}", this.name ?? this.guid.ToString());
        }

        public ActionInventoryRemove(Guid guid, string name) {
            this.guid = guid;
            this.name = name;
        }

        public override void apply(CampaignState state) {
            state.inventories.remove_inventory(this.guid);
        }

        public override void revert(CampaignState state) {
            state.inventories.new_inventory(this.name, this.guid);
        }
    }


    [Serializable]
    public class ActionInventoryRename : EventAction {
        public readonly Guid guid;
        public readonly string from;
        public readonly string to;

        public override string description {
            get {
                if (this.from is null) { return string.Format("Name inventory {0}", this.to); }
                if (this.to is null) { return string.Format("Remove name of inventory {0}", this.from); }
                return string.Format("Rename inventory {0} to {1}", this.from, this.to);
            }
        }

        public ActionInventoryRename(Guid guid, string from, string to) {
            if ((from is null) && (to is null)) { throw new ArgumentNullException(nameof(to)); }
            this.guid = guid;
            this.from = from;
            this.to = to;
        }

        public override void apply(CampaignState state) {
            if (!state.inventories.inventories.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            state.inventories.inventories[this.guid].name = this.to;
        }

        public override void revert(CampaignState state) {
            if (!state.inventories.inventories.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            state.inventories.inventories[this.guid].name = this.from;
        }
    }


   //TODO: item actions
}