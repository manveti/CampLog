﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestActionInventoryCreate {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryCreate foo = new ActionInventoryCreate(Guid.NewGuid(), "Some Inventory"), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryCreate));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryCreate)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.name, bar.name);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_already_exists() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Pre-existing Inventory", inv_guid);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            state.inventories.remove_inventory(inv_guid);
            action.revert(state, evt);
        }
    }


    [TestClass]
    public class TestActionInventoryRemove {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryRemove foo = new ActionInventoryRemove(Guid.NewGuid(), "Some Inventory"), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryRemove));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryRemove)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.name, bar.name);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_restored() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            state.inventories.new_inventory("Some Inventory", inv_guid);
            action.revert(state, evt);
        }
    }


    [TestClass]
    public class TestActionInventoryRename {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryRename foo = new ActionInventoryRename(Guid.NewGuid(), "Some Inventory", "The Inventory's New Groove"), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryRename));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryRename)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.from, bar.from);
            Assert.AreEqual(foo.to, bar.to);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "The Inventory's New Groove");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();

            action.apply(state, evt);
            state.inventories.remove_inventory(inv_guid);
            action.revert(state, evt);
        }
    }


    [TestClass]
    public class TestActionInventoryEntryAdd {
        [TestMethod]
        public void test_serialization() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            ActionInventoryEntryAdd foo = new ActionInventoryEntryAdd(Guid.NewGuid(), 1, Guid.NewGuid(), gem_stack), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryEntryAdd));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryEntryAdd)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.inv_guid, bar.inv_guid);
            Assert.AreEqual(foo.inv_idx, bar.inv_idx);
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.entry.name, bar.entry.name);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents[guid].item, gem);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid = Guid.NewGuid(), guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, evt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_already_exists() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            state.inventories.add_entry(inv_guid, gem_stack, guid);
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 0);
            Assert.AreEqual(state.inventories.active_entries.Count, 0);
            Assert.AreEqual(state.inventories.entries.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, evt);
            state.inventories.remove_entry(guid, inv_guid);
            action.revert(state, evt);
        }
    }


    [TestClass]
    public class TestActionInventoryEntryRemove {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryEntryRemove foo = new ActionInventoryEntryRemove(Guid.NewGuid(), 1, Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryEntryRemove));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryEntryRemove)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.inv_guid, bar.inv_guid);
            Assert.AreEqual(foo.inv_idx, bar.inv_idx);
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryRemove action = new ActionInventoryEntryRemove(inv_guid, null, guid);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 0);
            Assert.AreEqual(state.inventories.active_entries.Count, 0);
            Assert.AreEqual(state.inventories.entries.Count, 1);
            Assert.IsTrue(state.inventories.entries.ContainsKey(guid));
            Assert.AreEqual(state.inventories.entries[guid].item, gem);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_removed() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryRemove action = new ActionInventoryEntryRemove(inv_guid, null, guid);
            state.inventories.remove_entry(guid, inv_guid);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryRemove action = new ActionInventoryEntryRemove(inv_guid, null, guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents[guid].item, gem);
            Assert.AreEqual(state.inventories.active_entries.Count, 1);
            Assert.IsTrue(state.inventories.active_entries.Contains(guid));
            Assert.AreEqual(state.inventories.entries.Count, 1);
            Assert.IsTrue(state.inventories.entries.ContainsKey(guid));
            Assert.AreEqual(state.inventories.entries[guid].item, gem);
        }
    }


    [TestClass]
    public class TestActionItemStackSet {
        [TestMethod]
        public void test_serialization() {
            ActionItemStackSet foo = new ActionItemStackSet(Guid.NewGuid(), 1, 0, 2, 1), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionItemStackSet));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionItemStackSet)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.count_from, bar.count_from);
            Assert.AreEqual(foo.unidentified_from, bar.unidentified_from);
            Assert.AreEqual(foo.count_to, bar.count_to);
            Assert.AreEqual(foo.unidentified_to, bar.unidentified_to);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackSet action = new ActionItemStackSet(guid, 3, 0, 5, 1);

            action.apply(state, evt);
            Assert.AreEqual(gem_stack.count, 5);
            Assert.AreEqual(gem_stack.unidentified, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackSet action = new ActionItemStackSet(guid, 3, 0, 2, 3);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackSet action = new ActionItemStackSet(guid, 3, 0, 5, 1);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(gem_stack.count, 3);
            Assert.AreEqual(gem_stack.unidentified, 0);
        }
    }


    [TestClass]
    public class TestActionItemStackAdjust {
        [TestMethod]
        public void test_serialization() {
            ActionItemStackAdjust foo = new ActionItemStackAdjust(Guid.NewGuid(), 2, 1), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionItemStackAdjust));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionItemStackAdjust)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.count, bar.count);
            Assert.AreEqual(foo.unidentified, bar.unidentified);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackAdjust action = new ActionItemStackAdjust(guid, 2, 1);

            action.apply(state, evt);
            Assert.AreEqual(gem_stack.count, 5);
            Assert.AreEqual(gem_stack.unidentified, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackAdjust action = new ActionItemStackAdjust(guid, -1, 3);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackAdjust action = new ActionItemStackAdjust(guid, 2, 1);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(gem_stack.count, 3);
            Assert.AreEqual(gem_stack.unidentified, 0);
        }
    }


    [TestClass]
    public class TestActionSingleItemSet {
        [TestMethod]
        public void test_serialization() {
            Dictionary<string, string> from_props = new Dictionary<string, string>(), to_props = new Dictionary<string, string>();
            from_props["Charges"] = "50";
            to_props["Charges"] = "49";
            ActionSingleItemSet foo = new ActionSingleItemSet(Guid.NewGuid(), true, null, from_props, false, 490, to_props, true), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionSingleItemSet));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionSingleItemSet)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.unidentified_from, bar.unidentified_from);
            Assert.AreEqual(foo.value_override_from, bar.value_override_from);
            Assert.AreEqual(foo.properties_from.Count, bar.properties_from.Count);
            foreach (string key in foo.properties_from.Keys) {
                Assert.IsTrue(bar.properties_from.ContainsKey(key));
                Assert.AreEqual(foo.properties_from[key], bar.properties_from[key]);
            }
            Assert.AreEqual(foo.unidentified_to, bar.unidentified_to);
            Assert.AreEqual(foo.value_override_to, bar.value_override_to);
            Assert.AreEqual(foo.properties_to.Count, bar.properties_to.Count);
            foreach (string key in foo.properties_to.Keys) {
                Assert.IsTrue(bar.properties_to.ContainsKey(key));
                Assert.AreEqual(foo.properties_to[key], bar.properties_to[key]);
            }
            Assert.AreEqual(foo.set_value_override, bar.set_value_override);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, true);
            Dictionary<string, string> new_props = new Dictionary<string, string>();
            wand.properties["Charges"] = "50";
            new_props["Charges"] = "49";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, wand.properties, false, 490, new_props, true);

            action.apply(state, evt);
            Assert.IsFalse(wand.unidentified);
            Assert.AreEqual(wand.value_override, 490);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "49");
        }

        [TestMethod]
        public void test_apply_sparse() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, true, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, null, false, null, null, false);

            action.apply(state, evt);
            Assert.IsFalse(wand.unidentified);
            Assert.AreEqual(wand.value_override, 500);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid_entry_type() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, null, false, null, null, false);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, true);
            Dictionary<string, string> new_props = new Dictionary<string, string>();
            wand.properties["Charges"] = "50";
            new_props["Charges"] = "49";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, wand.properties, false, 490, new_props, true);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.IsTrue(wand.unidentified);
            Assert.IsNull(wand.value_override);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        public void test_revert_sparse() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, true, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, null, false, null, null, false);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.IsTrue(wand.unidentified);
            Assert.AreEqual(wand.value_override, 500);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }
    }


    [TestClass]
    public class TestActionSingleItemAdjust {
        [TestMethod]
        public void test_serialization() {
            Dictionary<string, string> sub_props = new Dictionary<string, string>(), add_props = new Dictionary<string, string>();
            sub_props["Charges"] = "50";
            add_props["Charges"] = "49";
            ActionSingleItemAdjust foo = new ActionSingleItemAdjust(Guid.NewGuid(), -10, sub_props, add_props), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionSingleItemAdjust));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionSingleItemAdjust)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.value_override, bar.value_override);
            Assert.AreEqual(foo.properties_subtract.Count, bar.properties_subtract.Count);
            foreach (string key in foo.properties_subtract.Keys) {
                Assert.IsTrue(bar.properties_subtract.ContainsKey(key));
                Assert.AreEqual(foo.properties_subtract[key], bar.properties_subtract[key]);
            }
            Assert.AreEqual(foo.properties_add.Count, bar.properties_add.Count);
            foreach (string key in foo.properties_add.Keys) {
                Assert.IsTrue(bar.properties_add.ContainsKey(key));
                Assert.AreEqual(foo.properties_add[key], bar.properties_add[key]);
            }
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false, 500);
            wand.properties["Charges"] = "50";
            Dictionary<string, string> sub_props = new Dictionary<string, string>(), add_props = new Dictionary<string, string>();
            sub_props["Charges"] = "50";
            add_props["Charges"] = "49";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, sub_props, add_props);

            action.apply(state, evt);
            Assert.AreEqual(wand.value_override, 490);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "49");
        }

        [TestMethod]
        public void test_apply_sparse() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, evt);
            Assert.AreEqual(wand.value_override, 490);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_value_override() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, evt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid_entry_type() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, evt);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false, 500);
            wand.properties["Charges"] = "50";
            Dictionary<string, string> sub_props = new Dictionary<string, string>(), add_props = new Dictionary<string, string>();
            sub_props["Charges"] = "50";
            add_props["Charges"] = "49";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, sub_props, add_props);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(wand.value_override, 500);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        public void test_revert_sparse() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(wand.value_override, 500);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }
    }


    [TestClass]
    public class TestActionInventoryEntryMove {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryEntryMove foo = new ActionInventoryEntryMove(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), 0), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryEntryMove));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryEntryMove)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.guid, bar.guid);
            Assert.AreEqual(foo.from_guid, bar.from_guid);
            Assert.AreEqual(foo.from_idx, bar.from_idx);
            Assert.AreEqual(foo.to_guid, bar.to_guid);
            Assert.AreEqual(foo.to_idx, bar.to_idx);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory c1 = new ItemCategory("Weapons", .5m), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sword = new ItemSpec("Longsword", c1, 30, 3), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem sack_itm = new SingleItem(sack);
            Guid inv_guid, sack_guid, sword_guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            sack_guid = state.inventories.add_entry(inv_guid, sack_itm);
            sword_guid = state.inventories.add_entry(inv_guid, sword_stack);
            ActionInventoryEntryMove action = new ActionInventoryEntryMove(sword_guid, inv_guid, null, sack_guid, 0);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(sack_guid));
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 1);
            Assert.IsTrue(sack_itm.containers[0].contents.ContainsKey(sword_guid));
            Assert.AreEqual(sack_itm.containers[0].contents[sword_guid].item, sword);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory c1 = new ItemCategory("Weapons", .5m), c2 = new ItemCategory("Magic", 1);
            ContainerSpec pouch = new ContainerSpec("Magic Pouch", 0, 30), reducer = new ContainerSpec("Weight Reducer", .5m, 100);
            ItemSpec sword = new ItemSpec("Longsword", c1, 30, 3), sack = new ItemSpec("Handy Haversack", c2, 2000, 20, 1800, new ContainerSpec[] { reducer, pouch, pouch });
            ItemStack sword_stack = new ItemStack(sword, 2);
            SingleItem sack_itm = new SingleItem(sack);
            Guid inv_guid, sack_guid, sword_guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            sack_guid = state.inventories.add_entry(inv_guid, sack_itm);
            sword_guid = state.inventories.add_entry(inv_guid, sword_stack);
            ActionInventoryEntryMove action = new ActionInventoryEntryMove(sword_guid, inv_guid, null, sack_guid, 0);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 2);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(sack_guid));
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(sword_guid));
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 0);
        }
    }


    [TestClass]
    public class TestActionInventoryEntryMerge {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryEntryMerge foo = new ActionInventoryEntryMerge(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryEntryMerge));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryEntryMerge)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.inv_guid, bar.inv_guid);
            Assert.AreEqual(foo.inv_idx, bar.inv_idx);
            Assert.AreEqual(foo.ent1, bar.ent1);
            Assert.AreEqual(foo.ent2, bar.ent2);
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            SingleItem gem_item = new SingleItem(gem, true);
            Guid inv_guid, stack_guid, item_guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack_guid = state.inventories.add_entry(inv_guid, gem_stack);
            item_guid = state.inventories.add_entry(inv_guid, gem_item);
            ActionInventoryEntryMerge action = new ActionInventoryEntryMerge(inv_guid, null, stack_guid, item_guid, stack_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(stack_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 1);
            Assert.IsTrue(state.inventories.active_entries.Contains(stack_guid));
            Assert.AreEqual(state.inventories.entries.Count, 2);
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack_guid));
            Assert.IsTrue(state.inventories.entries.ContainsKey(item_guid));
            Assert.AreEqual(gem_stack.count, 4);
            Assert.AreEqual(gem_stack.unidentified, 1);
        }

        [TestMethod]
        public void test_apply_single_items() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem_spec = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem1 = new SingleItem(gem_spec, true), gem2 = new SingleItem(gem_spec, false);
            Guid inv_guid, gem1_guid, gem2_guid, stack_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            gem1_guid = state.inventories.add_entry(inv_guid, gem1);
            gem2_guid = state.inventories.add_entry(inv_guid, gem2);
            ActionInventoryEntryMerge action = new ActionInventoryEntryMerge(inv_guid, null, gem1_guid, gem2_guid, stack_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(stack_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 1);
            Assert.IsTrue(state.inventories.active_entries.Contains(stack_guid));
            Assert.AreEqual(state.inventories.entries.Count, 3);
            Assert.IsTrue(state.inventories.entries.ContainsKey(gem1_guid));
            Assert.IsTrue(state.inventories.entries.ContainsKey(gem2_guid));
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack_guid));
            ItemStack stack = state.inventories.entries[stack_guid] as ItemStack;
            Assert.IsNotNull(stack);
            Assert.AreEqual(stack.count, 2);
            Assert.AreEqual(stack.unidentified, 1);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            SingleItem gem_item = new SingleItem(gem, true);
            Guid inv_guid, stack_guid, item_guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack_guid = state.inventories.add_entry(inv_guid, gem_stack);
            item_guid = state.inventories.add_entry(inv_guid, gem_item);
            ActionInventoryEntryMerge action = new ActionInventoryEntryMerge(inv_guid, null, stack_guid, item_guid, stack_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 2);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(stack_guid));
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(item_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 2);
            Assert.IsTrue(state.inventories.active_entries.Contains(stack_guid));
            Assert.IsTrue(state.inventories.active_entries.Contains(item_guid));
            Assert.AreEqual(state.inventories.entries.Count, 2);
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack_guid));
            Assert.IsTrue(state.inventories.entries.ContainsKey(item_guid));
            Assert.AreEqual(gem_stack.count, 3);
            Assert.AreEqual(gem_stack.unidentified, 0);
        }

        [TestMethod]
        public void test_revert_single_items() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem_spec = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem1 = new SingleItem(gem_spec, true), gem2 = new SingleItem(gem_spec, false);
            Guid inv_guid, gem1_guid, gem2_guid, stack_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            gem1_guid = state.inventories.add_entry(inv_guid, gem1);
            gem2_guid = state.inventories.add_entry(inv_guid, gem2);
            ActionInventoryEntryMerge action = new ActionInventoryEntryMerge(inv_guid, null, gem1_guid, gem2_guid, stack_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 2);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(gem1_guid));
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(gem2_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 2);
            Assert.IsTrue(state.inventories.active_entries.Contains(gem1_guid));
            Assert.IsTrue(state.inventories.active_entries.Contains(gem2_guid));
            Assert.AreEqual(state.inventories.entries.Count, 2);
            Assert.IsTrue(state.inventories.entries.ContainsKey(gem1_guid));
            Assert.IsTrue(state.inventories.entries.ContainsKey(gem2_guid));
        }
    }


    [TestClass]
    public class TestActionInventoryEntryUnstack {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryEntryUnstack foo = new ActionInventoryEntryUnstack(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryEntryUnstack));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryEntryUnstack)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.inv_guid, bar.inv_guid);
            Assert.AreEqual(foo.inv_idx, bar.inv_idx);
            Assert.AreEqual(foo.ent, bar.ent);
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1, 1);
            Guid inv_guid, stack_guid, item_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack_guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryUnstack action = new ActionInventoryEntryUnstack(inv_guid, null, stack_guid, item_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(item_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 1);
            Assert.IsTrue(state.inventories.active_entries.Contains(item_guid));
            Assert.AreEqual(state.inventories.entries.Count, 2);
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack_guid));
            Assert.IsTrue(state.inventories.entries.ContainsKey(item_guid));
            SingleItem gem_item = state.inventories.entries[item_guid] as SingleItem;
            Assert.IsNotNull(gem_item);
            Assert.AreEqual(gem_item.item, gem);
            Assert.IsTrue(gem_item.unidentified);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1, 1);
            Guid inv_guid, stack_guid, item_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack_guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryUnstack action = new ActionInventoryEntryUnstack(inv_guid, null, stack_guid, item_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(stack_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 1);
            Assert.IsTrue(state.inventories.active_entries.Contains(stack_guid));
            Assert.AreEqual(state.inventories.entries.Count, 1);
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack_guid));
        }
    }


    [TestClass]
    public class TestActionInventoryEntrySplit {
        [TestMethod]
        public void test_serialization() {
            ActionInventoryEntrySplit foo = new ActionInventoryEntrySplit(Guid.NewGuid(), 1, Guid.NewGuid(), 3, 1, Guid.NewGuid()), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(ActionInventoryEntrySplit));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (ActionInventoryEntrySplit)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.inv_guid, bar.inv_guid);
            Assert.AreEqual(foo.inv_idx, bar.inv_idx);
            Assert.AreEqual(foo.ent, bar.ent);
            Assert.AreEqual(foo.count, bar.count);
            Assert.AreEqual(foo.unidentified, bar.unidentified);
            Assert.AreEqual(foo.guid, bar.guid);
        }

        [TestMethod]
        public void test_apply() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 5, 3), gem_stack2;
            Guid inv_guid, stack1_guid, stack2_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack1_guid = state.inventories.add_entry(inv_guid, gem_stack1);
            ActionInventoryEntrySplit action = new ActionInventoryEntrySplit(inv_guid, null, stack1_guid, 2, 1, stack2_guid);

            action.apply(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 2);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(stack1_guid));
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(stack2_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 2);
            Assert.IsTrue(state.inventories.active_entries.Contains(stack1_guid));
            Assert.IsTrue(state.inventories.active_entries.Contains(stack2_guid));
            Assert.AreEqual(state.inventories.entries.Count, 2);
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack1_guid));
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack2_guid));
            Assert.AreEqual(gem_stack1.count, 3);
            Assert.AreEqual(gem_stack1.unidentified, 2);
            gem_stack2 = state.inventories.entries[stack2_guid] as ItemStack;
            Assert.IsNotNull(gem_stack2);
            Assert.AreEqual(gem_stack2.count, 2);
            Assert.AreEqual(gem_stack2.unidentified, 1);
        }

        [TestMethod]
        public void test_revert() {
            Event evt = new Event(42, DateTime.Now, "Some Event");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 5, 3);
            Guid inv_guid, stack1_guid, stack2_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack1_guid = state.inventories.add_entry(inv_guid, gem_stack1);
            ActionInventoryEntrySplit action = new ActionInventoryEntrySplit(inv_guid, null, stack1_guid, 2, 1, stack2_guid);

            action.apply(state, evt);
            action.revert(state, evt);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(stack1_guid));
            Assert.AreEqual(state.inventories.active_entries.Count, 1);
            Assert.IsTrue(state.inventories.active_entries.Contains(stack1_guid));
            Assert.AreEqual(state.inventories.entries.Count, 1);
            Assert.IsTrue(state.inventories.entries.ContainsKey(stack1_guid));
            Assert.AreEqual(gem_stack1.count, 5);
            Assert.AreEqual(gem_stack1.unidentified, 3);
        }
    }
}