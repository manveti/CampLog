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
            ContainerSpec foo = new ContainerSpec("blah", 1, 42), bar = new ContainerSpec("blah", 1, 42), baz = new ContainerSpec("bloh", 1, 42),
                qux = new ContainerSpec("blah", 1, 18), zap = new ContainerSpec("blah", 0, 42);

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
            ContainerSpec foo = new ContainerSpec("blah", .5m, 42), bar;
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
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec foo = new ItemSpec("Gem", c1, 100, 0), bar = new ItemSpec("Gem", c1, 100, 0), baz = new ItemSpec("Longsword", c2, 30, 3),
                qux = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }),
                zap = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, new ContainerSpec("Magic Pouch", 0, 30) }),
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
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
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

            ItemSpec qux = new ItemSpec("Magic Sword", c2, 1000, 3, 750);
            Assert.AreEqual(qux.value, 750);
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
            Inventory foo = new Inventory("Test Inventory"), bar;

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
            Assert.AreEqual(foo.name, bar.name);
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
        public void test_copy() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);
            Inventory foo = new Inventory("Test Inventory"), bar;

            foo.add(gem_stack);
            foo.add(gp_stack);
            foo.add(sword_stack);

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.AreEqual(foo.name, bar.name);
            Assert.IsFalse(ReferenceEquals(foo.contents, bar.contents));
            Assert.AreEqual(foo.contents.Count, bar.contents.Count);
            foreach (Guid guid in foo.contents.Keys) {
                Assert.IsTrue(bar.contents.ContainsKey(guid));
                Assert.IsFalse(ReferenceEquals(foo.contents[guid], bar.contents[guid]));
                Assert.AreEqual(foo.contents[guid].item, bar.contents[guid].item);
                Assert.AreEqual(foo.contents[guid].name, bar.contents[guid].name);
                Assert.AreEqual(foo.contents[guid].weight, bar.contents[guid].weight);
                Assert.AreEqual(foo.contents[guid].value, bar.contents[guid].value);
            }
            Assert.AreEqual(foo.weight, bar.weight);
            Assert.AreEqual(foo.value, bar.value);
        }

        [TestMethod]
        public void test_contains_inventory() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }),
                belt = new ItemSpec("Belt of Pockets", cat, 1000, 5, null, new ContainerSpec[] { pouch, pouch, pouch, pouch });
            SingleItem sack_itm = new SingleItem(sack), belt_itm = new SingleItem(belt);
            Inventory inv = new Inventory();

            inv.add(sack_itm);
            for (int i = 0; i < 3; i++) {
                Assert.IsTrue(inv.contains_inventory(sack_itm.containers[i]));
            }
            for (int i = 0; i < 4; i++) {
                Assert.IsFalse(inv.contains_inventory(belt_itm.containers[i]));
            }
            Assert.IsFalse(inv.contains_inventory(inv));

            sack_itm.add(0, belt_itm);
            for (int i = 0; i < 4; i++) {
                Assert.IsTrue(inv.contains_inventory(belt_itm.containers[i]));
            }
            Assert.IsFalse(inv.contains_inventory(inv));
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
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_cycle() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem itm = new SingleItem(sack);
            Inventory inv = new Inventory();

            itm.containers[1] = inv;
            inv.add(itm);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_duplicate_guid() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1), gp = new ItemSpec("GP", cat, 1, 0);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150);
            Guid test_guid = Guid.NewGuid(), gem_guid;
            Inventory inv = new Inventory();

            gem_guid = inv.add(gem_stack, test_guid);
            Assert.AreEqual(gem_guid, test_guid);

            _ = inv.add(gp_stack, test_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_no_such_guid() {
            Inventory inv = new Inventory();
            inv.remove(Guid.NewGuid());
        }

        [TestMethod]
        public void test_remove() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1), gp = new ItemSpec("GP", cat, 1, 0.02m);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150);
            Guid gem_guid, gp_guid;
            Inventory inv = new Inventory();

            gem_guid = inv.add(gem_stack);
            gp_guid = inv.add(gp_stack);
            Assert.AreEqual(inv.contents.Count, 2);
            Assert.IsTrue(inv.contents.ContainsKey(gem_guid));
            Assert.IsTrue(inv.contents.ContainsKey(gp_guid));
            Assert.AreEqual(inv.contents[gem_guid], gem_stack);
            Assert.AreEqual(inv.contents[gp_guid], gp_stack);
            Assert.AreEqual(inv.weight, 6);
            Assert.AreEqual(inv.value, 450);

            inv.remove(gem_guid);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsFalse(inv.contents.ContainsKey(gem_guid));
            Assert.IsTrue(inv.contents.ContainsKey(gp_guid));
            Assert.AreEqual(inv.contents[gp_guid], gp_stack);
            Assert.AreEqual(inv.weight, 3);
            Assert.AreEqual(inv.value, 150);
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

        [TestMethod]
        public void test_merge() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 3, 1), gem_stack2 = new ItemStack(gem, 5, 2);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);
            Guid gem_ent2 = inv.add(gem_stack2);
            Assert.AreEqual(inv.contents.Count, 2);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent2));

            Guid new_ent = inv.merge(gem_ent1, gem_ent2);
            Assert.AreEqual(new_ent, gem_ent1);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsFalse(inv.contents.ContainsKey(gem_ent2));
            Assert.AreEqual(gem_stack1.count, 8);
            Assert.AreEqual(gem_stack1.unidentified, 3);
        }

        [TestMethod]
        public void test_merge_stack_single() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3, 1);
            SingleItem gem_item = new SingleItem(gem, true);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack);
            Guid gem_ent2 = inv.add(gem_item);
            Assert.AreEqual(inv.contents.Count, 2);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent2));

            Guid new_ent = inv.merge(gem_ent1, gem_ent2);
            Assert.AreEqual(new_ent, gem_ent1);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsFalse(inv.contents.ContainsKey(gem_ent2));
            Assert.AreEqual(gem_stack.count, 4);
            Assert.AreEqual(gem_stack.unidentified, 2);
        }

        [TestMethod]
        public void test_merge_single_stack() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3, 1);
            SingleItem gem_item = new SingleItem(gem, true);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack);
            Guid gem_ent2 = inv.add(gem_item);
            Assert.AreEqual(inv.contents.Count, 2);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent2));

            Guid new_ent = inv.merge(gem_ent2, gem_ent1);
            Assert.AreEqual(new_ent, gem_ent1);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsFalse(inv.contents.ContainsKey(gem_ent2));
            Assert.AreEqual(gem_stack.count, 4);
            Assert.AreEqual(gem_stack.unidentified, 2);
        }

        [TestMethod]
        public void test_merge_single_single() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem_item1 = new SingleItem(gem, true), gem_item2 = new SingleItem(gem, false);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_item1);
            Guid gem_ent2 = inv.add(gem_item2);
            Assert.AreEqual(inv.contents.Count, 2);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent2));

            Guid new_ent = inv.merge(gem_ent1, gem_ent2);
            Assert.AreNotEqual(new_ent, gem_ent1);
            Assert.AreNotEqual(new_ent, gem_ent2);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(new_ent));
            Assert.IsFalse(inv.contents.ContainsKey(gem_ent1));
            Assert.IsFalse(inv.contents.ContainsKey(gem_ent2));

            ItemStack stack = inv.contents[new_ent] as ItemStack;
            Assert.IsFalse(stack is null);
            Assert.AreEqual(stack.count, 2);
            Assert.AreEqual(stack.unidentified, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void test_merge_different_items() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1), gp = new ItemSpec("GP", cat, 1, 0);
            ItemStack gem_stack = new ItemStack(gem, 3, 1), gp_stack = new ItemStack(gp, 50);
            Inventory inv = new Inventory();

            Guid gem_ent = inv.add(gem_stack);
            Guid gp_ent = inv.add(gp_stack);

            inv.merge(gem_ent, gp_ent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_merge_value_override() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem_item1 = new SingleItem(gem, true, 150), gem_item2 = new SingleItem(gem, false);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_item1);
            Guid gem_ent2 = inv.add(gem_item2);

            inv.merge(gem_ent1, gem_ent2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_merge_containers() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem sack_item1 = new SingleItem(sack), sack_item2 = new SingleItem(sack);
            Inventory inv = new Inventory();

            Guid sack_ent1 = inv.add(sack_item1);
            Guid sack_ent2 = inv.add(sack_item2);

            inv.merge(sack_ent1, sack_ent2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_merge_properties() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand_item1 = new SingleItem(wand), wand_item2 = new SingleItem(wand);
            Inventory inv = new Inventory();

            wand_item1.properties["charges"] = "42";

            Guid wand_ent1 = inv.add(wand_item1);
            Guid wand_ent2 = inv.add(wand_item2);

            inv.merge(wand_ent1, wand_ent2);
        }

        [TestMethod]
        public void test_unstack() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));

            Guid gem_ent2 = inv.unstack(gem_ent1);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsFalse(inv.contents.ContainsKey(gem_ent1));
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent2));

            SingleItem gem_item = inv.contents[gem_ent2] as SingleItem;
            Assert.IsFalse(gem_item is null);
            Assert.AreEqual(gem_item.item, gem);
            Assert.IsFalse(gem_item.unidentified);
        }

        [TestMethod]
        public void test_unstack_unidentified() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1, 1);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));

            Guid gem_ent2 = inv.unstack(gem_ent1);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsFalse(inv.contents.ContainsKey(gem_ent1));
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent2));

            SingleItem gem_item = inv.contents[gem_ent2] as SingleItem;
            Assert.IsFalse(gem_item is null);
            Assert.AreEqual(gem_item.item, gem);
            Assert.IsTrue(gem_item.unidentified);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_unstack_non_stack() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem_item = new SingleItem(gem);
            Inventory inv = new Inventory();

            Guid gem_ent = inv.add(gem_item);

            inv.unstack(gem_ent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_unstack_more_than_one() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 2, 1);
            Inventory inv = new Inventory();

            Guid gem_ent = inv.add(gem_stack);

            inv.unstack(gem_ent);
        }

        [TestMethod]
        public void test_split() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);
            Assert.AreEqual(inv.contents.Count, 1);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));

            Guid gem_ent2 = inv.split(gem_ent1, 2, 1);
            Assert.AreEqual(inv.contents.Count, 2);
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent1));
            Assert.IsTrue(inv.contents.ContainsKey(gem_ent2));

            ItemStack gem_stack2 = inv.contents[gem_ent2] as ItemStack;
            Assert.IsFalse(gem_stack2 is null);
            Assert.AreEqual(gem_stack1.count, 6);
            Assert.AreEqual(gem_stack1.unidentified, 2);
            Assert.AreEqual(gem_stack2.count, 2);
            Assert.AreEqual(gem_stack2.unidentified, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_non_stack() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem_item = new SingleItem(gem);
            Inventory inv = new Inventory();

            Guid gem_ent = inv.add(gem_item);

            inv.split(gem_ent, 1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_count_too_low() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);

            inv.split(gem_ent1, -1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_count_too_high() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);

            inv.split(gem_ent1, 10, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_unidentified_too_low() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);

            inv.split(gem_ent1, 2, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_unidentified_too_high() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);

            inv.split(gem_ent1, 2, 4);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_unidentified_higher_than_split_count() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);

            inv.split(gem_ent1, 1, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_remaining_unidentified_too_high() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            Inventory inv = new Inventory();

            Guid gem_ent1 = inv.add(gem_stack1);

            inv.split(gem_ent1, 6, 0);
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
        public void test_count_too_small() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec itm = new ItemSpec("Gem", cat, 100, 0);
            ItemStack _ = new ItemStack(itm, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_unidentified_too_small() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec itm = new ItemSpec("Gem", cat, 100, 0);
            ItemStack _ = new ItemStack(itm, 4, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_unidentified_too_large() {
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
        public void test_copy() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec itm = new ItemSpec("Gem", cat, 100, 0);
            ItemStack foo = new ItemStack(itm, 3, 1), bar = foo.copy();

            Assert.IsFalse(ReferenceEquals(foo, bar));
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
            Assert.IsFalse(itm.unidentified);
            Assert.IsNull(itm.containers);
            Assert.AreEqual(itm.contents_weight, 0);
            Assert.AreEqual(itm.contents_value, 0);
            Assert.AreEqual(itm.name, "Wand of Kaplowie");
            Assert.AreEqual(itm.weight, 1);
            Assert.AreEqual(itm.value, 100);
        }

        [TestMethod]
        public void test_unidentified() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem itm = new SingleItem(wand, true);

            Assert.AreEqual(itm.item, wand);
            Assert.IsTrue(itm.unidentified);
            Assert.IsNull(itm.containers);
            Assert.AreEqual(itm.contents_weight, 0);
            Assert.AreEqual(itm.contents_value, 0);
            Assert.AreEqual(itm.name, "Wand of Kaplowie");
            Assert.AreEqual(itm.weight, 1);
            Assert.AreEqual(itm.value, 100);
        }

        [TestMethod]
        public void test_value_override() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem itm = new SingleItem(wand, false, 50);

            Assert.AreEqual(itm.item, wand);
            Assert.IsFalse(itm.unidentified);
            Assert.IsNull(itm.containers);
            Assert.AreEqual(itm.contents_weight, 0);
            Assert.AreEqual(itm.contents_value, 0);
            Assert.AreEqual(itm.name, "Wand of Kaplowie");
            Assert.AreEqual(itm.weight, 1);
            Assert.AreEqual(itm.value, 50);
        }

        [TestMethod]
        public void test_container_item() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem itm = new SingleItem(sack);

            Assert.AreEqual(itm.item, sack);
            Assert.IsNotNull(itm.containers);
            Assert.AreEqual(itm.containers.Length, 3);
            Assert.AreEqual(itm.containers[0].name, reducer.name);
            Assert.AreEqual(itm.containers[1].name, pouch.name);
            Assert.AreEqual(itm.containers[2].name, pouch.name);
            Assert.AreEqual(itm.contents_weight, 0);
            Assert.AreEqual(itm.contents_value, 0);
            Assert.AreEqual(itm.name, "Handy Haversack");
            Assert.AreEqual(itm.weight, 20);
            Assert.AreEqual(itm.value, 1800);
        }

        [TestMethod]
        public void test_container_contents() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .6m), c3 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
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
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), sword = new ItemSpec("Longsword", c2, 30, 3),
                sack = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack gems = new ItemStack(gem, 5), swords = new ItemStack(sword, 2);
            SingleItem foo = new SingleItem(sack, true), bar;

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
            Assert.AreEqual(foo.unidentified, bar.unidentified);
            Assert.AreEqual(foo.containers.Length, bar.containers.Length);
            for (int i = 0; i < foo.containers.Length; i++) {
                Assert.AreEqual(foo.containers[i].name, bar.containers[i].name);
            }
            Assert.AreEqual(foo.contents_weight, bar.contents_weight);
            Assert.AreEqual(foo.contents_value, bar.contents_value);
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.weight, bar.weight);
            Assert.AreEqual(foo.value, bar.value);
        }

        [TestMethod]
        public void test_copy() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .6m), c3 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), sword = new ItemSpec("Longsword", c2, 30, 3),
                sack = new ItemSpec("Handy Haversack", c3, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack gems = new ItemStack(gem, 5), swords = new ItemStack(sword, 2);
            SingleItem foo = new SingleItem(sack, true), bar;

            foo.containers[0].add(swords);
            foo.containers[1].add(gems);

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.AreEqual(foo.item, bar.item);
            Assert.AreEqual(foo.unidentified, bar.unidentified);
            Assert.IsFalse(ReferenceEquals(foo.containers, bar.containers));
            Assert.AreEqual(foo.containers.Length, bar.containers.Length);
            for (int i = 0; i < foo.containers.Length; i++) {
                Assert.IsFalse(ReferenceEquals(foo.containers[i], bar.containers[i]));
                Assert.AreEqual(foo.containers[i].name, bar.containers[i].name);
            }
            Assert.AreEqual(foo.contents_weight, bar.contents_weight);
            Assert.AreEqual(foo.contents_value, bar.contents_value);
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.weight, bar.weight);
            Assert.AreEqual(foo.value, bar.value);
        }

        [TestMethod]
        public void test_contains_inventory() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }),
                belt = new ItemSpec("Belt of Pockets", cat, 1000, 5, null, new ContainerSpec[] { pouch, pouch, pouch, pouch });
            SingleItem sack_itm = new SingleItem(sack), belt_itm = new SingleItem(belt);

            foreach (Inventory inv in sack_itm.containers) {
                Assert.IsTrue(sack_itm.contains_inventory(inv));
            }
            foreach (Inventory inv in belt_itm.containers) {
                Assert.IsFalse(sack_itm.contains_inventory(inv));
            }

            sack_itm.add(0, belt_itm);
            foreach (Inventory inv in sack_itm.containers) {
                Assert.IsTrue(sack_itm.contains_inventory(inv));
            }
            foreach (Inventory inv in belt_itm.containers) {
                Assert.IsTrue(sack_itm.contains_inventory(inv));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void test_add_no_containers() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec wand = new ItemSpec("Wand of Kaplowie", c1, 100, 1), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(wand, false, 50);

            itm.add(0, sword_stack);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_negative_container_index() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            itm.add(-1, sword_stack);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_container_index_too_large() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            itm.add(5, sword_stack);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test_add_null() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem itm = new SingleItem(sack);

            itm.add(0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_cycle() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }),
                belt = new ItemSpec("Belt of Pockets", cat, 1000, 5, null, new ContainerSpec[] { pouch, pouch, pouch, pouch });
            SingleItem sack_itm = new SingleItem(sack), belt_itm = new SingleItem(belt);

            sack_itm.add(0, belt_itm);
            belt_itm.add(0, sack_itm);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_too_much_weight() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 5), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            itm.add(1, sword_stack);
        }

        [TestMethod]
        public void test_add() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            itm.add(0, sword_stack);
            Assert.AreEqual(itm.containers[0].contents.Count, 1);
            Assert.AreEqual(itm.containers[0].weight, 6);
        }

        [TestMethod]
        public void test_add_nest() {
            ItemCategory cat = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", cat, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }),
                belt = new ItemSpec("Belt of Pockets", cat, 1000, 5, null, new ContainerSpec[] { pouch, pouch, pouch, pouch });
            SingleItem sack_itm = new SingleItem(sack), belt_itm = new SingleItem(belt);

            sack_itm.add(0, belt_itm);
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 1);
            Assert.AreEqual(sack_itm.containers[0].weight, 5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_negative_container_index() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            Guid guid = itm.add(0, sword_stack);
            itm.remove(-1, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_container_index_too_large() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            Guid guid = itm.add(0, sword_stack);
            itm.remove(5, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_no_such_guid() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            itm.add(0, sword_stack);
            itm.remove(0, Guid.NewGuid());
        }

        [TestMethod]
        public void test_remove() {
            ItemCategory c1 = new ItemCategory("Magic", 1), c2 = new ItemCategory("Weapons", .5m);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sack = new ItemSpec("Handy Haversack", c1, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch }), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem itm = new SingleItem(sack);

            Guid guid = itm.add(0, sword_stack);
            Assert.AreEqual(itm.containers[0].contents.Count, 1);
            Assert.AreEqual(itm.containers[0].weight, 6);
            itm.remove(0, guid);
            Assert.AreEqual(itm.containers[0].contents.Count, 0);
            Assert.AreEqual(itm.containers[0].weight, 0);
        }
    }


    [TestClass]
    public class TestInventoryDomain {
        [TestMethod]
        public void test_serialization() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);
            InventoryDomain foo = new InventoryDomain(), bar;

            Guid test_inv = foo.new_inventory("Test Inventory");
            Guid gem_ent = foo.add_entry(test_inv, gem_stack);
            foo.add_entry(test_inv, gp_stack);
            Guid sword_ent = foo.add_entry(test_inv, sword_stack);

            DataContractSerializer fmt = new DataContractSerializer(typeof(InventoryDomain));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (InventoryDomain)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.items.Count, bar.items.Count);
            foreach (ItemCategory cat in foo.items.Keys) {
                Assert.IsTrue(bar.items.ContainsKey(cat));
                Assert.AreEqual(foo.items[cat].Count, bar.items[cat].Count);
                for (int i = 0; i < foo.items[cat].Count; i++) {
                    Assert.AreEqual(foo.items[cat][i], bar.items[cat][i]);
                }
            }
            Assert.AreEqual(foo.entries.Count, bar.entries.Count);
            foreach (Guid ent in foo.entries.Keys) {
                Assert.IsTrue(bar.entries.ContainsKey(ent));
                Assert.AreEqual(foo.entries[ent].name, bar.entries[ent].name);
            }
            Assert.AreEqual(foo.active_entries.Count, bar.active_entries.Count);
            foreach (Guid ent in foo.active_entries) {
                Assert.IsTrue(bar.active_entries.Contains(ent));
            }
            Assert.AreEqual(foo.inventories.Count, bar.inventories.Count);
            foreach (Guid inv in foo.inventories.Keys) {
                Assert.IsTrue(bar.inventories.ContainsKey(inv));
                Assert.AreEqual(foo.inventories[inv].name, bar.inventories[inv].name);
                Assert.AreEqual(foo.inventories[inv].contents.Count, bar.inventories[inv].contents.Count);
            }
            // make sure the things which are supposed to be references are such after deserialization
            Assert.IsTrue(ReferenceEquals(bar.items[c2][0], bar.entries[sword_ent].item));
            Assert.IsTrue(ReferenceEquals(bar.entries[gem_ent], bar.inventories[test_inv].contents[gem_ent]));
        }

        [TestMethod]
        public void test_copy() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);
            InventoryDomain foo = new InventoryDomain(), bar;

            Guid test_inv = foo.new_inventory("Test Inventory");
            Guid gem_ent = foo.add_entry(test_inv, gem_stack);
            foo.add_entry(test_inv, gp_stack);
            Guid sword_ent = foo.add_entry(test_inv, sword_stack);

            bar = foo.copy();
            Assert.IsFalse(ReferenceEquals(foo, bar));
            Assert.IsFalse(ReferenceEquals(foo.items, bar.items));
            Assert.AreEqual(foo.items.Count, bar.items.Count);
            foreach (ItemCategory cat in foo.items.Keys) {
                Assert.IsTrue(bar.items.ContainsKey(cat));
                Assert.IsFalse(ReferenceEquals(foo.items[cat], bar.items[cat]));
                Assert.AreEqual(foo.items[cat].Count, bar.items[cat].Count);
                for (int i = 0; i < foo.items[cat].Count; i++) {
                    Assert.AreEqual(foo.items[cat][i], bar.items[cat][i]);
                }
            }
            Assert.IsFalse(ReferenceEquals(foo.entries, bar.entries));
            Assert.AreEqual(foo.entries.Count, bar.entries.Count);
            foreach (Guid ent in foo.entries.Keys) {
                Assert.IsTrue(bar.entries.ContainsKey(ent));
                Assert.IsFalse(ReferenceEquals(foo.entries[ent], bar.entries[ent]));
                Assert.AreEqual(foo.entries[ent].name, bar.entries[ent].name);
            }
            Assert.IsFalse(ReferenceEquals(foo.active_entries, bar.active_entries));
            Assert.AreEqual(foo.active_entries.Count, bar.active_entries.Count);
            foreach (Guid ent in foo.active_entries) {
                Assert.IsTrue(bar.active_entries.Contains(ent));
            }
            Assert.IsFalse(ReferenceEquals(foo.inventories, bar.inventories));
            Assert.AreEqual(foo.inventories.Count, bar.inventories.Count);
            foreach (Guid inv in foo.inventories.Keys) {
                Assert.IsTrue(bar.inventories.ContainsKey(inv));
                Assert.IsFalse(ReferenceEquals(foo.inventories[inv], bar.inventories[inv]));
                Assert.AreEqual(foo.inventories[inv].name, bar.inventories[inv].name);
                Assert.AreEqual(foo.inventories[inv].contents.Count, bar.inventories[inv].contents.Count);
            }
            // make sure the things which are supposed to be references are such after deserialization
            Assert.IsTrue(ReferenceEquals(bar.items[c2][0], bar.entries[sword_ent].item));
            Assert.IsTrue(ReferenceEquals(bar.entries[gem_ent], bar.inventories[test_inv].contents[gem_ent]));
        }

        [TestMethod]
        public void test_new_inventory() {
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Assert.AreEqual(domain.inventories.Count, 1);
            Assert.IsTrue(domain.inventories.ContainsKey(inv));
            Assert.AreEqual(domain.inventories[inv].name, "Test Inventory");
            Assert.AreEqual(domain.inventories[inv].contents.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_new_inventory_duplicate() {
            InventoryDomain domain = new InventoryDomain();

            Guid inv = domain.new_inventory("Test Inventory");
            domain.new_inventory("Another Inventory", inv);
        }

        [TestMethod]
        public void test_remove_inventory() {
            InventoryDomain domain = new InventoryDomain();

            Guid inv1 = domain.new_inventory("Test Inventory");
            Guid inv2 = domain.new_inventory("Another Inventory");
            Assert.AreEqual(domain.inventories.Count, 2);
            Assert.IsTrue(domain.inventories.ContainsKey(inv1));
            Assert.IsTrue(domain.inventories.ContainsKey(inv2));

            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);

            domain.add_entry(inv1, gem_stack);
            domain.add_entry(inv1, gp_stack);
            domain.add_entry(inv1, sword_stack);
            Assert.AreEqual(domain.entries.Count, 3);
            Assert.AreEqual(domain.active_entries.Count, 3);

            domain.remove_inventory(inv1);
            Assert.AreEqual(domain.inventories.Count, 1);
            Assert.IsFalse(domain.inventories.ContainsKey(inv1));
            Assert.IsTrue(domain.inventories.ContainsKey(inv2));
            Assert.AreEqual(domain.entries.Count, 3);
            Assert.AreEqual(domain.active_entries.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_inventory_no_such_guid() {
            InventoryDomain domain = new InventoryDomain();
            domain.remove_inventory(Guid.NewGuid());
        }

        [TestMethod]
        public void test_move_entry() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);
            InventoryDomain domain = new InventoryDomain();

            Guid inv1 = domain.new_inventory("Test Inventory"), inv2 = domain.new_inventory("Another Inventory");

            domain.add_entry(inv1, gem_stack);
            Guid gp_ent = domain.add_entry(inv1, gp_stack);
            domain.add_entry(inv1, sword_stack);
            Assert.AreEqual(domain.inventories[inv1].contents.Count, 3);
            Assert.AreEqual(domain.inventories[inv2].contents.Count, 0);

            domain.move_entry(gp_ent, inv1, inv2);
            Assert.AreEqual(domain.inventories[inv1].contents.Count, 2);
            Assert.AreEqual(domain.inventories[inv2].contents.Count, 1);
            Assert.IsTrue(domain.inventories[inv2].contents.ContainsKey(gp_ent));
        }

        [TestMethod]
        public void test_move_entry_containers() {
            ItemCategory c1 = new ItemCategory("Weapons", .5m), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sword = new ItemSpec("Longsword", c1, 30, 3), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem sack_itm = new SingleItem(sack);
            InventoryDomain domain = new InventoryDomain();

            Guid inv = domain.new_inventory("Test Inventory");

            Guid sack_ent = domain.add_entry(inv, sack_itm);
            Guid sword_ent = domain.add_entry(sack_ent, 0, sword_stack);
            Assert.AreEqual(domain.inventories[inv].contents.Count, 1);
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 1);
            Assert.AreEqual(sack_itm.containers[1].contents.Count, 0);
            Assert.AreEqual(sack_itm.containers[2].contents.Count, 0);
            Assert.AreEqual(domain.inventories[inv].weight, 23);

            domain.move_entry(sword_ent, sack_ent, 0, sack_ent, 1);
            Assert.AreEqual(domain.inventories[inv].contents.Count, 1);
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 0);
            Assert.AreEqual(sack_itm.containers[1].contents.Count, 1);
            Assert.AreEqual(sack_itm.containers[2].contents.Count, 0);
            Assert.AreEqual(domain.inventories[inv].weight, 20);

            domain.move_entry(sword_ent, sack_ent, 1, inv);
            Assert.AreEqual(domain.inventories[inv].contents.Count, 2);
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 0);
            Assert.AreEqual(sack_itm.containers[1].contents.Count, 0);
            Assert.AreEqual(sack_itm.containers[2].contents.Count, 0);
            Assert.AreEqual(domain.inventories[inv].weight, 26);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_move_entry_no_such_entry() {
            InventoryDomain domain = new InventoryDomain();
            Guid inv1 = domain.new_inventory("Test Inventory"), inv2 = domain.new_inventory("Another Inventory");

            domain.move_entry(Guid.NewGuid(), inv1, inv2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_move_entry_from_no_such_inventory() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            domain.move_entry(gem_ent, Guid.NewGuid(), inv);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_move_entry_to_no_such_inventory() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            domain.move_entry(gem_ent, inv, Guid.NewGuid());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_move_entry_from_no_such_container() {
            ItemCategory c1 = new ItemCategory("Weapons", .5m), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sword = new ItemSpec("Longsword", c1, 30, 3), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem sack_itm = new SingleItem(sack);
            InventoryDomain domain = new InventoryDomain();

            Guid inv = domain.new_inventory("Test Inventory");

            Guid sack_ent = domain.add_entry(inv, sack_itm);
            Guid sword_ent = domain.add_entry(sack_ent, 0, sword_stack);

            domain.move_entry(sword_ent, Guid.NewGuid(), 0, sack_ent, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_move_entry_to_no_such_container() {
            ItemCategory c1 = new ItemCategory("Weapons", .5m), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sword = new ItemSpec("Longsword", c1, 30, 3), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem sack_itm = new SingleItem(sack);
            InventoryDomain domain = new InventoryDomain();

            Guid inv = domain.new_inventory("Test Inventory");

            Guid sack_ent = domain.add_entry(inv, sack_itm);
            Guid sword_ent = domain.add_entry(sack_ent, 0, sword_stack);

            domain.move_entry(sword_ent, sack_ent, 0, Guid.NewGuid(), 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_move_entry_item_not_in_container() {
            ItemCategory c1 = new ItemCategory("Weapons", .5m), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sword = new ItemSpec("Longsword", c1, 30, 3), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem sack_itm = new SingleItem(sack);
            InventoryDomain domain = new InventoryDomain();

            Guid inv = domain.new_inventory("Test Inventory");

            Guid sack_ent = domain.add_entry(inv, sack_itm);
            Guid sword_ent = domain.add_entry(sack_ent, 0, sword_stack);

            domain.move_entry(sword_ent, sack_ent, 1, sack_ent, 2);
        }

        [TestMethod]
        public void test_restore_entry() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            domain.remove_entry(gem_ent, inv);

            domain.restore_entry(gem_ent, inv);
            Assert.IsTrue(domain.active_entries.Contains(gem_ent));
            Assert.IsTrue(domain.inventories[inv].contents.ContainsKey(gem_ent));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_entry_no_such_entry() {
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            domain.restore_entry(Guid.NewGuid(), inv);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_entry_active_entry() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            domain.restore_entry(gem_ent, inv);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_restore_entry_no_such_inventory() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            domain.remove_entry(gem_ent, inv);

            domain.restore_entry(gem_ent, Guid.NewGuid());
        }

        [TestMethod]
        public void test_add_entry() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Weapons", .5m);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), gp = new ItemSpec("GP", c1, 1, 0), sword = new ItemSpec("Longsword", c2, 30, 3);
            ItemStack gem_stack = new ItemStack(gem, 3), gp_stack = new ItemStack(gp, 150), sword_stack = new ItemStack(sword, 2);
            InventoryDomain domain = new InventoryDomain();
            Guid inv1 = domain.new_inventory("Test Inventory"), inv2 = domain.new_inventory("Another Inventory");

            Guid gem_ent = domain.add_entry(inv1, gem_stack);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(c1));
            Assert.AreEqual(domain.items[c1].Count, 1);
            Assert.AreEqual(domain.entries.Count, 1);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent));
            Assert.AreEqual(domain.entries[gem_ent], gem_stack);
            Assert.IsTrue(domain.active_entries.Contains(gem_ent));
            Assert.AreEqual(domain.inventories[inv1].contents.Count, 1);
            Assert.AreEqual(domain.inventories[inv2].contents.Count, 0);
            Assert.IsTrue(domain.inventories[inv1].contents.ContainsKey(gem_ent));

            Guid gp_ent = domain.add_entry(inv1, gp_stack);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(c1));
            Assert.AreEqual(domain.items[c1].Count, 2);
            Assert.AreEqual(domain.entries.Count, 2);
            Assert.IsTrue(domain.entries.ContainsKey(gp_ent));
            Assert.AreEqual(domain.entries[gp_ent], gp_stack);
            Assert.IsTrue(domain.active_entries.Contains(gem_ent));
            Assert.IsTrue(domain.active_entries.Contains(gp_ent));
            Assert.AreEqual(domain.inventories[inv1].contents.Count, 2);
            Assert.AreEqual(domain.inventories[inv2].contents.Count, 0);
            Assert.IsTrue(domain.inventories[inv1].contents.ContainsKey(gem_ent));
            Assert.IsTrue(domain.inventories[inv1].contents.ContainsKey(gp_ent));

            Guid sword_ent = domain.add_entry(inv2, sword_stack);
            Assert.AreEqual(domain.items.Count, 2);
            Assert.IsTrue(domain.items.ContainsKey(c1));
            Assert.IsTrue(domain.items.ContainsKey(c2));
            Assert.AreEqual(domain.items[c1].Count, 2);
            Assert.AreEqual(domain.items[c2].Count, 1);
            Assert.AreEqual(domain.entries.Count, 3);
            Assert.IsTrue(domain.entries.ContainsKey(sword_ent));
            Assert.AreEqual(domain.entries[sword_ent], sword_stack);
            Assert.IsTrue(domain.active_entries.Contains(gem_ent));
            Assert.IsTrue(domain.active_entries.Contains(gp_ent));
            Assert.IsTrue(domain.active_entries.Contains(sword_ent));
            Assert.AreEqual(domain.inventories[inv1].contents.Count, 2);
            Assert.AreEqual(domain.inventories[inv2].contents.Count, 1);
            Assert.IsTrue(domain.inventories[inv1].contents.ContainsKey(gem_ent));
            Assert.IsTrue(domain.inventories[inv1].contents.ContainsKey(gp_ent));
            Assert.IsTrue(domain.inventories[inv2].contents.ContainsKey(sword_ent));
        }

        [TestMethod]
        public void test_add_entry_containers() {
            ItemCategory c1 = new ItemCategory("Weapons", .5m), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sword = new ItemSpec("Longsword", c1, 30, 3), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem sack_itm = new SingleItem(sack);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid sack_ent = domain.add_entry(inv, sack_itm);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(c2));
            Assert.AreEqual(domain.items[c2].Count, 1);
            Assert.AreEqual(domain.entries.Count, 1);
            Assert.IsTrue(domain.entries.ContainsKey(sack_ent));
            Assert.AreEqual(domain.entries[sack_ent], sack_itm);
            Assert.AreEqual(domain.inventories[inv].contents.Count, 1);
            Assert.IsTrue(domain.inventories[inv].contents.ContainsKey(sack_ent));

            Guid sword_ent = domain.add_entry(sack_ent, 0, sword_stack);
            Assert.AreEqual(domain.items.Count, 2);
            Assert.IsTrue(domain.items.ContainsKey(c1));
            Assert.IsTrue(domain.items.ContainsKey(c2));
            Assert.AreEqual(domain.items[c1].Count, 1);
            Assert.AreEqual(domain.items[c2].Count, 1);
            Assert.AreEqual(domain.entries.Count, 2);
            Assert.IsTrue(domain.entries.ContainsKey(sack_ent));
            Assert.IsTrue(domain.entries.ContainsKey(sword_ent));
            Assert.AreEqual(domain.entries[sword_ent], sword_stack);
            Assert.AreEqual(domain.inventories[inv].contents.Count, 1);
            Assert.IsTrue(domain.inventories[inv].contents.ContainsKey(sack_ent));
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 1);
            Assert.AreEqual(sack_itm.containers[1].contents.Count, 0);
            Assert.AreEqual(sack_itm.containers[2].contents.Count, 0);
            Assert.AreEqual(domain.inventories[inv].weight, 23);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_entry_active() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            domain.add_entry(inv, gem_stack);
            domain.add_entry(inv, gem_stack);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_add_entry_removed() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            domain.remove_entry(inv, gem_ent);
            domain.add_entry(inv, gem_stack, gem_ent);
        }

        [TestMethod]
        public void test_remove_entry() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(cat));
            Assert.AreEqual(domain.items[cat].Count, 1);
            Assert.AreEqual(domain.entries.Count, 1);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent));
            Assert.AreEqual(domain.entries[gem_ent], gem_stack);
            Assert.IsTrue(domain.active_entries.Contains(gem_ent));
            Assert.AreEqual(domain.inventories[inv].contents.Count, 1);
            Assert.IsTrue(domain.inventories[inv].contents.ContainsKey(gem_ent));

            domain.remove_entry(gem_ent, inv);
            Assert.AreEqual(domain.items.Count, 1);
            Assert.IsTrue(domain.items.ContainsKey(cat));
            Assert.AreEqual(domain.items[cat].Count, 1);
            Assert.AreEqual(domain.entries.Count, 1);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent));
            Assert.AreEqual(domain.entries[gem_ent], gem_stack);
            Assert.IsFalse(domain.active_entries.Contains(gem_ent));
            Assert.AreEqual(domain.inventories[inv].contents.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_entry_no_such_item() {
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            domain.remove_entry(inv, Guid.NewGuid());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_remove_entry_removed() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent = domain.add_entry(inv, gem_stack);
            domain.remove_entry(inv, gem_ent);

            domain.remove_entry(inv, gem_ent);
        }

        [TestMethod]
        public void test_merge_entries() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem_item1 = new SingleItem(gem, true), gem_item2 = new SingleItem(gem, false);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent1 = domain.add_entry(inv, gem_item1);
            Guid gem_ent2 = domain.add_entry(inv, gem_item2);

            Guid new_ent = domain.merge_entries(inv, gem_ent1, gem_ent2);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent1));
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent2));
            Assert.IsTrue(domain.entries.ContainsKey(new_ent));
            Assert.IsFalse(domain.active_entries.Contains(gem_ent1));
            Assert.IsFalse(domain.active_entries.Contains(gem_ent2));
            Assert.IsTrue(domain.active_entries.Contains(new_ent));
        }

        [TestMethod]
        public void test_merge_entries_container() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem sack_itm = new SingleItem(sack), gem_item1 = new SingleItem(gem, true), gem_item2 = new SingleItem(gem, false);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");
            Guid sack_ent = domain.add_entry(inv, sack_itm);

            Guid gem_ent1 = domain.add_entry(sack_ent, 0, gem_item1);
            Guid gem_ent2 = domain.add_entry(sack_ent, 0, gem_item2);

            Guid new_ent = domain.merge_entries(sack_ent, 0, gem_ent1, gem_ent2);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent1));
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent2));
            Assert.IsTrue(domain.entries.ContainsKey(new_ent));
            Assert.IsFalse(domain.active_entries.Contains(gem_ent1));
            Assert.IsFalse(domain.active_entries.Contains(gem_ent2));
            Assert.IsTrue(domain.active_entries.Contains(new_ent));
            Assert.IsFalse(sack_itm.containers[0].contents.ContainsKey(gem_ent1));
            Assert.IsFalse(sack_itm.containers[0].contents.ContainsKey(gem_ent2));
            Assert.IsTrue(sack_itm.containers[0].contents.ContainsKey(new_ent));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_merge_entries_removed() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem_item1 = new SingleItem(gem, true), gem_item2 = new SingleItem(gem, false);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent1 = domain.add_entry(inv, gem_item1);
            Guid gem_ent2 = domain.add_entry(inv, gem_item2);
            domain.remove_entry(inv, gem_ent1);

            domain.merge_entries(inv, gem_ent1, gem_ent2);
        }

        [TestMethod]
        public void test_unstack_entry() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent1 = domain.add_entry(inv, gem_stack);

            Guid gem_ent2 = domain.unstack_entry(inv, gem_ent1);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent1));
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent2));
            Assert.IsFalse(domain.active_entries.Contains(gem_ent1));
            Assert.IsTrue(domain.active_entries.Contains(gem_ent2));
        }

        [TestMethod]
        public void test_unstack_entry_container() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem sack_itm = new SingleItem(sack);
            ItemStack gem_stack = new ItemStack(gem, 1);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");
            Guid sack_ent = domain.add_entry(inv, sack_itm);

            Guid gem_ent1 = domain.add_entry(sack_ent, 0, gem_stack);

            Guid gem_ent2 = domain.unstack_entry(sack_ent, 0, gem_ent1);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent1));
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent2));
            Assert.IsFalse(domain.active_entries.Contains(gem_ent1));
            Assert.IsTrue(domain.active_entries.Contains(gem_ent2));
            Assert.IsFalse(sack_itm.containers[0].contents.ContainsKey(gem_ent1));
            Assert.IsTrue(sack_itm.containers[0].contents.ContainsKey(gem_ent2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_unstack_entry_removed() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent1 = domain.add_entry(inv, gem_stack);
            domain.remove_entry(inv, gem_ent1);

            domain.unstack_entry(inv, gem_ent1);
        }

        [TestMethod]
        public void test_split_entry() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent1 = domain.add_entry(inv, gem_stack1);

            Guid gem_ent2 = domain.split_entry(inv, gem_ent1, 2, 1);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent1));
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent2));
            Assert.IsTrue(domain.active_entries.Contains(gem_ent1));
            Assert.IsTrue(domain.active_entries.Contains(gem_ent2));

            ItemStack gem_stack2 = domain.entries[gem_ent2] as ItemStack;
            Assert.IsFalse(gem_stack2 is null);
            Assert.AreEqual(gem_stack1.count, 6);
            Assert.AreEqual(gem_stack1.unidentified, 2);
            Assert.AreEqual(gem_stack2.count, 2);
            Assert.AreEqual(gem_stack2.unidentified, 1);
        }

        [TestMethod]
        public void test_split_entry_container() {
            ItemCategory c1 = new ItemCategory("Wealth", 1), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec gem = new ItemSpec("Gem", c1, 100, 1), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            SingleItem sack_itm = new SingleItem(sack);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");
            Guid sack_ent = domain.add_entry(inv, sack_itm);

            Guid gem_ent1 = domain.add_entry(sack_ent, 0, gem_stack1);

            Guid gem_ent2 = domain.split_entry(sack_ent, 0, gem_ent1, 2, 1);
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent1));
            Assert.IsTrue(domain.entries.ContainsKey(gem_ent2));
            Assert.IsTrue(domain.active_entries.Contains(gem_ent1));
            Assert.IsTrue(domain.active_entries.Contains(gem_ent2));
            Assert.IsTrue(sack_itm.containers[0].contents.ContainsKey(gem_ent1));
            Assert.IsTrue(sack_itm.containers[0].contents.ContainsKey(gem_ent2));

            ItemStack gem_stack2 = domain.entries[gem_ent2] as ItemStack;
            Assert.IsFalse(gem_stack2 is null);
            Assert.AreEqual(gem_stack1.count, 6);
            Assert.AreEqual(gem_stack1.unidentified, 2);
            Assert.AreEqual(gem_stack2.count, 2);
            Assert.AreEqual(gem_stack2.unidentified, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_split_entry_removed() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 8, 3);
            InventoryDomain domain = new InventoryDomain();
            Guid inv = domain.new_inventory("Test Inventory");

            Guid gem_ent1 = domain.add_entry(inv, gem_stack1);
            domain.remove_entry(inv, gem_ent1);

            domain.split_entry(inv, gem_ent1, 2, 1);
        }
    }
}
