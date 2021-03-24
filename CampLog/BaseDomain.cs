using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class BaseDomain<T> {
        public Dictionary<Guid, T> items;
        public HashSet<Guid> active_items;

        public BaseDomain() {
            this.items = new Dictionary<Guid, T>();
            this.active_items = new HashSet<Guid>();
        }

        public Guid add_item(T item, Guid? guid = null) {
            if (item is null) { throw new ArgumentNullException(nameof(item)); }
            if ((guid is not null) && (this.items.ContainsKey(guid.Value))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            if (this.items.ContainsValue(item)) { throw new ArgumentOutOfRangeException(nameof(item)); }

            Guid item_guid = guid ?? Guid.NewGuid();
            this.items[item_guid] = item;
            this.active_items.Add(item_guid);
            return item_guid;
        }

        public void remove_item(Guid guid) {
            if ((!this.items.ContainsKey(guid)) || (!this.active_items.Contains(guid))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.active_items.Remove(guid);
        }

        public void restore_item(Guid guid) {
            if ((!this.items.ContainsKey(guid)) || (this.active_items.Contains(guid))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.active_items.Add(guid);
        }
    }
}