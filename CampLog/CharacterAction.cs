using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ActionCharacterSet : Action {
        public Guid guid;
        public Character from;
        public Character to;
        public bool restore;

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
            this.guid = guid;
            this.from = from;
            this.to = to;
            this.restore = restore;
        }

        public override void apply(CampaignState state) {
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

        public override void revert(CampaignState state) {
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


    //TODO: ...
}