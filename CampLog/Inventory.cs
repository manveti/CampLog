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
    }


    [DataContract(IsReference = true)]
    public class ItemStack : InventoryEntry {
        [DataMember] public ulong count;
        [DataMember] public ulong unidentified;

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

        public ItemStack(ItemSpec item, ulong count = 1, ulong unidentified = 0) {
            if (item is null) { throw new ArgumentNullException(nameof(item)); }
            if (unidentified > count) { throw new ArgumentOutOfRangeException(nameof(unidentified)); }

            this.item = item;
            this.count = count;
            this.unidentified = unidentified;
        }
    }


    [DataContract(IsReference = true)]
    public class SingleItem : InventoryEntry {
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
        public override string name { get => this.item.name; }
        public override decimal weight { get => this.item.weight + this.contents_weight; }
        public override decimal value { get => (this.value_override ?? this.item.value) + this.contents_value; }

        public SingleItem(ItemSpec item, decimal? value_override = null) {
            if (item is null) { throw new ArgumentNullException(nameof(item)); }

            this.item = item;
            this.value_override = value_override;
            if (item.containers is not null) {
                this.containers = new Inventory[item.containers.Length];
                for (int i = 0; i < this.containers.Length; i++) { this.containers[i] = new Inventory(item.containers[i].name); }
            }
            this.properties = new Dictionary<string, string>();
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
}