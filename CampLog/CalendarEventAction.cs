using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ActionCalendarEventCreate : EntryAction {
        public readonly Guid guid;
        public readonly CalendarEvent evt;

        public override string description { get => "Add event"; }

        public ActionCalendarEventCreate(Guid guid, CalendarEvent evt) {
            if (evt is null) { throw new ArgumentNullException(nameof(evt)); }
            this.guid = guid;
            this.evt = evt.copy();
        }

        public override void apply(CampaignState state, Entry ent) {
            state.events.add_event(this.evt.copy(), this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.events.remove_event(this.guid);
            state.events.events.Remove(this.guid);
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCalendarEventRemove ext_evt_remove) {
                    if (ext_evt_remove.guid == this.guid) {
                        // existing ActionCalendarEventRemove with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionCalendarEventRemove : EntryAction {
        public readonly Guid guid;

        public override string description { get => "Remove event"; }

        public ActionCalendarEventRemove(Guid guid) {
            this.guid = guid;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.events.remove_event(this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.events.restore_event(this.guid);
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCalendarEventCreate ext_evt_create) {
                    if (ext_evt_create.guid == this.guid) {
                        // existing ActionCalendarEventCreate with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
                if (actions[i] is ActionCalendarEventRestore ext_evt_restore) {
                    if (ext_evt_restore.guid == this.guid) {
                        // existing ActionCalendarEventRestore with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
                if (actions[i] is ActionCalendarEventUpdate ext_evt_update) {
                    if (ext_evt_update.guid == this.guid) {
                        // existing ActionCalendarEventUpdate with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionCalendarEventRestore : EntryAction {
        public readonly Guid guid;

        public override string description { get => "Restore event"; }

        public ActionCalendarEventRestore(Guid guid) {
            this.guid = guid;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.events.restore_event(this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.events.remove_event(this.guid);
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCalendarEventRemove ext_evt_remove) {
                    if (ext_evt_remove.guid == this.guid) {
                        // existing ActionCalendarEventRemove with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionCalendarEventUpdate : EntryAction {
        public readonly Guid guid;
        public CalendarEvent from;
        public readonly CalendarEvent to;
        public readonly bool set_timestamp;
        public readonly bool set_name;
        public readonly bool set_desc;
        public readonly bool set_interval;

        public override string description { get => "Update event"; }

        public ActionCalendarEventUpdate(Guid guid, CalendarEvent from, CalendarEvent to, bool set_timestamp, bool set_name, bool set_desc, bool set_interval) {
            if (from is null) { throw new ArgumentNullException(nameof(from)); }
            if (to is null) { throw new ArgumentNullException(nameof(to)); }
            if (!(set_timestamp || set_name || set_desc || set_interval)) { throw new ArgumentOutOfRangeException(nameof(set_interval)); }
            this.guid = guid;
            this.from = from.copy();
            this.to = to.copy();
            this.set_timestamp = set_timestamp;
            this.set_name = set_name;
            this.set_desc = set_desc;
            this.set_interval = set_interval;
        }

        public override void rebase(CampaignState state) {
            if (!state.events.events.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            CalendarEvent evt = state.events.events[this.guid];
            if (this.set_timestamp) { this.from.timestamp = evt.timestamp; }
            if (this.set_name) { this.from.name = evt.name; }
            if (this.set_desc) { this.from.description = evt.description; }
            if (this.set_interval) { this.from.interval = evt.interval; }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.events.events.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            CalendarEvent evt = state.events.events[this.guid];
            if (this.set_timestamp) { evt.timestamp = this.to.timestamp; }
            if (this.set_name) { evt.name = this.to.name; }
            if (this.set_desc) { evt.description = this.to.description; }
            if (this.set_interval) { evt.interval = this.to.interval; }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.events.events.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            CalendarEvent evt = state.events.events[this.guid];
            if (this.set_timestamp) { evt.timestamp = this.from.timestamp; }
            if (this.set_name) { evt.name = this.from.name; }
            if (this.set_desc) { evt.description = this.from.description; }
            if (this.set_interval) { evt.interval = this.from.interval; }
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionCalendarEventCreate ext_evt_create) {
                    if (ext_evt_create.guid == this.guid) {
                        // existing ActionCalendarEventCreate with this guid; update its "evt" field based on our "to" field and we're done
                        if (this.set_timestamp) { ext_evt_create.evt.timestamp = this.to.timestamp; }
                        if (this.set_name) { ext_evt_create.evt.name = this.to.name; }
                        if (this.set_desc) { ext_evt_create.evt.description = this.to.description; }
                        if (this.set_interval) { ext_evt_create.evt.interval = this.to.interval; }
                        return;
                    }
                }
                if (actions[i] is ActionCalendarEventUpdate ext_evt_update) {
                    if (ext_evt_update.guid == this.guid) {
                        // existing ActionCalendarEventUpdate with this guid; replace with a new adjust action with the sum of both adjustments
                        decimal from_timestamp = ext_evt_update.from.timestamp, to_timestamp = ext_evt_update.to.timestamp;
                        string from_name = ext_evt_update.from.name, to_name = ext_evt_update.to.name;
                        string from_description = ext_evt_update.from.description, to_description = ext_evt_update.to.description;
                        decimal? from_interval = ext_evt_update.from.interval, to_interval = ext_evt_update.to.interval;
                        bool set_timestamp = ext_evt_update.set_timestamp || this.set_timestamp, set_name = ext_evt_update.set_name || this.set_name,
                            set_desc = ext_evt_update.set_desc || this.set_desc, set_interval = ext_evt_update.set_interval || this.set_interval;
                        if (this.set_timestamp) {
                            if (!ext_evt_update.set_timestamp) { from_timestamp = this.from.timestamp; }
                            to_timestamp = this.to.timestamp;
                        }
                        if (this.set_name) {
                            if (!ext_evt_update.set_name) { from_name = this.from.name; }
                            to_name = this.to.name;
                        }
                        if (this.set_desc) {
                            if (!ext_evt_update.set_desc) { from_description = this.from.description; }
                            to_description = this.to.description;
                        }
                        if (this.set_interval) {
                            if (!ext_evt_update.set_interval) { from_interval = this.from.interval; }
                            to_interval = this.to.interval;
                        }
                        if ((set_timestamp) && (from_timestamp == to_timestamp)) { set_timestamp = false; }
                        if ((set_name) && (from_name == to_name)) {
                            set_name = false;
                            from_name = null;
                            to_name = null;
                        }
                        if ((set_desc) && (from_description == to_description)) {
                            set_desc = false;
                            from_description = null;
                            to_description = null;
                        }
                        if ((set_interval) && (from_interval == to_interval)) {
                            set_interval = false;
                            from_interval = null;
                            to_interval = null;
                        }
                        actions.RemoveAt(i);
                        if ((set_timestamp) || (set_name) || (set_desc) || (set_interval)) {
                            CalendarEvent from = new CalendarEvent(ext_evt_update.from.entry_guid, from_timestamp, from_name, from_description, from_interval),
                                to = new CalendarEvent(this.to.entry_guid, to_timestamp, to_name, to_description, to_interval);
                            new ActionCalendarEventUpdate(this.guid, from, to, set_timestamp, set_name, set_desc, set_interval).merge_to(actions);
                        }
                        return;
                    }
                }
            }
            actions.Add(this);
        }
    }
}