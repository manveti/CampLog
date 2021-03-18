using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestActionCharacterSet {
        [TestMethod]
        public void test_serialization() {
            Character somebody = new Character("Somebody");
            ActionCharacterSet foo = new ActionCharacterSet(Guid.NewGuid(), null, somebody), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCharacterSet));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCharacterSet)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.IsNull(bar.from);
            Assert.IsNotNull(bar.to);
            Assert.AreEqual(foo.to.name, bar.to.name);
            Assert.IsFalse(bar.restore);
        }

        [TestMethod]
        public void test_apply_add() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.IsFalse(ReferenceEquals(state.characters.characters[chr_guid], somebody));
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_add_removed() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state);
        }

        [TestMethod]
        public void test_apply_restore() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_restore_active() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            action.apply(state);
        }

        [TestMethod]
        public void test_apply_remove() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_remove_removed() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state);
        }

        [TestMethod]
        public void test_apply_update() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Someone Else");
        }

        [TestMethod]
        public void test_revert_add() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state);
            action.revert(state);
            Assert.AreEqual(state.characters.characters.Count, 0);
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_add_removed() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state);
            state.characters.remove_character(chr_guid);
            action.revert(state);
        }

        [TestMethod]
        public void test_revert_restore() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state);
            action.revert(state);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_restore_active() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state);
            state.characters.remove_character(chr_guid);
            action.revert(state);
        }

        [TestMethod]
        public void test_revert_remove() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state);
            action.revert(state);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_remove_active() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state);
            state.characters.restore_character(chr_guid);
            action.revert(state);
        }

        [TestMethod]
        public void test_revert_update() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state);
            action.revert(state);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
        }
    }


    //TODO: ...
}