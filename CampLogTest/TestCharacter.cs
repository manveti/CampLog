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

        [TestMethod]
        public void test_copy() {
            CharTextProperty foo = new CharTextProperty("blah"), bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.AreEqual(foo.value, bar.value);
        }

        [TestMethod]
        public void test_add() {
            CharTextProperty foo = new CharTextProperty("blah"), bar = new CharTextProperty(" bloh");
            foo.add(bar);
            Assert.AreEqual(foo.value, "blah bloh");
        }

        [TestMethod]
        public void test_subtract() {
            CharTextProperty foo = new CharTextProperty("blah bloh bleh"), bar = new CharTextProperty(" bloh");
            foo.subtract(bar);
            Assert.AreEqual(foo.value, "blah bleh");
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

        [TestMethod]
        public void test_copy() {
            CharNumProperty foo = new CharNumProperty(1.3m), bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.AreEqual(foo.value, bar.value);
        }

        [TestMethod]
        public void test_add() {
            CharNumProperty foo = new CharNumProperty(3.5m), bar = new CharNumProperty(1.3m);
            foo.add(bar);
            Assert.AreEqual(foo.value, 4.8m);
        }

        [TestMethod]
        public void test_subtract() {
            CharNumProperty foo = new CharNumProperty(3.5m), bar = new CharNumProperty(1.3m);
            foo.subtract(bar);
            Assert.AreEqual(foo.value, 2.2m);
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

        [TestMethod]
        public void test_copy() {
            CharSetProperty foo = new CharSetProperty(), bar;
            foo.value.Add("blah");
            foo.value.Add("bloh");

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.IsFalse(ReferenceEquals(foo.value, bar.value));
            Assert.AreEqual(foo.value.Count, bar.value.Count);
            foreach (string s in foo.value) {
                Assert.IsTrue(bar.value.Contains(s));
            }
        }

        [TestMethod]
        public void test_add() {
            CharSetProperty foo = new CharSetProperty(), bar = new CharSetProperty();
            foo.value.Add("blah");
            bar.value.Add("bloh");

            foo.add(bar);
            Assert.AreEqual(foo.value.Count, 2);
            Assert.IsTrue(foo.value.Contains("blah"));
            Assert.IsTrue(foo.value.Contains("bloh"));
        }

        [TestMethod]
        public void test_subtract() {
            CharSetProperty foo = new CharSetProperty(), bar = new CharSetProperty();
            foo.value.Add("blah");
            foo.value.Add("bloh");
            foo.value.Add("bleh");
            bar.value.Add("bloh");

            foo.subtract(bar);
            Assert.AreEqual(foo.value.Count, 2);
            Assert.IsTrue(foo.value.Contains("blah"));
            Assert.IsTrue(foo.value.Contains("bleh"));
        }

        [TestMethod]
        public void test_subtract_extra_refs() {
            CharSetProperty foo = new CharSetProperty(), bar = new CharSetProperty();
            foo.value.Add("blah", 2);
            foo.value.Add("bloh");
            foo.value.Add("bleh");
            bar.value.Add("blah");
            bar.value.Add("bloh");

            foo.subtract(bar);
            Assert.AreEqual(foo.value.Count, 2);
            Assert.IsTrue(foo.value.Contains("blah"));
            Assert.IsTrue(foo.value.Contains("bleh"));
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

        [TestMethod]
        public void test_copy() {
            CharDictProperty foo = new CharDictProperty(), bar;
            CharTextProperty tprop = new CharTextProperty("blah");
            CharNumProperty nprop = new CharNumProperty(1.23m);
            CharSetProperty sprop = new CharSetProperty();
            sprop.value.Add("bloh");
            sprop.value.Add("bleh");
            foo.value["Some Text"] = tprop;
            foo.value["Some Number"] = nprop;
            foo.value["Some Collection"] = sprop;

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.IsFalse(ReferenceEquals(foo.value, bar.value));
            Assert.AreEqual(foo.value.Count, bar.value.Count);
            foreach (string s in foo.value.Keys) {
                Assert.IsTrue(bar.value.ContainsKey(s));
                Assert.IsFalse(ReferenceEquals(foo.value[s], bar.value[s]));
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

        [TestMethod]
        public void test_add() {
            CharDictProperty foo = new CharDictProperty(), bar = new CharDictProperty();
            CharTextProperty tprop = new CharTextProperty("blah");
            CharNumProperty nprop = new CharNumProperty(1.23m);
            foo.value["Some Text"] = tprop;
            bar.value["Some Number"] = nprop;

            foo.add(bar);
            Assert.AreEqual(foo.value.Count, 2);
            Assert.IsTrue(foo.value.ContainsKey("Some Text"));
            Assert.IsTrue(foo.value.ContainsKey("Some Number"));
            Assert.IsFalse(ReferenceEquals(foo.value["Some Number"], bar.value["Some Number"]));
            Assert.AreEqual((foo.value["Some Number"] as CharNumProperty).value, (bar.value["Some Number"] as CharNumProperty).value);
        }

        [TestMethod]
        public void test_subtract() {
            CharDictProperty foo = new CharDictProperty(), bar = new CharDictProperty();
            CharTextProperty tprop = new CharTextProperty("blah");
            CharNumProperty nprop = new CharNumProperty(1.23m);
            foo.value["Some Text"] = tprop;
            foo.value["Some Number"] = nprop;
            bar.value["Some Number"] = nprop;

            foo.subtract(bar);
            Assert.AreEqual(foo.value.Count, 1);
            Assert.IsTrue(foo.value.ContainsKey("Some Text"));
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

        [TestMethod]
        public void test_copy() {
            Character foo = new Character("Somebody"), bar;
            CharTextProperty tprop = new CharTextProperty("blah");
            CharNumProperty nprop = new CharNumProperty(1.23m);
            CharSetProperty sprop = new CharSetProperty();
            sprop.value.Add("bloh");
            sprop.value.Add("bleh");
            foo.properties.value["Some Text"] = tprop;
            foo.properties.value["Some Number"] = nprop;
            foo.properties.value["Some Collection"] = sprop;

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.IsFalse(ReferenceEquals(foo.properties, bar.properties));
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.properties.value.Count, bar.properties.value.Count);
            foreach (string s in foo.properties.value.Keys) {
                Assert.IsTrue(bar.properties.value.ContainsKey(s));
                Assert.IsFalse(ReferenceEquals(foo.properties.value[s], bar.properties.value[s]));
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


    [TestClass]
    public class TestCharacterDomain {
        [TestMethod]
        public void test_serialization() {
            Character c1 = new Character("Somebody"), c2 = new Character("Mr. Boddy");
            CharacterDomain foo = new CharacterDomain(), bar;

            foo.add_character(c1);
            Guid mr_boddy = foo.add_character(c2);
            foo.remove_character(mr_boddy);

            DataContractSerializer fmt = new DataContractSerializer(typeof(CharacterDomain));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CharacterDomain)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.characters.Count, bar.characters.Count);
            foreach (Guid chr in foo.characters.Keys) {
                Assert.IsTrue(bar.characters.ContainsKey(chr));
                Assert.AreEqual(foo.characters[chr].name, bar.characters[chr].name);
            }
            Assert.AreEqual(foo.active_characters.Count, bar.active_characters.Count);
            foreach (Guid chr in foo.active_characters) {
                Assert.IsTrue(bar.active_characters.Contains(chr));
            }
        }

        [TestMethod]
        public void test_copy() {
            Character c1 = new Character("Somebody"), c2 = new Character("Mr. Boddy");
            CharacterDomain foo = new CharacterDomain(), bar;

            foo.add_character(c1);
            Guid mr_boddy = foo.add_character(c2);
            foo.remove_character(mr_boddy);

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.IsFalse(ReferenceEquals(foo.characters, bar.characters));
            Assert.AreEqual(foo.characters.Count, bar.characters.Count);
            foreach (Guid chr in foo.characters.Keys) {
                Assert.IsTrue(bar.characters.ContainsKey(chr));
                Assert.IsFalse(ReferenceEquals(foo.characters[chr], bar.characters[chr]));
                Assert.AreEqual(foo.characters[chr].name, bar.characters[chr].name);
            }
            Assert.IsFalse(ReferenceEquals(foo.active_characters, bar.active_characters));
            Assert.AreEqual(foo.active_characters.Count, bar.active_characters.Count);
            foreach (Guid chr in foo.active_characters) {
                Assert.IsTrue(bar.active_characters.Contains(chr));
            }
        }

        [TestMethod]
        public void test_add_character() {
            CharacterDomain domain = new CharacterDomain();
            Character chr = new Character("Somebody");

            Guid chr_guid = domain.add_character(chr);
            Assert.AreEqual(domain.characters.Count, 1);
            Assert.IsTrue(domain.characters.ContainsKey(chr_guid));
            Assert.AreEqual(domain.characters[chr_guid], chr);
            Assert.AreEqual(domain.active_characters.Count, 1);
            Assert.IsTrue(domain.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_add_character_null() {
            CharacterDomain domain = new CharacterDomain();

            domain.add_character(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_character_duplicate_character() {
            CharacterDomain domain = new CharacterDomain();
            Character chr = new Character("Somebody");

            domain.add_character(chr);
            domain.add_character(chr);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_character_duplicate_guid() {
            CharacterDomain domain = new CharacterDomain();
            Character chr1 = new Character("Somebody"), chr2 = new Character("Someone Else");

            Guid chr_guid = domain.add_character(chr1);
            domain.add_character(chr2, chr_guid);
        }

        [TestMethod]
        public void test_remove_character() {
            CharacterDomain domain = new CharacterDomain();
            Character chr = new Character("Somebody");
            Guid chr_guid = domain.add_character(chr);

            domain.remove_character(chr_guid);
            Assert.AreEqual(domain.characters.Count, 1);
            Assert.IsTrue(domain.characters.ContainsKey(chr_guid));
            Assert.AreEqual(domain.characters[chr_guid], chr);
            Assert.AreEqual(domain.active_characters.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_character_inactive() {
            CharacterDomain domain = new CharacterDomain();
            Character chr = new Character("Somebody");
            Guid chr_guid = domain.add_character(chr);

            domain.remove_character(chr_guid);
            domain.remove_character(chr_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_character_no_such_guid() {
            CharacterDomain domain = new CharacterDomain();
            domain.remove_character(Guid.NewGuid());
        }

        [TestMethod]
        public void test_restore_character() {
            CharacterDomain domain = new CharacterDomain();
            Character chr = new Character("Somebody");
            Guid chr_guid = domain.add_character(chr);

            domain.remove_character(chr_guid);
            domain.restore_character(chr_guid);
            Assert.AreEqual(domain.characters.Count, 1);
            Assert.IsTrue(domain.characters.ContainsKey(chr_guid));
            Assert.AreEqual(domain.characters[chr_guid], chr);
            Assert.AreEqual(domain.active_characters.Count, 1);
            Assert.IsTrue(domain.active_characters.Contains(chr_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_character_active() {
            CharacterDomain domain = new CharacterDomain();
            Character chr = new Character("Somebody");
            Guid chr_guid = domain.add_character(chr);

            domain.restore_character(chr_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_character_no_such_guid() {
            CharacterDomain domain = new CharacterDomain();
            domain.restore_character(Guid.NewGuid());
        }
    }
}