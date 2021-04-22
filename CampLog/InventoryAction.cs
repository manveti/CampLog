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
                if (actions[i] is ActionCharacterSetInventory ext_set_inv) {
                    if (ext_set_inv.to == this.guid) {
                        // existing ActionCharacterSetInventory with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
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
                        continue;
                    }
                }
                if (actions[i] is ActionInventoryEntryAdd ext_inv_entry_add) {
                    if (ext_inv_entry_add.inv_guid == this.guid) {
                        // existing ActionInventoryEntryAdd with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionInventoryEntryRemove ext_inv_entry_rem) {
                    if (ext_inv_entry_rem.inv_guid == this.guid) {
                        // existing ActionInventoryEntryRemove with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionInventoryEntryMove ext_inv_entry_move) {
                    if (ext_inv_entry_move.to_guid == this.guid) {
                        // existing ActionInventoryEntryMove from or to this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionInventoryEntryMerge ext_inv_entry_merge) {
                    if (ext_inv_entry_merge.inv_guid == this.guid) {
                        // existing ActionInventoryEntryMerge with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionInventoryEntryUnstack ext_inv_entry_unstack) {
                    if (ext_inv_entry_unstack.inv_guid == this.guid) {
                        // existing ActionInventoryEntryUnstack with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionInventoryEntrySplit ext_inv_entry_split) {
                    if (ext_inv_entry_split.inv_guid == this.guid) {
                        // existing ActionInventoryEntrySplit with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
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
                if (actions[i] is ActionInventoryRename ext_inv_rename) {
                    if (ext_inv_rename.guid == this.guid) {
                        // existing ActionInventoryRename with this guid; update its "to" field based on ours and we're done
                        actions[i] = new ActionInventoryRename(ext_inv_rename.guid, ext_inv_rename.from, this.to);
                        return;
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

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionInventoryEntryAdd ext_entry_add) {
                    if (ext_entry_add.guid == this.guid) {
                        // existing ActionInventoryEntryAdd with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
                if (actions[i] is ActionItemStackSet ext_stack_set) {
                    if (ext_stack_set.guid == this.guid) {
                        // existing ActionItemStackSet with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionItemStackAdjust ext_stack_adj) {
                    if (ext_stack_adj.guid == this.guid) {
                        // existing ActionItemStackAdjust with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionSingleItemSet ext_item_set) {
                    if (ext_item_set.guid == this.guid) {
                        // existing ActionSingleItemSet with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionSingleItemAdjust ext_item_adj) {
                    if (ext_item_adj.guid == this.guid) {
                        // existing ActionSingleItemAdjust with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
                if (actions[i] is ActionInventoryEntryMove ext_entry_move) {
                    if (ext_entry_move.guid == this.guid) {
                        // existing ActionInventoryEntryMove with this guid; replace it with a remove based on its "from" field and we're done
                        actions.RemoveAt(i);
                        new ActionInventoryEntryRemove(ext_entry_move.from_guid, ext_entry_move.from_idx, this.guid).merge_to(actions);
                        return;
                    }
                }
                if (actions[i] is ActionInventoryEntryMerge ext_entry_merge) {
                    if (ext_entry_merge.guid == this.guid) {
                        // existing ActionInventoryEntryMerge with this guid; replace it with a remove for its "ent1" and "ent2" fields and we're done
                        actions.RemoveAt(i);
                        new ActionInventoryEntryRemove(ext_entry_merge.inv_guid, ext_entry_merge.inv_idx, ext_entry_merge.ent1).merge_to(actions);
                        new ActionInventoryEntryRemove(ext_entry_merge.inv_guid, ext_entry_merge.inv_idx, ext_entry_merge.ent2).merge_to(actions);
                        return;
                    }
                }
                if (actions[i] is ActionInventoryEntryUnstack ext_entry_unstack) {
                    if (ext_entry_unstack.guid == this.guid) {
                        // existing ActionInventoryEntryUnstack with this guid; replace it with a remove based on its "ent" field and we're done
                        actions.RemoveAt(i);
                        new ActionInventoryEntryRemove(this.inv_guid, this.inv_idx, ext_entry_unstack.ent).merge_to(actions);
                        return;
                    }
                }
                if (actions[i] is ActionInventoryEntrySplit ext_entry_split) {
                    if (ext_entry_split.guid == this.guid) {
                        // existing ActionInventoryEntrySplit with this guid; replace it with a stack adjust action on its "ent" field and we're done
                        actions.RemoveAt(i);
                        new ActionItemStackAdjust(ext_entry_split.ent, -ext_entry_split.count, -ext_entry_split.unidentified).merge_to(actions);
                        return;
                    }
                }
            }
            actions.Add(this);
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

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionInventoryEntryAdd ext_entry_add) {
                    if ((ext_entry_add.guid == this.guid) && (ext_entry_add.entry is ItemStack add_stack)) {
                        // existing ActionInventoryEntryAdd with this guid; update its "entry" field based on our counts and we're done
                        add_stack.count = this.count_to;
                        add_stack.unidentified = this.unidentified_to;
                        return;
                    }
                }
                if (actions[i] is ActionItemStackSet ext_stack_set) {
                    if (ext_stack_set.guid == this.guid) {
                        // existing ActionItemStackSet with this guid; replace it with a new stack set action with its "from" and our "to" and we're done
                        actions[i] = new ActionItemStackSet(
                            this.guid, ext_stack_set.count_from, ext_stack_set.unidentified_from, this.count_to, this.unidentified_to
                        );
                        return;
                    }
                }
                if (actions[i] is ActionItemStackAdjust ext_stack_adj) {
                    if (ext_stack_adj.guid == this.guid) {
                        // existing ActionItemStackAdjust with this guid; replace it with a new stack set action with adjusted "from" and we're done
                        long new_count_from = this.count_from - ext_stack_adj.count, new_unidentified_from = this.unidentified_from - ext_stack_adj.unidentified;
                        actions[i] = new ActionItemStackSet(this.guid, new_count_from, new_unidentified_from, this.count_to, this.unidentified_to);
                        return;
                    }
                }
            }
            actions.Add(this);
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

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionInventoryEntryAdd ext_entry_add) {
                    if ((ext_entry_add.guid == this.guid) && (ext_entry_add.entry is ItemStack add_stack)) {
                        // existing ActionInventoryEntryAdd with this guid; update its "entry" field based on our counts and we're done
                        add_stack.count += this.count;
                        add_stack.unidentified += this.unidentified;
                        return;
                    }
                }
                if (actions[i] is ActionItemStackSet ext_stack_set) {
                    if (ext_stack_set.guid == this.guid) {
                        // existing ActionItemStackSet with this guid; add our counts to its "to" fields and we're done
                        long new_count_to = ext_stack_set.count_to + this.count, new_unidentified_to = ext_stack_set.unidentified_to + this.unidentified;
                        actions[i] = new ActionItemStackSet(
                            this.guid, ext_stack_set.count_from, ext_stack_set.unidentified_from, new_count_to, new_unidentified_to
                        );
                        return;
                    }
                }
                if (actions[i] is ActionItemStackAdjust ext_stack_adj) {
                    if (ext_stack_adj.guid == this.guid) {
                        // existing ActionItemStackAdjust with this guid; replace it with a new adjust action with the sum of both counts and we're done
                        actions[i] = new ActionItemStackAdjust(this.guid, ext_stack_adj.count + this.count, ext_stack_adj.unidentified + this.unidentified);
                        return;
                    }
                }
            }
            actions.Add(this);
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

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionInventoryEntryAdd ext_entry_add) {
                    if ((ext_entry_add.guid == this.guid) && (ext_entry_add.entry is SingleItem itm)) {
                        // existing ActionInventoryEntryAdd with this guid; update its "entry" field based on our values and we're done
                        if (this.unidentified_to is not null) { itm.unidentified = this.unidentified_to.Value; }
                        if (this.set_value_override) { itm.value_override = this.value_override_to; }
                        if (this.properties_to is not null) {
                            itm.properties.Clear();
                            foreach (string key in this.properties_to.Keys) { itm.properties[key] = this.properties_to[key]; }
                        }
                        return;
                    }
                }
                if (actions[i] is ActionSingleItemSet ext_item_set) {
                    if (ext_item_set.guid == this.guid) {
                        // existing ActionSingleItemSet with this guid; replace it with a new single item set action with its "from" and our "to" and we're done
                        actions.RemoveAt(i);
                        new ActionSingleItemSet(
                            this.guid,
                            ext_item_set.unidentified_from ?? this.unidentified_from,
                            (ext_item_set.set_value_override ? ext_item_set.value_override_from : this.value_override_from),
                            ext_item_set.properties_from ?? this.properties_from,
                            this.unidentified_to ?? ext_item_set.unidentified_to,
                            (this.set_value_override ? this.value_override_to : ext_item_set.value_override_to),
                            this.properties_to ?? ext_item_set.properties_to,
                            ext_item_set.set_value_override || this.set_value_override
                        ).merge_to(actions);
                        return;
                    }
                }
                if (actions[i] is ActionSingleItemAdjust ext_item_adj) {
                    if (ext_item_adj.guid == this.guid) {
                        // existing ActionSingleItemAdjust with this guid; replace as much of it as possible with a new single item set action
                        decimal? new_adj_value_override = ext_item_adj.value_override, new_value_override_from = this.value_override_from;
                        Dictionary<string, string> new_adj_props_subtract = ext_item_adj.properties_subtract,
                            new_adj_props_add = ext_item_adj.properties_add, new_props_from = this.properties_from;
                        bool can_generate_new_actions = false;
                        if ((new_adj_value_override is not null) && (new_value_override_from is not null)) {
                            new_value_override_from -= new_adj_value_override;
                            new_adj_value_override = null;
                            can_generate_new_actions = true;
                        }
                        if (((new_adj_props_subtract is not null) || (new_adj_props_add is not null)) && (new_props_from is not null)) {
                            new_props_from = new Dictionary<string, string>(this.properties_from);
                            if (new_adj_props_add is not null) {
                                foreach (string key in new_adj_props_add.Keys) { new_props_from.Remove(key); }
                            }
                            if (new_adj_props_subtract is not null) {
                                foreach (string key in new_adj_props_subtract.Keys) { new_props_from[key] = new_adj_props_subtract[key]; }
                            }
                            new_adj_props_subtract = null;
                            new_adj_props_add = null;
                            can_generate_new_actions = true;
                        }
                        if (can_generate_new_actions) {
                            actions.RemoveAt(i);
                            if ((new_adj_value_override is not null) || (new_adj_props_subtract is not null) || (new_adj_props_add is not null)) {
                                new ActionSingleItemAdjust(this.guid, new_adj_value_override, new_adj_props_subtract, new_adj_props_add).merge_to(actions);
                            }
                            new ActionSingleItemSet(
                                this.guid,
                                this.unidentified_from,
                                new_value_override_from,
                                new_props_from,
                                this.unidentified_to,
                                this.value_override_to,
                                this.properties_to,
                                this.set_value_override
                            ).merge_to(actions);
                            return;
                        }
                        continue;
                    }
                }
            }
            actions.Add(this);
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

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionInventoryEntryAdd ext_entry_add) {
                    if ((ext_entry_add.guid == this.guid) && (ext_entry_add.entry is SingleItem itm)) {
                        // existing ActionInventoryEntryAdd with this guid; update its "entry" field based on our values and we're done
                        if ((this.value_override is not null) && (itm.value_override is not null)) {
                            itm.value_override += this.value_override;
                        }
                        if (this.properties_subtract is not null) {
                            foreach (string key in this.properties_subtract.Keys) { itm.properties.Remove(key); }
                        }
                        if (this.properties_add is not null) {
                            foreach (string key in this.properties_add.Keys) { itm.properties[key] = this.properties_add[key]; }
                        }
                        return;
                    }
                }
                if (actions[i] is ActionSingleItemSet ext_item_set) {
                    if (ext_item_set.guid == this.guid) {
                        // existing ActionSingleItemSet with this guid; update its "to" as much as possible
                        decimal? new_value_override_to = ext_item_set.value_override_to, new_value_override = this.value_override;
                        Dictionary<string, string> new_props_to = ext_item_set.properties_to,
                            new_props_subtract = this.properties_subtract, new_props_add = this.properties_add;
                        bool can_generate_new_actions = false;
                        if ((new_value_override_to is not null) && (new_value_override is not null)) {
                            new_value_override_to += new_value_override;
                            new_value_override = null;
                            can_generate_new_actions = true;
                        }
                        if ((new_props_to is not null) && ((new_props_subtract is not null) || (new_props_add is not null))) {
                            new_props_to = new Dictionary<string, string>(ext_item_set.properties_to);
                            if (new_props_subtract is not null) {
                                foreach (string key in new_props_subtract.Keys) { new_props_to.Remove(key); }
                            }
                            if (new_props_add is not null) {
                                foreach (string key in new_props_add.Keys) { new_props_to[key] = new_props_add[key]; }
                            }
                            new_props_subtract = null;
                            new_props_add = null;
                            can_generate_new_actions = true;
                        }
                        if (can_generate_new_actions) {
                            actions.RemoveAt(i);
                            new ActionSingleItemSet(
                                this.guid,
                                ext_item_set.unidentified_from,
                                ext_item_set.value_override_from,
                                ext_item_set.properties_from,
                                ext_item_set.unidentified_to,
                                new_value_override_to,
                                new_props_to,
                                ext_item_set.set_value_override
                            ).merge_to(actions);
                            if ((new_value_override is not null) || (new_props_subtract is not null) || (new_props_add is not null)) {
                                new ActionSingleItemAdjust(this.guid, new_value_override, new_props_subtract, new_props_add).merge_to(actions);
                            }
                            return;
                        }
                        continue;
                    }
                }
                if (actions[i] is ActionSingleItemAdjust ext_item_adj) {
                    if (ext_item_adj.guid == this.guid) {
                        // existing ActionSingleItemAdjust with this guid; replace with a new adjust action with the sum of both adjustments
                        decimal? new_value_override = (ext_item_adj.value_override ?? 0) + (this.value_override ?? 0);
                        Dictionary<string, string> new_props_subtract, new_props_add;
                        if (ext_item_adj.properties_subtract is null) { new_props_subtract = new Dictionary<string, string>(); }
                        else { new_props_subtract = new Dictionary<string, string>(ext_item_adj.properties_subtract); }
                        if (ext_item_adj.properties_add is null) { new_props_add = new Dictionary<string, string>(); }
                        else { new_props_add = new Dictionary<string, string>(ext_item_adj.properties_add); }
                        if (this.properties_subtract is not null) {
                            foreach (string key in this.properties_subtract.Keys) {
                                if (new_props_add.ContainsKey(key)) { new_props_add.Remove(key); }
                                else { new_props_subtract[key] = this.properties_subtract[key]; }
                            }
                        }
                        if (this.properties_add is not null) {
                            foreach (string key in this.properties_add.Keys) {
                                if ((new_props_subtract.ContainsKey(key)) && (new_props_subtract[key] == this.properties_add[key])) {
                                    new_props_subtract.Remove(key);
                                }
                                else { new_props_add[key] = this.properties_add[key]; }
                            }
                        }
                        if (new_value_override == 0) { new_value_override = null; }
                        if (new_props_subtract.Count == 0) { new_props_subtract = null; }
                        if (new_props_add.Count == 0) { new_props_add = null; }
                        actions.RemoveAt(i);
                        if ((new_value_override is not null) || (new_props_subtract is not null) || (new_props_add is not null)) {
                            new ActionSingleItemAdjust(this.guid, new_value_override, new_props_subtract, new_props_add).merge_to(actions);
                        }
                        return;
                    }
                }
            }
            actions.Add(this);
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