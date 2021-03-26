using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestCalendarEvent {
        [TestMethod]
        public void test_serialization() {
            CalendarEvent foo = new CalendarEvent(Guid.NewGuid(), 42, "Some event", "A thing that will happen every day", 86400), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(CalendarEvent));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CalendarEvent)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.entry_guid, bar.entry_guid);
            Assert.AreEqual(foo.timestamp, bar.timestamp);
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.description, bar.description);
            Assert.AreEqual(foo.interval, bar.interval);
        }

        [TestMethod]
        public void test_copy() {
            CalendarEvent foo = new CalendarEvent(Guid.NewGuid(), 42, "Some event", "A thing that will happen every day", 86400), bar;

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.AreEqual(foo.entry_guid, bar.entry_guid);
            Assert.AreEqual(foo.timestamp, bar.timestamp);
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.description, bar.description);
            Assert.AreEqual(foo.interval, bar.interval);
        }
    }


    [TestClass]
    public class TestCalendarEventDomain {
        [TestMethod]
        public void test_serialization() {
            CalendarEvent e1 = new CalendarEvent(Guid.NewGuid(), 42, "Some current event"), e2 = new CalendarEvent(Guid.NewGuid(), 45, "Some former event");
            CalendarEventDomain foo = new CalendarEventDomain(), bar;

            foo.add_event(e1);
            Guid rem_guid = foo.add_event(e2);
            foo.remove_event(rem_guid);

            DataContractSerializer fmt = new DataContractSerializer(typeof(CalendarEventDomain));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CalendarEventDomain)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.events.Count, bar.events.Count);
            foreach (Guid evt in foo.events.Keys) {
                Assert.IsTrue(bar.events.ContainsKey(evt));
                Assert.AreEqual(foo.events[evt].name, bar.events[evt].name);
            }
            Assert.AreEqual(foo.active_events.Count, bar.active_events.Count);
            foreach (Guid evt in foo.active_events) {
                Assert.IsTrue(bar.active_events.Contains(evt));
            }
        }

        [TestMethod]
        public void test_add_event() {
            CalendarEventDomain domain = new CalendarEventDomain();
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");

            Guid event_guid = domain.add_event(evt);
            Assert.AreEqual(domain.events.Count, 1);
            Assert.IsTrue(domain.events.ContainsKey(event_guid));
            Assert.AreEqual(domain.events[event_guid], evt);
            Assert.AreEqual(domain.active_events.Count, 1);
            Assert.IsTrue(domain.active_events.Contains(event_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_add_event_null() {
            CalendarEventDomain domain = new CalendarEventDomain();

            domain.add_event(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_event_duplicate_event() {
            CalendarEventDomain domain = new CalendarEventDomain();
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");

            domain.add_event(evt);
            domain.add_event(evt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_event_duplicate_guid() {
            CalendarEventDomain domain = new CalendarEventDomain();
            CalendarEvent evt1 = new CalendarEvent(Guid.NewGuid(), 42, "Some event"), evt2 = new CalendarEvent(Guid.NewGuid(), 45, "Some other event");

            Guid event_guid = domain.add_event(evt1);
            domain.add_event(evt2, event_guid);
        }

        [TestMethod]
        public void test_remove_event() {
            CalendarEventDomain domain = new CalendarEventDomain();
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            Guid event_guid = domain.add_event(evt);

            domain.remove_event(event_guid);
            Assert.AreEqual(domain.events.Count, 1);
            Assert.IsTrue(domain.events.ContainsKey(event_guid));
            Assert.AreEqual(domain.events[event_guid], evt);
            Assert.AreEqual(domain.active_events.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_event_inactive() {
            CalendarEventDomain domain = new CalendarEventDomain();
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            Guid event_guid = domain.add_event(evt);

            domain.remove_event(event_guid);
            domain.remove_event(event_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_event_no_such_guid() {
            CalendarEventDomain domain = new CalendarEventDomain();
            domain.remove_event(Guid.NewGuid());
        }

        [TestMethod]
        public void test_restore_event() {
            CalendarEventDomain domain = new CalendarEventDomain();
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            Guid event_guid = domain.add_event(evt);

            domain.remove_event(event_guid);
            domain.restore_event(event_guid);
            Assert.AreEqual(domain.events.Count, 1);
            Assert.IsTrue(domain.events.ContainsKey(event_guid));
            Assert.AreEqual(domain.events[event_guid], evt);
            Assert.AreEqual(domain.active_events.Count, 1);
            Assert.IsTrue(domain.active_events.Contains(event_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_event_active() {
            CalendarEventDomain domain = new CalendarEventDomain();
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            Guid event_guid = domain.add_event(evt);

            domain.restore_event(event_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_event_no_such_guid() {
            CalendarEventDomain domain = new CalendarEventDomain();
            domain.restore_event(Guid.NewGuid());
        }
    }
}