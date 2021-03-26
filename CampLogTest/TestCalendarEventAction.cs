using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestActionCalendarEventCreate {
        [TestMethod]
        public void test_serialization() {
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            ActionCalendarEventCreate foo = new ActionCalendarEventCreate(Guid.NewGuid(), evt), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCalendarEventCreate));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCalendarEventCreate)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.evt.name, bar.evt.name);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            Guid event_guid = Guid.NewGuid();
            ActionCalendarEventCreate action = new ActionCalendarEventCreate(event_guid, evt);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            Assert.AreEqual(state.events.events.Count, 1);
            Assert.IsTrue(state.events.events.ContainsKey(event_guid));
            Assert.AreEqual(state.events.events[event_guid].name, "Some event");
            Assert.IsFalse(ReferenceEquals(state.events.events[event_guid], evt));
            Assert.AreEqual(state.events.active_events.Count, 1);
            Assert.IsTrue(state.events.active_events.Contains(event_guid));
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            Guid event_guid = Guid.NewGuid();
            ActionCalendarEventCreate action = new ActionCalendarEventCreate(event_guid, evt);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.events.events.Count, 0);
            Assert.AreEqual(state.events.active_events.Count, 0);
        }
    }


    [TestClass]
    public class TestActionCalendarEventRemove {
        [TestMethod]
        public void test_serialization() {
            ActionCalendarEventRemove foo = new ActionCalendarEventRemove(Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCalendarEventRemove));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCalendarEventRemove)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventRemove action = new ActionCalendarEventRemove(event_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.events.events.Count, 1);
            Assert.IsTrue(state.events.events.ContainsKey(event_guid));
            Assert.AreEqual(state.events.events[event_guid].name, "Some event");
            Assert.AreEqual(state.events.active_events.Count, 0);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventRemove action = new ActionCalendarEventRemove(event_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.events.events.Count, 1);
            Assert.IsTrue(state.events.events.ContainsKey(event_guid));
            Assert.AreEqual(state.events.events[event_guid].name, "Some event");
            Assert.AreEqual(state.events.active_events.Count, 1);
            Assert.IsTrue(state.events.active_events.Contains(event_guid));
        }
    }


    [TestClass]
    public class TestActionCalendarEventRestore {
        [TestMethod]
        public void test_serialization() {
            ActionCalendarEventRestore foo = new ActionCalendarEventRestore(Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCalendarEventRestore));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCalendarEventRestore)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventRestore action = new ActionCalendarEventRestore(event_guid);

            state.events.remove_event(event_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.events.events.Count, 1);
            Assert.IsTrue(state.events.events.ContainsKey(event_guid));
            Assert.AreEqual(state.events.events[event_guid].name, "Some event");
            Assert.AreEqual(state.events.active_events.Count, 1);
            Assert.IsTrue(state.events.active_events.Contains(event_guid));
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventRestore action = new ActionCalendarEventRestore(event_guid);

            state.events.remove_event(event_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.events.events.Count, 1);
            Assert.IsTrue(state.events.events.ContainsKey(event_guid));
            Assert.AreEqual(state.events.events[event_guid].name, "Some event");
            Assert.AreEqual(state.events.active_events.Count, 0);
        }
    }


    [TestClass]
    public class TestActionCalendarEventUpdate {
        [TestMethod]
        public void test_serialization() {
            CalendarEvent old_evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event"), new_evt = new CalendarEvent(old_evt.entry_guid, 0, "Some detailed event");
            ActionCalendarEventUpdate foo = new ActionCalendarEventUpdate(Guid.NewGuid(), old_evt, new_evt, false, false, true, false), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCalendarEventUpdate));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCalendarEventUpdate)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.from.name, bar.from.name);
            Assert.AreEqual(foo.to.description, bar.to.description);
            Assert.AreEqual(foo.set_timestamp, bar.set_timestamp);
            Assert.AreEqual(foo.set_name, bar.set_name);
            Assert.AreEqual(foo.set_desc, bar.set_desc);
            Assert.AreEqual(foo.set_interval, bar.set_interval);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event", "Something", 86400),
                new_evt = new CalendarEvent(evt.entry_guid, 84, "Some updated event", "Some updated thing");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventUpdate action = new ActionCalendarEventUpdate(event_guid, evt, new_evt, true, true, true, true);

            action.apply(state, ent);
            Assert.AreEqual(evt.timestamp, 84);
            Assert.AreEqual(evt.name, "Some updated event");
            Assert.AreEqual(evt.description, "Some updated thing");
            Assert.IsNull(evt.interval);
        }

        [TestMethod]
        public void test_apply_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event"),
                new_task = new CalendarEvent(evt.entry_guid, 0, "Some updated event", "Some updated thing");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventUpdate action = new ActionCalendarEventUpdate(event_guid, evt, new_task, false, false, true, false);

            action.apply(state, ent);
            Assert.AreEqual(evt.timestamp, 42);
            Assert.AreEqual(evt.name, "Some event");
            Assert.AreEqual(evt.description, "Some updated thing");
            Assert.IsNull(evt.interval);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event", "Something", 86400),
                new_evt = new CalendarEvent(evt.entry_guid, 84, "Some updated event", "Some updated thing");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventUpdate action = new ActionCalendarEventUpdate(event_guid, evt, new_evt, true, true, true, true);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(evt.timestamp, 42);
            Assert.AreEqual(evt.name, "Some event");
            Assert.AreEqual(evt.description, "Something");
            Assert.AreEqual(evt.interval, 86400);
        }

        [TestMethod]
        public void test_revert_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event"),
                new_task = new CalendarEvent(evt.entry_guid, 0, "Some updated event", "Some updated thing");
            CampaignState state = new CampaignState();
            Guid event_guid = state.events.add_event(evt);
            ActionCalendarEventUpdate action = new ActionCalendarEventUpdate(event_guid, evt, new_task, false, false, true, false);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(evt.timestamp, 42);
            Assert.AreEqual(evt.name, "Some event");
            Assert.IsNull(evt.description);
            Assert.IsNull(evt.interval);
        }
    }
}