using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CampLog {
    [DataContract]
    public class RefcountSet<T> : IEnumerable<T> {
        [DataMember] public Dictionary<T, int> contents;
        public int Count { get => this.contents.Count; }

        public RefcountSet() {
            this.contents = new Dictionary<T, int>();
        }

        public RefcountSet(Dictionary<T, int> contents) {
            this.contents = new Dictionary<T, int>(contents);
        }

        public RefcountSet(IEnumerable<T> contents) {
            this.contents = new Dictionary<T, int>();
            foreach (T item in contents) {
                this.contents[item] = 1;
            }
        }

        public IEnumerator<T> GetEnumerator() => this.contents.Keys.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.contents.Keys.GetEnumerator();

        public bool Add(T item, int count = 1) {
            if (this.contents.ContainsKey(item)) {
                this.contents[item] += count;
                return false;
            }
            this.contents[item] = count;
            return true;
        }

        public void Clear() => this.contents.Clear();

        public bool Contains(T item) => this.contents.ContainsKey(item);

        public void ExceptWith(IEnumerable<T> other) {
            foreach (T item in other) {
                this.contents.Remove(item);
            }
        }

        public void ExceptRefWith(IEnumerable<T> other) {
            foreach (T item in other) {
                this.RemoveRef(item);
            }
        }

        public bool Remove(T item) => this.contents.Remove(item);

        public bool RemoveRef(T item, int count = 1) {
            if (!this.contents.ContainsKey(item)) { return false; }
            this.contents[item] -= count;
            if (this.contents[item] <= 0) { this.contents.Remove(item); }
            return true;
        }

        public void UnionWith(IEnumerable<T> other) {
            foreach (T item in other) {
                this.Add(item);
            }
        }
    }
}