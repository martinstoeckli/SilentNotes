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
                "guiClose Close",
                "guiInfo Information with space.",
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
                @"guiClose Close\nBut on two lines.",
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
                "guiClose  Close ",
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
                "guiClose",
                "guiClose ",
                "guiClose  ",
                "888  Don't ignore this valid line. ",
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

        private Stream CreateStreamFromLines(List<string> lines)
        {
            string text = string.Join("\n", lines);
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            return new MemoryStream(buffer);
        }
    }
}
