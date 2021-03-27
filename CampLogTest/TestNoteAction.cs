using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestActionNoteCreate {
        [TestMethod]
        public void test_serialization() {
            Note note = new Note("Some note", Guid.NewGuid());
            ActionNoteCreate foo = new ActionNoteCreate(Guid.NewGuid(), note), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionNoteCreate));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionNoteCreate)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.note.contents, bar.note.contents);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Note note = new Note("Some note", ent.guid);
            Guid note_guid = Guid.NewGuid();
            ActionNoteCreate action = new ActionNoteCreate(note_guid, note);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.IsFalse(ReferenceEquals(state.notes.notes[note_guid], note));
            Assert.AreEqual(state.notes.active_notes.Count, 1);
            Assert.IsTrue(state.notes.active_notes.Contains(note_guid));
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Note note = new Note("Some note", ent.guid);
            Guid note_guid = Guid.NewGuid();
            ActionNoteCreate action = new ActionNoteCreate(note_guid, note);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.notes.notes.Count, 0);
            Assert.AreEqual(state.notes.active_notes.Count, 0);
        }
    }


    [TestClass]
    public class TestActionNoteRemove {
        [TestMethod]
        public void test_serialization() {
            ActionNoteRemove foo = new ActionNoteRemove(Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionNoteRemove));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionNoteRemove)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Note note = new Note("Some note", ent.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRemove action = new ActionNoteRemove(note_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(state.notes.active_notes.Count, 0);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Note note = new Note("Some note", ent.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRemove action = new ActionNoteRemove(note_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(state.notes.active_notes.Count, 1);
            Assert.IsTrue(state.notes.active_notes.Contains(note_guid));
        }
    }


    [TestClass]
    public class TestActionNoteRestore {
        [TestMethod]
        public void test_serialization() {
            ActionNoteRestore foo = new ActionNoteRestore(Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionNoteRestore));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionNoteRestore)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Note note = new Note("Some note", ent.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRestore action = new ActionNoteRestore(note_guid);

            state.notes.remove_note(note_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(state.notes.active_notes.Count, 1);
            Assert.IsTrue(state.notes.active_notes.Contains(note_guid));
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Note note = new Note("Some note", ent.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRestore action = new ActionNoteRestore(note_guid);

            state.notes.remove_note(note_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(state.notes.active_notes.Count, 0);
        }
    }


    [TestClass]
    public class TestActionNoteUpdate {
        [TestMethod]
        public void test_serialization() {
            Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>() {
                [Guid.NewGuid()] = -2,
                [Guid.NewGuid()] = 1,
            };
            ActionNoteUpdate foo = new ActionNoteUpdate(Guid.NewGuid(), "Old note", "New Note", adjust_topics), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionNoteUpdate));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionNoteUpdate)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.contents_from, bar.contents_from);
            Assert.AreEqual(foo.contents_to, bar.contents_to);
            Assert.AreEqual(foo.adjust_topics.Count, bar.adjust_topics.Count);
            foreach (Guid topic in foo.adjust_topics.Keys) {
                Assert.IsTrue(bar.adjust_topics.ContainsKey(topic));
                Assert.AreEqual(foo.adjust_topics[topic], bar.adjust_topics[topic]);
            }
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>() {
                [topic2] = -1,
                [topic3] = 1,
            };
            Note note = new Note("Some note", ent.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", adjust_topics);

            action.apply(state, ent);
            Assert.AreEqual(note.contents, "New note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic3));
        }

        [TestMethod]
        public void test_apply_contents_only() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid();
            Note note = new Note("Some note", ent.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", null);

            action.apply(state, ent);
            Assert.AreEqual(note.contents, "New note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
        }

        [TestMethod]
        public void test_apply_topics_only() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>() {
                [topic2] = -1,
                [topic3] = 1,
            };
            Note note = new Note("Some note", ent.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, null, null, adjust_topics);

            action.apply(state, ent);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic3));
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>() {
                [topic2] = -1,
                [topic3] = 1,
            };
            Note note = new Note("Some note", ent.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", adjust_topics);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
        }

        [TestMethod]
        public void test_revert_contents_only() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid();
            Note note = new Note("Some note", ent.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", null);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
        }

        [TestMethod]
        public void test_revert_topics_only() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>() {
                [topic2] = -2,
                [topic3] = 1,
            };
            Note note = new Note("Some note", ent.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, null, null, adjust_topics);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
            Assert.AreEqual(note.topics.contents[topic2], 2);
        }
    }
}