using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestActionTaskCreate {
        [TestMethod]
        public void test_serialization() {
            Task task = new Task(Guid.NewGuid(), "Some task");
            ActionTaskCreate foo = new ActionTaskCreate(Guid.NewGuid(), task), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionTaskCreate));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionTaskCreate)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.task.name, bar.task.name);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(ent.guid, "Some task");
            Guid task_guid = Guid.NewGuid();
            ActionTaskCreate action = new ActionTaskCreate(task_guid, task);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            Assert.AreEqual(state.tasks.tasks.Count, 1);
            Assert.IsTrue(state.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(state.tasks.tasks[task_guid].name, "Some task");
            Assert.IsFalse(ReferenceEquals(state.tasks.tasks[task_guid], task));
            Assert.AreEqual(state.tasks.active_tasks.Count, 1);
            Assert.IsTrue(state.tasks.active_tasks.Contains(task_guid));
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(ent.guid, "Some task");
            Guid task_guid = Guid.NewGuid();
            ActionTaskCreate action = new ActionTaskCreate(task_guid, task);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.tasks.tasks.Count, 0);
            Assert.AreEqual(state.tasks.active_tasks.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_remove_create() {
            Guid task_guid = Guid.NewGuid();
            ActionTaskRemove remove_action = new ActionTaskRemove(task_guid);
            List<EntryAction> actions = new List<EntryAction>() { remove_action };

            Task task = new Task(Guid.NewGuid(), "Some task");
            ActionTaskCreate create_action = new ActionTaskCreate(task_guid, task);
            create_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }
    }


    [TestClass]
    public class TestActionTaskRemove {
        [TestMethod]
        public void test_serialization() {
            ActionTaskRemove foo = new ActionTaskRemove(Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionTaskRemove));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionTaskRemove)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(ent.guid, "Some task");
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskRemove action = new ActionTaskRemove(task_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.tasks.tasks.Count, 1);
            Assert.IsTrue(state.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(state.tasks.tasks[task_guid].name, "Some task");
            Assert.AreEqual(state.tasks.active_tasks.Count, 0);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(ent.guid, "Some task");
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskRemove action = new ActionTaskRemove(task_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.tasks.tasks.Count, 1);
            Assert.IsTrue(state.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(state.tasks.tasks[task_guid].name, "Some task");
            Assert.AreEqual(state.tasks.active_tasks.Count, 1);
            Assert.IsTrue(state.tasks.active_tasks.Contains(task_guid));
        }

        [TestMethod]
        public void test_merge_to_create_remove() {
            Guid task_guid = Guid.NewGuid();
            Task task = new Task(Guid.NewGuid(), "Some task");
            ActionTaskCreate create_action = new ActionTaskCreate(task_guid, task);
            List<EntryAction> actions = new List<EntryAction>() { create_action };

            ActionTaskRemove remove_action = new ActionTaskRemove(task_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_restore_remove() {
            Guid task_guid = Guid.NewGuid();
            ActionTaskRestore restore_action = new ActionTaskRestore(task_guid);
            List<EntryAction> actions = new List<EntryAction>() { restore_action };

            ActionTaskRemove remove_action = new ActionTaskRemove(task_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_update_remove() {
            Guid task_guid = Guid.NewGuid();
            Task from = new Task(Guid.NewGuid(), "Some task"), to = new Task(Guid.NewGuid(), "Some updated task");
            ActionTaskUpdate update_action = new ActionTaskUpdate(task_guid, from, to, true, false, false, false, false);
            List<EntryAction> actions = new List<EntryAction>() { update_action };

            ActionTaskRemove remove_action = new ActionTaskRemove(task_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            Assert.IsTrue(ReferenceEquals(actions[0], remove_action));
        }
    }


    [TestClass]
    public class TestActionTaskRestore {
        [TestMethod]
        public void test_serialization() {
            ActionTaskRestore foo = new ActionTaskRestore(Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionTaskRestore));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionTaskRestore)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(ent.guid, "Some task");
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskRestore action = new ActionTaskRestore(task_guid);

            state.tasks.remove_task(task_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.tasks.tasks.Count, 1);
            Assert.IsTrue(state.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(state.tasks.tasks[task_guid].name, "Some task");
            Assert.AreEqual(state.tasks.active_tasks.Count, 1);
            Assert.IsTrue(state.tasks.active_tasks.Contains(task_guid));
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(ent.guid, "Some task");
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskRestore action = new ActionTaskRestore(task_guid);

            state.tasks.remove_task(task_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.tasks.tasks.Count, 1);
            Assert.IsTrue(state.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(state.tasks.tasks[task_guid].name, "Some task");
            Assert.AreEqual(state.tasks.active_tasks.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_remove_restore() {
            Guid task_guid = Guid.NewGuid();
            ActionTaskRemove remove_action = new ActionTaskRemove(task_guid);
            List<EntryAction> actions = new List<EntryAction>() { remove_action };

            ActionTaskRestore restore_action = new ActionTaskRestore(task_guid);
            restore_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }
    }


    [TestClass]
    public class TestActionTaskUpdate {
        [TestMethod]
        public void test_serialization() {
            Task old_task = new Task(Guid.NewGuid(), "Some task"), new_task = new Task(old_task.entry_guid, "", "Do something detailed");
            ActionTaskUpdate foo = new ActionTaskUpdate(Guid.NewGuid(), old_task, new_task, false, true, false, false, false), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionTaskUpdate));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionTaskUpdate)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.from.name, bar.from.name);
            Assert.AreEqual(foo.to.description, bar.to.description);
            Assert.AreEqual(foo.set_name, bar.set_name);
            Assert.AreEqual(foo.set_desc, bar.set_desc);
            Assert.AreEqual(foo.set_completed, bar.set_completed);
            Assert.AreEqual(foo.set_failed, bar.set_failed);
            Assert.AreEqual(foo.set_due, bar.set_due);
        }

        [TestMethod]
        public void test_rebase() {
            Task task = new Task(Guid.NewGuid(), "Some task", "Do a thing", 42), new_task = new Task(task.entry_guid, "Some updated task", "Do an updated thing", 84);
            task.failed = true;
            new_task.completed_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskUpdate action = new ActionTaskUpdate(task_guid, task, new_task, true, true, true, true, true);
            action.from.description = "Do a modified thing";

            action.rebase(state);
            Assert.AreEqual(action.from.description, "Do a thing");
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(Guid.NewGuid(), "Some task", "Do a thing", 42), new_task = new Task(task.entry_guid, "Some updated task", "Do an updated thing", 84);
            task.failed = true;
            new_task.completed_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskUpdate action = new ActionTaskUpdate(task_guid, task, new_task, true, true, true, true, true);

            action.apply(state, ent);
            Assert.AreEqual(task.name, "Some updated task");
            Assert.AreEqual(task.description, "Do an updated thing");
            Assert.IsNotNull(task.completed_guid);
            Assert.AreEqual(task.completed_guid, new_task.completed_guid);
            Assert.IsFalse(task.failed);
            Assert.AreEqual(task.due, 84);
        }

        [TestMethod]
        public void test_apply_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(Guid.NewGuid(), "Some task"), new_task = new Task(task.entry_guid, "Some updated task", "Do something detailed");
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskUpdate action = new ActionTaskUpdate(task_guid, task, new_task, false, true, false, false, false);

            action.apply(state, ent);
            Assert.AreEqual(task.name, "Some task");
            Assert.AreEqual(task.description, "Do something detailed");
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(Guid.NewGuid(), "Some task", "Do a thing", 42), new_task = new Task(task.entry_guid, "Some updated task", "Do an updated thing", 84);
            task.failed = true;
            new_task.completed_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskUpdate action = new ActionTaskUpdate(task_guid, task, new_task, true, true, true, true, true);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(task.name, "Some task");
            Assert.AreEqual(task.description, "Do a thing");
            Assert.IsNull(task.completed_guid);
            Assert.IsTrue(task.failed);
            Assert.AreEqual(task.due, 42);
        }

        [TestMethod]
        public void test_revert_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Task task = new Task(Guid.NewGuid(), "Some task"), new_task = new Task(task.entry_guid, "Some updated task", "Do something detailed");
            CampaignState state = new CampaignState();
            Guid task_guid = state.tasks.add_task(task);
            ActionTaskUpdate action = new ActionTaskUpdate(task_guid, task, new_task, false, true, false, false, false);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(task.name, "Some task");
            Assert.IsNull(task.description);
        }

        [TestMethod]
        public void test_merge_to_create_update() {
            Guid task_guid = Guid.NewGuid();
            Task from = new Task(Guid.NewGuid(), "Some task"), to = new Task(Guid.NewGuid(), "Some updated task");
            ActionTaskCreate create_action = new ActionTaskCreate(task_guid, from);
            List<EntryAction> actions = new List<EntryAction>() { create_action };

            ActionTaskUpdate update_action = new ActionTaskUpdate(task_guid, from, to, true, false, false, false, false);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionTaskCreate merged_action = actions[0] as ActionTaskCreate;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, task_guid);
            Assert.AreEqual(merged_action.task.entry_guid, from.entry_guid);
            Assert.AreEqual(merged_action.task.name, "Some updated task");
        }

        [TestMethod]
        public void test_merge_to_update_update() {
            Guid task_guid = Guid.NewGuid();
            Task from1 = new Task(Guid.NewGuid(), "Some task", "Do something"), to1 = new Task(Guid.NewGuid(), "Some updated task", "Do something new"),
                from2 = new Task(Guid.NewGuid(), "", "Do something new"), to2 = new Task(Guid.NewGuid(), "", "Do something fast", 42);
            ActionTaskUpdate existing_action = new ActionTaskUpdate(task_guid, from1, to1, true, true, false, false, false);
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            ActionTaskUpdate update_action = new ActionTaskUpdate(task_guid, from2, to2, false, true, false, false, true);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionTaskUpdate merged_action = actions[0] as ActionTaskUpdate;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, task_guid);
            Assert.IsTrue(merged_action.set_name);
            Assert.AreEqual(merged_action.from.name, "Some task");
            Assert.AreEqual(merged_action.to.name, "Some updated task");
            Assert.IsTrue(merged_action.set_desc);
            Assert.AreEqual(merged_action.from.description, "Do something");
            Assert.AreEqual(merged_action.to.description, "Do something fast");
            Assert.IsFalse(merged_action.set_completed);
            Assert.IsFalse(merged_action.set_failed);
            Assert.IsTrue(merged_action.set_due);
            Assert.IsNull(merged_action.from.due);
            Assert.IsNotNull(merged_action.to.due);
            Assert.AreEqual(merged_action.to.due, 42);
        }
    }
}