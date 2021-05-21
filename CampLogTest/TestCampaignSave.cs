using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestCampaignSave {
        [TestMethod]
        public void test_serialization() {
            Character chr = new Character("Somebody");
            CampaignSave foo = new CampaignSave(new Calendar(), new CharacterSheet()), bar;

            Guid chr_guid = foo.domain.state.characters.add_character(chr);

            DataContractSerializer fmt = new DataContractSerializer(typeof(CampaignSave));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (CampaignSave)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.domain.state.characters.characters.Count, bar.domain.state.characters.characters.Count);
            Assert.IsTrue(bar.domain.state.characters.characters.ContainsKey(chr_guid));
            Assert.AreEqual(bar.domain.state.characters.characters[chr_guid].name, "Somebody");
            Assert.AreEqual(foo.calendar.GetType(), bar.calendar.GetType());
            Assert.AreEqual(foo.character_sheet.GetType(), bar.character_sheet.GetType());
            Assert.AreEqual(foo.show_past_events, bar.show_past_events);
            Assert.AreEqual(foo.show_inactive_tasks, bar.show_inactive_tasks);
        }

        [TestMethod]
        public void test_add_remove_category_reference() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gp = new ItemSpec("GP", cat, 1, 0), gem = new ItemSpec("Gem", cat, 100, 1);
            CampaignSave state = new CampaignSave(new Calendar(), new CharacterSheet());

            state.add_category_reference(gp);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 1);

            state.add_category_reference(gem);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 2);

            state.remove_category_reference(gp);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 1);
        }

        [TestMethod]
        public void test_add_reference() {
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gp = new ItemSpec("GP", cat, 1, 0), gem = new ItemSpec("Gem", cat, 100, 1);
            CampaignSave state = new CampaignSave(new Calendar(), new CharacterSheet());

            state.add_item_reference(gp);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 1);
            Assert.AreEqual(state.items.Count, 1);
            Assert.IsTrue(state.items.ContainsKey("GP"));
            Assert.AreEqual(state.items["GP"].element, gp);
            Assert.AreEqual(state.items["GP"].ref_count, 1);

            state.add_item_reference(gem);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 2);
            Assert.AreEqual(state.items.Count, 2);
            Assert.IsTrue(state.items.ContainsKey("GP"));
            Assert.AreEqual(state.items["GP"].element, gp);
            Assert.AreEqual(state.items["GP"].ref_count, 1);
            Assert.IsTrue(state.items.ContainsKey("Gem"));
            Assert.AreEqual(state.items["Gem"].element, gem);
            Assert.AreEqual(state.items["Gem"].ref_count, 1);

            state.add_item_reference(gp);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 2);
            Assert.AreEqual(state.items.Count, 2);
            Assert.IsTrue(state.items.ContainsKey("GP"));
            Assert.AreEqual(state.items["GP"].element, gp);
            Assert.AreEqual(state.items["GP"].ref_count, 2);
            Assert.IsTrue(state.items.ContainsKey("Gem"));
            Assert.AreEqual(state.items["Gem"].element, gem);
            Assert.AreEqual(state.items["Gem"].ref_count, 1);
        }

        [TestMethod]
        public void test_add_references() {
            Guid inv_guid = Guid.NewGuid();
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gp = new ItemSpec("GP", cat, 1, 0), gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gp_stack = new ItemStack(gp, 350), gem_stack = new ItemStack(gem, 3), big_stack = new ItemStack(gp, 700);
            List<EntryAction> actions = new List<EntryAction>() {
                new ActionInventoryCreate(inv_guid, "Some Inventory"),
                new ActionInventoryEntryAdd(inv_guid, null, Guid.NewGuid(), gp_stack),
                new ActionInventoryEntryAdd(inv_guid, null, Guid.NewGuid(), gem_stack),
                new ActionInventoryEntryAdd(inv_guid, null, Guid.NewGuid(), big_stack),
            };
            CampaignSave state = new CampaignSave(new Calendar(), new CharacterSheet());

            state.add_references(actions);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 2);
            Assert.AreEqual(state.items.Count, 2);
            Assert.IsTrue(state.items.ContainsKey("GP"));
            Assert.AreEqual(state.items["GP"].element, gp);
            Assert.AreEqual(state.items["GP"].ref_count, 2);
            Assert.IsTrue(state.items.ContainsKey("Gem"));
            Assert.AreEqual(state.items["Gem"].element, gem);
            Assert.AreEqual(state.items["Gem"].ref_count, 1);
        }

        [TestMethod]
        public void test_remove_references() {
            Guid inv_guid = Guid.NewGuid();
            ItemCategory cat = new ItemCategory("Wealth", 1);
            ItemSpec gp = new ItemSpec("GP", cat, 1, 0), gem = new ItemSpec("Gem", cat, 100, 1);
            ItemStack gp_stack = new ItemStack(gp, 350), gem_stack = new ItemStack(gem, 3), big_stack = new ItemStack(gp, 700);
            List<EntryAction> actions = new List<EntryAction>() {
                new ActionInventoryCreate(inv_guid, "Some Inventory"),
                new ActionInventoryEntryAdd(inv_guid, null, Guid.NewGuid(), gp_stack),
                new ActionInventoryEntryAdd(inv_guid, null, Guid.NewGuid(), gem_stack),
                new ActionInventoryEntryAdd(inv_guid, null, Guid.NewGuid(), big_stack),
            };
            CampaignSave state = new CampaignSave(new Calendar(), new CharacterSheet());

            state.categories["Wealth"] = new ElementReference<ItemCategory>(cat) { ref_count = 5 };
            state.items["GP"] = new ElementReference<ItemSpec>(gp) { ref_count = 10 };
            state.items["Gem"] = new ElementReference<ItemSpec>(gem) { ref_count = 7 };

            state.remove_references(actions);
            Assert.AreEqual(state.categories.Count, 1);
            Assert.IsTrue(state.categories.ContainsKey("Wealth"));
            Assert.AreEqual(state.categories["Wealth"].element, cat);
            Assert.AreEqual(state.categories["Wealth"].ref_count, 5);
            Assert.AreEqual(state.items.Count, 2);
            Assert.IsTrue(state.items.ContainsKey("GP"));
            Assert.AreEqual(state.items["GP"].element, gp);
            Assert.AreEqual(state.items["GP"].ref_count, 8);
            Assert.IsTrue(state.items.ContainsKey("Gem"));
            Assert.AreEqual(state.items["Gem"].element, gem);
            Assert.AreEqual(state.items["Gem"].ref_count, 6);
        }
    }
}