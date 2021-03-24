﻿using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class Task {
        public Guid event_guid;
        public string name;
        public string description;
        public bool completed;
        public bool failed;
        public decimal? due;

        public Task(Guid event_guid, string name, string description = null, decimal? due = null) {
            if (name is null) { throw new ArgumentNullException(nameof(name)); }
            this.event_guid = event_guid;
            this.name = name;
            this.description = description;
            this.completed = false;
            this.failed = false;
            this.due = due;
        }

        public Task copy() {
            return new Task(this.event_guid, this.name, this.description, this.due) {
                completed = this.completed,
                failed = this.failed,
            };
        }
    }


    [Serializable]
    public class TaskDomain : BaseDomain<Task> {
        public Dictionary<Guid, Task> tasks {
            get => this.items;
            set => this.items = value;
        }
        public HashSet<Guid> active_tasks {
            get => this.active_items;
            set => this.active_items = value;
        }

        public Guid add_task(Task task, Guid? guid = null) => this.add_item(task, guid);

        public void remove_task(Guid guid) => this.remove_item(guid);

        public void restore_task(Guid guid) => this.restore_item(guid);
    }
}