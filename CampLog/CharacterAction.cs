using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ActionCharacterSet : EntryAction {
        public readonly Guid guid;
        public Character from;
        public readonly Character to;
        public readonly bool restore;

        public override string description {
            get {
                if (this.from is null) {
                    if (this.restore) {
                        return string.Format("Restore removed character {0}", this.to.name);
                    }
                    return string.Format("Add character {0}", this.to.name);
                }
                if (this.to is null) {
                    return string.Format("Remove character {0}", this.from.name);
                }
                if (this.from.name == this.to.name) {
                    return string.Format("Update character {0}", this.to.name);
                }
                return string.Format("Update character {0} (renamed from {1})", this.to.name, this.from.name);
            }
        }

        public ActionCharacterSet(Guid guid, Character from, Character to, bool restore = false) {
            if ((from is null) && (to is null)) { throw new ArgumentNullException(nameof(to)); }
            if (from is not null) { from = from.copy(); }
            if (to is not null) { to = to.copy(); }
            this.guid = guid;
            this.from = from;
            this.to = to;
            this.restore = restore;
        }

        public override void rebase(CampaignState state) {
            if (this.from is not null) {
                if (!state.characters.characters.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
                this.from = state.characters.characters[this.guid].copy();
            }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (this.from is null) {
                // add new character
                if (this.restore) {
                    state.characters.restore_character(this.guid);
                }
                else {
                    state.characters.add_character(this.to.copy(), this.guid);
                }
            }
            else if (this.to is null) {
                // remove existing character
                state.characters.remove_character(this.guid);
            }
            else {
                // update existing character to new value
                state.characters.characters[this.guid] = this.to.copy();
            }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (this.from is null) {
                // revert addition of new character
                state.characters.remove_character(this.guid);
                if (!this.restore) {
                    // if we're not reverting a restore, purge "deleted" copy of character too
                    state.characters.characters.Remove(this.guid);
                }
            }
            else if (this.to is null) {
                // revert removal of existing character
                state.characters.restore_character(this.guid);
            }
            else {
                // revert update of existing character to new value
                state.characters.characters[this.guid] = this.from.copy();
            }
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCharacterSet ext_char_set) {
                    if (ext_char_set.guid != this.guid) { continue; }
                    // existing ActionCharacterSet with this guid
                    if ((ext_char_set.from is null) && (this.to is null)) {
                        // we're deleting character; remove action creating it and we're done
                        actions.RemoveAt(i);
                    }
                    else {
                        // we're creating or updating character; update existing action's "to" field to match ours and we're done
                        actions[i] = new ActionCharacterSet(this.guid, ext_char_set.from, this.to, ext_char_set.restore);
                    }
                    return;
                }
                if (actions[i] is ActionCharacterPropertySet ext_prop_set) {
                    if (ext_prop_set.guid != this.guid) { continue; }
                    // existing ActionCharacterPropertySet with this guid; delete it and update our "from" field
                    if (ext_prop_set.from is null) {
                        this.from.remove_property(ext_prop_set.path);
                    }
                    else {
                        this.from.set_property(ext_prop_set.path, ext_prop_set.from);
                    }
                    actions.RemoveAt(i);
                    continue;
                }
                if (actions[i] is ActionCharacterPropertyAdjust ext_prop_adj) {
                    if ((ext_prop_adj.guids is null) || (ext_prop_adj.guids.Count != 1) || (!ext_prop_adj.guids.Contains(this.guid))) {
                        continue;
                    }
                    // existing ActionCharacterPropertyAdjust with this guid; delete it and update our "from" field
                    CharProperty prop = this.from.get_property(ext_prop_adj.path);
                    if (ext_prop_adj.add is not null) { prop.subtract(ext_prop_adj.add); }
                    if (ext_prop_adj.subtract is not null) { prop.add(ext_prop_adj.subtract); }
                    actions.RemoveAt(i);
                    continue;
                }
                if (actions[i] is ActionCharacterSetInventory ext_set_inv) {
                    if ((ext_set_inv.guid != this.guid) || (this.to is not null)) { continue; }
                    // existing ActionCharacterSetInventory with this guid and we're deleting the character; delete the inventory set action
                    actions.RemoveAt(i);
                    continue;
                }
            }
            actions.Add(this);
        }
    }


    static class PropertyUtil {
        public static int path_common_prefix_length(List<string> p1, List<string> p2) {
            int i;
            for (i = 0; (i < p1.Count) && (i < p2.Count); i++) {
                if (p1[i] != p2[i]) { break; }
            }
            return i;
        }
    }


    [Serializable]
    public class ActionCharacterPropertySet : EntryAction {
        public readonly Guid guid;
        public readonly List<string> path;
        public CharProperty from;
        public readonly CharProperty to;

        public override string description {
            get {
                string path_str = String.Join(":", this.path);
                if (this.from is null) {
                    return string.Format("Set character property {0}", path_str);
                }
                if (this.to is null) {
                    return string.Format("Remove character property {0}", path_str);
                }
                return string.Format("Update character property {0}", path_str);
            }
        }

        public ActionCharacterPropertySet(Guid guid, List<string> path, CharProperty from, CharProperty to) {
            if (path is null) { throw new ArgumentNullException(nameof(path)); }
            if (path.Count <= 0) { throw new ArgumentOutOfRangeException(nameof(path)); }
            if ((from is null) && (to is null)) { throw new ArgumentNullException(nameof(to)); }
            if (from is not null) { from = from.copy(); }
            if (to is not null) { to = to.copy(); }
            this.guid = guid;
            this.path = new List<string>(path);
            this.from = from;
            this.to = to;
        }

        public override void rebase(CampaignState state) {
            if (!state.characters.characters.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            try {
                CharProperty prop = state.characters.characters[this.guid].get_property(this.path);
                this.from = prop.copy();
            }
            catch (ArgumentOutOfRangeException) {
                this.from = null;
            }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.characters.characters.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            if (this.to is null) {
                // remove existing property
                state.characters.characters[this.guid].remove_property(this.path);
            }
            else {
                // add new property or update existing property
                state.characters.characters[this.guid].set_property(this.path, this.to.copy());
            }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.characters.characters.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            if (this.from is null) {
                // revert addition of new property
                state.characters.characters[this.guid].remove_property(this.path);
            }
            else {
                // revert removal or update of existing property
                state.characters.characters[this.guid].set_property(this.path, this.from.copy());
            }
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCharacterSet ext_char_set) {
                    if (ext_char_set.guid != this.guid) { continue; }
                    // existing ActionCharacterSet with this guid; update its "to" field based on ours and we're done
                    if (this.to is null) {
                        ext_char_set.to.remove_property(this.path);
                    }
                    else {
                        ext_char_set.to.set_property(this.path, this.to);
                    }
                    return;
                }
                if (actions[i] is ActionCharacterPropertySet ext_prop_set) {
                    if (ext_prop_set.guid != this.guid) { continue; }
                    //TODO: maybe handle ancestor path which contains this path or its immediate parent
                    if (PropertyUtil.path_common_prefix_length(ext_prop_set.path, this.path) < this.path.Count) { continue; }
                    // existing ActionCharacterPropertySet with this guid and this or a child path
                    if (ext_prop_set.path.Count == this.path.Count) {
                        // action refers to this path; update its "to" field based on ours and we're done
                        actions[i] = new ActionCharacterPropertySet(ext_prop_set.guid, ext_prop_set.path, ext_prop_set.from, this.to);
                        return;
                    }
                    // action refers to a child path; update our "from" field based on it then delete it
                    CharDictProperty dict_prop = this.from as CharDictProperty;
                    for (int j = this.path.Count; j < ext_prop_set.path.Count - 1; j++) {
                        if ((dict_prop is null) || (!dict_prop.value.ContainsKey(ext_prop_set.path[j]))) {
                            dict_prop = null;
                            break;
                        }
                        dict_prop = dict_prop.value[ext_prop_set.path[j]] as CharDictProperty;
                    }
                    if (dict_prop is not null) {
                        if (ext_prop_set.from is null) {
                            dict_prop.value.Remove(ext_prop_set.path[^1]);
                        }
                        else {
                            dict_prop.value[ext_prop_set.path[^1]] = ext_prop_set.from;
                        }
                    }
                    actions.RemoveAt(i);
                    continue;
                }
                if (actions[i] is ActionCharacterPropertyAdjust ext_prop_adj) {
                    if ((ext_prop_adj.guids is null) || (ext_prop_adj.guids.Count != 1) || (!ext_prop_adj.guids.Contains(this.guid))) {
                        continue;
                    }
                    if (PropertyUtil.path_common_prefix_length(ext_prop_adj.path, this.path) < this.path.Count) { continue; }
                    // existing ActionCharacterPropertyAdjust with this guid and this or a child path; update our "from" field based on it then delete it
                    CharProperty prop = this.from;
                    for (int j = this.path.Count; j < ext_prop_adj.path.Count; j++) {
                        if ((prop is CharDictProperty dict_prop) && (dict_prop.value.ContainsKey(ext_prop_adj.path[j]))) {
                            prop = dict_prop.value[ext_prop_adj.path[j]];
                        }
                        else {
                            prop = null;
                            break;
                        }
                    }
                    if (prop is not null) {
                        if (ext_prop_adj.add is not null) { prop.subtract(ext_prop_adj.add); }
                        if (ext_prop_adj.subtract is not null) { prop.add(ext_prop_adj.subtract); }
                    }
                    actions.RemoveAt(i);
                    continue;
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionCharacterPropertyAdjust : EntryAction {
        public readonly HashSet<Guid> guids;
        public readonly List<string> path;
        public readonly CharProperty subtract;
        public readonly CharProperty add;

        public override string description {
            get {
                string path_str = String.Join(":", this.path);
                return string.Format("Adjust character property {0}", path_str);
            }
        }

        public ActionCharacterPropertyAdjust(HashSet<Guid> guids, List<string> path, CharProperty subtract, CharProperty add) {
            if (path is null) { throw new ArgumentNullException(nameof(path)); }
            if (path.Count <= 0) { throw new ArgumentOutOfRangeException(nameof(path)); }
            if ((subtract is null) && (add is null)) { throw new ArgumentNullException(nameof(add)); }
            if (subtract is not null) { subtract = subtract.copy(); }
            if (add is not null) { add = add.copy(); }
            this.guids = guids;
            this.path = path;
            this.subtract = subtract;
            this.add = add;
        }

        public ActionCharacterPropertyAdjust(
            Guid guid, List<string> path, CharProperty subtract, CharProperty add
        ) : this(new HashSet<Guid>() { guid }, path, subtract, add) { }

        private void apply_one(CampaignState state, Guid guid) {
            CharProperty prop = state.characters.characters[guid].get_property(this.path);
            if (this.subtract is not null) { prop.subtract(this.subtract); }
            if (this.add is not null) { prop.add(this.add); }
        }

        private void revert_one(CampaignState state, Guid guid) {
            CharProperty prop = state.characters.characters[guid].get_property(this.path);
            if (this.add is not null) { prop.subtract(this.add); }
            if (this.subtract is not null) { prop.add(this.subtract); }
        }

        public override void apply(CampaignState state, Entry ent) {
            HashSet<Guid> chars = this.guids ?? state.characters.active_characters, done = new HashSet<Guid>();
            foreach (Guid guid in chars) {
                try {
                    this.apply_one(state, guid);
                    done.Add(guid);
                }
                catch (ArgumentException) {
                    foreach (Guid revert_guid in done) {
                        this.revert_one(state, revert_guid);
                    }
                    throw;
                }
            }
        }

        public override void revert(CampaignState state, Entry ent) {
            HashSet<Guid> chars = this.guids ?? state.characters.active_characters, done = new HashSet<Guid>();
            foreach (Guid guid in chars) {
                try {
                    this.revert_one(state, guid);
                    done.Add(guid);
                }
                catch (ArgumentException) {
                    foreach (Guid reapply_guid in done) {
                        this.apply_one(state, reapply_guid);
                    }
                    throw;
                }
            }
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCharacterSet ext_char_set) {
                    if ((this.guids is null) || (this.guids.Count != 1) || (!this.guids.Contains(ext_char_set.guid))) {
                        continue;
                    }
                    // existing ActionCharacterSet with this (single) guid; update its "to" field based on this and we're done
                    CharProperty prop = ext_char_set.to.get_property(this.path);
                    if (this.subtract is not null) { prop.subtract(this.subtract); }
                    if (this.add is not null) { prop.add(this.add); }
                    return;
                }
                if (actions[i] is ActionCharacterPropertySet ext_prop_set) {
                    if ((this.guids is null) || (this.guids.Count != 1) || (!this.guids.Contains(ext_prop_set.guid))) {
                        continue;
                    }
                    if (PropertyUtil.path_common_prefix_length(ext_prop_set.path, this.path) < ext_prop_set.path.Count) { continue; }
                    // existing ActionCharacterPropertySet with this (single) guid and this or a parent path
                    //TODO: handle ancestor path which contains this path
                    if (ext_prop_set.path.Count != this.path.Count) { continue; }
                    // action refers to this path; update its "to" field based on this and we're done
                    if (this.subtract is not null) { ext_prop_set.to.subtract(this.subtract); }
                    if (this.add is not null) { ext_prop_set.to.add(this.add); }
                    return;
                }
                if (actions[i] is ActionCharacterPropertyAdjust ext_prop_adj) {
                    if ((ext_prop_adj.guids is null) != (this.guids is null)) { continue; }
                    if (ext_prop_adj.guids is not null) {
                        if ((ext_prop_adj.guids.Count != this.guids.Count) || (!ext_prop_adj.guids.IsSubsetOf(this.guids))) {
                            continue;
                        }
                    }
                    if (ext_prop_adj.path.Count != this.path.Count) { continue; }
                    if (PropertyUtil.path_common_prefix_length(ext_prop_adj.path, this.path) < this.path.Count) { continue; }
                    // existing ActionCharacterPropertyAdjust with this guid (or set of guids) and this path; update it based on this and we're done
                    CharProperty new_subtract, new_add;
                    if (ext_prop_adj.subtract is null) { new_subtract = this.subtract; }
                    else {
                        new_subtract = ext_prop_adj.subtract.copy();
                        if (this.subtract is not null) { new_subtract.add(this.subtract); }
                    }
                    if (ext_prop_adj.add is null) { new_add = this.add; }
                    else {
                        new_add = ext_prop_adj.add.copy();
                        if (this.add is not null) { new_add.add(this.add); }
                    }
                    actions[i] = new ActionCharacterPropertyAdjust(ext_prop_adj.guids, ext_prop_adj.path, new_subtract, new_add);
                    return;
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionCharacterSetInventory : EntryAction {
        public readonly Guid guid;
        public Guid? from;
        public readonly Guid? to;

        public override string description {
            get {
                if (this.from is null) {
                    return "Set character inventory association";
                }
                if (this.to is null) {
                    return "Clear character inventory association";
                }
                return "Update character inventory association";
            }
        }

        public ActionCharacterSetInventory(Guid guid, Guid? from, Guid? to) {
            if ((from is null) && (to is null)) { throw new ArgumentNullException(nameof(to)); }
            this.guid = guid;
            this.from = from;
            this.to = to;
        }

        public override void rebase(CampaignState state) {
            if (state.character_inventory.ContainsKey(this.guid)) {
                this.from = state.character_inventory[this.guid];
            }
            else {
                this.from = null;
            }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (this.to is null) {
                state.character_inventory.Remove(this.guid);
            }
            else {
                state.character_inventory[this.guid] = this.to.Value;
            }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (this.from is null) {
                state.character_inventory.Remove(this.guid);
            }
            else {
                state.character_inventory[this.guid] = this.from.Value;
            }
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCharacterSetInventory ext_set_inv) {
                    if (ext_set_inv.guid != this.guid) { continue; }
                    // existing ActionCharacterSetInventory with this guid; update its "to" field based on ours and we're done
                    actions[i] = new ActionCharacterSetInventory(ext_set_inv.guid, ext_set_inv.from, this.to);
                    return;
                }
            }
            actions.Add(this);
        }
    }
}