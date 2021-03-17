using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [KnownType(typeof(CharTextProperty))]
    [KnownType(typeof(CharNumProperty))]
    [KnownType(typeof(CharSetProperty))]
    [KnownType(typeof(CharDictProperty))]
    [Serializable]
    public abstract class CharProperty { }


    [Serializable]
    public class CharTextProperty : CharProperty {
        public string value;

        public CharTextProperty(string value) {
            this.value = value;
        }
    }


    [Serializable]
    public class CharNumProperty : CharProperty {
        public decimal value;

        public CharNumProperty(decimal value) {
            this.value = value;
        }
    }


    [Serializable]
    public class CharSetProperty : CharProperty {
        public HashSet<string> value;

        public CharSetProperty() {
            this.value = new HashSet<string>();
        }
    }


    [Serializable]
    public class CharDictProperty : CharProperty {
        public Dictionary<string, CharProperty> value;

        public CharDictProperty() {
            this.value = new Dictionary<string, CharProperty>();
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
}