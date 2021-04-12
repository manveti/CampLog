using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [DataContract(IsReference = true)]
    public class ItemCategory {
        [DataMember] public string name;
        [DataMember] public decimal sale_value;

        public ItemCategory(string name, decimal sale_value) {
            this.name = name;
            this.sale_value = sale_value;
        }

        public override bool Equals(object obj) => obj is ItemCategory category && this.name == category.name && this.sale_value == category.sale_value;
        public override int GetHashCode() => HashCode.Combine(this.name, this.sale_value);
        public static bool operator ==(ItemCategory c1, ItemCategory c2) {
            if (c1 is not null) { return c1.Equals(c2); }
            if (c2 is not null) { return c2.Equals(c1); }
            return true;
        }
        public static bool operator !=(ItemCategory c1, ItemCategory c2) => !(c1 == c2);
    }


    [DataContract(IsReference = true)]
    public class ContainerSpec {
        [DataMember] public string name;
        [DataMember] public decimal weight_factor;
        [DataMember] public decimal? weight_capacity;

        public ContainerSpec(string name, decimal weight_factor, decimal? weight_capacity = null) {
            this.name = name;
            this.weight_capacity = weight_capacity;
            this.weight_factor = weight_factor;
        }

        public override bool Equals(object obj) => obj is ContainerSpec spec && this.name == spec.name && this.weight_factor == spec.weight_factor && this.weight_capacity == spec.weight_capacity;
        public override int GetHashCode() => HashCode.Combine(this.name, this.weight_factor, this.weight_capacity);
        public static bool operator ==(ContainerSpec c1, ContainerSpec c2) {
            if (c1 is not null) { return c1.Equals(c2); }
            if (c2 is not null) { return c2.Equals(c1); }
            return true;
        }
        public static bool operator !=(ContainerSpec c1, ContainerSpec c2) => !(c1 == c2);
    }


    [DataContract(IsReference = true)]
    public class ItemSpec {
        [DataMember] public string name;
        [DataMember] public ItemCategory category;
        [DataMember] public decimal cost;
        [DataMember] public decimal? sale_value;
        public decimal value { get => this.sale_value ?? (this.cost * this.category.sale_value); }
        [DataMember] public decimal weight;
        [DataMember] public ContainerSpec[] containers;
        [DataMember] public Dictionary<string, string> properties;

        public ItemSpec(string name, ItemCategory category, decimal cost, decimal weight, decimal? sale_value = null, ContainerSpec[] containers = null) {
            if (category is null) { throw new ArgumentNullException(nameof(category)); }
            this.name = name;
            this.category = category;
            this.cost = cost;
            this.weight = weight;
            this.sale_value = sale_value;
            this.containers = containers;
            this.properties = new Dictionary<string, string>();
        }

        public override bool Equals(object obj) {
            ItemSpec spec = (ItemSpec)obj;
            if (spec is null || this.name != spec.name || this.category != spec.category || this.cost != spec.cost || this.sale_value != spec.sale_value || this.weight != spec.weight) { return false; }
            if (!ReferenceEquals(this.containers, spec.containers)) {
                if (this.containers is null || spec.containers is null || this.containers.Length != spec.containers.Length) { return false; }
                for (int i = 0; i < this.containers.Length; i++) {
                    if (this.containers[i] != spec.containers[i]) { return false; }
                }
            }
            if (!ReferenceEquals(this.properties, spec.properties)) {
                if (this.properties is null || spec.properties is null || this.properties.Count != spec.properties.Count) { return false; }
                foreach (string key in this.properties.Keys) {
                    if ((!spec.properties.ContainsKey(key)) || (this.properties[key] != spec.properties[key])) { return false; }
                }
            }
            return true;
        }
        public override int GetHashCode() => HashCode.Combine(this.name, this.category, this.cost, this.sale_value, this.weight, this.containers, this.properties);
        public static bool operator ==(ItemSpec i1, ItemSpec i2) {
            if (i1 is not null) { return i1.Equals(i2); }
            if (i2 is not null) { return i2.Equals(i1); }
            return true;
        }
        public static bool operator !=(ItemSpec i1, ItemSpec i2) => !(i1 == i2);
    }


    [KnownType(typeof(ItemStack))]
    [KnownType(typeof(SingleItem))]
    [DataContract(IsReference = true)]
    public abstract class InventoryEntry {
        [DataMember] public ItemSpec item;
        public abstract string name { get; }
        public abstract decimal weight { get; }
        public abstract decimal value { get; }

        public abstract InventoryEntry copy();

        public virtual bool contains_inventory(Inventory inv) { return false; }
    }


    [Serializable]
    public class Inventory {
        public string name;
        public Dictionary<Guid, InventoryEntry> contents;
        public decimal weight {
            get {
                decimal weight = 0;
                foreach (InventoryEntry entry in contents.Values) {
                    weight += entry.weight;
                }
                return weight;
            }
        }
        public decimal value {
            get {
                decimal value = 0;
                foreach (InventoryEntry entry in contents.Values) {
                    value += entry.value;
                }
                return value;
            }
        }

        public Inventory(string name = null) {
            this.name = name;
            this.contents = new Dictionary<Guid, InventoryEntry>();
        }

        public Inventory copy() {
            Inventory result = new Inventory(this.name);
            foreach (Guid key in this.contents.Keys) {
                result.contents[key] = this.contents[key].copy();
            }
            return result;
        }

        public bool contains_inventory(Inventory inv) {
            foreach (InventoryEntry ent in this.contents.Values) {
                if (ent.contains_inventory(inv)) { return true; }
            }
            return false;
        }

        public Guid add(InventoryEntry ent, Guid? guid = null) {
            if (ent is null) { throw new ArgumentNullException(nameof(ent)); }
            if (ent.contains_inventory(this)) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            Guid ent_guid = guid ?? Guid.NewGuid();
            if (this.contents.ContainsKey(ent_guid)) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.contents[ent_guid] = ent;
            return ent_guid;
        }

        public void remove(Guid guid) {
            if (!this.contents.ContainsKey(guid)) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.contents.Remove(guid);
        }

        public Guid merge(Guid ent1, Guid ent2, Guid? guid = null) {
            if (!this.contents.ContainsKey(ent1)) { throw new ArgumentOutOfRangeException(nameof(ent1)); }
            if (!this.contents.ContainsKey(ent2)) { throw new ArgumentOutOfRangeException(nameof(ent2)); }
            if (this.contents[ent1].item != this.contents[ent2].item) { throw new InvalidOperationException(); }

            ItemStack stack1 = this.contents[ent1] as ItemStack, stack2 = this.contents[ent2] as ItemStack;
            SingleItem si1 = this.contents[ent1] as SingleItem, si2 = this.contents[ent2] as SingleItem;

            if (stack1 is not null) {
                // ent1 is a stack; merge ent2 into it
                if ((guid is not null) && (guid.Value != ent1)) { throw new ArgumentOutOfRangeException(nameof(guid)); }
                if (stack2 is not null) {
                    // ent2 is a stack too; just combine their counts
                    stack1.count += stack2.count;
                    stack1.unidentified += stack2.unidentified;
                }
                else {
                    // ent2 is a single item; make sure it doesn't have anything unique then add it in
                    if ((si2 is null) || (si2.value_override is not null) || (si2.containers is not null) || (si2.properties.Count > 0)) {
                        throw new ArgumentOutOfRangeException(nameof(ent2));
                    }
                    stack1.count += 1;
                    if (si2.unidentified) {
                        stack1.unidentified += 1;
                    }
                }
                this.remove(ent2);
                return ent1;
            }
            if (stack2 is not null) {
                // ent1 isn't a stack but ent2 is; make sure ent1 doesn't have anything unique then add it into ent2
                if ((guid is not null) && (guid.Value != ent2)) { throw new ArgumentOutOfRangeException(nameof(guid)); }
                if ((si1 is null) || (si1.value_override is not null) || (si1.containers is not null) || (si1.properties.Count > 0)) {
                    throw new ArgumentOutOfRangeException(nameof(ent1));
                }
                stack2.count += 1;
                if (si1.unidentified) {
                    stack2.unidentified += 1;
                }
                this.remove(ent1);
                return ent2;
            }
            // neither ent1 nor ent2 is a stack; make sure neither has anything unique then merge into a new stack
            if ((si1 is null) || (si1.value_override is not null) || (si1.containers is not null) || (si1.properties.Count > 0)) {
                throw new ArgumentOutOfRangeException(nameof(ent1));
            }
            if ((si2 is null) || (si2.value_override is not null) || (si2.containers is not null) || (si2.properties.Count > 0)) {
                throw new ArgumentOutOfRangeException(nameof(ent2));
            }
            long unidentified = (si1.unidentified ? 1 : 0) + (si2.unidentified ? 1 : 0);
            ItemStack merged_stack = new ItemStack(si1.item, 2, unidentified);
            Guid result = this.add(merged_stack, guid);
            this.remove(ent1);
            this.remove(ent2);
            return result;
        }

        public Guid unstack(Guid ent, Guid? guid = null) {
            if (!this.contents.ContainsKey(ent)) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            ItemStack stack = this.contents[ent] as ItemStack;
            if ((stack is null) || (stack.count != 1)) { throw new ArgumentOutOfRangeException(nameof(ent)); }

            SingleItem new_item = new SingleItem(stack.item, stack.unidentified >= 1);
            Guid result = this.add(new_item, guid);
            this.remove(ent);
            return result;
        }

        public Guid split(Guid ent, long count, long unidentified, Guid? guid = null) {
            if (!this.contents.ContainsKey(ent)) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            ItemStack stack = this.contents[ent] as ItemStack;
            if (stack is null) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            if ((count <= 0) || (count >= stack.count)) { throw new ArgumentOutOfRangeException(nameof(count)); }
            if ((unidentified < 0) || (unidentified > stack.unidentified) || (unidentified > count)) { throw new ArgumentOutOfRangeException(nameof(unidentified)); }
            if ((stack.count - count) < (stack.unidentified - unidentified)) { throw new ArgumentOutOfRangeException(nameof(unidentified)); }

            ItemStack new_stack = new ItemStack(stack.item, count, unidentified);
            Guid result = this.add(new_stack, guid);
            stack.count -= count;
            stack.unidentified -= unidentified;
            return result;
        }
    }


    [DataContract(IsReference = true)]
    public class ItemStack : InventoryEntry {
        [DataMember] public long count;
        [DataMember] public long unidentified;

        public override string name {
            get {
                string n;
                if (this.count == 1) { n = this.item.name; }
                else { n = string.Format("{0}x {1}", this.count, this.item.name); }
                if (this.unidentified == this.count) { n += " (unidentified)"; }
                else if (this.unidentified > 0) { n += string.Format(" ({0} unidentified)", this.unidentified); }
                return n;
            }
        }
        public override decimal weight { get => this.count * this.item.weight; }
        public override decimal value { get => this.count * this.item.value; }

        public ItemStack(ItemSpec item, long count = 1, long unidentified = 0) {
            if (item is null) { throw new ArgumentNullException(nameof(item)); }
            if (count <= 0) { throw new ArgumentOutOfRangeException(nameof(count)); }
            if ((unidentified < 0) || (unidentified > count)) { throw new ArgumentOutOfRangeException(nameof(unidentified)); }

            this.item = item;
            this.count = count;
            this.unidentified = unidentified;
        }

        public override ItemStack copy() {
            return new ItemStack(this.item, this.count, this.unidentified);
        }
    }


    [DataContract(IsReference = true)]
    public class SingleItem : InventoryEntry {
        [DataMember] public bool unidentified;
        [DataMember] public decimal? value_override;
        [DataMember] public Inventory[] containers;
        public decimal contents_weight {
            get {
                if (containers is null) { return 0; }
                decimal weight = 0;
                for (int i = 0; i < this.containers.Length; i++) {
                    weight += this.containers[i].weight * this.item.containers[i].weight_factor;
                }
                return weight;
            }
        }
        public decimal contents_value {
            get {
                if (containers is null) { return 0; }
                decimal value = 0;
                foreach (Inventory inv in containers) {
                    value += inv.value;
                }
                return value;
            }
        }
        public Dictionary<string, string> properties;
        public override string name {
            get {
                string n = this.item.name;
                if (this.unidentified) { n += " (unidentified)"; }
                return n;
            }
        }
        public override decimal weight { get => this.item.weight + this.contents_weight; }
        public override decimal value { get => (this.value_override ?? this.item.value) + this.contents_value; }

        public SingleItem(ItemSpec item, bool unidentified = false, decimal? value_override = null) {
            if (item is null) { throw new ArgumentNullException(nameof(item)); }

            this.item = item;
            this.unidentified = unidentified;
            this.value_override = value_override;
            if (item.containers is not null) {
                this.containers = new Inventory[item.containers.Length];
                for (int i = 0; i < this.containers.Length; i++) { this.containers[i] = new Inventory(item.containers[i].name); }
            }
            this.properties = new Dictionary<string, string>();
        }

        public override SingleItem copy() {
            SingleItem result = new SingleItem(this.item, this.unidentified, this.value_override);
            if (this.containers is not null) {
                for (int i = 0; i < this.containers.Length; i++) {
                    result.containers[i] = this.containers[i].copy();
                }
            }
            foreach (string key in this.properties.Keys) {
                result.properties[key] = this.properties[key];
            }
            return result;
        }

        public override bool contains_inventory(Inventory inv) {
            if (this.containers is null) { return false; }
            foreach (Inventory container in this.containers) {
                if ((container == inv) || (container.contains_inventory(inv))) { return true; }
            }
            return false;
        }

        public Guid add(int idx, InventoryEntry ent, Guid? guid = null) {
            if (this.containers is null) { throw new InvalidOperationException(); }
            if ((idx < 0) || (idx >= this.containers.Length)) { throw new ArgumentOutOfRangeException(nameof(idx)); }
            if (this.item.containers[idx].weight_capacity is not null) {
                if (ent is null) { throw new ArgumentNullException(nameof(ent)); }
                if (ent.weight > this.item.containers[idx].weight_capacity - this.containers[idx].weight) {
                    throw new ArgumentOutOfRangeException(nameof(ent));
                }
            }
            return this.containers[idx].add(ent, guid);
        }

        public void remove(int idx, Guid guid) {
            if (this.containers is null) { throw new InvalidOperationException(); }
            if ((idx < 0) || (idx >= this.containers.Length)) { throw new ArgumentOutOfRangeException(nameof(idx)); }
            this.containers[idx].remove(guid);
        }
    }


    [Serializable]
    public class InventoryDomain {
        public Dictionary<ItemCategory, List<ItemSpec>> items;
        public Dictionary<Guid, InventoryEntry> entries;
        public HashSet<Guid> active_entries;
        public Dictionary<Guid, Inventory> inventories;

        public InventoryDomain() {
            this.items = new Dictionary<ItemCategory, List<ItemSpec>>();
            this.entries = new Dictionary<Guid, InventoryEntry>();
            this.active_entries = new HashSet<Guid>();
            this.inventories = new Dictionary<Guid, Inventory>();
        }

        private void record_inventory_contents(Inventory inv) {
            foreach (Guid guid in inv.contents.Keys) {
                this.add_item(inv.contents[guid].item);
                this.entries[guid] = inv.contents[guid];
                if ((this.entries[guid] is SingleItem si) && (si.containers is not null)) {
                    foreach (Inventory subinv in si.containers) {
                        this.record_inventory_contents(subinv);
                    }
                }
            }
        }

        public InventoryDomain copy() {
            InventoryDomain result = new InventoryDomain() {
                active_entries = new HashSet<Guid>(this.active_entries)
            };
            foreach (Guid guid in this.inventories.Keys) {
                result.inventories[guid] = this.inventories[guid].copy();
                result.record_inventory_contents(result.inventories[guid]);
            }
            return result;
        }

        public Guid new_inventory(string name = null, Guid? guid = null) {
            Guid inv_guid = guid ?? Guid.NewGuid();
            if (this.inventories.ContainsKey(inv_guid)) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.inventories[inv_guid] = new Inventory(name);
            return inv_guid;
        }

        private void purge_inventory_entry(InventoryEntry entry) {
            if ((entry is SingleItem entry_itm) && (entry_itm.containers is not null)) {
                foreach (Inventory inv in entry_itm.containers) {
                    this.purge_inventory(inv);
                }
            }
        }

        private void purge_inventory(Inventory inv) {
            foreach (Guid ent in inv.contents.Keys) {
                this.purge_inventory_entry(inv.contents[ent]);
                this.active_entries.Remove(ent);
            }
        }

        public void remove_inventory(Guid guid) {
            if (!this.inventories.ContainsKey(guid)) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            this.purge_inventory(this.inventories[guid]);
            this.inventories.Remove(guid);
        }

        private Inventory get_inventory(Guid guid, int? idx) {
            if (idx is null) {
                if (!this.inventories.ContainsKey(guid)) { throw new ArgumentOutOfRangeException(nameof(guid)); }
                return this.inventories[guid];
            }
            if ((!this.entries.ContainsKey(guid)) || (!this.active_entries.Contains(guid))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            if (this.entries[guid] is not SingleItem to_itm) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            return to_itm.containers[idx.Value];
        }

        public void move_entry(Guid entry, Guid from_guid, int? from_idx, Guid to_guid, int? to_idx = null) {
            if ((!this.entries.ContainsKey(entry)) || (!this.active_entries.Contains(entry))) { throw new ArgumentOutOfRangeException(nameof(entry)); }

            // do error-checking for removal first so operation is atomic
            if (from_idx is null) {
                if (!this.inventories.ContainsKey(from_guid)) { throw new ArgumentOutOfRangeException(nameof(from_guid)); }
            }
            else {
                if (!this.entries.ContainsKey(from_guid)) { throw new ArgumentOutOfRangeException(nameof(from_guid)); }
                if (this.entries[from_guid] is not SingleItem from_itm) { throw new ArgumentOutOfRangeException(nameof(from_guid)); }
                int rem_idx = from_idx ?? 0;
                if ((rem_idx < 0) || (rem_idx >= from_itm.containers.Length)) { throw new ArgumentOutOfRangeException(nameof(from_idx)); }
            }

            // if we got here we know removal will succeed so attempt add
            if (to_idx is null) {
                if (!this.inventories.ContainsKey(to_guid)) { throw new ArgumentOutOfRangeException(nameof(to_guid)); }
                this.inventories[to_guid].add(this.entries[entry], entry);
            }
            else {
                if (!this.entries.ContainsKey(to_guid)) { throw new ArgumentOutOfRangeException(nameof(to_guid)); }
                if (this.entries[to_guid] is not SingleItem to_itm) { throw new ArgumentOutOfRangeException(nameof(to_guid)); }
                to_itm.add(to_idx.Value, this.entries[entry], entry);
            }

            // we've already done error-checking, so just remove
            if (from_idx is null) {
                this.inventories[from_guid].remove(entry);
            }
            else {
                SingleItem from_itm = this.entries[from_guid] as SingleItem;
                from_itm.remove(from_idx.Value, entry);
            }
        }
        public void move_entry(Guid entry, Guid from_guid, Guid to_guid, int? to_idx = null) {
            this.move_entry(entry, from_guid, null, to_guid, to_idx);
        }

        public void restore_entry(Guid entry, Guid to_guid, int? to_idx = null) {
            if (!this.entries.ContainsKey(entry)) { throw new ArgumentOutOfRangeException(nameof(entry)); }
            if (this.active_entries.Contains(entry)) { throw new ArgumentOutOfRangeException(nameof(entry)); }

            if (to_idx is null) {
                if (!this.inventories.ContainsKey(to_guid)) { throw new ArgumentOutOfRangeException(nameof(to_guid)); }
                this.inventories[to_guid].add(this.entries[entry], entry);
            }
            else {
                if (!this.entries.ContainsKey(to_guid)) { throw new ArgumentOutOfRangeException(nameof(to_guid)); }
                if (this.entries[to_guid] is not SingleItem to_itm) { throw new ArgumentOutOfRangeException(nameof(to_guid)); }
                to_itm.add(to_idx.Value, this.entries[entry], entry);
            }
            this.active_entries.Add(entry);
        }

        private void add_item(ItemSpec item) {
            if (item is null) { throw new ArgumentNullException(nameof(item)); }
            if (item.category is null) { throw new ArgumentOutOfRangeException(nameof(item)); }
            if (!this.items.ContainsKey(item.category)) {
                this.items[item.category] = new List<ItemSpec>();
            }
            if (this.items[item.category].Contains(item)) { return; }
            this.items[item.category].Add(item);
            this.items[item.category].Sort((x, y) => x.name.CompareTo(y.name));
        }

        public Guid add_entry(Guid to_guid, int? to_idx, InventoryEntry entry, Guid? guid = null) {
            if (entry is null) { throw new ArgumentNullException(nameof(entry)); }
            if ((guid is not null) && (this.active_entries.Contains(guid.Value))) { throw new ArgumentOutOfRangeException(nameof(guid)); }
            if (this.entries.ContainsValue(entry)) { throw new ArgumentOutOfRangeException(nameof(entry)); }

            this.add_item(entry.item);
            Guid ent_guid = guid ?? Guid.NewGuid();
            this.entries[ent_guid] = entry;
            this.restore_entry(ent_guid, to_guid, to_idx);
            return ent_guid;
        }
        public Guid add_entry(Guid to_guid, InventoryEntry entry, Guid? guid = null) {
            return this.add_entry(to_guid, null, entry, guid);
        }

        public void remove_entry(Guid entry, Guid from_guid, int? from_idx = null) {
            if (!this.entries.ContainsKey(entry)) { throw new ArgumentOutOfRangeException(nameof(entry)); }
            if (!this.active_entries.Contains(entry)) { throw new ArgumentOutOfRangeException(nameof(entry)); }

            if (from_idx is null) {
                if (!this.inventories.ContainsKey(from_guid)) { throw new ArgumentOutOfRangeException(nameof(from_guid)); }
                this.inventories[from_guid].remove(entry);
            }
            else {
                if (!this.entries.ContainsKey(from_guid)) { throw new ArgumentOutOfRangeException(nameof(from_guid)); }
                if (this.entries[from_guid] is not SingleItem from_itm) { throw new ArgumentOutOfRangeException(nameof(from_guid)); }
                from_itm.remove(from_idx.Value, entry);
            }
            this.purge_inventory_entry(this.entries[entry]);
            this.active_entries.Remove(entry);
        }

        public Guid merge_entries(Guid in_guid, int? in_idx, Guid ent1, Guid ent2, Guid? guid = null) {
            if ((!this.entries.ContainsKey(ent1)) || (!this.active_entries.Contains(ent1))) { throw new ArgumentOutOfRangeException(nameof(ent1)); }
            if ((!this.entries.ContainsKey(ent2)) || (!this.active_entries.Contains(ent2))) { throw new ArgumentOutOfRangeException(nameof(ent2)); }
            if ((guid is not null) && (guid != ent1) && (guid != ent2) && (this.entries.ContainsKey(guid.Value))) { throw new ArgumentOutOfRangeException(nameof(guid)); }

            Inventory inv = this.get_inventory(in_guid, in_idx);
            Guid result = inv.merge(ent1, ent2, guid);
            if (!inv.contents.ContainsKey(ent1)) {
                this.active_entries.Remove(ent1);
            }
            if (!inv.contents.ContainsKey(ent2)) {
                this.active_entries.Remove(ent2);
            }
            if (!this.entries.ContainsKey(result)) {
                this.entries[result] = inv.contents[result];
            }
            this.active_entries.Add(result);
            return result;
        }
        public Guid merge_entries(Guid in_guid, Guid ent1, Guid ent2, Guid? guid = null) {
            return this.merge_entries(in_guid, null, ent1, ent2, guid);
        }

        public Guid unstack_entry(Guid in_guid, int? in_idx, Guid ent, Guid? guid = null) {
            if ((!this.entries.ContainsKey(ent)) || (!this.active_entries.Contains(ent))) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            if ((guid is not null) && (this.active_entries.Contains(guid.Value))) { throw new ArgumentOutOfRangeException(nameof(guid)); }

            Inventory inv = this.get_inventory(in_guid, in_idx);
            Guid result = inv.unstack(ent, guid);
            this.active_entries.Remove(ent);
            if (!this.entries.ContainsKey(result)) {
                this.entries[result] = inv.contents[result];
            }
            this.active_entries.Add(result);
            return result;
        }
        public Guid unstack_entry(Guid in_guid, Guid ent, Guid? guid = null) {
            return this.unstack_entry(in_guid, null, ent, guid);
        }

        public Guid split_entry(Guid in_guid, int? in_idx, Guid ent, long count, long unidentified, Guid? guid = null) {
            if ((!this.entries.ContainsKey(ent)) || (!this.active_entries.Contains(ent))) { throw new ArgumentOutOfRangeException(nameof(ent)); }
            if ((guid is not null) && (this.active_entries.Contains(guid.Value))) { throw new ArgumentOutOfRangeException(nameof(guid)); }

            Inventory inv = this.get_inventory(in_guid, in_idx);
            Guid result = inv.split(ent, count, unidentified, guid);
            if (!this.entries.ContainsKey(result)) {
                this.entries[result] = inv.contents[result];
            }
            this.active_entries.Add(result);
            return result;
        }
        public Guid split_entry(Guid in_guid, Guid ent, long count, long unidentified, Guid? guid = null) {
            return this.split_entry(in_guid, null, ent, count, unidentified, guid);
        }
    }
}