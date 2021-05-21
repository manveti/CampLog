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

        [TestMethod]
        public void test_merge_to_create_remove() {
            Guid note_guid = Guid.NewGuid();
            Note note = new Note("Some note", Guid.NewGuid());
            ActionNoteCreate create_action = new ActionNoteCreate(note_guid, note);
            List<EntryAction> actions = new List<EntryAction>() { create_action };

            ActionNoteRemove remove_action = new ActionNoteRemove(note_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_restore_remove() {
            Guid note_guid = Guid.NewGuid();
            ActionNoteRestore restore_action = new ActionNoteRestore(note_guid);
            List<EntryAction> actions = new List<EntryAction>() { restore_action };

            ActionNoteRemove remove_action = new ActionNoteRemove(note_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_update_remove() {
            Guid note_guid = Guid.NewGuid();
            ActionNoteUpdate update_action = new ActionNoteUpdate(note_guid, "Some note", "Some updated note", null);
            List<EntryAction> actions = new List<EntryAction>() { update_action };

            ActionNoteRemove remove_action = new ActionNoteRemove(note_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            Assert.IsTrue(ReferenceEquals(actions[0], remove_action));
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

        [TestMethod]
        public void test_merge_to_remove_restore() {
            Guid note_guid = Guid.NewGuid();
            ActionNoteRemove remove_action = new ActionNoteRemove(note_guid);
            List<EntryAction> actions = new List<EntryAction>() { remove_action };

            ActionNoteRestore restore_action = new ActionNoteRestore(note_guid);
            restore_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
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
        public void test_rebase() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid topic1 = Guid.NewGuid(), topic2 = Guid.NewGuid(), topic3 = Guid.NewGuid();
            Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>() {
                [topic2] = -1,
                [topic3] = 1,
            };
            Note note = new Note("Some note", ent.guid, new HashSet<Guid>() { topic1, topic2 });
            CampaignState state = new CampaignState();
            Guid note_guid = state.notes.add_note(note);
            ActionNoteUpdate action = new ActionNoteUpdate(note_guid, "Some modified note", "New note", adjust_topics);

            note.topics.Add(topic2);

            action.rebase(state);
            Assert.AreEqual(action.contents_from, "Some note");
            Assert.AreEqual(action.adjust_topics[topic2], -2);
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

        [TestMethod]
        public void test_merge_to_create_update() {
            Guid note_guid = Guid.NewGuid(), topic1_guid = Guid.NewGuid(), topic2_guid = Guid.NewGuid();
            Note note = new Note("Some note", Guid.NewGuid(), new HashSet<Guid>() { topic1_guid });
            ActionNoteCreate create_action = new ActionNoteCreate(note_guid, note);
            List<EntryAction> actions = new List<EntryAction>() { create_action };

            Dictionary<Guid, int> adjust_topics = new Dictionary<Guid, int>() { [topic1_guid] = -1, [topic2_guid] = 1 };
            ActionNoteUpdate update_action = new ActionNoteUpdate(note_guid, "Some note", "Some updated note", adjust_topics);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionNoteCreate merged_action = actions[0] as ActionNoteCreate;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, note_guid);
            Assert.AreEqual(merged_action.note.entry_guid, note.entry_guid);
            Assert.AreEqual(merged_action.note.contents, "Some updated note");
            Assert.IsNotNull(merged_action.note.topics);
            Assert.AreEqual(merged_action.note.topics.Count, 1);
            Assert.IsTrue(merged_action.note.topics.Contains(topic2_guid));
        }

        [TestMethod]
        public void test_merge_to_update_update() {
            Guid note_guid = Guid.NewGuid(), topic1_guid = Guid.NewGuid(), topic2_guid = Guid.NewGuid();
            Dictionary<Guid, int> adjust_topics1 = new Dictionary<Guid, int>() { [topic1_guid] = 1 },
                adjust_topics2 = new Dictionary<Guid, int>() { [topic1_guid] = -1, [topic2_guid] = 1 };
            ActionNoteUpdate existing_action = new ActionNoteUpdate(note_guid, "Some note", "Some updated note", adjust_topics1);
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            ActionNoteUpdate update_action = new ActionNoteUpdate(note_guid, null, null, adjust_topics2);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionNoteUpdate merged_action = actions[0] as ActionNoteUpdate;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, note_guid);
            Assert.AreEqual(merged_action.contents_from, "Some note");
            Assert.AreEqual(merged_action.contents_to, "Some updated note");
            Assert.IsNotNull(merged_action.adjust_topics);
            Assert.AreEqual(merged_action.adjust_topics.Count, 1);
            Assert.IsTrue(merged_action.adjust_topics.ContainsKey(topic2_guid));
            Assert.AreEqual(merged_action.adjust_topics[topic2_guid], 1);
        }
    }
}