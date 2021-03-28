using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [KnownType(typeof(CharTextProperty))]
    [KnownType(typeof(CharNumProperty))]
    [KnownType(typeof(CharSetProperty))]
    [KnownType(typeof(CharDictProperty))]
    [Serializable]
    public abstract class CharProperty {
        public abstract CharProperty copy();
        public abstract void add(CharProperty prop);
        public abstract void subtract(CharProperty prop);
    }


    [Serializable]
    public class CharTextProperty : CharProperty {
        public string value;

        public CharTextProperty(string value) {
            this.value = value;
        }

        public override CharTextProperty copy() {
            return new CharTextProperty(this.value);
        }

        public override void add(CharProperty prop) {
            CharTextProperty p = prop as CharTextProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            this.value += p.value;
        }

        public override void subtract(CharProperty prop) {
            CharTextProperty p = prop as CharTextProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            this.value = this.value.Replace(p.value, "");
        }
    }


    [Serializable]
    public class CharNumProperty : CharProperty {
        public decimal value;

        public CharNumProperty(decimal value) {
            this.value = value;
        }

        public override CharNumProperty copy() {
            return new CharNumProperty(this.value);
        }

        public override void add(CharProperty prop) {
            CharNumProperty p = prop as CharNumProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            this.value += p.value;
        }

        public override void subtract(CharProperty prop) {
            CharNumProperty p = prop as CharNumProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            this.value -= p.value;
        }
    }


    [Serializable]
    public class CharSetProperty : CharProperty {
        public RefcountSet<string> value;

        public CharSetProperty() {
            this.value = new RefcountSet<string>();
        }

        public override CharSetProperty copy() {
            return new CharSetProperty {
                value = new RefcountSet<string>(this.value.contents)
            };
        }

        public override void add(CharProperty prop) {
            CharSetProperty p = prop as CharSetProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            this.value.AddRefs(p.value);
        }

        public override void subtract(CharProperty prop) {
            CharSetProperty p = prop as CharSetProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            this.value.SubtractRefs(p.value);
        }
    }


    [Serializable]
    public class CharDictProperty : CharProperty {
        public Dictionary<string, CharProperty> value;

        public CharDictProperty() {
            this.value = new Dictionary<string, CharProperty>();
        }

        public override CharDictProperty copy() {
            CharDictProperty result = new CharDictProperty();
            foreach (string s in this.value.Keys) {
                result.value[s] = this.value[s].copy();
            }
            return result;
        }

        public override void add(CharProperty prop) {
            CharDictProperty p = prop as CharDictProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            foreach (string k in p.value.Keys) {
                this.value[k] = p.value[k].copy();
            }
        }

        public override void subtract(CharProperty prop) {
            CharDictProperty p = prop as CharDictProperty;
            if (p is null) { throw new ArgumentOutOfRangeException(nameof(prop)); }
            foreach (string k in p.value.Keys) {
                this.value.Remove(k);
            }
        }
    }


    [Serializable]
    public class Character {
        public string name;
        public CharDictProperty properties;

        public Character(string name) {
            this.name = name;
            this.properties = new CharDictProperty();
        }

        public Character copy() {
            return new Character(this.name) {
                properties = this.properties.copy()
            };
        }

        public CharProperty get_property(List<string> path) {
            CharProperty prop = this.properties;

            foreach (string s in path) {
                if (prop is not CharDictProperty dict_prop) { throw new ArgumentOutOfRangeException(nameof(path)); }
                if (!dict_prop.value.ContainsKey(s)) { throw new ArgumentOutOfRangeException(nameof(path)); }
                prop = dict_prop.value[s];
            }
            return prop;
        }

        public CharProperty set_property(List<string> path, CharProperty prop) {
            CharProperty result = null;
            CharDictProperty dict_prop = this.properties;
            int i;

            if (prop is null) { throw new ArgumentNullException(nameof(prop)); }

            for (i = 0; i < path.Count - 1; i++) {
                if (!dict_prop.value.ContainsKey(path[i])) { throw new ArgumentOutOfRangeException(nameof(path)); }
                dict_prop = dict_prop.value[path[i]] as CharDictProperty;
                if (dict_prop is null) { throw new ArgumentOutOfRangeException(nameof(path)); }
            }

            if (dict_prop.value.ContainsKey(path[i])) {
                result = dict_prop.value[path[i]];
            }
            dict_prop.value[path[i]] = prop;

            return result;
        }

        public CharProperty remove_property(List<string> path) {
            CharDictProperty dict_prop = this.properties;
            int i;

            for (i = 0; i < path.Count - 1; i++) {
                if (!dict_prop.value.ContainsKey(path[i])) { throw new ArgumentOutOfRangeException(nameof(path)); }
                dict_prop = dict_prop.value[path[i]] as CharDictProperty;
                if (dict_prop is null) { throw new ArgumentOutOfRangeException(nameof(path)); }
            }
            if (!dict_prop.value.ContainsKey(path[i])) { throw new ArgumentOutOfRangeException(nameof(path)); }

            CharProperty result = dict_prop.value[path[i]];
            dict_prop.value.Remove(path[i]);

            return result;
        }
    }


    [Serializable]
    public class CharacterDomain : BaseDomain<Character> {
        public Dictionary<Guid, Character> characters {
            get => this.items;
            set => this.items = value;
        }
        public HashSet<Guid> active_characters {
            get => this.active_items;
            set => this.active_items = value;
        }

        public CharacterDomain copy() {
            CharacterDomain result = new CharacterDomain() {
                active_items = new HashSet<Guid>(this.active_items)
            };
            foreach (Guid guid in this.items.Keys) {
                result.items[guid] = this.items[guid].copy();
            }
            return result;
        }

        public Guid add_character(Character chr, Guid? guid = null) => this.add_item(chr, guid);

        public void remove_character(Guid guid) => this.remove_item(guid);

        public void restore_character(Guid guid) => this.restore_item(guid);
    }
}