using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_category_null_checking() {
            ItemSpec _ = new ItemSpec("Gem", null, 100, 0);
        }

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


    [TestClass]
    public class TestInventory {
        [TestMethod]
        public void test_empty() {
            Inventory inv = new Inventory();

            Assert.IsFalse(inv.contents is null);
            Assert.AreEqual(inv.contents.Count, 0);
            Assert.AreEqual(inv.weight, 0);
            Assert.AreEqual(inv.value, 0);
        }

        [TestMethod]
        public void test_serialization() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);
            Inventory foo = new Inventory(), bar;

            foo.add(gem_stack);
            foo.add(gp_stack);
            foo.add(sword_stack);

            DataContractSerializer fmt = new DataContractSerializer(typeof(Inventory));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (Inventory)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.contents.Count, bar.contents.Count);
            foreach (Guid guid in foo.contents.Keys) {
                Assert.IsTrue(bar.contents.ContainsKey(guid));
                Assert.AreEqual(foo.contents[guid].item, bar.contents[guid].item);
                Assert.AreEqual(foo.contents[guid].name, bar.contents[guid].name);
                Assert.AreEqual(foo.contents[guid].weight, bar.contents[guid].weight);
                Assert.AreEqual(foo.contents[guid].value, bar.contents[guid].value);
            }
            Assert.AreEqual(foo.weight, bar.weight);
            Assert.AreEqual(foo.value, bar.value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_add_null() {
            Inventory inv = new Inventory();
            inv.add(null);
        }

        [TestMethod]
        public void test_add() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);
            Inventory inv = new Inventory();
            Guid gem_guid, gp_guid, sword_guid;

            gem_guid = inv.add(gem_stack);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(gem_guid));
            Assert.AreEqual(inv.contents[gem_guid], gem_stack);
            Assert.AreEqual(inv.weight, 3);
            Assert.AreEqual(inv.value, 300);

            gp_guid = inv.add(gp_stack);
            Assert.AreEqual(inv.contents.Count, 2);
            Assert.IsTrue(inv.contents.ContainsKey(gem_guid));
            Assert.IsTrue(inv.contents.ContainsKey(gp_guid));
            Assert.AreEqual(inv.contents[gem_guid], gem_stack);
            Assert.AreEqual(inv.contents[gp_guid], gp_stack);
            Assert.AreEqual(inv.weight, 3);
            Assert.AreEqual(inv.value, 450);

            sword_guid = inv.add(sword_stack);
            Assert.AreEqual(inv.contents.Count, 3);
            Assert.IsTrue(inv.contents.ContainsKey(gem_guid));
            Assert.IsTrue(inv.contents.ContainsKey(gp_guid));
            Assert.IsTrue(inv.contents.ContainsKey(sword_guid));
            Assert.AreEqual(inv.contents[gem_guid], gem_stack);
            Assert.AreEqual(inv.contents[gp_guid], gp_stack);
            Assert.AreEqual(inv.contents[sword_guid], sword_stack);
            Assert.AreEqual(inv.weight, 9);
            Assert.AreEqual(inv.value, 480);
        }

        [TestMethod]
        public void test_update() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Inventory inv = new Inventory();

            inv.add(gem_stack);
            Assert.AreEqual(inv.weight, 3);
            Assert.AreEqual(inv.value, 300);

            gem_stack.count = 7;
            Assert.AreEqual(inv.weight, 7);
            Assert.AreEqual(inv.value, 700);
        }
    }


    [TestClass]
    public class TestItemStack {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_item_null_checking() {
            ItemStack _ = new ItemStack(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_unidentified_bounds_checking() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec itm = new ItemSpec("Gem", cat, 100, 0);
            ItemStack _ = new ItemStack(itm, 4, 5);
        }

        [TestMethod]
        public void test_serialization() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec itm = new ItemSpec("Gem", cat, 100, 0);
            ItemStack foo = new ItemStack(itm, 3, 1), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ItemStack));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ItemStack)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.item, bar.item);
            Assert.AreEqual(foo.count, bar.count);
            Assert.AreEqual(foo.unidentified, bar.unidentified);
            Assert.AreEqual(foo.name, bar.name);
        }

        [TestMethod]
        public void test_properties() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec itm = new ItemSpec("Gem", cat, 100, 1);

            ItemStack foo = new ItemStack(itm);
            Assert.AreEqual(foo.name, "Gem");
            Assert.AreEqual(foo.weight, 1);
            Assert.AreEqual(foo.value, 100);

            ItemStack bar = new ItemStack(itm, 1, 1);
            Assert.AreEqual(bar.name, "Gem (unidentified)");
            Assert.AreEqual(bar.weight, 1);
            Assert.AreEqual(bar.value, 100);

            ItemStack baz = new ItemStack(itm, 5);
            Assert.AreEqual(baz.name, "5x Gem");
            Assert.AreEqual(baz.weight, 5);
            Assert.AreEqual(baz.value, 500);

            ItemStack qux = new ItemStack(itm, 5, 5);
            Assert.AreEqual(qux.name, "5x Gem (unidentified)");
            Assert.AreEqual(qux.weight, 5);
            Assert.AreEqual(qux.value, 500);

            ItemStack zap = new ItemStack(itm, 5, 3);
            Assert.AreEqual(zap.name, "5x Gem (3 unidentified)");
            Assert.AreEqual(zap.weight, 5);
            Assert.AreEqual(zap.value, 500);
        }
    }


    [TestClass]
    public class TestSingleItem {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_item_null_checking() {
            SingleItem _ = new SingleItem(null);
        }

        [TestMethod]
        public void test_simple_item() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem itm = new SingleItem(wand);

            Assert.AreEqual(itm.item, wand);
            Assert.IsNull(itm.containers);
            Assert.AreEqual(itm.contents_weight, 0);
            Assert.AreEqual(itm.contents_value, 0);
            Assert.AreEqual(itm.name, "Wand of Kaplowie");
            Assert.AreEqual(itm.weight, 1);
            Assert.AreEqual(itm.value, 100);
        }

        [TestMethod]
        public void test_container_item() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 30, 0), reducer = new ContainerSpec("Weight Reducer", 100, .5m);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem itm = new SingleItem(sack);

            Assert.AreEqual(itm.item, sack);
            Assert.IsNotNull(itm.containers);
            Assert.AreEqual(itm.containers.Length, 3);
            Assert.AreEqual(itm.contents_weight, 0);
            Assert.AreEqual(itm.contents_value, 0);
            Assert.AreEqual(itm.name, "Handy Haversack");
            Assert.AreEqual(itm.weight, 20);
            Assert.AreEqual(itm.value, 1800);
        }

        [TestMethod]
        public void test_container_contents() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .6m), c3 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 30, 0), reducer = new ContainerSpec("Weight Reducer", 100, .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), sword = new ItemSpec("Longsword", c2, 30, 3),
                sack = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack gems = new ItemStack(gem, 5), swords = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            Assert.AreEqual(itm.item, sack);
            Assert.IsNotNull(itm.containers);
            Assert.AreEqual(itm.containers.Length, 3);
            Assert.AreEqual(itm.contents_weight, 0);
            Assert.AreEqual(itm.contents_value, 0);
            Assert.AreEqual(itm.name, "Handy Haversack");
            Assert.AreEqual(itm.weight, 20);
            Assert.AreEqual(itm.value, 1800);

            itm.containers[0].add(swords);
            Assert.AreEqual(itm.contents_weight, 3);
            Assert.AreEqual(itm.contents_value, 36);
            Assert.AreEqual(itm.weight, 23);
            Assert.AreEqual(itm.value, 1836);

            itm.containers[1].add(gems);
            Assert.AreEqual(itm.contents_weight, 3);
            Assert.AreEqual(itm.contents_value, 536);
            Assert.AreEqual(itm.weight, 23);
            Assert.AreEqual(itm.value, 2336);
        }

        [TestMethod]
        public void test_serialization() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .6m), c3 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 30, 0), reducer = new ContainerSpec("Weight Reducer", 100, .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), sword = new ItemSpec("Longsword", c2, 30, 3),
                sack = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack gems = new ItemStack(gem, 5), swords = new ItemStack(sword, 2);
            SingleItem foo = new SingleItem(sack), bar;

            foo.containers[0].add(swords);
            foo.containers[1].add(gems);

            DataContractSerializer fmt = new DataContractSerializer(typeof(SingleItem));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (SingleItem)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.item, bar.item);
            Assert.AreEqual(foo.containers.Length, bar.containers.Length);
            Assert.AreEqual(foo.contents_weight, bar.contents_weight);
            Assert.AreEqual(foo.contents_value, bar.contents_value);
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.weight, bar.weight);
            Assert.AreEqual(foo.value, bar.value);
        }
    }
}
