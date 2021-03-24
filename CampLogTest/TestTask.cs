using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestTask {
        [TestMethod]
        public void test_serialization() {
            Task foo = new Task(Guid.NewGuid(), "Do a thing", "Go to a place and do a thing for a person", 42), bar;

            foo.completed = true;

            DataContractSerializer fmt = new DataContractSerializer(typeof(Task));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (Task)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.event_guid, bar.event_guid);
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.description, bar.description);
            Assert.AreEqual(foo.completed, bar.completed);
            Assert.AreEqual(foo.failed, bar.failed);
            Assert.AreEqual(foo.due, bar.due);
        }

        [TestMethod]
        public void test_copy() {
            Task foo = new Task(Guid.NewGuid(), "Do a thing", "Go to a place and do a thing for a person", 42), bar;

            foo.completed = true;

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.AreEqual(foo.event_guid, bar.event_guid);
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.description, bar.description);
            Assert.AreEqual(foo.completed, bar.completed);
            Assert.AreEqual(foo.failed, bar.failed);
            Assert.AreEqual(foo.due, bar.due);
        }
    }


    [TestClass]
    public class TestTaskDomain {
        [TestMethod]
        public void test_serialization() {
            Task t1 = new Task(Guid.NewGuid(), "Some current task"), t2 = new Task(Guid.NewGuid(), "Some former task");
            TaskDomain foo = new TaskDomain(), bar;

            foo.add_task(t1);
            Guid rem_guid = foo.add_task(t2);
            foo.remove_task(rem_guid);

            DataContractSerializer fmt = new DataContractSerializer(typeof(TaskDomain));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (TaskDomain)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.tasks.Count, bar.tasks.Count);
            foreach (Guid task in foo.tasks.Keys) {
                Assert.IsTrue(bar.tasks.ContainsKey(task));
                Assert.AreEqual(foo.tasks[task].name, bar.tasks[task].name);
            }
            Assert.AreEqual(foo.active_tasks.Count, bar.active_tasks.Count);
            foreach (Guid task in foo.active_tasks) {
                Assert.IsTrue(bar.active_tasks.Contains(task));
            }
        }

        [TestMethod]
        public void test_add_task() {
            TaskDomain domain = new TaskDomain();
            Task task = new Task(Guid.NewGuid(), "Some task");

            Guid task_guid = domain.add_task(task);
            Assert.AreEqual(domain.tasks.Count, 1);
            Assert.IsTrue(domain.tasks.ContainsKey(task_guid));
            Assert.AreEqual(domain.tasks[task_guid], task);
            Assert.AreEqual(domain.active_tasks.Count, 1);
            Assert.IsTrue(domain.active_tasks.Contains(task_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_add_task_null() {
            TaskDomain domain = new TaskDomain();

            domain.add_task(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_task_duplicate_task() {
            TaskDomain domain = new TaskDomain();
            Task task = new Task(Guid.NewGuid(), "Some task");

            domain.add_task(task);
            domain.add_task(task);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_task_duplicate_guid() {
            TaskDomain domain = new TaskDomain();
            Task task1 = new Task(Guid.NewGuid(), "Some task"), task2 = new Task(Guid.NewGuid(), "Some other task");

            Guid task_guid = domain.add_task(task1);
            domain.add_task(task2, task_guid);
        }

        [TestMethod]
        public void test_remove_task() {
            TaskDomain domain = new TaskDomain();
            Task task = new Task(Guid.NewGuid(), "Some task");
            Guid task_guid = domain.add_task(task);

            domain.remove_task(task_guid);
            Assert.AreEqual(domain.tasks.Count, 1);
            Assert.IsTrue(domain.tasks.ContainsKey(task_guid));
            Assert.AreEqual(domain.tasks[task_guid], task);
            Assert.AreEqual(domain.active_tasks.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_task_inactive() {
            TaskDomain domain = new TaskDomain();
            Task task = new Task(Guid.NewGuid(), "Some task");
            Guid task_guid = domain.add_task(task);

            domain.remove_task(task_guid);
            domain.remove_task(task_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_removetask_no_such_guid() {
            TaskDomain domain = new TaskDomain();
            domain.remove_task(Guid.NewGuid());
        }

        [TestMethod]
        public void test_restore_task() {
            TaskDomain domain = new TaskDomain();
            Task task = new Task(Guid.NewGuid(), "Some task");
            Guid task_guid = domain.add_task(task);

            domain.remove_task(task_guid);
            domain.restore_task(task_guid);
            Assert.AreEqual(domain.tasks.Count, 1);
            Assert.IsTrue(domain.tasks.ContainsKey(task_guid));
            Assert.AreEqual(domain.tasks[task_guid], task);
            Assert.AreEqual(domain.active_tasks.Count, 1);
            Assert.IsTrue(domain.active_tasks.Contains(task_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_task_active() {
            TaskDomain domain = new TaskDomain();
            Task task = new Task(Guid.NewGuid(), "Some task");
            Guid task_guid = domain.add_task(task);

            domain.restore_task(task_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_task_no_such_guid() {
            TaskDomain domain = new TaskDomain();
            domain.restore_task(Guid.NewGuid());
        }
    }
}