using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ActionInventoryCreate : EntryAction {
        public readonly Guid guid;
        public readonly string name;

        public override string description {
            get => string.Format("Create inventory {0}", this.name ?? this.guid.ToString());
        }

        public ActionInventoryCreate(Guid guid, string name) {
            this.guid = guid;
            this.name = name;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.new_inventory(this.name, this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.inventories.remove_inventory(this.guid);
        }
    }


    [Serializable]
    public class ActionInventoryRemove : EntryAction {
        public readonly Guid guid;
        public string name;

        public override string description {
            get => string.Format("Remove inventory {0}", this.name ?? this.guid.ToString());
        }

        public ActionInventoryRemove(Guid guid, string name) {
            this.guid = guid;
            this.name = name;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.remove_inventory(this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.inventories.new_inventory(this.name, this.guid);
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionInventoryCreate ext_inv_create) {
                    if (ext_inv_create.guid == this.guid) {
                        // existing ActionInventoryCreate with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
                if (actions[i] is ActionInventoryRename ext_inv_rename) {
                    if (ext_inv_rename.guid == this.guid) {
                        // existing ActionInventoryRename with this guid; remove it and update our "name" field
                        this.name = ext_inv_rename.from;
                        actions.RemoveAt(i);
                    }
                }
                if (actions[i] is ActionInventoryEntryAdd ext_inv_entry_add) {
                    if (ext_inv_entry_add.inv_guid == this.guid) {
                        // existing ActionInventoryEntryAdd with this guid; remove it
                        actions.RemoveAt(i);
                    }
                }
                if (actions[i] is ActionInventoryEntryRemove ext_inv_entry_rem) {
                    if (ext_inv_entry_rem.inv_guid == this.guid) {
                        // existing ActionInventoryEntryRemove with this guid; remove it
                        actions.RemoveAt(i);
                    }
                }
                //TODO: ActionItemStackSet, ActionItemStackAdjust, ActionSingleItemSet, ActionSingleItemAdjust with guid in inventories[this.guid]
                if (actions[i] is ActionInventoryEntryMove ext_inv_entry_move) {
                    if ((ext_inv_entry_move.from_guid == this.guid) || ((ext_inv_entry_move.to_guid == this.guid))) {
                        // existing ActionInventoryEntryMove from or to this guid; remove it
                        actions.RemoveAt(i);
                    }
                }
                if (actions[i] is ActionInventoryEntryMerge ext_inv_entry_merge) {
                    if (ext_inv_entry_merge.inv_guid == this.guid) {
                        // existing ActionInventoryEntryMerge with this guid; remove it
                        actions.RemoveAt(i);
                    }
                }
                if (actions[i] is ActionInventoryEntryUnstack ext_inv_entry_unstack) {
                    if (ext_inv_entry_unstack.inv_guid == this.guid) {
                        // existing ActionInventoryEntryUnstack with this guid; remove it
                        actions.RemoveAt(i);
                    }
                }
                if (actions[i] is ActionInventoryEntrySplit ext_inv_entry_split) {
                    if (ext_inv_entry_split.inv_guid == this.guid) {
                        // existing ActionInventoryEntrySplit with this guid; remove it
                        actions.RemoveAt(i);
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionInventoryRename : EntryAction {
        public readonly Guid guid;
        public string from;
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

        public override void rebase(CampaignState state) {
            if (!state.inventories.inventories.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            this.from = state.inventories.inventories[this.guid].name;
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.inventories.inventories.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            state.inventories.inventories[this.guid].name = this.to;
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.inventories.inventories.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            state.inventories.inventories[this.guid].name = this.from;
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionInventoryCreate ext_inv_create) {
                    if (ext_inv_create.guid == this.guid) {
                        // existing ActionInventoryCreate with this guid; update its "name" field based on ours and we're done
                        actions[i] = new ActionInventoryCreate(ext_inv_create.guid, this.to);
                        return;
                    }
                }
                if (actions[i] is ActionInventoryRemove ext_inv_remove) {
                    if (ext_inv_remove.guid == this.guid) {
                        // existing ActionInventoryRemove with this guid; we're done
                        return;
                    }
                }
                if (actions[i] is ActionInventoryRename ext_inv_rename) {
                    if (ext_inv_rename.guid == this.guid) {
                        // existing ActionInventoryRename with this guid; update its "to" field based on ours and we're done
                        actions[i] = new ActionInventoryRename(ext_inv_rename.guid, ext_inv_rename.from, this.to);
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionInventoryEntryAdd : EntryAction {
        public readonly Guid inv_guid;
        public readonly int? inv_idx;
        public readonly Guid guid;
        public readonly InventoryEntry entry;

        public override string description {
            get => string.Format("Add inventory entry {0}", this.entry.name);
        }

        public ActionInventoryEntryAdd(Guid inv_guid, int? inv_idx, Guid guid, InventoryEntry entry) {
            if (entry is null){ throw new ArgumentNullException(nameof(entry)); }
            if ((entry is SingleItem si) && (si.containers is not null)) {
                foreach (Inventory inv in si.containers) {
                    if (inv.contents.Count > 0) { throw new ArgumentOutOfRangeException(nameof(entry)); }
                }
            }
            this.inv_guid = inv_guid;
            this.inv_idx = inv_idx;
            this.guid = guid;
            this.entry = entry.copy();
        }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.add_entry(this.inv_guid, this.inv_idx, this.entry, this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.inventories.remove_entry(this.guid, this.inv_guid, this.inv_idx);
            state.inventories.entries.Remove(this.guid);
        }
    }


    [Serializable]
    public class ActionInventoryEntryRemove : EntryAction {
        public readonly Guid inv_guid;
        public readonly int? inv_idx;
        public readonly Guid guid;

        public override string description { get => "Remove inventory entry"; }

        public ActionInventoryEntryRemove(Guid inv_guid, int? inv_idx, Guid guid) {
            this.inv_guid = inv_guid;
            this.inv_idx = inv_idx;
            this.guid = guid;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.remove_entry(this.guid, this.inv_guid, this.inv_idx);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.inventories.restore_entry(this.guid, this.inv_guid, this.inv_idx);
        }
    }


    [Serializable]
    public class ActionItemStackSet : EntryAction {
        public readonly Guid guid;
        public long count_from;
        public long unidentified_from;
        public readonly long count_to;
        public readonly long unidentified_to;

        public override string description { get => "Set inventory item stack counts"; }

        public ActionItemStackSet(Guid guid, long count_from, long unidentified_from, long count_to, long unidentified_to) {
            if (count_from <= 0) { throw new ArgumentOutOfRangeException(nameof(count_from)); }
            if ((unidentified_from < 0) || (unidentified_from > count_from)) { throw new ArgumentOutOfRangeException(nameof(count_from)); }
            if (count_to <= 0) { throw new ArgumentOutOfRangeException(nameof(count_to)); }
            if ((unidentified_to < 0) || (unidentified_to > count_to)) { throw new ArgumentOutOfRangeException(nameof(count_to)); }
            this.guid = guid;
            this.count_from = count_from;
            this.unidentified_from = unidentified_from;
            this.count_to = count_to;
            this.unidentified_to = unidentified_to;
        }

        public override void rebase(CampaignState state) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            ItemStack stack = state.inventories.entries[guid] as ItemStack;
            if (stack is null) { throw new ArgumentOutOfRangeException(); }
            this.count_from = stack.count;
            this.unidentified_from = stack.unidentified;
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            ItemStack stack = state.inventories.entries[guid] as ItemStack;
            if (stack is null) { throw new ArgumentOutOfRangeException(); }
            stack.count = this.count_to;
            stack.unidentified = this.unidentified_to;
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            ItemStack stack = state.inventories.entries[guid] as ItemStack;
            if (stack is null) { throw new ArgumentOutOfRangeException(); }
            stack.count = this.count_from;
            stack.unidentified = this.unidentified_from;
        }
    }


    [Serializable]
    public class ActionItemStackAdjust : EntryAction {
        public readonly Guid guid;
        public readonly long count;
        public readonly long unidentified;

        public override string description { get => "Adjust inventory item stack counts"; }

        public ActionItemStackAdjust(Guid guid, long count, long unidentified) {
            this.guid = guid;
            this.count = count;
            this.unidentified = unidentified;
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            ItemStack stack = state.inventories.entries[guid] as ItemStack;
            if (stack is null) { throw new ArgumentOutOfRangeException(); }
            long count = stack.count + this.count, unidentified = stack.unidentified + this.unidentified;
            if ((count <= 0) || (unidentified < 0) || (unidentified > count)) { throw new ArgumentOutOfRangeException(); }
            stack.count = count;
            stack.unidentified = unidentified;
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            ItemStack stack = state.inventories.entries[guid] as ItemStack;
            if (stack is null) { throw new ArgumentOutOfRangeException(); }
            long count = stack.count - this.count, unidentified = stack.unidentified - this.unidentified;
            if ((count <= 0) || (unidentified < 0) || (unidentified > count)) { throw new ArgumentOutOfRangeException(); }
            stack.count = count;
            stack.unidentified = unidentified;
        }
    }


    [Serializable]
    public class ActionSingleItemSet : EntryAction {
        public readonly Guid guid;
        public bool? unidentified_from;
        public decimal? value_override_from;
        public Dictionary<string, string> properties_from;
        public readonly bool? unidentified_to;
        public readonly decimal? value_override_to;
        public readonly Dictionary<string, string> properties_to;
        public readonly bool set_value_override;

        public override string description { get => "Set inventory item properties"; }

        public ActionSingleItemSet(
            Guid guid,
            bool? unidentified_from,
            decimal? value_override_from,
            Dictionary<string, string> properties_from,
            bool? unidentified_to,
            decimal? value_override_to,
            Dictionary<string, string> properties_to,
            bool set_value_override
        ) {
            if ((unidentified_from is null) != (unidentified_to is null)) { throw new ArgumentNullException(nameof(unidentified_from)); }
            if ((properties_from is null) != (properties_to is null)) { throw new ArgumentNullException(nameof(properties_from)); }
            this.guid = guid;
            this.unidentified_from = unidentified_from;
            this.value_override_from = value_override_from;
            if (properties_from is null) {
                this.properties_from = null;
            }
            else {
                this.properties_from = new Dictionary<string, string>(properties_from);
            }
            this.unidentified_to = unidentified_to;
            this.value_override_to = value_override_to;
            if (properties_to is null) {
                this.properties_to = null;
            }
            else {
                this.properties_to = new Dictionary<string, string>(properties_to);
            }
            this.set_value_override = set_value_override;
        }

        public override void rebase(CampaignState state) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            SingleItem itm = state.inventories.entries[guid] as SingleItem;
            if (itm is null) { throw new ArgumentOutOfRangeException(); }
            if (this.unidentified_from is not null) { this.unidentified_from = itm.unidentified; }
            if (this.set_value_override) { this.value_override_from = itm.value_override; }
            if (this.properties_from is not null) {
                this.properties_from.Clear();
                foreach (string key in itm.properties.Keys) { this.properties_from[key] = itm.properties[key]; }
            }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            SingleItem itm = state.inventories.entries[guid] as SingleItem;
            if (itm is null) { throw new ArgumentOutOfRangeException(); }
            if (this.unidentified_to is not null) { itm.unidentified = this.unidentified_to.Value; }
            if (this.set_value_override) { itm.value_override = this.value_override_to; }
            if (this.properties_to is not null) {
                itm.properties.Clear();
                foreach (string key in this.properties_to.Keys) { itm.properties[key] = this.properties_to[key]; }
            }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            SingleItem itm = state.inventories.entries[guid] as SingleItem;
            if (itm is null) { throw new ArgumentOutOfRangeException(); }
            if (this.unidentified_from is not null) { itm.unidentified = this.unidentified_from.Value; }
            if (this.set_value_override) { itm.value_override = this.value_override_from; }
            if (this.properties_from is not null) {
                itm.properties.Clear();
                foreach (string key in this.properties_from.Keys) { itm.properties[key] = this.properties_from[key]; }
            }
        }
    }


    [Serializable]
    public class ActionSingleItemAdjust : EntryAction {
        public readonly Guid guid;
        public readonly decimal? value_override;
        public readonly Dictionary<string, string> properties_subtract;
        public readonly Dictionary<string, string> properties_add;

        public override string description { get => "Adjust inventory item properties"; }

        public ActionSingleItemAdjust(Guid guid, decimal? value_override, Dictionary<string, string> properties_subtract, Dictionary<string, string> properties_add) {
            this.guid = guid;
            this.value_override = value_override;
            if (properties_subtract is null) {
                this.properties_subtract = null;
            }
            else {
                this.properties_subtract = new Dictionary<string, string>();
                foreach (string key in properties_subtract.Keys) { this.properties_subtract[key] = properties_subtract[key]; }
            }
            if (properties_add is null) {
                this.properties_add = null;
            }
            else {
                this.properties_add = new Dictionary<string, string>();
                foreach (string key in properties_add.Keys) { this.properties_add[key] = properties_add[key]; }
            }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            SingleItem itm = state.inventories.entries[guid] as SingleItem;
            if (itm is null) { throw new ArgumentOutOfRangeException(); }
            if (this.value_override is not null) {
                if (itm.value_override is null) { throw new ArgumentOutOfRangeException(); }
                itm.value_override += this.value_override;
            }
            if (this.properties_subtract is not null) {
                foreach (string key in this.properties_subtract.Keys) { itm.properties.Remove(key); }
            }
            if (this.properties_add is not null) {
                foreach (string key in this.properties_add.Keys) { itm.properties[key] = this.properties_add[key]; }
            }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.inventories.entries.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            SingleItem itm = state.inventories.entries[guid] as SingleItem;
            if (itm is null) { throw new ArgumentOutOfRangeException(); }
            if (this.value_override is not null) {
                if (itm.value_override is null) { throw new ArgumentOutOfRangeException(); }
                itm.value_override -= this.value_override;
            }
            if (this.properties_add is not null) {
                foreach (string key in this.properties_add.Keys) { itm.properties.Remove(key); }
            }
            if (this.properties_subtract is not null) {
                foreach (string key in this.properties_subtract.Keys) { itm.properties[key] = this.properties_subtract[key]; }
            }
        }
    }


    [Serializable]
    public class ActionInventoryEntryMove : EntryAction {
        public readonly Guid guid;
        public readonly Guid from_guid;
        public readonly int? from_idx;
        public readonly Guid to_guid;
        public readonly int? to_idx;

        public override string description { get => "Move inventory entry"; }

        public ActionInventoryEntryMove(Guid guid, Guid from_guid, int? from_idx, Guid to_guid, int? to_idx) {
            this.guid = guid;
            this.from_guid = from_guid;
            this.from_idx = from_idx;
            this.to_guid = to_guid;
            this.to_idx = to_idx;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.move_entry(this.guid, this.from_guid, this.from_idx, this.to_guid, this.to_idx);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.inventories.move_entry(this.guid, this.to_guid, this.to_idx, this.from_guid, this.from_idx);
        }
    }


    [Serializable]
    public class ActionInventoryEntryMerge : EntryAction {
        public readonly Guid inv_guid;
        public readonly int? inv_idx;
        public readonly Guid ent1;
        public readonly Guid ent2;
        public readonly Guid guid;

        public ActionInventoryEntryMerge(Guid inv_guid, int? inv_idx, Guid ent1, Guid ent2, Guid guid) {
            if (ent1 == ent2) { throw new ArgumentOutOfRangeException(nameof(ent2)); }
            this.inv_guid = inv_guid;
            this.inv_idx = inv_idx;
            this.ent1 = ent1;
            this.ent2 = ent2;
            this.guid = guid;
        }

        public override string description { get => "Merge inventory entries"; }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.merge_entries(this.inv_guid, this.inv_idx, this.ent1, this.ent2, this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            if ((!state.inventories.entries.ContainsKey(this.ent1)) || (!state.inventories.entries.ContainsKey(this.ent2))) {
                throw new ArgumentOutOfRangeException();
            }
            if (this.guid == this.ent1) {
                // ent2 was merged into stack ent1; restore ent2 and subtract it from ent1
                state.inventories.restore_entry(this.ent2, this.inv_guid, this.inv_idx);
                ItemStack stack1 = state.inventories.entries[this.ent1] as ItemStack;
                if (stack1 is null) { throw new ArgumentOutOfRangeException(); }
                if (state.inventories.entries[this.ent2] is ItemStack stack2) {
                    // ent2 was a stack; just subtract its counts
                    stack1.count -= stack2.count;
                    stack1.unidentified -= stack2.unidentified;
                }
                else if (state.inventories.entries[this.ent2] is SingleItem item2) {
                    // ent2 was an item; just subtract one
                    stack1.count -= 1;
                    if (item2.unidentified) {
                        stack1.unidentified -= 1;
                    }
                }
                else { throw new ArgumentOutOfRangeException(); }
            }
            else if (this.guid == this.ent2) {
                // item ent1 was merged into stack ent2; restore ent1 and subtract it from ent2
                state.inventories.restore_entry(this.ent1, this.inv_guid, this.inv_idx);
                ItemStack stack2 = state.inventories.entries[this.ent2] as ItemStack;
                if (stack2 is null) { throw new ArgumentOutOfRangeException(); }
                SingleItem item1 = state.inventories.entries[this.ent1] as SingleItem;
                if (item1 is null) { throw new ArgumentOutOfRangeException(); }
                stack2.count -= 1;
                if (item1.unidentified) {
                    stack2.unidentified -= 1;
                }
            }
            else {
                // two items were merged; purge merged stack and restore items
                state.inventories.remove_entry(this.guid, this.inv_guid, this.inv_idx);
                state.inventories.entries.Remove(this.guid);
                state.inventories.restore_entry(this.ent1, this.inv_guid, this.inv_idx);
                state.inventories.restore_entry(this.ent2, this.inv_guid, this.inv_idx);
            }
        }
    }


    [Serializable]
    public class ActionInventoryEntryUnstack : EntryAction {
        public readonly Guid inv_guid;
        public readonly int? inv_idx;
        public readonly Guid ent;
        public readonly Guid guid;

        public ActionInventoryEntryUnstack(Guid inv_guid, int? inv_idx, Guid ent, Guid guid) {
            if (ent == guid) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            this.inv_guid = inv_guid;
            this.inv_idx = inv_idx;
            this.ent = ent;
            this.guid = guid;
        }

        public override string description { get => "Unstack inventory stack"; }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.unstack_entry(this.inv_guid, this.inv_idx, this.ent, this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.inventories.remove_entry(this.guid, this.inv_guid, this.inv_idx);
            state.inventories.entries.Remove(this.guid);
            state.inventories.restore_entry(this.ent, this.inv_guid, this.inv_idx);
        }
    }


    [Serializable]
    public class ActionInventoryEntrySplit : EntryAction {
        public readonly Guid inv_guid;
        public readonly int? inv_idx;
        public readonly Guid ent;
        public readonly long count;
        public readonly long unidentified;
        public readonly Guid guid;

        public ActionInventoryEntrySplit(Guid inv_guid, int? inv_idx, Guid ent, long count, long unidentified, Guid guid) {
            if (ent == guid) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            if (count <= 0) { throw new ArgumentOutOfRangeException(nameof(count)); }
            if ((unidentified < 0) || (unidentified > count)) { throw new ArgumentOutOfRangeException(nameof(unidentified)); }
            this.inv_guid = inv_guid;
            this.inv_idx = inv_idx;
            this.ent = ent;
            this.count = count;
            this.unidentified = unidentified;
            this.guid = guid;
        }

        public override string description { get => "Split inventory stack"; }

        public override void apply(CampaignState state, Entry ent) {
            state.inventories.split_entry(this.inv_guid, this.inv_idx, this.ent, this.count, this.unidentified, this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.inventories.merge_entries(this.inv_guid, this.inv_idx, this.ent, this.guid, this.ent);
            state.inventories.entries.Remove(this.guid);
        }
    }
}