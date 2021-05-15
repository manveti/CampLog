using System;

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
    }
}