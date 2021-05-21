using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestCampaignState {
        [TestMethod]
        public void test_serialization() {
            Character chr = new Character("Somebody");
            Note note = new Note("Some note", Guid.NewGuid());
            Task task = new Task(Guid.NewGuid(), "Some task");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            CampaignState foo = new CampaignState(), bar;

            Guid chr_guid = foo.characters.add_character(chr);
            Guid inv_guid = foo.inventories.new_inventory("Somebody's Inventory");
            foo.character_inventory[chr_guid] = inv_guid;
            Guid note_guid = foo.notes.add_note(note);
            Guid task_guid = foo.tasks.add_task(task);
            Guid event_guid = foo.events.add_event(evt);

            DataContractSerializer fmt = new DataContractSerializer(typeof(CampaignState));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CampaignState)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.characters.characters.Count, bar.characters.characters.Count);
            Assert.IsTrue(bar.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(bar.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(foo.inventories.inventories.Count, bar.inventories.inventories.Count);
            Assert.IsTrue(bar.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(bar.inventories.inventories[inv_guid].name, "Somebody's Inventory");
            Assert.AreEqual(foo.character_inventory.Count, bar.character_inventory.Count);
            Assert.IsTrue(bar.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(bar.character_inventory[chr_guid], inv_guid);
            Assert.AreEqual(foo.notes.notes.Count, bar.notes.notes.Count);
            Assert.IsTrue(bar.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(bar.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(foo.tasks.tasks.Count, bar.tasks.tasks.Count);
            Assert.IsTrue(bar.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(bar.tasks.tasks[task_guid].name, "Some task");
            Assert.AreEqual(foo.events.events.Count, bar.events.events.Count);
            Assert.IsTrue(bar.events.events.ContainsKey(event_guid));
            Assert.AreEqual(bar.events.events[event_guid].name, "Some event");
        }

        [TestMethod]
        public void test_copy() {
            Character chr = new Character("Somebody");
            Note note = new Note("Some note", Guid.NewGuid());
            Task task = new Task(Guid.NewGuid(), "Some task");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            CampaignState foo = new CampaignState(), bar;

            Guid chr_guid = foo.characters.add_character(chr);
            Guid inv_guid = foo.inventories.new_inventory("Somebody's Inventory");
            foo.character_inventory[chr_guid] = inv_guid;
            Guid note_guid = foo.notes.add_note(note);
            Guid task_guid = foo.tasks.add_task(task);
            Guid event_guid = foo.events.add_event(evt);

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.IsFalse(ReferenceEquals(foo.characters, bar.characters));
            Assert.AreEqual(foo.characters.characters.Count, bar.characters.characters.Count);
            Assert.IsTrue(bar.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(bar.characters.characters[chr_guid].name, "Somebody");
            Assert.IsFalse(ReferenceEquals(foo.inventories, bar.inventories));
            Assert.AreEqual(foo.inventories.inventories.Count, bar.inventories.inventories.Count);
            Assert.IsTrue(bar.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(bar.inventories.inventories[inv_guid].name, "Somebody's Inventory");
            Assert.IsFalse(ReferenceEquals(foo.character_inventory, bar.character_inventory));
            Assert.AreEqual(foo.character_inventory.Count, bar.character_inventory.Count);
            Assert.IsTrue(bar.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(bar.character_inventory[chr_guid], inv_guid);
            Assert.IsFalse(ReferenceEquals(foo.notes, bar.notes));
            Assert.AreEqual(foo.notes.notes.Count, bar.notes.notes.Count);
            Assert.IsTrue(bar.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(bar.notes.notes[note_guid].contents, "Some note");
            Assert.IsFalse(ReferenceEquals(foo.tasks, bar.tasks));
            Assert.AreEqual(foo.tasks.tasks.Count, bar.tasks.tasks.Count);
            Assert.IsTrue(bar.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(bar.tasks.tasks[task_guid].name, "Some task");
            Assert.IsFalse(ReferenceEquals(foo.events, bar.events));
            Assert.AreEqual(foo.events.events.Count, bar.events.events.Count);
            Assert.IsTrue(bar.events.events.ContainsKey(event_guid));
            Assert.AreEqual(bar.events.events[event_guid].name, "Some event");
        }
    }


    [TestClass]
    public class TestCampaignDomain {
        [TestMethod]
        public void test_serialization() {
            Character chr = new Character("Somebody");
            Note note = new Note("Some note", Guid.NewGuid());
            Task task = new Task(Guid.NewGuid(), "Some task");
            CalendarEvent evt = new CalendarEvent(Guid.NewGuid(), 42, "Some event");
            Entry ent = new Entry(42, DateTime.Now, "Some log entry", 1);
            Topic topic = new Topic("Some topic");
            ExternalNote oog_note = new ExternalNote("Some out-of-game note", DateTime.Now);
            CampaignDomain foo = new CampaignDomain(), bar;

            Guid chr_guid = foo.state.characters.add_character(chr);
            Guid inv_guid = foo.state.inventories.new_inventory("Somebody's Inventory");
            foo.state.character_inventory[chr_guid] = inv_guid;
            Guid note_guid = foo.state.notes.add_note(note);
            Guid task_guid = foo.state.tasks.add_task(task);
            Guid event_guid = foo.state.events.add_event(evt);
            foo.entries.Add(ent);
            foo.valid_entries = 1;
            Guid topic_guid = Guid.NewGuid();
            foo.topics[topic_guid] = topic;
            Guid oog_note_guid = Guid.NewGuid();
            foo.notes[oog_note_guid] = oog_note;

            DataContractSerializer fmt = new DataContractSerializer(typeof(CampaignDomain));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CampaignDomain)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.state.characters.characters.Count, bar.state.characters.characters.Count);
            Assert.IsTrue(bar.state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(bar.state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(foo.state.inventories.inventories.Count, bar.state.inventories.inventories.Count);
            Assert.IsTrue(bar.state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(bar.state.inventories.inventories[inv_guid].name, "Somebody's Inventory");
            Assert.AreEqual(foo.state.character_inventory.Count, bar.state.character_inventory.Count);
            Assert.IsTrue(bar.state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(bar.state.character_inventory[chr_guid], inv_guid);
            Assert.AreEqual(foo.state.notes.notes.Count, bar.state.notes.notes.Count);
            Assert.IsTrue(bar.state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(bar.state.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(foo.state.tasks.tasks.Count, bar.state.tasks.tasks.Count);
            Assert.IsTrue(bar.state.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(bar.state.tasks.tasks[task_guid].name, "Some task");
            Assert.AreEqual(foo.state.events.events.Count, bar.state.events.events.Count);
            Assert.IsTrue(bar.state.events.events.ContainsKey(event_guid));
            Assert.AreEqual(bar.state.events.events[event_guid].name, "Some event");
            Assert.AreEqual(foo.entries.Count, bar.entries.Count);
            for (int i = 0; i < foo.entries.Count; i++) {
                Assert.AreEqual(foo.entries[i].guid, bar.entries[i].guid);
            }
            Assert.AreEqual(foo.valid_entries, bar.valid_entries);
            Assert.AreEqual(foo.topics.Count, bar.topics.Count);
            Assert.IsTrue(bar.topics.ContainsKey(topic_guid));
            Assert.AreEqual(bar.topics[topic_guid].name, "Some topic");
            Assert.AreEqual(foo.notes.Count, bar.notes.Count);
            foreach (Guid guid in foo.notes.Keys) {
                Assert.AreEqual(foo.notes[guid].contents, bar.notes[guid].contents);
            }
        }

        [TestMethod]
        public void test_get_entry_state() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2),
                a3 = new ActionCharacterSet(c1_guid, c1, null);
            Entry e1 = new Entry(10, new DateTime(1), "Somebody joined party", actions: new List<EntryAction>() { a1 }),
                e2 = new Entry(20, new DateTime(2), "Someone Else joined party", actions: new List<EntryAction>() { a2 }),
                e3 = new Entry(30, new DateTime(3), "Sombody left party", actions: new List<EntryAction>() { a3 });
            CampaignDomain domain = new CampaignDomain() { entries = new List<Entry>() { e1, e2, e3 } };

            e1.apply(domain.state);
            e2.apply(domain.state);
            e3.apply(domain.state);
            domain.valid_entries = 3;

            CampaignState state = domain.get_entry_state(1);
            Assert.IsFalse(ReferenceEquals(state, domain.state));
            Assert.AreEqual(state.characters.characters.Count, 2);
            Assert.IsTrue(state.characters.characters.ContainsKey(c1_guid));
            Assert.IsTrue(state.characters.characters.ContainsKey(c2_guid));
            Assert.AreEqual(state.characters.active_characters.Count, 2);
            Assert.IsTrue(state.characters.active_characters.Contains(c1_guid));
            Assert.IsTrue(state.characters.active_characters.Contains(c2_guid));
        }

        [TestMethod]
        public void test_get_timestamp_index() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2),
                a3 = new ActionCharacterSet(c1_guid, c1, null);
            Entry e1 = new Entry(10, new DateTime(1), "Somebody joined party", actions: new List<EntryAction>() { a1 }),
                e2 = new Entry(20, new DateTime(2), "Someone Else joined party", actions: new List<EntryAction>() { a2 }),
                e3 = new Entry(30, new DateTime(3), "Sombody left party", actions: new List<EntryAction>() { a3 });
            CampaignDomain domain = new CampaignDomain() { entries = new List<Entry>() { e1, e2, e3 } };

            e1.apply(domain.state);
            e2.apply(domain.state);
            e3.apply(domain.state);
            domain.valid_entries = 3;

            int idx_before = domain.get_timestamp_index(5);
            Assert.AreEqual(idx_before, -1);
            int idx0 = domain.get_timestamp_index(10);
            Assert.AreEqual(idx0, 0);
            int idx1 = domain.get_timestamp_index(25);
            Assert.AreEqual(idx1, 1);
            int idx2 = domain.get_timestamp_index(40);
            Assert.AreEqual(idx2, 2);
        }

        [TestMethod]
        public void test_get_timestamp_state() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2),
                a3 = new ActionCharacterSet(c1_guid, c1, null);
            Entry e1 = new Entry(10, new DateTime(1), "Somebody joined party", actions: new List<EntryAction>() { a1 }),
                e2 = new Entry(20, new DateTime(2), "Someone Else joined party", actions: new List<EntryAction>() { a2 }),
                e3 = new Entry(30, new DateTime(3), "Sombody left party", actions: new List<EntryAction>() { a3 });
            CampaignDomain domain = new CampaignDomain() { entries = new List<Entry>() { e1, e2, e3 } };

            e1.apply(domain.state);
            e2.apply(domain.state);
            e3.apply(domain.state);
            domain.valid_entries = 3;

            CampaignState state = domain.get_timestamp_state(15);
            Assert.IsFalse(ReferenceEquals(state, domain.state));
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(c1_guid));
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(c1_guid));
        }

        [TestMethod]
        public void test_add_entry() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2),
                a3 = new ActionCharacterSet(c1_guid, c1, null);
            Entry e1 = new Entry(10, new DateTime(1), "Somebody joined party", actions: new List<EntryAction>() { a1 }),
                e2 = new Entry(20, new DateTime(2), "Someone Else joined party", actions: new List<EntryAction>() { a2 }),
                e3 = new Entry(30, new DateTime(3), "Sombody left party", actions: new List<EntryAction>() { a3 });
            CampaignDomain domain = new CampaignDomain();

            domain.add_entry(e1);
            Assert.AreEqual(domain.state.characters.characters.Count, 1);
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c1_guid));
            Assert.AreEqual(domain.state.characters.active_characters.Count, 1);
            Assert.IsTrue(domain.state.characters.active_characters.Contains(c1_guid));
            Assert.AreEqual(domain.entries.Count, 1);
            Assert.AreEqual(domain.entries[0], e1);
            Assert.AreEqual(domain.valid_entries, 1);

            domain.add_entry(e3);
            Assert.AreEqual(domain.state.characters.characters.Count, 1);
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c1_guid));
            Assert.AreEqual(domain.state.characters.active_characters.Count, 0);
            Assert.AreEqual(domain.entries.Count, 2);
            Assert.AreEqual(domain.entries[0], e1);
            Assert.AreEqual(domain.entries[1], e3);
            Assert.AreEqual(domain.valid_entries, 2);

            domain.add_entry(e2);
            Assert.AreEqual(domain.state.characters.characters.Count, 2);
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c1_guid));
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c2_guid));
            Assert.AreEqual(domain.state.characters.active_characters.Count, 1);
            Assert.IsTrue(domain.state.characters.active_characters.Contains(c2_guid));
            Assert.AreEqual(domain.entries.Count, 3);
            Assert.AreEqual(domain.entries[0], e1);
            Assert.AreEqual(domain.entries[1], e2);
            Assert.AreEqual(domain.entries[2], e3);
            Assert.AreEqual(domain.valid_entries, 3);
        }

        [TestMethod]
        public void test_add_entry_invalid_states() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2),
                a3 = new ActionCharacterSet(c1_guid, c1, null);
            Entry e1 = new Entry(10, new DateTime(1), "Somebody joined party", actions: new List<EntryAction>() { a1 }),
                e2 = new Entry(20, new DateTime(2), "Someone Else joined party", actions: new List<EntryAction>() { a2 }),
                e3 = new Entry(30, new DateTime(3), "Sombody left party", actions: new List<EntryAction>() { a3 });
            CampaignDomain domain = new CampaignDomain();

            domain.add_entry(e3);
            Assert.AreEqual(domain.state.characters.characters.Count, 0);
            Assert.AreEqual(domain.state.characters.active_characters.Count, 0);
            Assert.AreEqual(domain.entries.Count, 1);
            Assert.AreEqual(domain.entries[0], e3);
            Assert.AreEqual(domain.valid_entries, 0);

            domain.add_entry(e2);
            Assert.AreEqual(domain.state.characters.characters.Count, 1);
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c2_guid));
            Assert.AreEqual(domain.state.characters.active_characters.Count, 1);
            Assert.IsTrue(domain.state.characters.active_characters.Contains(c2_guid));
            Assert.AreEqual(domain.entries.Count, 2);
            Assert.AreEqual(domain.entries[0], e2);
            Assert.AreEqual(domain.entries[1], e3);
            Assert.AreEqual(domain.valid_entries, 1);

            domain.add_entry(e1);
            Assert.AreEqual(domain.state.characters.characters.Count, 2);
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c1_guid));
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c2_guid));
            Assert.AreEqual(domain.state.characters.active_characters.Count, 1);
            Assert.IsTrue(domain.state.characters.active_characters.Contains(c2_guid));
            Assert.AreEqual(domain.entries.Count, 3);
            Assert.AreEqual(domain.entries[0], e1);
            Assert.AreEqual(domain.entries[1], e2);
            Assert.AreEqual(domain.entries[2], e3);
            Assert.AreEqual(domain.valid_entries, 3);
        }

        [TestMethod]
        public void test_remove_entry() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2),
                a3 = new ActionCharacterSet(c1_guid, c1, null);
            Entry e1 = new Entry(10, new DateTime(1), "Somebody joined party", actions: new List<EntryAction>() { a1 }),
                e2 = new Entry(20, new DateTime(2), "Someone Else joined party", actions: new List<EntryAction>() { a2 }),
                e3 = new Entry(30, new DateTime(3), "Sombody left party", actions: new List<EntryAction>() { a3 });
            CampaignDomain domain = new CampaignDomain() { entries = new List<Entry>() { e1, e2, e3 } };

            e1.apply(domain.state);
            e2.apply(domain.state);
            e3.apply(domain.state);
            domain.valid_entries = 3;

            domain.remove_entry(1);
            Assert.AreEqual(domain.state.characters.characters.Count, 1);
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c1_guid));
            Assert.AreEqual(domain.state.characters.active_characters.Count, 0);
            Assert.AreEqual(domain.entries.Count, 2);
            Assert.AreEqual(domain.entries[0], e1);
            Assert.AreEqual(domain.entries[1], e3);
            Assert.AreEqual(domain.valid_entries, 2);

            domain.remove_entry(0);
            Assert.AreEqual(domain.state.characters.characters.Count, 0);
            Assert.AreEqual(domain.state.characters.active_characters.Count, 0);
            Assert.AreEqual(domain.entries.Count, 1);
            Assert.AreEqual(domain.entries[0], e3);
            Assert.AreEqual(domain.valid_entries, 0);
        }

        [TestMethod]
        public void test_remove_entry_invalidate() {
            Character c1 = new Character("Somebody"), c2 = new Character("Someone Else");
            Guid c1_guid = Guid.NewGuid(), c2_guid = Guid.NewGuid();
            ActionCharacterSet a1 = new ActionCharacterSet(c1_guid, null, c1), a2 = new ActionCharacterSet(c2_guid, null, c2),
                a3 = new ActionCharacterSet(c1_guid, c1, null);
            Entry e1 = new Entry(10, new DateTime(1), "Somebody joined party", actions: new List<EntryAction>() { a1 }),
                e2 = new Entry(20, new DateTime(2), "Someone Else joined party", actions: new List<EntryAction>() { a2 }),
                e3 = new Entry(30, new DateTime(3), "Sombody left party", actions: new List<EntryAction>() { a3 });
            CampaignDomain domain = new CampaignDomain() { entries = new List<Entry>() { e1, e2, e3 } };

            e1.apply(domain.state);
            e2.apply(domain.state);
            e3.apply(domain.state);
            domain.valid_entries = 3;

            domain.remove_entry(1, true);
            Assert.AreEqual(domain.state.characters.characters.Count, 1);
            Assert.IsTrue(domain.state.characters.characters.ContainsKey(c1_guid));
            Assert.AreEqual(domain.state.characters.active_characters.Count, 1);
            Assert.IsTrue(domain.state.characters.active_characters.Contains(c1_guid));
            Assert.AreEqual(domain.entries.Count, 2);
            Assert.AreEqual(domain.entries[0], e1);
            Assert.AreEqual(domain.entries[1], e3);
            Assert.AreEqual(domain.valid_entries, 1);
        }
    }
}