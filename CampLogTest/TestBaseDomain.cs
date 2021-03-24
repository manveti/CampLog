using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestBaseDomain {
        [TestMethod]
        public void test_serialization() {
            string s1 = "Some current item", s2 = "Some former item";
            BaseDomain<string> foo = new BaseDomain<string>(), bar;

            foo.add_item(s1);
            Guid rem_guid = foo.add_item(s2);
            foo.remove_item(rem_guid);

            DataContractSerializer fmt = new DataContractSerializer(typeof(BaseDomain<string>));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (BaseDomain<string>)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.items.Count, bar.items.Count);
            foreach (Guid item in foo.items.Keys) {
                Assert.IsTrue(bar.items.ContainsKey(item));
                Assert.AreEqual(foo.items[item], bar.items[item]);
            }
            Assert.AreEqual(foo.active_items.Count, bar.active_items.Count);
            foreach (Guid item in foo.active_items) {
                Assert.IsTrue(bar.active_items.Contains(item));
            }
        }

        [TestMethod]
        public void test_add_item() {
            BaseDomain<string> domain = new BaseDomain<string>();
            string s = "Some item";

            Guid item_guid = domain.add_item(s);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(item_guid));
            Assert.AreEqual(domain.items[item_guid], s);
            Assert.AreEqual(domain.active_items.Count, 1);
            Assert.IsTrue(domain.active_items.Contains(item_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_add_item_null() {
            BaseDomain<string> domain = new BaseDomain<string>();

            domain.add_item(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_item_duplicate() {
            BaseDomain<string> domain = new BaseDomain<string>();
            string s = "Some item";

            domain.add_item(s);
            domain.add_item(s);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_item_duplicate_guid() {
            BaseDomain<string> domain = new BaseDomain<string>();
            string s1 = "Some item", s2 = "Some other item";

            Guid item_guid = domain.add_item(s1);
            domain.add_item(s2, item_guid);
        }

        [TestMethod]
        public void test_remove_item() {
            BaseDomain<string> domain = new BaseDomain<string>();
            string s = "Some item";
            Guid item_guid = domain.add_item(s);

            domain.remove_item(item_guid);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(item_guid));
            Assert.AreEqual(domain.items[item_guid], s);
            Assert.AreEqual(domain.active_items.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_item_inactive() {
            BaseDomain<string> domain = new BaseDomain<string>();
            string s = "Some item";
            Guid item_guid = domain.add_item(s);

            domain.remove_item(item_guid);
            domain.remove_item(item_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_removeitem_no_such_guid() {
            BaseDomain<string> domain = new BaseDomain<string>();
            domain.remove_item(Guid.NewGuid());
        }

        [TestMethod]
        public void test_restore_item() {
            BaseDomain<string> domain = new BaseDomain<string>();
            string s = "Some item";
            Guid item_guid = domain.add_item(s);

            domain.remove_item(item_guid);
            domain.restore_item(item_guid);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(item_guid));
            Assert.AreEqual(domain.items[item_guid], s);
            Assert.AreEqual(domain.active_items.Count, 1);
            Assert.IsTrue(domain.active_items.Contains(item_guid));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_item_active() {
            BaseDomain<string> domain = new BaseDomain<string>();
            string s = "Some item";
            Guid item_guid = domain.add_item(s);

            domain.restore_item(item_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_item_no_such_guid() {
            BaseDomain<string> domain = new BaseDomain<string>();
            domain.restore_item(Guid.NewGuid());
        }
    }
}