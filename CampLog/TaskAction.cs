using System;
using System.Collections.Generic;

namespace CampLog {
    [Serializable]
    public class ActionTaskCreate : EntryAction {
        public readonly Guid guid;
        public readonly Task task;

        public override string description { get => "Add task"; }

        public ActionTaskCreate(Guid guid, Task task) {
            if (task is null) { throw new ArgumentNullException(nameof(task)); }
            this.guid = guid;
            this.task = task.copy();
        }

        public override void apply(CampaignState state, Entry ent) {
            state.tasks.add_task(this.task.copy(), this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.tasks.remove_task(this.guid);
            state.tasks.tasks.Remove(this.guid);
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionTaskRemove ext_task_remove) {
                    if (ext_task_remove.guid == this.guid) {
                        // existing ActionTaskRemove with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionTaskRemove : EntryAction {
        public readonly Guid guid;

        public override string description { get => "Remove task"; }

        public ActionTaskRemove(Guid guid) {
            this.guid = guid;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.tasks.remove_task(this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.tasks.restore_task(this.guid);
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionTaskCreate ext_task_create) {
                    if (ext_task_create.guid == this.guid) {
                        // existing ActionTaskCreate with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
                if (actions[i] is ActionTaskRestore ext_task_restore) {
                    if (ext_task_restore.guid == this.guid) {
                        // existing ActionTaskRestore with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
                if (actions[i] is ActionTaskUpdate ext_task_update) {
                    if (ext_task_update.guid == this.guid) {
                        // existing ActionTaskUpdate with this guid; remove it
                        actions.RemoveAt(i);
                        continue;
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionTaskRestore : EntryAction {
        public readonly Guid guid;

        public override string description { get => "Restore task"; }

        public ActionTaskRestore(Guid guid) {
            this.guid = guid;
        }

        public override void apply(CampaignState state, Entry ent) {
            state.tasks.restore_task(this.guid);
        }

        public override void revert(CampaignState state, Entry ent) {
            state.tasks.remove_task(this.guid);
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionTaskRemove ext_task_remove) {
                    if (ext_task_remove.guid == this.guid) {
                        // existing ActionTaskRemove with this guid; remove it and we're done
                        actions.RemoveAt(i);
                        return;
                    }
                }
            }
            actions.Add(this);
        }
    }


    [Serializable]
    public class ActionTaskUpdate : EntryAction {
        public readonly Guid guid;
        public Task from;
        public readonly Task to;
        public readonly bool set_name;
        public readonly bool set_desc;
        public readonly bool set_completed;
        public readonly bool set_failed;
        public readonly bool set_due;

        public override string description { get => "Update task"; }

        public ActionTaskUpdate(Guid guid, Task from, Task to, bool set_name, bool set_desc, bool set_completed, bool set_failed, bool set_due) {
            if (from is null) { throw new ArgumentNullException(nameof(from)); }
            if (to is null) { throw new ArgumentNullException(nameof(to)); }
            if (!(set_name || set_desc || set_completed || set_failed || set_due)) { throw new ArgumentOutOfRangeException(nameof(set_due)); }
            this.guid = guid;
            this.from = from.copy();
            this.to = to.copy();
            this.set_name = set_name;
            this.set_desc = set_desc;
            this.set_completed = set_completed;
            this.set_failed = set_failed;
            this.set_due = set_due;
        }

        public override void rebase(CampaignState state) {
            if (!state.tasks.tasks.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Task task = state.tasks.tasks[this.guid];
            if (this.set_name) { this.from.name = task.name; }
            if (this.set_desc) { this.from.description = task.description; }
            if (this.set_completed) { this.from.completed_guid = task.completed_guid; }
            if (this.set_failed) { this.from.failed = task.failed; }
            if (this.set_due) { this.from.due = task.due; }
        }

        public override void apply(CampaignState state, Entry ent) {
            if (!state.tasks.tasks.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Task task = state.tasks.tasks[this.guid];
            if (this.set_name) { task.name = this.to.name; }
            if (this.set_desc) { task.description = this.to.description; }
            if (this.set_completed) { task.completed_guid = this.to.completed_guid; }
            if (this.set_failed) { task.failed = this.to.failed; }
            if (this.set_due) { task.due = this.to.due; }
        }

        public override void revert(CampaignState state, Entry ent) {
            if (!state.tasks.tasks.ContainsKey(this.guid)) { throw new ArgumentOutOfRangeException(); }
            Task task = state.tasks.tasks[this.guid];
            if (this.set_name) { task.name = this.from.name; }
            if (this.set_desc) { task.description = this.from.description; }
            if (this.set_completed) { task.completed_guid = this.from.completed_guid; }
            if (this.set_failed) { task.failed = this.from.failed; }
            if (this.set_due) { task.due = this.from.due; }
        }

        public override void merge_to(List<EntryAction> actions) {
            for (int i = actions.Count - 1; i >= 0; i--) {
                if (actions[i] is ActionTaskCreate ext_task_create) {
                    if (ext_task_create.guid == this.guid) {
                        // existing ActionTaskCreate with this guid; update its "task" field based on our "to" field and we're done
                        if (this.set_name) { ext_task_create.task.name = this.to.name; }
                        if (this.set_desc) { ext_task_create.task.description = this.to.description; }
                        if (this.set_completed) { ext_task_create.task.completed_guid = this.to.completed_guid; }
                        if (this.set_failed) { ext_task_create.task.failed = this.to.failed; }
                        if (this.set_due) { ext_task_create.task.due = this.to.due; }
                        return;
                    }
                }
                if (actions[i] is ActionTaskUpdate ext_task_update) {
                    if (ext_task_update.guid == this.guid) {
                        // existing ActionTaskUpdate with this guid; replace with a new adjust action with the sum of both adjustments
                        string from_name = ext_task_update.from.name, to_name = ext_task_update.to.name;
                        string from_description = ext_task_update.from.description, to_description = ext_task_update.to.description;
                        Guid? from_completed = ext_task_update.from.completed_guid, to_completed = ext_task_update.to.completed_guid;
                        bool from_failed = ext_task_update.from.failed, to_failed = ext_task_update.to.failed;
                        decimal? from_due = ext_task_update.from.due, to_due = ext_task_update.to.due;
                        bool set_name = ext_task_update.set_name || this.set_name, set_desc = ext_task_update.set_desc || this.set_desc,
                            set_completed = ext_task_update.set_completed || this.set_completed, set_failed = ext_task_update.set_failed || this.set_failed,
                            set_due = ext_task_update.set_due || this.set_due;
                        if (this.set_name) {
                            if (!ext_task_update.set_name) { from_name = this.from.name; }
                            to_name = this.to.name;
                        }
                        if (this.set_desc) {
                            if (!ext_task_update.set_desc) { from_description = this.from.description; }
                            to_description = this.to.description;
                        }
                        if (this.set_completed) {
                            if (!ext_task_update.set_completed) { from_completed = this.from.completed_guid; }
                            to_completed = this.to.completed_guid;
                        }
                        if (this.set_failed) {
                            if (!ext_task_update.set_failed) { from_failed = this.from.failed; }
                            to_failed = this.to.failed;
                        }
                        if (this.set_due) {
                            if (!ext_task_update.set_due) { from_due = this.from.due; }
                            to_due = this.to.due;
                        }
                        if ((set_name) && (from_name == to_name)) {
                            set_name = false;
                            from_name = null;
                            to_name = null;
                        }
                        if ((set_desc) && (from_description == to_description)) {
                            set_desc = false;
                            from_description = null;
                            to_description = null;
                        }
                        if ((set_completed) && (from_completed == to_completed)) { set_completed = false; }
                        if ((set_failed) && (from_failed == to_failed)) { set_failed = false; }
                        if ((set_due) && (from_due == to_due)) {
                            set_due = false;
                            from_due = null;
                            to_due = null;
                        }
                        actions.RemoveAt(i);
                        if ((set_name) || (set_desc) || (set_completed) || (set_failed) || (set_due)) {
                            Task from = new Task(ext_task_update.from.entry_guid, from_name, from_description, from_due),
                                to = new Task(this.to.entry_guid, to_name, to_description, to_due);
                            if (set_completed) {
                                from.completed_guid = from_completed;
                                to.completed_guid = to_completed;
                            }
                            if (set_failed) {
                                from.failed = from_failed;
                                to.failed = to_failed;
                            }
                            new ActionTaskUpdate(this.guid, from, to, set_name, set_desc, set_completed, set_failed, set_due).merge_to(actions);
                        }
                        return;
                    }
                }
            }
            actions.Add(this);
        }
    }
}