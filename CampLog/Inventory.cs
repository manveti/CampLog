using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [DataContract(IsReference=true)]
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
        [DataMember] public decimal weight_capacity;
        [DataMember] public decimal weight_factor;

        public ContainerSpec(string name, decimal weight_capacity, decimal weight_factor) {
            this.name = name;
            this.weight_capacity = weight_capacity;
            this.weight_factor = weight_factor;
        }

        public override bool Equals(object obj) => obj is ContainerSpec spec && this.name == spec.name && this.weight_capacity == spec.weight_capacity && this.weight_factor == spec.weight_factor;
        public override int GetHashCode() => HashCode.Combine(this.name, this.weight_capacity, this.weight_factor);
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
        public decimal value { get => this.sale_value ?? this.cost * this.category.sale_value; }
        [DataMember] public decimal weight;
        [DataMember] public ContainerSpec[] containers;
        [DataMember] public Dictionary<string, string> properties;

        public ItemSpec(string name, ItemCategory category, decimal cost, decimal weight, decimal? sale_value = null, ContainerSpec[] containers = null) {
            this.name = name;
            this.category = category;
            this.cost = cost;
            this.weight = weight;
            this.sale_value = sale_value;
            this.containers = containers;
        }

        public override bool Equals(object obj) {
            ItemSpec spec = (ItemSpec)obj;
            if (spec is null || this.name != spec.name || this.category != spec.category || this.cost != spec.cost || this .sale_value != spec.sale_value || this.weight != spec.weight) { return false; }
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
}