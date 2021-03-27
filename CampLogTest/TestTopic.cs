using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

using CampLog;

namespace CampLogTest {
    [TestClass]
    public class TestTopic {
        [TestMethod]
        public void test_serialization() {
            Topic foo = new Topic("Some topic", "A thing to know things about"), bar;
            DataContractSerializer fmt = new DataContractSerializer(typeof(Topic));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                fmt.WriteObject(ms, foo);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                System.Xml.XmlDictionaryReader xr = System.Xml.XmlDictionaryReader.CreateTextReader(ms, new System.Xml.XmlDictionaryReaderQuotas());
                bar = (Topic)(fmt.ReadObject(xr, true));
            }
            Assert.AreEqual(foo.name, bar.name);
            Assert.AreEqual(foo.description, bar.description);
        }
    }
}