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
        public void test_rebase() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            action.from.name = "Someone Else";

            action.rebase(state);
            Assert.AreEqual(action.from.name, "Somebody");
        }

        [TestMethod]
        public void test_apply_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_apply_restore() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_restore_active() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_apply_remove() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_remove_removed() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_apply_update() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Someone Else");
        }

        [TestMethod]
        public void test_revert_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.characters.characters.Count, 0);
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_add_removed() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            state.characters.remove_character(chr_guid);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_revert_restore() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);
            state.characters.remove_character(chr_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_restore_active() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, null, somebody, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, ent);
            state.characters.remove_character(chr_guid);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_revert_remove() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.characters.characters.Count, 1);
            Assert.IsTrue(state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(state.characters.active_characters.Count, 1);
            Assert.IsTrue(state.characters.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_remove_active() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, null, true);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, ent);
            state.characters.restore_character(chr_guid);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_revert_update() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            CampaignState state = new CampaignState();

            state.characters.add_character(somebody, chr_guid);

            action.apply(state, ent);
            action.revert(state, ent);
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
        public void test_rebase() {
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Skills" };
            somebody.set_property(path, new CharDictProperty());
            path.Add("Jump");
            CharNumProperty jump = new CharNumProperty(7);
            somebody.set_property(path, jump);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, jump, null);
            (action.from as CharNumProperty).value = 10;

            action.rebase(state);
            Assert.AreEqual((action.from as CharNumProperty).value, 7);
        }

        [TestMethod]
        public void test_rebase_no_such_property() {
            Character somebody = new Character("Somebody");
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            List<String> path = new List<string>() { "Skills", "Jump" };
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, new CharNumProperty(123), new CharNumProperty(42));

            action.rebase(state);
            Assert.IsNull(action.from);
        }

        [TestMethod]
        public void test_apply_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, null, new CharNumProperty(123));

            action.apply(state, ent);
            Assert.IsTrue(somebody.properties.value.ContainsKey("XP"));
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 123);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_add_no_such_parent() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, new List<string>() { "Skills", "Jump" }, null, new CharNumProperty(123));

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_apply_remove() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Skills" };
            somebody.set_property(path, new CharDictProperty());
            path.Add("Jump");
            CharNumProperty jump = new CharNumProperty(7);
            somebody.set_property(path, jump);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, jump, null);

            action.apply(state, ent);
            CharDictProperty skills = somebody.properties.value["Skills"] as CharDictProperty;
            Assert.AreEqual(skills.value.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_remove_no_such_property() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, new List<string>() { "Skills", "Jump" }, new CharNumProperty(123), null);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_apply_update() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CharNumProperty xp = new CharNumProperty(42);
            somebody.set_property(path, xp);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, xp, new CharNumProperty(123));

            action.apply(state, ent);
            Assert.IsTrue(somebody.properties.value.ContainsKey("XP"));
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 123);
        }

        [TestMethod]
        public void test_revert_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, new List<string>() { "XP" }, null, new CharNumProperty(123));

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(somebody.properties.value.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_add_no_such_property() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<string> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, null, new CharNumProperty(123));

            action.apply(state, ent);
            state.characters.characters[chr_guid].remove_property(path);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_revert_remove() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Skills" };
            somebody.set_property(path, new CharDictProperty());
            path.Add("Jump");
            CharNumProperty jump = new CharNumProperty(7);
            somebody.set_property(path, jump);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, jump, null);

            action.apply(state, ent);
            action.revert(state, ent);
            CharDictProperty skills = somebody.properties.value["Skills"] as CharDictProperty;
            Assert.IsTrue(skills.value.ContainsKey("Jump"));
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 7);
        }

        [TestMethod]
        public void test_revert_update() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CharNumProperty xp = new CharNumProperty(42);
            somebody.set_property(path, xp);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertySet action = new ActionCharacterPropertySet(chr_guid, path, xp, new CharNumProperty(123));

            action.apply(state, ent);
            action.revert(state, ent);
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
            Assert.AreEqual(foo.guids.Count, bar.guids.Count);
            foreach (Guid guid in foo.guids) {
                Assert.IsTrue(bar.guids.Contains(guid));
            }
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            somebody.set_property(path, new CharNumProperty(100));
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.apply(state, ent);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 85);
        }

        [TestMethod]
        public void test_apply_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Feats" };
            CharSetProperty feats = new CharSetProperty(), add_feats = new CharSetProperty();
            feats.value.Add("Power Attack");
            add_feats.value.Add("Cleave");
            somebody.set_property(path, feats);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, null, add_feats);

            action.apply(state, ent);
            CharSetProperty prop = somebody.get_property(path) as CharSetProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value.Count, 2);
            Assert.IsTrue(prop.value.Contains("Power Attack"));
            Assert.IsTrue(prop.value.Contains("Cleave"));
        }

        [TestMethod]
        public void test_apply_both() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 10);
        }

        [TestMethod]
        public void test_apply_multiple() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character chr1 = new Character("Somebody"), chr2 = new Character("Someone else");
            List<String> path = new List<string>() { "XP" };
            chr1.set_property(path, new CharNumProperty(100));
            chr2.set_property(path, new CharNumProperty(150));
            CampaignState state = new CampaignState();
            Guid c1_guid = state.characters.add_character(chr1), c2_guid = state.characters.add_character(chr2);
            HashSet<Guid> chr_guids = new HashSet<Guid>() { c1_guid, c2_guid };
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guids, path, new CharNumProperty(15), null);

            action.apply(state, ent);
            CharNumProperty prop = chr1.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 85);
            prop = chr2.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 135);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_property() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.apply(state, ent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_multiple_no_such_property() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character chr1 = new Character("Somebody"), chr2 = new Character("Someone else"), chr3 = new Character("Yet another character");
            List<String> path = new List<string>() { "XP" };
            chr1.set_property(path, new CharNumProperty(100));
            chr3.set_property(path, new CharNumProperty(150));
            CampaignState state = new CampaignState();
            HashSet<Guid> chr_guids = new HashSet<Guid>() {
                state.characters.add_character(chr1),
                state.characters.add_character(chr2),
                state.characters.add_character(chr3),
            };
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guids, path, new CharNumProperty(15), null);

            try {
                action.apply(state, ent);
            }
            catch (ArgumentOutOfRangeException) {
                CharNumProperty prop = chr1.get_property(path) as CharNumProperty;
                Assert.IsNotNull(prop);
                Assert.AreEqual(prop.value, 100);
                prop = chr3.get_property(path) as CharNumProperty;
                Assert.IsNotNull(prop);
                Assert.AreEqual(prop.value, 150);
                throw;
            }
        }

        [TestMethod]
        public void test_revert_subtract() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            somebody.set_property(path, new CharNumProperty(100));
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.apply(state, ent);
            action.revert(state, ent);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 100);
        }

        [TestMethod]
        public void test_revert_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Feats" };
            CharSetProperty feats = new CharSetProperty(), add_feats = new CharSetProperty();
            feats.value.Add("Power Attack");
            add_feats.value.Add("Cleave");
            somebody.set_property(path, feats);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, null, add_feats);

            action.apply(state, ent);
            action.revert(state, ent);
            CharSetProperty prop = somebody.get_property(path) as CharSetProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value.Count, 1);
            Assert.IsTrue(prop.value.Contains("Power Attack"));
        }

        [TestMethod]
        public void test_revert_both() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            action.revert(state, ent);
            CharNumProperty prop = somebody.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 7);
        }

        [TestMethod]
        public void test_revert_multiple() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character chr1 = new Character("Somebody"), chr2 = new Character("Someone else");
            List<String> path = new List<string>() { "XP" };
            chr1.set_property(path, new CharNumProperty(100));
            chr2.set_property(path, new CharNumProperty(150));
            CampaignState state = new CampaignState();
            Guid c1_guid = state.characters.add_character(chr1), c2_guid = state.characters.add_character(chr2);
            HashSet<Guid> chr_guids = new HashSet<Guid>() { c1_guid, c2_guid };
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guids, path, new CharNumProperty(15), null);

            action.apply(state, ent);
            action.revert(state, ent);
            CharNumProperty prop = chr1.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 100);
            prop = chr2.get_property(path) as CharNumProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value, 150);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_no_such_property() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "XP" };
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, new CharNumProperty(15), null);

            action.revert(state, ent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_multiple_no_such_property() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character chr1 = new Character("Somebody"), chr2 = new Character("Someone else"), chr3 = new Character("Yet another character");
            List<String> path = new List<string>() { "XP" };
            chr1.set_property(path, new CharNumProperty(100));
            chr3.set_property(path, new CharNumProperty(150));
            CampaignState state = new CampaignState();
            HashSet<Guid> chr_guids = new HashSet<Guid>() {
                state.characters.add_character(chr1),
                state.characters.add_character(chr2),
                state.characters.add_character(chr3),
            };
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guids, path, new CharNumProperty(15), null);

            try {
                action.revert(state, ent);
            }
            catch (ArgumentOutOfRangeException) {
                CharNumProperty prop = chr1.get_property(path) as CharNumProperty;
                Assert.IsNotNull(prop);
                Assert.AreEqual(prop.value, 100);
                prop = chr3.get_property(path) as CharNumProperty;
                Assert.IsNotNull(prop);
                Assert.AreEqual(prop.value, 150);
                throw;
            }
        }

        [TestMethod]
        public void test_set_add_add_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Feats" };
            CharSetProperty feats = new CharSetProperty(), add_feats = new CharSetProperty();
            feats.value.Add("Power Attack");
            feats.value.Add("Cleave");
            add_feats.value.Add("Cleave");
            somebody.set_property(path, feats);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, null, add_feats);

            action.apply(state, ent);
            action.revert(state, ent);
            CharSetProperty prop = somebody.get_property(path) as CharSetProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value.Count, 2);
            Assert.IsTrue(prop.value.Contains("Power Attack"));
            Assert.IsTrue(prop.value.Contains("Cleave"));
        }

        [TestMethod]
        public void test_set_add_add_revert_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Character somebody = new Character("Somebody");
            List<String> path = new List<string>() { "Feats" };
            CharSetProperty feats = new CharSetProperty(), add_feats = new CharSetProperty();
            feats.value.Add("Power Attack");
            add_feats.value.Add("Cleave");
            somebody.set_property(path, feats);
            CampaignState state = new CampaignState();
            Guid chr_guid = state.characters.add_character(somebody);
            ActionCharacterPropertyAdjust action = new ActionCharacterPropertyAdjust(chr_guid, path, null, add_feats);

            action.apply(state, ent);
            action.apply(state, ent);
            action.revert(state, ent);
            action.revert(state, ent);
            CharSetProperty prop = somebody.get_property(path) as CharSetProperty;
            Assert.IsNotNull(prop);
            Assert.AreEqual(prop.value.Count, 1);
            Assert.IsTrue(prop.value.Contains("Power Attack"));
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
        public void test_rebase() {
            Guid chr_guid = Guid.NewGuid(), inv_guid1 = Guid.NewGuid(), inv_guid2 = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, Guid.NewGuid(), inv_guid2);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid1;

            action.rebase(state);
            Assert.AreEqual(action.from, inv_guid1);
        }

        [TestMethod]
        public void test_rebase_no_association() {
            Guid chr_guid = Guid.NewGuid(), inv_guid1 = Guid.NewGuid(), inv_guid2 = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid1, inv_guid2);
            CampaignState state = new CampaignState();

            action.rebase(state);
            Assert.IsNull(action.from);
        }

        [TestMethod]
        public void test_apply_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, null, inv_guid);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid);
        }

        [TestMethod]
        public void test_apply_remove() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid, null);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid;

            action.apply(state, ent);
            Assert.AreEqual(state.character_inventory.Count, 0);
        }

        [TestMethod]
        public void test_apply_update() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid chr_guid = Guid.NewGuid(), inv_guid1 = Guid.NewGuid(), inv_guid2 = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid1, inv_guid2);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid1;

            action.apply(state, ent);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid2);
        }

        [TestMethod]
        public void test_revert_add() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, null, inv_guid);
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.character_inventory.Count, 0);
        }

        [TestMethod]
        public void test_revert_remove() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid, null);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid;

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid);
        }

        [TestMethod]
        public void test_revert_update() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid chr_guid = Guid.NewGuid(), inv_guid1 = Guid.NewGuid(), inv_guid2 = Guid.NewGuid();
            ActionCharacterSetInventory action = new ActionCharacterSetInventory(chr_guid, inv_guid1, inv_guid2);
            CampaignState state = new CampaignState();
            state.character_inventory[chr_guid] = inv_guid1;

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.character_inventory.Count, 1);
            Assert.IsTrue(state.character_inventory.ContainsKey(chr_guid));
            Assert.AreEqual(state.character_inventory[chr_guid], inv_guid1);
        }
    }
}