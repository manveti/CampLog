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
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Note note = new Note("Some note", evt.guid);
            Guid note_guid = Guid.NewGuid();
            ActionNoteCreate action = new ActionNoteCreate(note_guid, note);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.IsFalse(ReferenceEquals(state.notes.notes[note_guid], note));
            Assert.AreEqual(state.notes.active_notes.Count, 1);
            Assert.IsTrue(state.notes.active_notes.Contains(note_guid));
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Note note = new Note("Some note", evt.guid);
            Guid note_guid = Guid.NewGuid();
            ActionNoteCreate action = new ActionNoteCreate(note_guid, note);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            action.revert(state, evt);
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
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Note note = new Note("Some note", evt.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRemove action = new ActionNoteRemove(note_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(state.notes.active_notes.Count, 0);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Note note = new Note("Some note", evt.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRemove action = new ActionNoteRemove(note_guid);

            action.apply(state, evt);
            action.revert(state, evt);
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
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Note note = new Note("Some note", evt.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRestore action = new ActionNoteRestore(note_guid);

            state.notes.remove_note(note_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.notes.notes.Count, 1);
            Assert.IsTrue(state.notes.notes.ContainsKey(note_guid));
            Assert.AreEqual(state.notes.notes[note_guid].contents, "Some note");
            Assert.AreEqual(state.notes.active_notes.Count, 1);
            Assert.IsTrue(state.notes.active_notes.Contains(note_guid));
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Note note = new Note("Some note", evt.guid);
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteRestore action = new ActionNoteRestore(note_guid);

            state.notes.remove_note(note_guid);

            action.apply(state, evt);
            action.revert(state, evt);
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
            HashSet<Guid> rem_topics = new HashSet<Guid>() { Guid.NewGuid() }, add_topics = new HashSet<Guid>() { Guid.NewGuid() };
            ActionNoteUpdate foo = new ActionNoteUpdate(Guid.NewGuid(), "Old note", "New Note", rem_topics, add_topics), bar;
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
            Assert.AreEqual(foo.remove_topics.Count, bar.remove_topics.Count);
            foreach (Guid topic in foo.remove_topics) {
                Assert.IsTrue(bar.remove_topics.Contains(topic));
            }
            Assert.AreEqual(foo.add_topics.Count, bar.add_topics.Count);
            foreach (Guid topic in foo.add_topics) {
                Assert.IsTrue(bar.add_topics.Contains(topic));
            }
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            HashSet<Guid> rem_topics = new HashSet<Guid>() { topic2 }, add_topics = new HashSet<Guid>() { topic3 };
            Note note = new Note("Some note", evt.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", rem_topics, add_topics);

            action.apply(state, evt);
            Assert.AreEqual(note.contents, "New note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic3));
        }

        [TestMethod]
        public void test_apply_contents_only() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid();
            Note note = new Note("Some note", evt.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", null, null);

            action.apply(state, evt);
            Assert.AreEqual(note.contents, "New note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
        }

        [TestMethod]
        public void test_apply_topics_only() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            HashSet<Guid> rem_topics = new HashSet<Guid>() { topic2 }, add_topics = new HashSet<Guid>() { topic3 };
            Note note = new Note("Some note", evt.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, null, null, rem_topics, add_topics);

            action.apply(state, evt);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic3));
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            HashSet<Guid> rem_topics = new HashSet<Guid>() { topic2 }, add_topics = new HashSet<Guid>() { topic3 };
            Note note = new Note("Some note", evt.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", rem_topics, add_topics);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
        }

        [TestMethod]
        public void test_revert_contents_only() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid();
            Note note = new Note("Some note", evt.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some note", "New note", null, null);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
        }

        [TestMethod]
        public void test_revert_topics_only() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            HashSet<Guid> rem_topics = new HashSet<Guid>() { topic2 }, add_topics = new HashSet<Guid>() { topic3 };
            Note note = new Note("Some note", evt.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, null, null, rem_topics, add_topics);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(note.contents, "Some note");
            Assert.AreEqual(note.topics.Count, 2);
            Assert.IsTrue(note.topics.Contains(topic1));
            Assert.IsTrue(note.topics.Contains(topic2));
        }
    }
}