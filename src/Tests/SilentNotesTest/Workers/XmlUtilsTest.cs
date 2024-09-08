using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class XmlUtilsTest
    {
        [TestMethod]
        public void SerializeToStringUsesUtf16()
        {
            XmlSerializationTest testCandidate = new XmlSerializationTest
            {
                TestAttribute = "attr",
                TestElement = "elem",
            };

            string result = XmlUtils.SerializeToString(testCandidate);
            Assert.IsTrue(result.Contains("utf-16"));
        }

        [TestMethod]
        public void CanSerializeDeserializeSanitizedXmlString()
        {
            // Build string with all valid letters
            StringBuilder sb = new StringBuilder();
            for (char letter = (char)0x00; letter <= (char)0xFFFD; letter++)
            {
                sb.Append(letter);
            }
            sb.Append("🐢🐼👍🍒🥋💪⚕㊙🖐🏿");

            string before = sb.ToString();
            string allValidCharacters = XmlUtils.SanitizeXmlString(before);

            // Serialize
            XmlSerializationTest testCandidate = new XmlSerializationTest
            {
                TestAttribute = allValidCharacters,
                TestElement = allValidCharacters,
            };
            byte[] buffer = XmlUtils.SerializeToXmlBytes(testCandidate);

            // Deserialize
            XDocument resultXml = XmlUtils.LoadFromXmlBytes(buffer);
            XmlSerializationTest result = XmlUtils.DeserializeFromXmlDocument<XmlSerializationTest>(resultXml);

            // No exception occured so far, the sanitization worked
            Assert.IsNotNull(result);
            Assert.AreEqual(allValidCharacters, result.TestAttribute);

            // \r are deserialized as \n so we make the original comparable
            allValidCharacters = allValidCharacters.Replace('\r', '\n');
            Assert.AreEqual(allValidCharacters, result.TestElement);
        }

        [TestMethod]
        public void CanSerializeDeserializeXmlDocument()
        {
            // Serialize
            XmlSerializationTest testCandidate = new XmlSerializationTest
            {
                TestAttribute = "attr",
                TestElement = "elem",
            };
            XDocument xmlDocument = XmlUtils.SerializeToXmlDocument(testCandidate);
            XElement element = xmlDocument.Root.Elements().FirstOrDefault(elem => "te" == elem.Name);
            XAttribute attribute = xmlDocument.Root.Attributes().FirstOrDefault(attr => "ta" == attr.Name);
            Assert.AreEqual("elem", element.Value);
            Assert.AreEqual("attr", attribute.Value);

            XmlSerializationTest deserializedCandidate  = XmlUtils.DeserializeFromXmlDocument<XmlSerializationTest>(xmlDocument);
            Assert.AreEqual("attr", deserializedCandidate.TestAttribute);
            Assert.AreEqual("elem", deserializedCandidate.TestElement);
        }

        [TestMethod]
        public void FuzzyTestForSerializationDeserialization()
        {
            //return;
            int maxRounds = 100;
            int charactersCount = 1000;

            Random randomGenerator = new Random();
            for (int round = 0; round < maxRounds; round++)
            {
                StringBuilder sb = new StringBuilder(charactersCount);
                for (int characterIndex = 0; characterIndex < charactersCount; characterIndex++)
                {
                    int maxPlus1 = 65535 + 1;
                    int charNumber = randomGenerator.Next(maxPlus1);
                    sb.Append((char)charNumber);
                }

                string before = sb.ToString();
                string allValidCharacters = XmlUtils.SanitizeXmlString(before);

                // Serialize
                XmlSerializationTest testCandidate = new XmlSerializationTest
                {
                    TestAttribute = allValidCharacters,
                    TestElement = allValidCharacters,
                };
                byte[] buffer = XmlUtils.SerializeToXmlBytes(testCandidate);

                // Deserialize
                XDocument resultXml = XmlUtils.LoadFromXmlBytes(buffer);
                XmlSerializationTest result = XmlUtils.DeserializeFromXmlDocument<XmlSerializationTest>(resultXml);

                // No exception occured so far, the sanitization worked
                Assert.IsNotNull(result);
                Assert.AreEqual(allValidCharacters, result.TestAttribute);

                // \r are deserialized as \n so we make the original comparable
                allValidCharacters = allValidCharacters.Replace('\r', '\n');
                Assert.AreEqual(allValidCharacters, result.TestElement);
            }
        }

        [TestMethod]
        public void SanitizeValidXmlCharactersAcceptsEmojis()
        {
            string emojiCharacters = "🐢🐼👍🍒🥋💪⚕㊙🖐🏿";
            string allValidCharacters = XmlUtils.SanitizeXmlString(emojiCharacters);
            Assert.AreEqual(emojiCharacters, allValidCharacters);
        }

        [TestMethod]
        public void SanitizeValidXmlCharactersRemovesInvalidUtf8()
        {
            // A high surrogate character must be followed by low surrogate character.
            // High surrogate = 55296...56319 = D800...‭DBFF‬
            // Low surrogate = 56320, 57343 = ‭DC00‬...‭DFFF‬
            StringBuilder sb = new StringBuilder();
            sb.Append('a');
            sb.Append((char)55300);
            sb.Append((char)55301); // second high surrogate is invalid.
            sb.Append('z');

            string before = sb.ToString();
            string after = XmlUtils.SanitizeXmlString(before);
            Assert.AreEqual("az", after);

            before = "🖐🏿";
            after = XmlUtils.SanitizeXmlString(before);
            Assert.AreEqual("🖐🏿", after);
        }

        [TestMethod]
        public void CanDeserializeEmojis()
        {
            // Build string with all valid letters
            string emojiCharacters = "🐢🐼👍🍒🥋💪⚕㊙🖐🏿";

            // Serialize
            XmlSerializationTest testCandidate = new XmlSerializationTest
            {
                TestAttribute = emojiCharacters,
                TestElement = emojiCharacters,
            };
            byte[] buffer = XmlUtils.SerializeToXmlBytes(testCandidate);

            // Deserialize
            XDocument resultXml = XmlUtils.LoadFromXmlBytes(buffer);
            XmlSerializationTest result = XmlUtils.DeserializeFromXmlDocument<XmlSerializationTest>(resultXml);

            // \r are deserialized as \n so we make the original comparable
            emojiCharacters = emojiCharacters.Replace('\r', '\n');

            // No exception occured so far, the sanitization worked
            Assert.IsNotNull(result);
            Assert.AreEqual(emojiCharacters, result.TestElement);
        }
    }

    public class XmlSerializationTest
    {
        [XmlAttribute(AttributeName = "ta")]
        public string TestAttribute { get; set; }

        [XmlElement(ElementName = "te")]
        public string TestElement { get; set; }
    }
}
