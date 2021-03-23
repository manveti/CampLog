using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestEvent {
        [TestMethod]
        public void test_serialization() {
            Character somebody = new Character("Somebody");
            ActionCharacterSet action = new ActionCharacterSet(Guid.NewGuid(), null, somebody);
            Event foo = new Event(42, DateTime.Now, "Somebody joined the party", 3, guid: Guid.NewGuid()), bar;

            foo.actions.Add(action);

            DataContractSerializer fmt = new DataContractSerializer(typeof(Event));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (Event)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.timestamp, bar.timestamp);
            Assert.AreEqual(foo.created, bar.created);
            Assert.AreEqual(foo.description, bar.description);
            Assert.AreEqual(foo.session, bar.session);
            Assert.AreEqual(foo.actions.Count, bar.actions.Count);
            for (int i = 0; i < foo.actions.Count; i++) {
                Assert.AreEqual(foo.actions[i].description, bar.actions[i].description);
            }
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_sort() {
            DateTime d1 = new DateTime(1000), d2 = new DateTime(990), d3 = new DateTime(1500), d4 = new DateTime(1800);
            Event e1 = new Event(42, d1, "First Event"), e2 = new Event(45, d2, "Second Event"), e3 = new Event(45, d3, "Third Event"), e4 = new Event(50, d4, "Fourth Event");
            List<Event> events = new List<Event>() {
                e4,
                e2,
                e1,
                e3,
            };

            events.Sort();
            Assert.AreEqual(events[0], e1);
            Assert.AreEqual(events[1], e2);
            Assert.AreEqual(events[2], e3);
            Assert.AreEqual(events[3], e4);
        }

        [TestMethod]
        public void test_insert() {
            DateTime d1 = new DateTime(1000), d2 = new DateTime(990), d3 = new DateTime(1500), d4 = new DateTime(1800);
            Event e1 = new Event(42, d1, "First Event"), e2 = new Event(45, d2, "Second Event"), e3 = new Event(45, d3, "Third Event"), e4 = new Event(43, d4, "Retcon Event");
            List<Event> events = new List<Event>() {
                e1,
                e2,
                e3,
            };

            int idx = events.BinarySearch(e4);
            Assert.AreEqual(~idx, 1);

            events.Insert(~idx, e4);
            Assert.AreEqual(events[0], e1);
            Assert.AreEqual(events[1], e4);
            Assert.AreEqual(events[2], e2);
            Assert.AreEqual(events[3], e3);
        }

        [TestMethod]
        public void test_apply() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Event evt = new Event(42, DateTime.Now, "Do all the things");
            evt.actions.Add(a1);
            evt.actions.Add(a2);
            evt.actions.Add(a3);

            evt.apply(state);
            Assert.AreEqual(state.characters.characters.Count, 2);
            Assert.IsTrue(state.characters.characters.ContainsKey(c1_guid));
            Assert.IsTrue(state.characters.characters.ContainsKey(c2_guid));
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(c2_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_bad_order() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Event evt = new Event(42, DateTime.Now, "Do all the things");
            evt.actions.Add(a3);
            evt.actions.Add(a1);
            evt.actions.Add(a2);

            evt.apply(state);
        }

        [TestMethod]
        public void test_revert() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Event evt = new Event(42, DateTime.Now, "Do all the things");
            evt.actions.Add(a1);
            evt.actions.Add(a2);
            evt.actions.Add(a3);

            evt.apply(state);
            evt.revert(state);
            Assert.AreEqual(state.characters.characters.Count, 0);
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_bad_order() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Event evt = new Event(42, DateTime.Now, "Do all the things");
            evt.actions.Add(a1);
            evt.actions.Add(a2);
            evt.actions.Add(a3);

            evt.apply(state);
            evt.actions[0] = a3;
            evt.actions[1] = a1;
            evt.actions[2] = a2;
            evt.revert(state);
        }
    }
}