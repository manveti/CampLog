using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

using CampLog;


namespace CampLogTest {
    [TestClass]
    public class TestItemCategory {
        [TestMethod]
        public void test_equals() {
            ItemCategory foo = new ItemCategory("blah", 1), bar = new ItemCategory("blah", 1), baz = new ItemCategory("bloh", 1), qux = new ItemCategory("blah", .5m);

            Assert.IsTrue(foo.Equals(foo), "same object");
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(foo == foo, "same object, == operator");
            Assert.IsFalse(foo != foo, "same object, != operator");
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.AreEqual(foo, foo, "same object, AreEqual");

            Assert.IsTrue(foo.Equals(bar), "same properties");
            Assert.IsTrue(foo == bar, "same properties, == operator");
            Assert.IsFalse(foo != bar, "same properties, != operator");
            Assert.AreEqual(foo, bar, "same properties, AreEqual");
            Assert.IsTrue(bar == foo, "same properties, == operator");
            Assert.IsFalse(bar != foo, "same properties, != operator");
            Assert.AreEqual(bar, foo, "same properties, AreEqual");

            Assert.IsFalse(foo.Equals(baz), "different name");
            Assert.IsFalse(foo == baz, "different name, != operator");
            Assert.IsTrue(foo != baz, "different name, == operator");
            Assert.AreNotEqual(foo, baz, "different name, AreNotEqual");
            Assert.IsFalse(baz == foo, "different name, != operator");
            Assert.IsTrue(baz != foo, "different name, == operator");
            Assert.AreNotEqual(baz, foo, "different name, AreNotEqual");

            Assert.IsFalse(foo.Equals(qux), "different sale_value");
            Assert.IsFalse(foo == qux, "different sale_value, != operator");
            Assert.IsTrue(foo != qux, "different sale_value, == operator");
            Assert.AreNotEqual(foo, qux, "different sale_value, AreNotEqual");
            Assert.IsFalse(qux == foo, "different sale_value, != operator");
            Assert.IsTrue(qux != foo, "different sale_value, == operator");
            Assert.AreNotEqual(qux, foo, "different sale_value, AreNotEqual");

            Assert.IsFalse(foo.Equals(null), "null");
            Assert.IsFalse(foo == null, "null, != operator");
            Assert.IsTrue(foo != null, "null, == operator");
            Assert.AreNotEqual(foo, null, "null, AreNotEqual");
            Assert.IsFalse(null == foo, "null, != operator");
            Assert.IsTrue(null != foo, "null, == operator");
            Assert.AreNotEqual(null, foo, "null, AreNotEqual");
        }

        [TestMethod]
        public void test_serialization() {
            ItemCategory foo = new ItemCategory("blah", .5m), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ItemCategory));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ItemCategory)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo, bar);
        }
    }


    [TestClass]
    public class TestContainerSpec {
        [TestMethod]
        public void test_equals() {
            ContainerSpec foo = new ContainerSpec("blah", 42, 1), bar = new ContainerSpec("blah", 42, 1), baz = new ContainerSpec("bloh", 42, 1),
                qux = new ContainerSpec("blah", 18, 1), zap = new ContainerSpec("blah", 42, 0);

            Assert.IsTrue(foo.Equals(foo), "same object");
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(foo == foo, "same object, == operator");
            Assert.IsFalse(foo != foo, "same object, != operator");
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.AreEqual(foo, foo, "same object, AreEqual");

            Assert.IsTrue(foo.Equals(bar), "same properties");
            Assert.IsTrue(foo == bar, "same properties, == operator");
            Assert.IsFalse(foo != bar, "same properties, != operator");
            Assert.AreEqual(foo, bar, "same properties, AreEqual");
            Assert.IsTrue(bar.Equals(foo), "same properties");
            Assert.IsTrue(bar == foo, "same properties, == operator");
            Assert.IsFalse(bar != foo, "same properties, != operator");
            Assert.AreEqual(bar, foo, "same properties, AreEqual");

            Assert.IsFalse(foo.Equals(baz), "different name");
            Assert.IsFalse(foo == baz, "different name, != operator");
            Assert.IsTrue(foo != baz, "different name, == operator");
            Assert.AreNotEqual(foo, baz, "different name, AreNotEqual");
            Assert.IsFalse(baz.Equals(foo), "different name");
            Assert.IsFalse(baz == foo, "different name, != operator");
            Assert.IsTrue(baz != foo, "different name, == operator");
            Assert.AreNotEqual(baz, foo, "different name, AreNotEqual");

            Assert.IsFalse(foo.Equals(qux), "different weight_capacity");
            Assert.IsFalse(foo == qux, "different weight_capacity, != operator");
            Assert.IsTrue(foo != qux, "different weight_capacity, == operator");
            Assert.AreNotEqual(foo, qux, "different weight_capacity, AreNotEqual");
            Assert.IsFalse(qux.Equals(foo), "different weight_capacity");
            Assert.IsFalse(qux == foo, "different weight_capacity, != operator");
            Assert.IsTrue(qux != foo, "different weight_capacity, == operator");
            Assert.AreNotEqual(qux, foo, "different weight_capacity, AreNotEqual");

            Assert.IsFalse(foo.Equals(zap), "different weight_factor");
            Assert.IsFalse(foo == zap, "different weight_factor, != operator");
            Assert.IsTrue(foo != zap, "different weight_factor, == operator");
            Assert.AreNotEqual(foo, zap, "different weight_factor, AreNotEqual");
            Assert.IsFalse(zap.Equals(foo), "different weight_factor");
            Assert.IsFalse(zap == foo, "different weight_factor, != operator");
            Assert.IsTrue(zap != foo, "different weight_factor, == operator");
            Assert.AreNotEqual(zap, foo, "different weight_factor, AreNotEqual");

            Assert.IsFalse(foo.Equals(null), "null");
            Assert.IsFalse(foo == null, "null, != operator");
            Assert.IsTrue(foo != null, "null, == operator");
            Assert.AreNotEqual(foo, null, "null, AreNotEqual");
            Assert.IsFalse(null == foo, "null, != operator");
            Assert.IsTrue(null != foo, "null, == operator");
            Assert.AreNotEqual(null, foo, "null, AreNotEqual");
        }

        [TestMethod]
        public void test_serialization() {
            ContainerSpec foo = new ContainerSpec("blah", 42, .5m), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ContainerSpec));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ContainerSpec)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo, bar);
        }
    }

    [TestClass]
    public class TestItemSpec {
        [TestMethod]
        public void test_equals() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m), c3 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 30, 0), reducer = new ContainerSpec("Weight Reducer", 100, .5m);
            ItemSpec foo = new ItemSpec("Gem", c1, 100, 0), bar = new ItemSpec("Gem", c1, 100, 0), baz = new ItemSpec("Longsword", c2, 30, 3),
                qux = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }),
                zap = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, new ContainerSpec("Magic Pouch", 30, 0) }),
                tap = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch });

            Assert.IsTrue(foo.Equals(foo), "same object");
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(foo == foo, "same object, == operator");
            Assert.IsFalse(foo != foo, "same object, != operator");
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.AreEqual(foo, foo, "same object, AreEqual");

            Assert.IsTrue(foo.Equals(bar), "same properties");
            Assert.IsTrue(foo == bar, "same properties, == operator");
            Assert.IsFalse(foo != bar, "same properties, != operator");
            Assert.AreEqual(foo, bar, "same properties, AreEqual");
            Assert.IsTrue(bar.Equals(foo), "same properties");
            Assert.IsTrue(bar == foo, "same properties, == operator");
            Assert.IsFalse(bar != foo, "same properties, != operator");
            Assert.AreEqual(bar, foo, "same properties, AreEqual");

            Assert.IsFalse(foo.Equals(baz), "different properties");
            Assert.IsFalse(foo == baz, "different properties, != operator");
            Assert.IsTrue(foo != baz, "different properties, == operator");
            Assert.AreNotEqual(foo, baz, "different properties, AreNotEqual");
            Assert.IsFalse(baz.Equals(foo), "different properties");
            Assert.IsFalse(baz == foo, "different properties, != operator");
            Assert.IsTrue(baz != foo, "different properties, == operator");
            Assert.AreNotEqual(baz, foo, "different properties, AreNotEqual");

            Assert.IsTrue(qux.Equals(zap), "same containers");
            Assert.IsTrue(qux == zap, "same containers, == operator");
            Assert.IsFalse(qux != zap, "same containers, != operator");
            Assert.AreEqual(qux, zap, "same containers, AreEqual");
            Assert.IsTrue(zap.Equals(qux), "same containers");
            Assert.IsTrue(zap == qux, "same containers, == operator");
            Assert.IsFalse(zap != qux, "same containers, != operator");
            Assert.AreEqual(zap, qux, "same containers, AreEqual");

            Assert.IsFalse(qux.Equals(tap), "different containers");
            Assert.IsFalse(qux == tap, "different containers, != operator");
            Assert.IsTrue(qux != tap, "different containers, == operator");
            Assert.AreNotEqual(qux, tap, "different containers, AreNotEqual");
            Assert.IsFalse(tap.Equals(qux), "different containers");
            Assert.IsFalse(tap == qux, "different containers, != operator");
            Assert.IsTrue(tap != qux, "different containers, == operator");
            Assert.AreNotEqual(tap, qux, "different containers, AreNotEqual");

            Assert.IsFalse(foo.Equals(null), "null");
            Assert.IsFalse(foo == null, "null, != operator");
            Assert.IsTrue(foo != null, "null, == operator");
            Assert.AreNotEqual(foo, null, "null, AreNotEqual");
            Assert.IsFalse(null == foo, "null, != operator");
            Assert.IsTrue(null != foo, "null, == operator");
            Assert.AreNotEqual(null, foo, "null, AreNotEqual");
        }

        [TestMethod]
        public void test_serialization() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 30, 0), reducer = new ContainerSpec("Weight Reducer", 100, .5m);
            ItemSpec foo = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ItemSpec));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ItemSpec)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo, bar);
            Assert.IsTrue(ReferenceEquals(bar.containers[1], bar.containers[2]));
        }

        [TestMethod]
        public void test_value() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);

            ItemSpec foo = new ItemSpec("Gem", c1, 100, 0);
            Assert.AreEqual(foo.value, 100);

            ItemSpec bar = new ItemSpec("Longsword", c2, 30, 3);
            Assert.AreEqual(bar.value, 15);

            ItemSpec baz = new ItemSpec("MacGuffin", c1, 100, 0, 75);
            Assert.AreEqual(baz.value, 75);
        }
    }
}
