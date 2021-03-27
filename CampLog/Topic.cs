using System;

namespace CampLog {
    [Serializable]
    public class Topic {
        public string name;
        public string description;

        public Topic(string name, string description = null) {
            if (name is null) { throw new ArgumentNullException(nameof(name)); }
            this.name = name;
            this.description = description;
        }
    }
}