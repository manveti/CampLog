using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestRefcountSet {
        [TestMethod]
        public void test_serialization() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 }, bar;

            foo.Add(3);

            DataContractSerializer fmt = new DataContractSerializer(typeof(RefcountSet<int>));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (RefcountSet<int>)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.Count, bar.Count);
            Assert.AreEqual(foo.contents.Count, bar.contents.Count);
            foreach (int i in foo.contents.Keys) {
                Assert.IsTrue(bar.contents.ContainsKey(i));
                Assert.AreEqual(foo.contents[i], bar.contents[i]);
            }
        }

        [TestMethod]
        public void test_constructor_dict() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 }, bar;

            foo.Add(3);

            bar = new RefcountSet<int>(foo.contents);
            Assert.AreEqual(foo.Count, bar.Count);
            Assert.IsFalse(ReferenceEquals(foo.contents, bar.contents));
            Assert.AreEqual(foo.contents.Count, bar.contents.Count);
            foreach (int i in foo.contents.Keys) {
                Assert.IsTrue(bar.contents.ContainsKey(i));
                Assert.AreEqual(foo.contents[i], bar.contents[i]);
            }
        }

        [TestMethod]
        public void test_constructor_set() {
            HashSet<int> foo = new HashSet<int>() { 1, 2, 3 };
            RefcountSet<int> bar = new RefcountSet<int>(foo);
            Assert.AreEqual(foo.Count, bar.Count);
            Assert.IsFalse(ReferenceEquals(foo, bar.contents));
            Assert.AreEqual(foo.Count, bar.contents.Count);
            foreach (int i in foo) {
                Assert.IsTrue(bar.contents.ContainsKey(i));
                Assert.AreEqual(bar.contents[i], 1);
            }
        }

        [TestMethod]
        public void test_constructor_refcount_set() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 }, bar;

            foo.Add(3);

            bar = new RefcountSet<int>(foo);
            Assert.AreEqual(foo.Count, bar.Count);
            Assert.IsFalse(ReferenceEquals(foo.contents, bar.contents));
            Assert.AreEqual(foo.contents.Count, bar.contents.Count);
            foreach (int i in foo.contents.Keys) {
                Assert.IsTrue(bar.contents.ContainsKey(i));
                Assert.AreEqual(bar.contents[i], 1);
            }
        }

        [TestMethod]
        public void test_add() {
            RefcountSet<string> foo = new RefcountSet<string>() { "foo", "bar" };
            foo.Add("baz");
            Assert.AreEqual(foo.Count, 3);
            Assert.IsTrue(foo.contents.ContainsKey("baz"));
            Assert.AreEqual(foo.contents["baz"], 1);
        }

        [TestMethod]
        public void test_add_increment() {
            RefcountSet<string> foo = new RefcountSet<string>() { "foo", "bar", "baz" };
            foo.Add("baz");
            Assert.AreEqual(foo.Count, 3);
            Assert.IsTrue(foo.contents.ContainsKey("baz"));
            Assert.AreEqual(foo.contents["baz"], 2);
        }

        [TestMethod]
        public void test_add_increment_count() {
            RefcountSet<string> foo = new RefcountSet<string>() { "foo", "bar", "baz" };
            foo.Add("baz", 5);
            Assert.AreEqual(foo.Count, 3);
            Assert.IsTrue(foo.contents.ContainsKey("baz"));
            Assert.AreEqual(foo.contents["baz"], 6);
        }

        [TestMethod]
        public void test_clear() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 };
            foo.Clear();
            Assert.AreEqual(foo.Count, 0);
            Assert.AreEqual(foo.contents.Count, 0);
        }

        [TestMethod]
        public void test_contains() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 };
            foo.Add(3);
            Assert.IsTrue(foo.Contains(1));
            Assert.IsTrue(foo.Contains(2));
            Assert.IsTrue(foo.Contains(3));
            Assert.IsFalse(foo.Contains(4));
        }

        [TestMethod]
        public void test_except_with() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 };
            HashSet<int> bar = new HashSet<int>() { 2, 3, 4 };

            foo.Add(3);

            foo.ExceptWith(bar);
            Assert.AreEqual(foo.Count, 1);
            Assert.IsTrue(foo.Contains(1));
            Assert.IsFalse(foo.Contains(2));
            Assert.IsFalse(foo.Contains(3));
            Assert.IsFalse(foo.Contains(4));
        }

        [TestMethod]
        public void test_except_with_refcount_set() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 }, bar = new RefcountSet<int>() { 2, 3, 4 };

            foo.Add(3);
            bar.Add(3);

            foo.ExceptWith(bar);
            Assert.AreEqual(foo.Count, 1);
            Assert.IsTrue(foo.Contains(1));
            Assert.IsFalse(foo.Contains(2));
            Assert.IsFalse(foo.Contains(3));
            Assert.IsFalse(foo.Contains(4));
        }

        [TestMethod]
        public void test_except_ref_with() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 }, bar = new RefcountSet<int>() { 2, 3, 4 };

            foo.Add(3);
            bar.Add(3);

            foo.ExceptRefWith(bar);
            Assert.AreEqual(foo.Count, 2);
            Assert.IsTrue(foo.Contains(1));
            Assert.IsFalse(foo.Contains(2));
            Assert.IsTrue(foo.Contains(3));
            Assert.IsFalse(foo.Contains(4));
        }

        [TestMethod]
        public void test_remove() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 };

            foo.Add(3);

            foo.Remove(2);
            foo.Remove(3);
            Assert.AreEqual(foo.Count, 1);
            Assert.IsTrue(foo.Contains(1));
            Assert.IsFalse(foo.Contains(2));
            Assert.IsFalse(foo.Contains(3));
            Assert.IsFalse(foo.Contains(4));
        }

        [TestMethod]
        public void test_remove_ref() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 };

            foo.Add(3);

            foo.RemoveRef(2);
            foo.RemoveRef(3);
            Assert.AreEqual(foo.Count, 2);
            Assert.IsTrue(foo.Contains(1));
            Assert.IsFalse(foo.Contains(2));
            Assert.IsTrue(foo.Contains(3));
            Assert.IsFalse(foo.Contains(4));
        }

        [TestMethod]
        public void test_remove_ref_count() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 };

            foo.Add(3, 5);

            foo.RemoveRef(2, 2);
            foo.RemoveRef(3, 2);
            Assert.AreEqual(foo.Count, 2);
            Assert.IsTrue(foo.Contains(1));
            Assert.IsFalse(foo.Contains(2));
            Assert.IsTrue(foo.Contains(3));
            Assert.IsFalse(foo.Contains(4));
            Assert.AreEqual(foo.contents[3], 4);
        }

        [TestMethod]
        public void test_union_with() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 };
            HashSet<int> bar = new HashSet<int>() { 2, 3, 4 };

            foo.Add(3);

            foo.UnionWith(bar);
            Assert.AreEqual(foo.Count, 4);
            Assert.AreEqual(foo.contents[1], 1);
            Assert.AreEqual(foo.contents[2], 2);
            Assert.AreEqual(foo.contents[3], 3);
            Assert.AreEqual(foo.contents[4], 1);
        }

        [TestMethod]
        public void test_union_with_refcount_set() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 }, bar = new RefcountSet<int>() { 2, 3, 4 };

            foo.Add(3);
            bar.Add(3);

            foo.UnionWith(bar);
            Assert.AreEqual(foo.Count, 4);
            Assert.AreEqual(foo.contents[1], 1);
            Assert.AreEqual(foo.contents[2], 2);
            Assert.AreEqual(foo.contents[3], 3);
            Assert.AreEqual(foo.contents[4], 1);
        }

        [TestMethod]
        public void test_add_refs() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3 }, bar = new RefcountSet<int>() { 2, 3, 4 };

            foo.Add(3);
            bar.Add(3);
            bar.Add(4, 4);

            foo.AddRefs(bar);
            Assert.AreEqual(foo.Count, 4);
            Assert.AreEqual(foo.contents[1], 1);
            Assert.AreEqual(foo.contents[2], 2);
            Assert.AreEqual(foo.contents[3], 4);
            Assert.AreEqual(foo.contents[4], 5);
        }

        [TestMethod]
        public void test_subtract_refs() {
            RefcountSet<int> foo = new RefcountSet<int>() { 1, 2, 3, 4 }, bar = new RefcountSet<int>() { 2, 3, 4 };

            foo.Add(3);
            foo.Add(4);
            bar.Add(3);

            foo.SubtractRefs(bar);
            Assert.AreEqual(foo.Count, 2);
            Assert.AreEqual(foo.contents[1], 1);
            Assert.IsFalse(foo.Contains(2));
            Assert.IsFalse(foo.Contains(3));
            Assert.AreEqual(foo.contents[4], 1);
        }
    }
}