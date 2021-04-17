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

        [TestMethod]
        public void test_merge_to_add_update() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet add_action = new ActionCharacterSet(chr_guid, null, somebody, true);
            List<EntryAction> actions = new List<EntryAction>() { add_action };

            ActionCharacterSet update_action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNull(merged_action.from);
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, someone_else.name);
            Assert.AreEqual(merged_action.restore, add_action.restore);
        }

        [TestMethod]
        public void test_merge_to_update_update() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else"), yet_another = new Character("Yet Another");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet existing_action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            ActionCharacterSet update_action = new ActionCharacterSet(chr_guid, someone_else, yet_another);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, yet_another.name);
        }

        [TestMethod]
        public void test_merge_to_propset_update() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42);
            somebody.set_property(xp_path, xp_prop);
            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, xp_path, null, xp_prop);
            List<EntryAction> actions = new List<EntryAction>() { propset_action };

            ActionCharacterSet update_action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsFalse(merged_action.from.properties.value.ContainsKey("XP"));
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, someone_else.name);
        }

        [TestMethod]
        public void test_merge_to_adjust_update() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42), prop_adj = new CharNumProperty(10);
            somebody.set_property(xp_path, xp_prop);
            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guid, xp_path, null, prop_adj);
            List<EntryAction> actions = new List<EntryAction>() { adjust_action };

            ActionCharacterSet update_action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            update_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsTrue(merged_action.from.properties.value.ContainsKey("XP"));
            CharNumProperty from_prop = merged_action.from.properties.value["XP"] as CharNumProperty;
            Assert.IsNotNull(from_prop);
            Assert.AreEqual(from_prop.value, 32);
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, someone_else.name);
        }

        [TestMethod]
        public void test_merge_to_add_remove() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet add_action = new ActionCharacterSet(chr_guid, null, somebody);
            List<EntryAction> actions = new List<EntryAction>() { add_action };

            ActionCharacterSet remove_action = new ActionCharacterSet(chr_guid, somebody, null);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_update_remove() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet update_action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            List<EntryAction> actions = new List<EntryAction>() { update_action };

            ActionCharacterSet remove_action = new ActionCharacterSet(chr_guid, someone_else, null);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsNull(merged_action.to);
        }

        [TestMethod]
        public void test_merge_to_propset_remove() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42);
            somebody.set_property(xp_path, xp_prop);
            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, xp_path, null, xp_prop);
            List<EntryAction> actions = new List<EntryAction>() { propset_action };

            ActionCharacterSet remove_action = new ActionCharacterSet(chr_guid, somebody, null);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsFalse(merged_action.from.properties.value.ContainsKey("XP"));
            Assert.IsNull(merged_action.to);
        }

        [TestMethod]
        public void test_merge_to_adjust_remove() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42), prop_adj = new CharNumProperty(10);
            somebody.set_property(xp_path, xp_prop);
            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guid, xp_path, null, prop_adj);
            List<EntryAction> actions = new List<EntryAction>() { adjust_action };

            ActionCharacterSet remove_action = new ActionCharacterSet(chr_guid, somebody, null);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsTrue(merged_action.from.properties.value.ContainsKey("XP"));
            CharNumProperty from_prop = merged_action.from.properties.value["XP"] as CharNumProperty;
            Assert.IsNotNull(from_prop);
            Assert.AreEqual(from_prop.value, 32);
            Assert.IsNull(merged_action.to);
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

        [TestMethod]
        public void test_merge_to_add_propset() {
            Character somebody = new Character("Somebody");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet add_action = new ActionCharacterSet(chr_guid, null, somebody, true);
            List<EntryAction> actions = new List<EntryAction>() { add_action };

            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42);
            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, xp_path, null, xp_prop);
            propset_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNull(merged_action.from);
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, somebody.name);
            Assert.IsTrue(merged_action.to.properties.value.ContainsKey("XP"));
            CharNumProperty to_prop = merged_action.to.properties.value["XP"] as CharNumProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value, 42);
            Assert.AreEqual(merged_action.restore, add_action.restore);
        }

        [TestMethod]
        public void test_merge_to_update_propset() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet update_action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            List<EntryAction> actions = new List<EntryAction>() { update_action };

            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42);
            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, xp_path, null, xp_prop);
            propset_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, someone_else.name);
            Assert.IsTrue(merged_action.to.properties.value.ContainsKey("XP"));
            CharNumProperty to_prop = merged_action.to.properties.value["XP"] as CharNumProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value, 42);
        }

        [TestMethod]
        public void test_merge_to_propset_propset() {
            Guid chr_guid = Guid.NewGuid();
            List<string> jump_path = new List<string>() { "Skills", "Jump" };
            CharNumProperty jump_prop = new CharNumProperty(7);
            ActionCharacterPropertySet existing_action = new ActionCharacterPropertySet(chr_guid, jump_path, null, jump_prop);
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, jump_path, jump_prop, new CharNumProperty(10));
            propset_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterPropertySet merged_action = actions[0] as ActionCharacterPropertySet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.AreEqual(merged_action.path.Count, jump_path.Count);
            for (int i = 0; i < jump_path.Count; i++) {
                Assert.AreEqual(merged_action.path[i], jump_path[i]);
            }
            Assert.IsNull(merged_action.from);
            CharNumProperty to_prop = merged_action.to as CharNumProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value, 10);
        }

        [TestMethod]
        public void test_merge_to_propset_child_propset() {
            Guid chr_guid = Guid.NewGuid();
            List<string> skills_path = new List<string>() { "Skills" }, jump_path = new List<string>() { "Skills", "Jump" };
            CharNumProperty jump_prop = new CharNumProperty(7);
            CharDictProperty skills_prop = new CharDictProperty(new Dictionary<string, CharProperty>() { ["Jump"] = jump_prop });
            ActionCharacterPropertySet existing_action = new ActionCharacterPropertySet(chr_guid, jump_path, null, jump_prop);
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            CharDictProperty new_skills = new CharDictProperty(new Dictionary<string, CharProperty>() { ["Stealth"] = new CharNumProperty(10) });
            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, skills_path, skills_prop, new_skills);
            propset_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterPropertySet merged_action = actions[0] as ActionCharacterPropertySet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.AreEqual(merged_action.path.Count, skills_path.Count);
            for (int i = 0; i < skills_path.Count; i++) {
                Assert.AreEqual(merged_action.path[i], skills_path[i]);
            }
            CharDictProperty from_prop = merged_action.from as CharDictProperty;
            Assert.IsNotNull(from_prop);
            Assert.AreEqual(from_prop.value.Count, 0);
            CharDictProperty to_prop = merged_action.to as CharDictProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value.Count, 1);
            Assert.IsTrue(to_prop.value.ContainsKey("Stealth"));
        }

        [TestMethod]
        public void test_merge_to_adjust_propset() {
            Guid chr_guid = Guid.NewGuid();
            List<string> jump_path = new List<string>() { "Skills", "Jump" };
            CharNumProperty jump_prop = new CharNumProperty(7), prop_adj = new CharNumProperty(2);
            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guid, jump_path, null, prop_adj);
            List<EntryAction> actions = new List<EntryAction>() { adjust_action };

            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, jump_path, jump_prop, new CharNumProperty(10));
            propset_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterPropertySet merged_action = actions[0] as ActionCharacterPropertySet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.AreEqual(merged_action.path.Count, jump_path.Count);
            for (int i = 0; i < jump_path.Count; i++) {
                Assert.AreEqual(merged_action.path[i], jump_path[i]);
            }
            CharNumProperty from_prop = merged_action.from as CharNumProperty;
            Assert.IsNotNull(from_prop);
            Assert.AreEqual(from_prop.value, 5);
            CharNumProperty to_prop = merged_action.to as CharNumProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value, 10);
        }

        [TestMethod]
        public void test_merge_to_adjust_child_propset() {
            Guid chr_guid = Guid.NewGuid();
            List<string> skills_path = new List<string>() { "Skills" }, jump_path = new List<string>() { "Skills", "Jump" };
            CharNumProperty jump_prop = new CharNumProperty(7), prop_adj = new CharNumProperty(2);
            CharDictProperty skills_prop = new CharDictProperty(new Dictionary<string, CharProperty>() { ["Jump"] = jump_prop });
            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guid, jump_path, null, prop_adj);
            List<EntryAction> actions = new List<EntryAction>() { adjust_action };

            CharDictProperty new_skills = new CharDictProperty(new Dictionary<string, CharProperty>() { ["Stealth"] = new CharNumProperty(10) });
            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, skills_path, skills_prop, new_skills);
            propset_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterPropertySet merged_action = actions[0] as ActionCharacterPropertySet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.AreEqual(merged_action.path.Count, skills_path.Count);
            for (int i = 0; i < skills_path.Count; i++) {
                Assert.AreEqual(merged_action.path[i], skills_path[i]);
            }
            CharDictProperty from_prop = merged_action.from as CharDictProperty;
            Assert.IsNotNull(from_prop);
            Assert.AreEqual(from_prop.value.Count, 1);
            Assert.IsTrue(from_prop.value.ContainsKey("Jump"));
            CharNumProperty from_jump = from_prop.value["Jump"] as CharNumProperty;
            Assert.IsNotNull(from_jump);
            Assert.AreEqual(from_jump.value, 5);
            CharDictProperty to_prop = merged_action.to as CharDictProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value.Count, 1);
            Assert.IsTrue(to_prop.value.ContainsKey("Stealth"));
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

        [TestMethod]
        public void test_merge_to_add_adjust() {
            Character somebody = new Character("Somebody");
            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42), prop_adj = new CharNumProperty(10);
            somebody.set_property(xp_path, xp_prop);
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet add_action = new ActionCharacterSet(chr_guid, null, somebody, true);
            List<EntryAction> actions = new List<EntryAction>() { add_action };

            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guid, xp_path, null, prop_adj);
            adjust_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNull(merged_action.from);
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, somebody.name);
            Assert.IsTrue(merged_action.to.properties.value.ContainsKey("XP"));
            CharNumProperty to_prop = merged_action.to.properties.value["XP"] as CharNumProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value, 52);
            Assert.AreEqual(merged_action.restore, add_action.restore);
        }

        [TestMethod]
        public void test_merge_to_update_adjust() {
            Character somebody = new Character("Somebody"), someone_else = new Character("Someone Else");
            List<string> xp_path = new List<string> { "XP" };
            CharNumProperty xp_prop = new CharNumProperty(42), prop_adj = new CharNumProperty(10);
            someone_else.set_property(xp_path, xp_prop);
            Guid chr_guid = Guid.NewGuid();
            ActionCharacterSet update_action = new ActionCharacterSet(chr_guid, somebody, someone_else);
            List<EntryAction> actions = new List<EntryAction>() { update_action };

            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guid, xp_path, null, prop_adj);
            adjust_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterSet merged_action = actions[0] as ActionCharacterSet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.IsNotNull(merged_action.from);
            Assert.AreEqual(merged_action.from.name, somebody.name);
            Assert.IsNotNull(merged_action.to);
            Assert.AreEqual(merged_action.to.name, someone_else.name);
            Assert.IsTrue(merged_action.to.properties.value.ContainsKey("XP"));
            CharNumProperty to_prop = merged_action.to.properties.value["XP"] as CharNumProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value, 52);
        }

        [TestMethod]
        public void test_merge_to_propset_adjust() {
            Guid chr_guid = Guid.NewGuid();
            List<string> jump_path = new List<string>() { "Skills", "Jump" };
            CharNumProperty jump_prop = new CharNumProperty(7), prop_adj = new CharNumProperty(3);
            ActionCharacterPropertySet propset_action = new ActionCharacterPropertySet(chr_guid, jump_path, null, jump_prop);
            List<EntryAction> actions = new List<EntryAction>() { propset_action };

            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guid, jump_path, null, prop_adj);
            adjust_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterPropertySet merged_action = actions[0] as ActionCharacterPropertySet;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, chr_guid);
            Assert.AreEqual(merged_action.path.Count, jump_path.Count);
            for (int i = 0; i < jump_path.Count; i++) {
                Assert.AreEqual(merged_action.path[i], jump_path[i]);
            }
            Assert.IsNull(merged_action.from);
            CharNumProperty to_prop = merged_action.to as CharNumProperty;
            Assert.IsNotNull(to_prop);
            Assert.AreEqual(to_prop.value, 10);
        }

        [TestMethod]
        public void test_merge_to_adjust_adjust() {
            HashSet<Guid> chr_guids = new HashSet<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<string> jump_path = new List<string>() { "Skills", "Jump" };
            CharNumProperty existing_adj = new CharNumProperty(3), prop_adj = new CharNumProperty(2);
            ActionCharacterPropertyAdjust existing_action = new ActionCharacterPropertyAdjust(chr_guids, jump_path, null, existing_adj);
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(chr_guids, jump_path, null, prop_adj);
            adjust_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterPropertyAdjust merged_action = actions[0] as ActionCharacterPropertyAdjust;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guids.Count, chr_guids.Count);
            Assert.IsTrue(merged_action.guids.IsSubsetOf(chr_guids));
            Assert.AreEqual(merged_action.path.Count, jump_path.Count);
            for (int i = 0; i < jump_path.Count; i++) {
                Assert.AreEqual(merged_action.path[i], jump_path[i]);
            }
            Assert.IsNull(merged_action.subtract);
            CharNumProperty add_prop = merged_action.add as CharNumProperty;
            Assert.IsNotNull(add_prop);
            Assert.AreEqual(add_prop.value, 5);
        }

        [TestMethod]
        public void test_merge_to_same_adjust_different_guid() {
            HashSet<Guid> existing_guids = new HashSet<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
                new_guids = new HashSet<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }, expected_guids = new HashSet<Guid>(existing_guids);
            expected_guids.UnionWith(new_guids);
            List<string> jump_path = new List<string>() { "Skills", "Jump" };
            CharNumProperty prop_adj = new CharNumProperty(2);
            ActionCharacterPropertyAdjust existing_action = new ActionCharacterPropertyAdjust(existing_guids, jump_path, null, prop_adj);
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            ActionCharacterPropertyAdjust adjust_action = new ActionCharacterPropertyAdjust(new_guids, jump_path, null, prop_adj);
            adjust_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionCharacterPropertyAdjust merged_action = actions[0] as ActionCharacterPropertyAdjust;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guids.Count, expected_guids.Count);
            Assert.IsTrue(merged_action.guids.IsSubsetOf(expected_guids));
            Assert.AreEqual(merged_action.path.Count, jump_path.Count);
            for (int i = 0; i < jump_path.Count; i++) {
                Assert.AreEqual(merged_action.path[i], jump_path[i]);
            }
            Assert.IsNull(merged_action.subtract);
            CharNumProperty add_prop = merged_action.add as CharNumProperty;
            Assert.IsNotNull(add_prop);
            Assert.AreEqual(add_prop.value, 2);
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