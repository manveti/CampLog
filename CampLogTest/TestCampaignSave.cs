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
    }
}