using System;

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
    }


    [Serializable]
    public class ActionCalendarEventUpdate : EntryAction {
        public readonly Guid guid;
        public readonly CalendarEvent from;
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
    }
}