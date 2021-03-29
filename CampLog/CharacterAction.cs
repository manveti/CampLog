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
    }
}