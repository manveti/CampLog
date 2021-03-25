using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            Assert.AreEqual(foo.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(foo.tasks.tasks.Count, bar.tasks.tasks.Count);
            Assert.IsTrue(bar.tasks.tasks.ContainsKey(task_guid));
            Assert.AreEqual(foo.tasks.tasks[task_guid].name, "Some task");
            Assert.AreEqual(foo.events.events.Count, bar.events.events.Count);
            Assert.IsTrue(bar.events.events.ContainsKey(event_guid));
            Assert.AreEqual(foo.events.events[event_guid].name, "Some event");
        }
    }
}