using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes;
using SilentNotes.Workers;
using System;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SilentNotesTest
{
    [TestClass]
    public class KeyValueListTest
    {
        [TestMethod]
        public void AddOrReplaceWorksAddsNewEntry()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            Assert.AreEqual(0, list.Count);

            list.AddOrReplace("one", "once");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("one", list[0].Key);
            Assert.AreEqual("once", list[0].Value);
        }

        [TestMethod]
        public void AddOrReplaceWorksReplacesEntry()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            Assert.AreEqual(0, list.Count);

            list.AddOrReplace("one", "once");
            list.AddOrReplace("one", "twice");
            Assert.AreEqual("one", list[0].Key);
            Assert.AreEqual("twice", list[0].Value);
        }

        [TestMethod]
        public void AddOrReplaceAcceptsNullValue()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            list.AddOrReplace("one", null);
            Assert.AreEqual(1, list.Count);
            Assert.IsNull(list[0].Value);

            list.AddOrReplace(null, "twice");
            Assert.IsNull(list[1].Key);
        }

        [TestMethod]
        public void GetterFindsCorrectValue()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            list["one"] = "once";
            list["two"] = "twice";

            string result = list["one"];
            Assert.AreEqual("once", result);
            result = list["two"];
            Assert.AreEqual("twice", result);

            result = list["notexisting"];
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetterReturnsDefaultValueForNonExistingItem()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();

            string result = list["one"];
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetterAcceptsNull()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();

            string result;
            result = list[null];
            Assert.IsNull(result);

            list.AddOrReplace(null, "none");
            result = list[null];
            Assert.AreEqual("none", result);
        }

        [TestMethod]
        public void TryGetValueWorksCorrectly()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            list["one"] = "once";

            bool result;
            string value;
            result = list.TryGetValue("one", out value);
            Assert.IsTrue(result);
            Assert.AreEqual("once", value);

            result = list.TryGetValue("notexisting", out value);
            Assert.IsFalse(result);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TryGetKeyWorksCorrectly()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            list["one"] = "once";

            bool result;
            string key;
            result = list.TryGetKey("once", out key);
            Assert.IsTrue(result);
            Assert.AreEqual("one", key);

            result = list.TryGetKey("notexisting", out key);
            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void ListRespectsEqualityComparer()
        {
            // case insensitive comparer
            KeyValueList<string, string> list = new KeyValueList<string, string>(StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);
            list["one"] = "once";

            var result = list["OnE"];
            Assert.AreEqual("once", result);

            list.TryGetKey("OnCe", out result);
            Assert.AreEqual("one", result);
        }

        [TestMethod]
        public void IntIndexerReturnsValueInsteadOfPair()
        {
            KeyValueList<int, string> list = new KeyValueList<int, string>();
            list[1] = "once";
            list[2] = "twice";
            var result = list[1];
            Assert.AreEqual("once", result);
        }

        [TestMethod]
        public void IsEnumerable()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            list["one"] = "once";
            list["two"] = "twice";
            int index = 0;
            foreach (var item in list)
            {
                switch (index)
                {
                    case 0:
                        Assert.AreEqual("one", item.Key);
                        break;
                    case 1:
                        Assert.AreEqual("two", item.Key);
                        break;
                    default:
                        throw new Exception("Too many iterations");
                }
                index++;
            }
        }

        [TestMethod]
        public void GetByIndexReturnsPair()
        {
            KeyValueList<string, string> list = new KeyValueList<string, string>();
            list["one"] = "once";

            var pair = list.GetByIndex(0);
            Assert.AreEqual("one", pair.Key);
        }

        [TestMethod]
        public void RemoveByKeyRemovesAllMatchingItems()
        {
            KeyValueList<int, string> list = new KeyValueList<int, string>();
            list[1] = "one";
            list[2] = "two";
            list[1] = "secondone";

            list.RemoveByKey(1);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(2, list.GetByIndex(0).Key);
        }

        [TestMethod]
        public void CanSerialize()
        {
            ListContainer listContainer = new ListContainer();
            listContainer.List = new KeyValueList<string, string>();
            listContainer.List.AddOrReplace("one", "once");
            listContainer.List.AddOrReplace("two", "twice");

            string serializedList = XmlUtils.SerializeToString(listContainer);
            Assert.IsTrue(serializedList.Contains("<items>"));
            Assert.IsTrue(serializedList.Contains("<item>"));
            Assert.IsTrue(serializedList.Contains("<key>one</key>"));
            Assert.IsTrue(serializedList.Contains("<value>twice</value>"));
        }

        [TestMethod]
        public void CanDeserialize()
        {
            XDocument xml = XDocument.Parse(SerializedList);
            ListContainer listContainer = XmlUtils.DeserializeFromXmlDocument<ListContainer>(xml);
            Assert.IsNotNull(listContainer);
            Assert.AreEqual(2, listContainer.List.Count);
            Assert.AreEqual("one", listContainer.List.GetByIndex(0).Key);
            Assert.AreEqual("two", listContainer.List.GetByIndex(1).Key);
        }

        public class ListContainer
        {
            [XmlArray(ElementName = "items")]
            [XmlArrayItem(ElementName = "item")]
            public KeyValueList<string, string> List { get; set; }
        }

        private const string SerializedList =
@"<?xml version='1.0' encoding='utf-16'?>
<ListContainer>
  <items>
    <item>
      <key>one</key>
      <value>once</value>
    </item>
    <item>
      <key>two</key>
      <value>twice</value>
    </item>
  </items>
</ListContainer>";
    }
}
