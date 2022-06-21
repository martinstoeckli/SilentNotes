using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestFixture]
    public class LanguageServiceTest
    {
        [Test]
        public void ReadFromStream_ReadsNormalResources()
        {
            List<string> resFile = new List<string>()
            {
                "guiClose=Close",
                "guiInfo=Information with space.",
            };

            Dictionary<string, string> resDictionary;
            using (Stream stream = CreateStreamFromLines(resFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                LanguageService service = new LanguageService(null, string.Empty, string.Empty);
                resDictionary = service.ReadFromStream(reader);
            }

            Assert.AreEqual(2, resDictionary.Count);
            Assert.AreEqual("Close", resDictionary["guiClose"]);
            Assert.AreEqual("Information with space.", resDictionary["guiInfo"]);
        }

        [Test]
        public void ReadFromStream_ReplaceNewlines()
        {
            List<string> resFile = new List<string>()
            {
                @"guiClose=Close\nBut on two lines.",
            };

            Dictionary<string, string> resDictionary;
            using (Stream stream = CreateStreamFromLines(resFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                LanguageService service = new LanguageService(null, string.Empty, string.Empty);
                resDictionary = service.ReadFromStream(reader);
            }

            Assert.AreEqual("Close\r\nBut on two lines.", resDictionary["guiClose"]);
        }

        [Test]
        public void ReadFromStream_SkipComments()
        {
            List<string> resFile = new List<string>()
            {
                "// Just a comment",
                "guiClose= Close ",
                "  // Another comment with inset",
            };

            Dictionary<string, string> resDictionary;
            using (Stream stream = CreateStreamFromLines(resFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                LanguageService service = new LanguageService(null, string.Empty, string.Empty);
                resDictionary = service.ReadFromStream(reader);
            }

            Assert.AreEqual(1, resDictionary.Count);
            Assert.AreEqual("Close", resDictionary["guiClose"]);
        }

        [Test]
        public void ReadFromStream_IgnoresInvalidLines()
        {
            List<string> resFile = new List<string>()
            {
                "",
                "key1",
                "=value2",
                " =value3",
                " 888 = Don't ignore this valid line. ",
            };

            Dictionary<string, string> resDictionary;
            using (Stream stream = CreateStreamFromLines(resFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                LanguageService service = new LanguageService(null, string.Empty, string.Empty);
                resDictionary = service.ReadFromStream(reader);
            }

            Assert.AreEqual(1, resDictionary.Count);
            Assert.AreEqual("Don't ignore this valid line.", resDictionary["888"]);
        }

        [Test]
        public void TrySplitLine_HandlesDelimiterCorrectly()
        {
            List<string> resFile = new List<string>()
            {
                "key1=First text",
                "key2 = Second text",
                "key3= Third text",
                "key4 =Forth text",
                "key5 ===",
                "key6====",
                "key 7=Please no",
                "key8="
            };

            Dictionary<string, string> resDictionary;
            using (Stream stream = CreateStreamFromLines(resFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                LanguageService service = new LanguageService(null, string.Empty, string.Empty);
                resDictionary = service.ReadFromStream(reader);
            }

            Assert.AreEqual("First text", resDictionary["key1"]);
            Assert.AreEqual("Second text", resDictionary["key2"]);
            Assert.AreEqual("Third text", resDictionary["key3"]);
            Assert.AreEqual("Forth text", resDictionary["key4"]);
            Assert.AreEqual("==", resDictionary["key5"]);
            Assert.AreEqual("===", resDictionary["key6"]);
            Assert.AreEqual("Please no", resDictionary["key 7"]);
            Assert.AreEqual(string.Empty, resDictionary["key8"]);
        }

        private Stream CreateStreamFromLines(List<string> lines)
        {
            string text = string.Join("\n", lines);
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            return new MemoryStream(buffer);
        }
    }
}
