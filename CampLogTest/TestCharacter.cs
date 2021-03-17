using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestCharTextProperty {
        [TestMethod]
        public void test_serialization() {
            CharTextProperty foo = new CharTextProperty("blah"), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(CharTextProperty));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CharTextProperty)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.value, bar.value);
        }
    }


    [TestClass]
    public class TestCharNumProperty {
        [TestMethod]
        public void test_serialization() {
            CharNumProperty foo = new CharNumProperty(1.3m), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(CharNumProperty));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CharNumProperty)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.value, bar.value);
        }
    }


    [TestClass]
    public class TestCharSetProperty {
        [TestMethod]
        public void test_serialization() {
            CharSetProperty foo = new CharSetProperty(), bar;
            foo.value.Add("blah");
            foo.value.Add("bloh");
            DataContractSerializer fmt = new DataContractSerializer(typeof(CharSetProperty));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CharSetProperty)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.value.Count, bar.value.Count);
            foreach (string s in foo.value) {
                Assert.IsTrue(bar.value.Contains(s));
            }
        }
    }


    [TestClass]
    public class TestCharDictProperty {
        [TestMethod]
        public void test_serialization() {
            CharDictProperty foo = new CharDictProperty(), bar;
            CharTextProperty tprop = new CharTextProperty("blah");
            CharNumProperty nprop = new CharNumProperty(1.23m);
            CharSetProperty sprop = new CharSetProperty();
            sprop.value.Add("bloh");
            sprop.value.Add("bleh");
            foo.value["Some Text"] = tprop;
            foo.value["Some Number"] = nprop;
            foo.value["Some Collection"] = sprop;
            DataContractSerializer fmt = new DataContractSerializer(typeof(CharDictProperty));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CharDictProperty)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.value.Count, bar.value.Count);
            foreach (string s in foo.value.Keys) {
                Assert.IsTrue(bar.value.ContainsKey(s));
            }
            CharTextProperty tp = bar.value["Some Text"] as CharTextProperty;
            Assert.IsFalse(tp is null);
            Assert.AreEqual(tp.value, "blah");
            CharNumProperty np = bar.value["Some Number"] as CharNumProperty;
            Assert.IsFalse(np is null);
            Assert.AreEqual(np.value, 1.23m);
            CharSetProperty sp = bar.value["Some Collection"] as CharSetProperty;
            Assert.IsFalse(sp is null);
            Assert.AreEqual(sp.value.Count, 2);
            Assert.IsTrue(sp.value.Contains("bloh"));
            Assert.IsTrue(sp.value.Contains("bleh"));
        }
    }


    [TestClass]
    public class TestCharacter {
        [TestMethod]
        public void test_serialization() {
            Character foo = new Character("Somebody"), bar;
            CharTextProperty tprop = new CharTextProperty("blah");
            CharNumProperty nprop = new CharNumProperty(1.23m);
            CharSetProperty sprop = new CharSetProperty();
            sprop.value.Add("bloh");
            sprop.value.Add("bleh");
            foo.properties.value["Some Text"] = tprop;
            foo.properties.value["Some Number"] = nprop;
            foo.properties.value["Some Collection"] = sprop;
            DataContractSerializer fmt = new DataContractSerializer(typeof(Character));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (Character)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.properties.value.Count, bar.properties.value.Count);
            foreach (string s in foo.properties.value.Keys) {
                Assert.IsTrue(bar.properties.value.ContainsKey(s));
            }
            CharTextProperty tp = bar.properties.value["Some Text"] as CharTextProperty;
            Assert.IsFalse(tp is null);
            Assert.AreEqual(tp.value, "blah");
            CharNumProperty np = bar.properties.value["Some Number"] as CharNumProperty;
            Assert.IsFalse(np is null);
            Assert.AreEqual(np.value, 1.23m);
            CharSetProperty sp = bar.properties.value["Some Collection"] as CharSetProperty;
            Assert.IsFalse(sp is null);
            Assert.AreEqual(sp.value.Count, 2);
            Assert.IsTrue(sp.value.Contains("bloh"));
            Assert.IsTrue(sp.value.Contains("bleh"));
        }

        private Character get_test_character() {
            Character c = new Character("Krakrox the Barbarian");

            c.properties.value["Player"] = new CharTextProperty("Philip");
            c.properties.value["XP"] = new CharNumProperty(42);
            CharSetProperty feats = new CharSetProperty();
            feats.value.Add("Power Attack");
            feats.value.Add("Cleave");
            c.properties.value["Feats"] = feats;
            CharDictProperty skills = new CharDictProperty();
            skills.value["Diplomacy"] = new CharNumProperty(0);
            skills.value["Seal Clubbing"] = new CharNumProperty(9001);
            c.properties.value["Skills"] = skills;

            return c;
        }

        [TestMethod]
        public void test_get_property() {
            Character c = this.get_test_character();

            CharTextProperty prop = c.get_property(new List<string>() { "Player" }) as CharTextProperty;
            Assert.IsFalse(prop is null);
            Assert.AreEqual(prop.value, "Philip");
        }

        [TestMethod]
        public void test_get_property_nested() {
            Character c = this.get_test_character();

            CharNumProperty prop = c.get_property(new List<string>() { "Skills", "Seal Clubbing" }) as CharNumProperty;
            Assert.IsFalse(prop is null);
            Assert.AreEqual(prop.value, 9001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_get_property_no_such_property() {
            Character c = this.get_test_character();

            c.get_property(new List<string>() { "Skills", "Fishing" });
        }

        [TestMethod]
        public void test_set_property_new() {
            Character c = this.get_test_character();
            CharNumProperty new_prop = new CharNumProperty(42);

            CharProperty old_prop = c.set_property(new List<string>() { "Skills", "Fury" }, new_prop);
            Assert.IsTrue(old_prop is null);

            CharNumProperty prop = c.get_property(new List<string>() { "Skills", "Fury" }) as CharNumProperty;
            Assert.IsFalse(prop is null);
            Assert.AreEqual(prop.value, new_prop.value);
        }

        [TestMethod]
        public void test_set_property_replace() {
            Character c = this.get_test_character();
            CharTextProperty new_prop = new CharTextProperty("Krakrox has no time for skills!");

            CharDictProperty old_prop = c.set_property(new List<string>() { "Skills" }, new_prop) as CharDictProperty;
            Assert.IsFalse(old_prop is null);
            Assert.AreEqual(old_prop.value.Count, 2);
            Assert.IsTrue(old_prop.value.ContainsKey("Diplomacy"));
            Assert.IsTrue(old_prop.value.ContainsKey("Seal Clubbing"));

            CharTextProperty prop = c.get_property(new List<string>() { "Skills" }) as CharTextProperty;
            Assert.IsFalse(prop is null);
            Assert.AreEqual(prop.value, new_prop.value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_set_property_no_such_parent() {
            Character c = this.get_test_character();
            CharNumProperty new_prop = new CharNumProperty(2);

            c.set_property(new List<string>() { "Spells per Day", "Level 1" }, new_prop);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_set_property_non_dict_parent() {
            Character c = this.get_test_character();
            CharNumProperty new_prop = new CharNumProperty(1000);

            c.set_property(new List<string>() { "XP", "Next Level" }, new_prop);
        }

        [TestMethod]
        public void test_remove_property() {
            Character c = this.get_test_character();

            Assert.IsTrue(c.properties.value.ContainsKey("XP"));

            CharNumProperty old_prop = c.remove_property(new List<string>() { "XP" }) as CharNumProperty;
            Assert.IsFalse(old_prop is null);
            Assert.AreEqual(old_prop.value, 42);

            Assert.IsFalse(c.properties.value.ContainsKey("XP"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_property_no_such_property() {
            Character c = this.get_test_character();

            c.remove_property(new List<string>() { "Spells" });
        }
    }
}