using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestEntry {
        [TestMethod]
        public void test_serialization() {
            Character somebody = new Character("Somebody");
            ActionCharacterSet action = new ActionCharacterSet(Guid.NewGuid(), null, somebody);
            Entry foo = new Entry(42, DateTime.Now, "Somebody joined the party", 3, guid: Guid.NewGuid()), bar;

            foo.actions.Add(action);

            DataContractSerializer fmt = new DataContractSerializer(typeof(Entry));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (Entry)(fmt.ReadObject(xr, true));
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
            Entry e1 = new Entry(42, d1, "First Entry"), e2 = new Entry(45, d2, "Second Entry"), e3 = new Entry(45, d3, "Third Entry"), e4 = new Entry(50, d4, "Fourth Entry");
            List<Entry> entries = new List<Entry>() {
                e4,
                e2,
                e1,
                e3,
            };

            entries.Sort();
            Assert.AreEqual(entries[0], e1);
            Assert.AreEqual(entries[1], e2);
            Assert.AreEqual(entries[2], e3);
            Assert.AreEqual(entries[3], e4);
        }

        [TestMethod]
        public void test_insert() {
            DateTime d1 = new DateTime(1000), d2 = new DateTime(990), d3 = new DateTime(1500), d4 = new DateTime(1800);
            Entry e1 = new Entry(42, d1, "First Entry"), e2 = new Entry(45, d2, "Second Entry"), e3 = new Entry(45, d3, "Third Entry"), e4 = new Entry(43, d4, "Retcon Entry");
            List<Entry> entries = new List<Entry>() {
                e1,
                e2,
                e3,
            };

            int idx = entries.BinarySearch(e4);
            Assert.AreEqual(~idx, 1);

            entries.Insert(~idx, e4);
            Assert.AreEqual(entries[0], e1);
            Assert.AreEqual(entries[1], e4);
            Assert.AreEqual(entries[2], e2);
            Assert.AreEqual(entries[3], e3);
        }

        [TestMethod]
        public void test_rebase() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Entry ent = new Entry(42, DateTime.Now, "Do all the things");
            ent.actions.Add(a1);
            ent.actions.Add(a2);
            ent.actions.Add(a3);

            a1.apply(state, ent);
            a2.apply(state, ent);
            a3.from.name = "Somebody different";

            ent.rebase(state);
            Assert.AreEqual(a3.from.name, "Somebody");
        }

        [TestMethod]
        public void test_apply() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Entry ent = new Entry(42, DateTime.Now, "Do all the things");
            ent.actions.Add(a1);
            ent.actions.Add(a2);
            ent.actions.Add(a3);

            ent.apply(state);
            Assert.AreEqual(state.characters.characters.Count, 2);
            Assert.IsTrue(state.characters.characters.ContainsKey(c1_guid));
            Assert.IsTrue(state.characters.characters.ContainsKey(c2_guid));
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(c2_guid));
        }

        [TestMethod]
        public void test_apply_start_index() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2);
            CampaignState state = new CampaignState();
            Entry ent = new Entry(42, DateTime.Now, "Do all the things");
            ent.actions.Add(a1);
            ent.actions.Add(a2);

            ent.apply(state, 1);
            Assert.AreEqual(state.characters.characters.Count, 1);
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
            Entry ent = new Entry(42, DateTime.Now, "Do all the things");
            ent.actions.Add(a3);
            ent.actions.Add(a1);
            ent.actions.Add(a2);

            try {
                ent.apply(state);
            }
            catch (ArgumentException e) {
                Assert.IsTrue(e.Data.Contains("action_index"));
                Assert.AreEqual(e.Data["action_index"], 0);
                throw;
            }
        }

        [TestMethod]
        public void test_revert() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Entry ent = new Entry(42, DateTime.Now, "Do all the things");
            ent.actions.Add(a1);
            ent.actions.Add(a2);
            ent.actions.Add(a3);

            ent.apply(state);
            ent.revert(state);
            Assert.AreEqual(state.characters.characters.Count, 0);
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        public void test_revert_start_index() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2);
            CampaignState state = new CampaignState();
            Entry ent = new Entry(42, DateTime.Now, "Do all the things");
            ent.actions.Add(a1);
            ent.actions.Add(a2);

            ent.apply(state);
            ent.revert(state, 0);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(c2_guid));
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(c2_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_bad_order() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2), a3 = new ActionCharacterSet(c1_guid, c1, null);
            CampaignState state = new CampaignState();
            Entry ent = new Entry(42, DateTime.Now, "Do all the things");
            ent.actions.Add(a1);
            ent.actions.Add(a2);
            ent.actions.Add(a3);

            ent.apply(state);
            ent.actions[0] = a3;
            ent.actions[1] = a1;
            ent.actions[2] = a2;
            try {
                ent.revert(state);
            }
            catch (ArgumentException e) {
                Assert.IsTrue(e.Data.Contains("action_index"));
                Assert.AreEqual(e.Data["action_index"], 1);
                throw;
            }
        }
    }
}