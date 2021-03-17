using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    }
}