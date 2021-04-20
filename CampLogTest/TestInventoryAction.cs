using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_already_exists() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Pre-existing Inventory", inv_guid);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.inventories.inventories.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            state.inventories.remove_inventory(inv_guid);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_merge_to() {
            Guid inv_guid = Guid.NewGuid();
            List<EntryAction> actions = new List<EntryAction>();

            ActionInventoryCreate create_action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            create_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryCreate merged_action = actions[0] as ActionInventoryCreate;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, inv_guid);
            Assert.AreEqual(merged_action.name, "Some Inventory");
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.inventories.inventories.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_restored() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            state.inventories.new_inventory("Some Inventory", inv_guid);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_merge_to_invset_remove() {
            Guid chr_guid = Guid.NewGuid(), inv_guid = Guid.NewGuid();
            ActionCharacterSetInventory invset_action = new ActionCharacterSetInventory(chr_guid, null, inv_guid);
            List<EntryAction> actions = new List<EntryAction>() { invset_action };

            ActionInventoryRemove remove_action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryRemove merged_action = actions[0] as ActionInventoryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, inv_guid);
            Assert.AreEqual(merged_action.name, "Some Inventory");
        }

        [TestMethod]
        public void test_merge_to_create_remove() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate create_action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            List<EntryAction> actions = new List<EntryAction>() { create_action };

            ActionInventoryRemove remove_action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_rename_remove() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename rename_action = new ActionInventoryRename(inv_guid, "Some Inventory", "Some Renamed Inventory");
            List<EntryAction> actions = new List<EntryAction>() { rename_action };

            ActionInventoryRemove remove_action = new ActionInventoryRemove(inv_guid, "Some Renamed Inventory");
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryRemove merged_action = actions[0] as ActionInventoryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, inv_guid);
            Assert.AreEqual(merged_action.name, "Some Inventory");
        }

        [TestMethod]
        public void test_merge_to_modentries_remove() {
            Guid inv_guid = Guid.NewGuid();
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            ActionInventoryEntryAdd entry_add_action = new ActionInventoryEntryAdd(inv_guid, null, Guid.NewGuid(), gem_stack);
            ActionInventoryEntryRemove entry_rem_action = new ActionInventoryEntryRemove(inv_guid, null, Guid.NewGuid());
            ActionInventoryEntryMove entry_move_in_action = new ActionInventoryEntryMove(Guid.NewGuid(), Guid.NewGuid(), null, inv_guid, null),
                entry_move_out_action = new ActionInventoryEntryMove(Guid.NewGuid(), inv_guid, null, Guid.NewGuid(), null);
            ActionInventoryEntryMerge entry_merge_action = new ActionInventoryEntryMerge(inv_guid, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            ActionInventoryEntryUnstack entry_unstack_action = new ActionInventoryEntryUnstack(inv_guid, null, Guid.NewGuid(), Guid.NewGuid());
            ActionInventoryEntrySplit entry_split_action = new ActionInventoryEntrySplit(inv_guid, null, Guid.NewGuid(), 2, 1, Guid.NewGuid());
            List<EntryAction> actions = new List<EntryAction>() {
                entry_add_action,
                entry_rem_action,
                entry_move_in_action,
                entry_move_out_action,
                entry_merge_action,
                entry_unstack_action,
                entry_split_action,
            };

            ActionInventoryRemove remove_action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 2);
            ActionInventoryEntryMove remaining_action = actions[0] as ActionInventoryEntryMove;
            Assert.IsNotNull(remaining_action);
            Assert.AreEqual(remaining_action.guid, entry_move_out_action.guid);
            Assert.AreEqual(remaining_action.from_guid, entry_move_out_action.from_guid);
            Assert.AreEqual(remaining_action.from_idx, entry_move_out_action.from_idx);
            Assert.AreEqual(remaining_action.to_guid, entry_move_out_action.to_guid);
            Assert.AreEqual(remaining_action.to_idx, entry_move_out_action.to_idx);
            ActionInventoryRemove merged_action = actions[1] as ActionInventoryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, inv_guid);
            Assert.AreEqual(merged_action.name, "Some Inventory");
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
        public void test_rebase() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Modified Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.rebase(state);
            Assert.AreEqual(action.from, "Some Inventory");
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, ent);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "The Inventory's New Groove");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();

            action.apply(state, ent);
            state.inventories.remove_inventory(inv_guid);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_merge_to_create_rename() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate create_action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            List<EntryAction> actions = new List<EntryAction>() { create_action };

            ActionInventoryRename rename_action = new ActionInventoryRename(inv_guid, "Some Inventory", "Renamed Inventory");
            rename_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryCreate merged_action = actions[0] as ActionInventoryCreate;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, inv_guid);
            Assert.AreEqual(merged_action.name, "Renamed Inventory");
        }

        [TestMethod]
        public void test_merge_to_rename_rename() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename existing_action = new ActionInventoryRename(inv_guid, "Some Inventory", "Renamed Inventory");
            List<EntryAction> actions = new List<EntryAction>() { existing_action };

            ActionInventoryRename rename_action = new ActionInventoryRename(inv_guid, "Renamed Inventory", "Twice-Renamed Inventory");
            rename_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryRename merged_action = actions[0] as ActionInventoryRename;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, inv_guid);
            Assert.AreEqual(merged_action.from, "Some Inventory");
            Assert.AreEqual(merged_action.to, "Twice-Renamed Inventory");
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, ent);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents[guid].item, gem);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid = Guid.NewGuid(), guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, ent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_already_exists() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            state.inventories.add_entry(inv_guid, gem_stack, guid);
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 0);
            Assert.AreEqual(state.inventories.active_entries.Count, 0);
            Assert.AreEqual(state.inventories.entries.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            ActionInventoryEntryAdd action = new ActionInventoryEntryAdd(inv_guid, null, guid, gem_stack);

            action.apply(state, ent);
            state.inventories.remove_entry(guid, inv_guid);
            action.revert(state, ent);
        }

        [TestMethod]
        public void test_merge_to() {
            Guid inv_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            List<EntryAction> actions = new List<EntryAction>();

            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            ActionInventoryEntryAdd add_action = new ActionInventoryEntryAdd(inv_guid, null, stack_guid, gem_stack);
            add_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryEntryAdd merged_action = actions[0] as ActionInventoryEntryAdd;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.inv_guid, inv_guid);
            Assert.IsNull(merged_action.inv_idx);
            Assert.AreEqual(merged_action.guid, stack_guid);
            Assert.AreEqual(merged_action.entry.item, gem);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryRemove action = new ActionInventoryEntryRemove(inv_guid, null, guid);

            action.apply(state, ent);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 0);
            Assert.AreEqual(state.inventories.active_entries.Count, 0);
            Assert.AreEqual(state.inventories.entries.Count, 1);
            Assert.IsTrue(state.inventories.entries.ContainsKey(guid));
            Assert.AreEqual(state.inventories.entries[guid].item, gem);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_removed() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryRemove action = new ActionInventoryEntryRemove(inv_guid, null, guid);
            state.inventories.remove_entry(guid, inv_guid);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryRemove action = new ActionInventoryEntryRemove(inv_guid, null, guid);

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents[guid].item, gem);
            Assert.AreEqual(state.inventories.active_entries.Count, 1);
            Assert.IsTrue(state.inventories.active_entries.Contains(guid));
            Assert.AreEqual(state.inventories.entries.Count, 1);
            Assert.IsTrue(state.inventories.entries.ContainsKey(guid));
            Assert.AreEqual(state.inventories.entries[guid].item, gem);
        }

        [TestMethod]
        public void test_merge_to_add_remove() {
            Guid inv_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            ActionInventoryEntryAdd add_action = new ActionInventoryEntryAdd(inv_guid, null, stack_guid, gem_stack);
            List<EntryAction> actions = new List<EntryAction>() { add_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, null, stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 0);
        }

        [TestMethod]
        public void test_merge_to_stackset_remove() {
            Guid inv_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            ActionItemStackSet set_action = new ActionItemStackSet(stack_guid, 3, 1, 4, 2);
            List<EntryAction> actions = new List<EntryAction>() { set_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, null, stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryEntryRemove merged_action = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.inv_guid, inv_guid);
            Assert.IsNull(merged_action.inv_idx);
            Assert.AreEqual(merged_action.guid, stack_guid);
        }

        [TestMethod]
        public void test_merge_to_stackadj_remove() {
            Guid inv_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            ActionItemStackAdjust adjust_action = new ActionItemStackAdjust(stack_guid, 0, -2);
            List<EntryAction> actions = new List<EntryAction>() { adjust_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, null, stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryEntryRemove merged_action = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.inv_guid, inv_guid);
            Assert.IsNull(merged_action.inv_idx);
            Assert.AreEqual(merged_action.guid, stack_guid);
        }

        [TestMethod]
        public void test_merge_to_itemset_remove() {
            Guid inv_guid = Guid.NewGuid(), item_guid = Guid.NewGuid();
            ActionSingleItemSet set_action = new ActionSingleItemSet(item_guid, true, null, null, false, 42, null, true);
            List<EntryAction> actions = new List<EntryAction>() { set_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, null, item_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryEntryRemove merged_action = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.inv_guid, inv_guid);
            Assert.IsNull(merged_action.inv_idx);
            Assert.AreEqual(merged_action.guid, item_guid);
        }

        [TestMethod]
        public void test_merge_to_itemadj_remove() {
            Guid inv_guid = Guid.NewGuid(), item_guid = Guid.NewGuid();
            ActionSingleItemAdjust adjust_action = new ActionSingleItemAdjust(item_guid, -5, null, null);
            List<EntryAction> actions = new List<EntryAction>() { adjust_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, null, item_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryEntryRemove merged_action = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.inv_guid, inv_guid);
            Assert.IsNull(merged_action.inv_idx);
            Assert.AreEqual(merged_action.guid, item_guid);
        }

        [TestMethod]
        public void test_merge_to_move_remove() {
            Guid from_inv_guid = Guid.NewGuid(), to_inv_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            ActionInventoryEntryMove move_action = new ActionInventoryEntryMove(stack_guid, from_inv_guid, 1, to_inv_guid, 2);
            List<EntryAction> actions = new List<EntryAction>() { move_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(to_inv_guid, 2, stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryEntryRemove merged_action = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.inv_guid, from_inv_guid);
            Assert.AreEqual(merged_action.inv_idx, 1);
            Assert.AreEqual(merged_action.guid, stack_guid);
        }

        [TestMethod]
        public void test_merge_to_merge_remove() {
            Guid inv_guid = Guid.NewGuid(), ent1_guid = Guid.NewGuid(), ent2_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            ActionInventoryEntryMerge merge_action = new ActionInventoryEntryMerge(inv_guid, 2, ent1_guid, ent2_guid, stack_guid);
            List<EntryAction> actions = new List<EntryAction>() { merge_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, 2, stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 2);
            ActionInventoryEntryRemove merged_action1 = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action1);
            Assert.AreEqual(merged_action1.inv_guid, inv_guid);
            Assert.AreEqual(merged_action1.inv_idx, 2);
            Assert.AreEqual(merged_action1.guid, ent1_guid);
            ActionInventoryEntryRemove merged_action2 = actions[1] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action2);
            Assert.AreEqual(merged_action2.inv_guid, inv_guid);
            Assert.AreEqual(merged_action2.inv_idx, 2);
            Assert.AreEqual(merged_action2.guid, ent2_guid);
        }

        [TestMethod]
        public void test_merge_to_merge_move_remove() {
            Guid inv1_guid = Guid.NewGuid(), inv2_guid = Guid.NewGuid(), ent1_guid = Guid.NewGuid(), ent2_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            ActionInventoryEntryMerge merge_action = new ActionInventoryEntryMerge(inv1_guid, 2, ent1_guid, ent2_guid, stack_guid);
            ActionInventoryEntryMove move_action = new ActionInventoryEntryMove(stack_guid, inv1_guid, 2, inv2_guid, 1);
            List<EntryAction> actions = new List<EntryAction>() { merge_action, move_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv2_guid, 1, stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 2);
            ActionInventoryEntryRemove merged_action1 = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action1);
            Assert.AreEqual(merged_action1.inv_guid, inv1_guid);
            Assert.AreEqual(merged_action1.inv_idx, 2);
            Assert.AreEqual(merged_action1.guid, ent1_guid);
            ActionInventoryEntryRemove merged_action2 = actions[1] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action2);
            Assert.AreEqual(merged_action2.inv_guid, inv1_guid);
            Assert.AreEqual(merged_action2.inv_idx, 2);
            Assert.AreEqual(merged_action2.guid, ent2_guid);
        }

        [TestMethod]
        public void test_merge_to_move_merge_remove() {
            Guid inv1_guid = Guid.NewGuid(), inv2_guid = Guid.NewGuid(), inv3_guid = Guid.NewGuid(),
                ent1_guid = Guid.NewGuid(), ent2_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid();
            ActionInventoryEntryMove move1_action = new ActionInventoryEntryMove(ent1_guid, inv1_guid, 1, inv3_guid, 3),
                move2_action = new ActionInventoryEntryMove(ent2_guid, inv2_guid, 2, inv3_guid, 3);
            ActionInventoryEntryMerge merge_action = new ActionInventoryEntryMerge(inv3_guid, 3, ent1_guid, ent2_guid, stack_guid);
            List<EntryAction> actions = new List<EntryAction>() { move1_action, move2_action, merge_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv3_guid, 3, stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 2);
            ActionInventoryEntryRemove merged_action1 = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action1);
            Assert.AreEqual(merged_action1.inv_guid, inv1_guid);
            Assert.AreEqual(merged_action1.inv_idx, 1);
            Assert.AreEqual(merged_action1.guid, ent1_guid);
            ActionInventoryEntryRemove merged_action2 = actions[1] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action2);
            Assert.AreEqual(merged_action2.inv_guid, inv2_guid);
            Assert.AreEqual(merged_action2.inv_idx, 2);
            Assert.AreEqual(merged_action2.guid, ent2_guid);
        }

        [TestMethod]
        public void test_merge_to_unstack_remove() {
            Guid inv_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid(), item_guid = Guid.NewGuid();
            ActionInventoryEntryUnstack unstack_action = new ActionInventoryEntryUnstack(inv_guid, 2, stack_guid, item_guid);
            List<EntryAction> actions = new List<EntryAction>() { unstack_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, 2, item_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionInventoryEntryRemove merged_action = actions[0] as ActionInventoryEntryRemove;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.inv_guid, inv_guid);
            Assert.AreEqual(merged_action.inv_idx, 2);
            Assert.AreEqual(merged_action.guid, stack_guid);
        }

        [TestMethod]
        public void test_merge_to_split_remove() {
            Guid inv_guid = Guid.NewGuid(), stack_guid = Guid.NewGuid(), new_stack_guid = Guid.NewGuid();
            ActionInventoryEntrySplit split_action = new ActionInventoryEntrySplit(inv_guid, 2, stack_guid, 2, 1, new_stack_guid);
            List<EntryAction> actions = new List<EntryAction>() { split_action };

            ActionInventoryEntryRemove remove_action = new ActionInventoryEntryRemove(inv_guid, 2, new_stack_guid);
            remove_action.merge_to(actions);

            Assert.AreEqual(actions.Count, 1);
            ActionItemStackAdjust merged_action = actions[0] as ActionItemStackAdjust;
            Assert.IsNotNull(merged_action);
            Assert.AreEqual(merged_action.guid, stack_guid);
            Assert.AreEqual(merged_action.count, -2);
            Assert.AreEqual(merged_action.unidentified, -1);
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
        public void test_rebase() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackSet action = new ActionItemStackSet(guid, 2, 1, 5, 1);

            action.rebase(state);
            Assert.AreEqual(action.count_from, 3);
            Assert.AreEqual(action.unidentified_from, 0);
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackSet action = new ActionItemStackSet(guid, 3, 0, 5, 1);

            action.apply(state, ent);
            Assert.AreEqual(gem_stack.count, 5);
            Assert.AreEqual(gem_stack.unidentified, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackSet action = new ActionItemStackSet(guid, 3, 0, 2, 3);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackSet action = new ActionItemStackSet(guid, 3, 0, 5, 1);

            action.apply(state, ent);
            action.revert(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackAdjust action = new ActionItemStackAdjust(guid, 2, 1);

            action.apply(state, ent);
            Assert.AreEqual(gem_stack.count, 5);
            Assert.AreEqual(gem_stack.unidentified, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackAdjust action = new ActionItemStackAdjust(guid, -1, 3);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionItemStackAdjust action = new ActionItemStackAdjust(guid, 2, 1);

            action.apply(state, ent);
            action.revert(state, ent);
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
        public void test_rebase() {
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
            ActionSingleItemSet action = new ActionSingleItemSet(guid, false, 200, wand.properties, false, 490, new_props, true);
            action.properties_from["Charges"] = "51";

            action.rebase(state);
            Assert.AreEqual(action.unidentified_from, true);
            Assert.IsNull(action.value_override_from);
            Assert.IsTrue(action.properties_from.ContainsKey("Charges"));
            Assert.AreEqual(action.properties_from["Charges"], "50");
        }

        [TestMethod]
        public void test_apply() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            Assert.IsFalse(wand.unidentified);
            Assert.AreEqual(wand.value_override, 490);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "49");
        }

        [TestMethod]
        public void test_apply_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, true, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, null, false, null, null, false);

            action.apply(state, ent);
            Assert.IsFalse(wand.unidentified);
            Assert.AreEqual(wand.value_override, 500);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid_entry_type() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, null, false, null, null, false);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.IsTrue(wand.unidentified);
            Assert.IsNull(wand.value_override);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        public void test_revert_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, true, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemSet action = new ActionSingleItemSet(guid, true, null, null, false, null, null, false);

            action.apply(state, ent);
            action.revert(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            Assert.AreEqual(wand.value_override, 490);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "49");
        }

        [TestMethod]
        public void test_apply_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, ent);
            Assert.AreEqual(wand.value_override, 490);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_value_override() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, ent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_invalid_entry_type() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 3);
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, ent);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            action.revert(state, ent);
            Assert.AreEqual(wand.value_override, 500);
            Assert.AreEqual(wand.properties.Count, 1);
            Assert.IsTrue(wand.properties.ContainsKey("Charges"));
            Assert.AreEqual(wand.properties["Charges"], "50");
        }

        [TestMethod]
        public void test_revert_sparse() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Magic", 1);
            ItemSpec wand_spec = new ItemSpec("Wand of Kaplowie", cat, 100, 1);
            SingleItem wand = new SingleItem(wand_spec, false, 500);
            wand.properties["Charges"] = "50";
            Guid inv_guid, guid;
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            guid = state.inventories.add_entry(inv_guid, wand);
            ActionSingleItemAdjust action = new ActionSingleItemAdjust(guid, -10, null, null);

            action.apply(state, ent);
            action.revert(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            Assert.AreEqual(state.inventories.inventories[inv_guid].contents.Count, 1);
            Assert.IsTrue(state.inventories.inventories[inv_guid].contents.ContainsKey(sack_guid));
            Assert.AreEqual(sack_itm.containers[0].contents.Count, 1);
            Assert.IsTrue(sack_itm.containers[0].contents.ContainsKey(sword_guid));
            Assert.AreEqual(sack_itm.containers[0].contents[sword_guid].item, sword);
        }

        [TestMethod]
        public void test_revert() {
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            action.revert(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem_spec = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem1 = new SingleItem(gem_spec, true), gem2 = new SingleItem(gem_spec, false);
            Guid inv_guid, gem1_guid, gem2_guid, stack_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            gem1_guid = state.inventories.add_entry(inv_guid, gem1);
            gem2_guid = state.inventories.add_entry(inv_guid, gem2);
            ActionInventoryEntryMerge action = new ActionInventoryEntryMerge(inv_guid, null, gem1_guid, gem2_guid, stack_guid);

            action.apply(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
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

            action.apply(state, ent);
            action.revert(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem_spec = new ItemSpec("Gem", cat, 100, 1);
            SingleItem gem1 = new SingleItem(gem_spec, true), gem2 = new SingleItem(gem_spec, false);
            Guid inv_guid, gem1_guid, gem2_guid, stack_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            gem1_guid = state.inventories.add_entry(inv_guid, gem1);
            gem2_guid = state.inventories.add_entry(inv_guid, gem2);
            ActionInventoryEntryMerge action = new ActionInventoryEntryMerge(inv_guid, null, gem1_guid, gem2_guid, stack_guid);

            action.apply(state, ent);
            action.revert(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1, 1);
            Guid inv_guid, stack_guid, item_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack_guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryUnstack action = new ActionInventoryEntryUnstack(inv_guid, null, stack_guid, item_guid);

            action.apply(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack = new ItemStack(gem, 1, 1);
            Guid inv_guid, stack_guid, item_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack_guid = state.inventories.add_entry(inv_guid, gem_stack);
            ActionInventoryEntryUnstack action = new ActionInventoryEntryUnstack(inv_guid, null, stack_guid, item_guid);

            action.apply(state, ent);
            action.revert(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 5, 3), gem_stack2;
            Guid inv_guid, stack1_guid, stack2_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack1_guid = state.inventories.add_entry(inv_guid, gem_stack1);
            ActionInventoryEntrySplit action = new ActionInventoryEntrySplit(inv_guid, null, stack1_guid, 2, 1, stack2_guid);

            action.apply(state, ent);
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
            Entry ent = new Entry(42, DateTime.Now, "Some Entry");
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gem_stack1 = new ItemStack(gem, 5, 3);
            Guid inv_guid, stack1_guid, stack2_guid = Guid.NewGuid();
            CampaignState state = new CampaignState();
            inv_guid = state.inventories.new_inventory("Test Inventory");
            stack1_guid = state.inventories.add_entry(inv_guid, gem_stack1);
            ActionInventoryEntrySplit action = new ActionInventoryEntrySplit(inv_guid, null, stack1_guid, 2, 1, stack2_guid);

            action.apply(state, ent);
            action.revert(state, ent);
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