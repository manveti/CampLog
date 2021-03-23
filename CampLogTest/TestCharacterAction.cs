using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
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
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_apply_restore() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_restore_active() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_apply_remove() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_remove_removed() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_apply_update() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Someone Else");
        }

        [TestMethod]
        public void test_revert_add() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.characters.characters.Count, 0);
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_add_removed() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            state.characters.remove_character(chr_guid);
            action.revert(state, evt);
        }

        [TestMethod]
        public void test_revert_restore() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_restore_active() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, evt);
            state.characters.remove_character(chr_guid);
            action.revert(state, evt);
        }

        [TestMethod]
        public void test_revert_remove() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_remove_active() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, evt);
            state.characters.restore_character(chr_guid);
            action.revert(state, evt);
        }

        [TestMethod]
        public void test_revert_update() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
        }
    }


    [TestClass]
    public class TestActionCharacterPropertySet {
        [TestMethod]
        public void test_serialization() {
            CharNumProperty jump = new CharNumProperty(42);
            ActionCharacterPropertySet foo = new ActionCharacterPropertySet(Guid.NewGuid(), new List<string>() { "Skills", "Jump" }, null, jump), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCharacterPropertySet));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCharacterPropertySet)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.path.Count, bar.path.Count);
            for (int i = 0; i < foo.path.Count; i++) {
                Assert.AreEqual(foo.path[i], bar.path[i]);
            }
            Assert.IsNull(bar.from);
            Assert.IsNotNull(bar.to);
            Assert.AreEqual((foo.to as CharNumProperty).value, (bar.to as CharNumProperty).value);
        }

        [TestMethod]
        public void test_apply_add() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, null, new CharNumProperty(123));

            action.apply(state, evt);
            Assert.IsTrue(somebody.properties.value.ContainsKey("XP"));
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 123);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_add_no_such_parent() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, new List<string>() { "Skills", "Jump" }, null, new CharNumProperty(123));

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_apply_remove() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Skills" };
            somebody.set_property(path, new CharDictProperty());
            path.Add("Jump");
            CharNumProperty jump = new CharNumProperty(7);
            somebody.set_property(path, jump);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, jump, null);

            action.apply(state, evt);
            CharDictProperty skills = somebody.properties.value["Skills"] as CharDictProperty;
            Assert.AreEqual(skills.value.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_remove_no_such_property() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, new List<string>() { "Skills", "Jump" }, new CharNumProperty(123), null);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_apply_update() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CharNumProperty xp = new CharNumProperty(42);
            somebody.set_property(path, xp);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, xp, new CharNumProperty(123));

            action.apply(state, evt);
            Assert.IsTrue(somebody.properties.value.ContainsKey("XP"));
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 123);
        }

        [TestMethod]
        public void test_revert_add() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, new List<string>() { "XP" }, null, new CharNumProperty(123));

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(somebody.properties.value.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_add_no_such_property() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<string> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, null, new CharNumProperty(123));

            action.apply(state, evt);
            state.characters.characters[chr_guid].remove_property(path);
            action.revert(state, evt);
        }

        [TestMethod]
        public void test_revert_remove() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Skills" };
            somebody.set_property(path, new CharDictProperty());
            path.Add("Jump");
            CharNumProperty jump = new CharNumProperty(7);
            somebody.set_property(path, jump);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, jump, null);

            action.apply(state, evt);
            action.revert(state, evt);
            CharDictProperty skills = somebody.properties.value["Skills"] as CharDictProperty;
            Assert.IsTrue(skills.value.ContainsKey("Jump"));
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 7);
        }

        [TestMethod]
        public void test_revert_update() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CharNumProperty xp = new CharNumProperty(42);
            somebody.set_property(path, xp);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, xp, new CharNumProperty(123));

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.IsTrue(somebody.properties.value.ContainsKey("XP"));
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 42);
        }
    }


    [TestClass]
    public class TestActionCharacterPropertyAdjust {
        [TestMethod]
        public void test_serialization() {
            CharNumProperty jump = new CharNumProperty(2);
            ActionCharacterPropertyAdjust foo = new ActionCharacterPropertyAdjust(Guid.NewGuid(), new List<string>() { "Skills", "Jump" }, null, jump), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCharacterPropertyAdjust));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCharacterPropertyAdjust)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.path.Count, bar.path.Count);
            for (int i = 0; i < foo.path.Count; i++) {
                Assert.AreEqual(foo.path[i], bar.path[i]);
            }
            Assert.IsNull(bar.subtract);
            Assert.IsNotNull(bar.add);
            Assert.AreEqual((foo.add as CharNumProperty).value, (bar.add as CharNumProperty).value);
        }

        [TestMethod]
        public void test_apply_subtract() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            somebody.set_property(path, new CharNumProperty(100));
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.apply(state, evt);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 85);
        }

        [TestMethod]
        public void test_apply_add() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Feats" };
            CharSetProperty feats = new CharSetProperty(), add_feats = new CharSetProperty();
            feats.value.Add("Power Attack");
            add_feats.value.Add("Cleave");
            somebody.set_property(path, feats);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, null, add_feats);

            action.apply(state, evt);
            CharSetProperty prop = somebody.get_property(path) as CharSetProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value.Count, 2);
            Assert.IsTrue(prop.value.Contains("Power Attack"));
            Assert.IsTrue(prop.value.Contains("Cleave"));
        }

        [TestMethod]
        public void test_apply_both() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Skills" };
            somebody.set_property(path, new CharDictProperty());
            path.Add("Jump");
            CharNumProperty jump = new CharNumProperty(7), new_jump = new CharNumProperty(10);
            somebody.set_property(path, jump);
            CharDictProperty subtract_skills = new CharDictProperty(), add_skills = new CharDictProperty();
            subtract_skills.value["Jump"] = jump;
            add_skills.value["Jump"] = new_jump;
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, new List<string>() { "Skills" }, subtract_skills, add_skills);

            action.apply(state, evt);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_property() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert_subtract() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            somebody.set_property(path, new CharNumProperty(100));
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.apply(state, evt);
            action.revert(state, evt);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 100);
        }

        [TestMethod]
        public void test_revert_add() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Feats" };
            CharSetProperty feats = new CharSetProperty(), add_feats = new CharSetProperty();
            feats.value.Add("Power Attack");
            add_feats.value.Add("Cleave");
            somebody.set_property(path, feats);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, null, add_feats);

            action.apply(state, evt);
            action.revert(state, evt);
            CharSetProperty prop = somebody.get_property(path) as CharSetProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value.Count, 1);
            Assert.IsTrue(prop.value.Contains("Power Attack"));
        }

        [TestMethod]
        public void test_revert_both() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Skills" };
            somebody.set_property(path, new CharDictProperty());
            path.Add("Jump");
            CharNumProperty jump = new CharNumProperty(7), new_jump = new CharNumProperty(10);
            somebody.set_property(path, jump);
            CharDictProperty subtract_skills = new CharDictProperty(), add_skills = new CharDictProperty();
            subtract_skills.value["Jump"] = jump;
            add_skills.value["Jump"] = new_jump;
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, new List<string>() { "Skills" }, subtract_skills, add_skills);

            action.apply(state, evt);
            action.revert(state, evt);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 7);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_no_such_property() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.revert(state, evt);
        }
    }


    [TestClass]
    public class TestActionCharacterSetInventory {
        [TestMethod]
        public void test_serialization() {
            ActionCharacterSetInventory foo = new ActionCharacterSetInventory(Guid.NewGuid(), null, Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionCharacterSetInventory));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionCharacterSetInventory)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.IsNull(bar.from);
            Assert.IsNotNull(bar.to);
            Assert.AreEqual(foo.to, bar.to);
        }

        [TestMethod]
        public void test_apply_add() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, null, inv_guid);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid);
        }

        [TestMethod]
        public void test_apply_remove() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid, null);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid;

            action.apply(state, evt);
            Assert.AreEqual(state.character_inventory.Count, 0);
        }

        [TestMethod]
        public void test_apply_update() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid chr_guid = Guid.NewGuid(), inv_guid1 = Guid.NewGuid(), inv_guid2 = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid1, inv_guid2);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid1;

            action.apply(state, evt);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid2);
        }

        [TestMethod]
        public void test_revert_add() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, null, inv_guid);
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.character_inventory.Count, 0);
        }

        [TestMethod]
        public void test_revert_remove() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid, null);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid;

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid);
        }

        [TestMethod]
        public void test_revert_update() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid chr_guid = Guid.NewGuid(), inv_guid1 = Guid.NewGuid(), inv_guid2 = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid1, inv_guid2);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid1;

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid1);
        }
    }
}