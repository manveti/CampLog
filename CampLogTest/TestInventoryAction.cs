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
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_already_exists() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Pre-existing Inventory", inv_guid);

            action.apply(state);
        }

        [TestMethod]
        public void test_revert() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state);
            action.revert(state);
            Assert.AreEqual(state.inventories.inventories.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryCreate action = new ActionInventoryCreate(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state);
            state.inventories.remove_inventory(inv_guid);
            action.revert(state);
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
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state);
            Assert.AreEqual(state.inventories.inventories.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state);
        }

        [TestMethod]
        public void test_revert() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state);
            action.revert(state);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_restored() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRemove action = new ActionInventoryRemove(inv_guid, "Some Inventory");
            CampaignState state = new CampaignState();

            action.apply(state);
            state.inventories.new_inventory("Some Inventory", inv_guid);
            action.revert(state);
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
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "The Inventory's New Groove");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_apply_no_such_inventory() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();

            action.apply(state);
        }

        [TestMethod]
        public void test_revert() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();
            state.inventories.new_inventory("Some Inventory", inv_guid);

            action.apply(state);
            action.revert(state);
            Assert.AreEqual(state.inventories.inventories.Count, 1);
            Assert.IsTrue(state.inventories.inventories.ContainsKey(inv_guid));
            Assert.AreEqual(state.inventories.inventories[inv_guid].name, "Some Inventory");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void test_revert_removed() {
            Guid inv_guid = Guid.NewGuid();
            ActionInventoryRename action = new ActionInventoryRename(inv_guid, "Some Inventory", "The Inventory's New Groove");
            CampaignState state = new CampaignState();

            action.apply(state);
            state.inventories.remove_inventory(inv_guid);
            action.revert(state);
        }
    }


   // TODO: set inventory contents, item actions
}