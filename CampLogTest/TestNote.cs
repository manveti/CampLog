using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestNote {
        [TestMethod]
        public void test_serialization() {
            Note foo = new Note("Some note", Guid.NewGuid()), bar;

            foo.topics.Add(Guid.NewGuid());

            DataContractSerializer fmt = new DataContractSerializer(typeof(Note));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (Note)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.contents, bar.contents);
            Assert.AreEqual(foo.entry_guid, bar.entry_guid);
            Assert.AreEqual(foo.topics.Count, bar.topics.Count);
            foreach (Guid guid in foo.topics) {
                Assert.IsTrue(bar.topics.Contains(guid));
            }
        }

        [TestMethod]
        public void test_copy() {
            Note foo = new Note("Some note", Guid.NewGuid()), bar;

            foo.topics.Add(Guid.NewGuid());

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.AreEqual(foo.contents, bar.contents);
            Assert.AreEqual(foo.entry_guid, bar.entry_guid);
            Assert.IsFalse(ReferenceEquals(foo.topics, bar.topics));
            Assert.AreEqual(foo.topics.Count, bar.topics.Count);
            foreach (Guid guid in foo.topics) {
                Assert.IsTrue(bar.topics.Contains(guid));
            }
        }
    }


    [TestClass]
    public class TestNoteDomain {
        [TestMethod]
        public void test_serialization() {
            Note n1 = new Note("Some current note", Guid.NewGuid()), n2 = new Note("Some former note", Guid.NewGuid());
            NoteDomain foo = new NoteDomain(), bar;

            foo.add_note(n1);
            Guid rem_guid = foo.add_note(n2);
            foo.remove_note(rem_guid);

            DataContractSerializer fmt = new DataContractSerializer(typeof(NoteDomain));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (NoteDomain)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.notes.Count, bar.notes.Count);
            foreach (Guid note in foo.notes.Keys) {
                Assert.IsTrue(bar.notes.ContainsKey(note));
                Assert.AreEqual(foo.notes[note].contents, bar.notes[note].contents);
            }
            Assert.AreEqual(foo.active_notes.Count, bar.active_notes.Count);
            foreach (Guid note in foo.active_notes) {
                Assert.IsTrue(bar.active_notes.Contains(note));
            }
        }

        [TestMethod]
        public void test_add_note() {
            NoteDomain domain = new NoteDomain();
            Note note = new Note("Some note", Guid.NewGuid());

            Guid note_guid = domain.add_note(note);
            Assert.AreEqual(domain.notes.Count, 1);
            Assert.IsTrue(domain.notes.ContainsKey(note_guid));
            Assert.AreEqual(domain.notes[note_guid], note);
            Assert.AreEqual(domain.active_notes.Count, 1);
            Assert.IsTrue(domain.active_notes.Contains(note_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_add_note_null() {
            NoteDomain domain = new NoteDomain();

            domain.add_note(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_note_duplicate_note() {
            NoteDomain domain = new NoteDomain();
            Note note = new Note("Some note", Guid.NewGuid());

            domain.add_note(note);
            domain.add_note(note);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_note_duplicate_guid() {
            NoteDomain domain = new NoteDomain();
            Note note1 = new Note("Some note", Guid.NewGuid()), note2 = new Note("Some other note", Guid.NewGuid());

            Guid note_guid = domain.add_note(note1);
            domain.add_note(note2, note_guid);
        }

        [TestMethod]
        public void test_remove_note() {
            NoteDomain domain = new NoteDomain();
            Note note = new Note("Some note", Guid.NewGuid());
            Guid note_guid = domain.add_note(note);

            domain.remove_note(note_guid);
            Assert.AreEqual(domain.notes.Count, 1);
            Assert.IsTrue(domain.notes.ContainsKey(note_guid));
            Assert.AreEqual(domain.notes[note_guid], note);
            Assert.AreEqual(domain.active_notes.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_note_inactive() {
            NoteDomain domain = new NoteDomain();
            Note note = new Note("Some note", Guid.NewGuid());
            Guid note_guid = domain.add_note(note);

            domain.remove_note(note_guid);
            domain.remove_note(note_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_note_no_such_guid() {
            NoteDomain domain = new NoteDomain();
            domain.remove_note(Guid.NewGuid());
        }

        [TestMethod]
        public void test_restore_note() {
            NoteDomain domain = new NoteDomain();
            Note note = new Note("Some note", Guid.NewGuid());
            Guid note_guid = domain.add_note(note);

            domain.remove_note(note_guid);
            domain.restore_note(note_guid);
            Assert.AreEqual(domain.notes.Count, 1);
            Assert.IsTrue(domain.notes.ContainsKey(note_guid));
            Assert.AreEqual(domain.notes[note_guid], note);
            Assert.AreEqual(domain.active_notes.Count, 1);
            Assert.IsTrue(domain.active_notes.Contains(note_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_note_active() {
            NoteDomain domain = new NoteDomain();
            Note note = new Note("Some note", Guid.NewGuid());
            Guid note_guid = domain.add_note(note);

            domain.restore_note(note_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_note_no_such_guid() {
            NoteDomain domain = new NoteDomain();
            domain.restore_note(Guid.NewGuid());
        }
    }
}