using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class CalendarEvent {
        public Guid entry_guid;
        public decimal timestamp;
        public string name;
        public string description;
        public decimal? interval;

        public CalendarEvent(Guid entry_guid, decimal timestamp, string name, string description = null, decimal? interval = null) {
            if (name is null) { throw new ArgumentNullException(nameof(name)); }
            this.entry_guid = entry_guid;
            this.timestamp = timestamp;
            this.name = name;
            this.description = description;
            this.interval = interval;
        }

        public CalendarEvent copy() {
            return new CalendarEvent(this.entry_guid, this.timestamp, this.name, this.description, this.interval);
        }
    }


    [Serializable]
    public class CalendarEventDomain : BaseDomain<CalendarEvent> {
        public Dictionary<Guid, CalendarEvent> events {
            get => this.items;
            set => this.items = value;
        }
        public HashSet<Guid> active_events {
            get => this.active_items;
            set => this.active_items = value;
        }

        public Guid add_event(CalendarEvent evt, Guid? guid = null) => this.add_item(evt, guid);

        public void remove_event(Guid guid) => this.remove_item(guid);

        public void restore_event(Guid guid) => this.restore_item(guid);
    }
}