using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class FontMetadataTest
    {
        [TestMethod]
        [Ignore]
        public async Task ParseWindowsFonts()
        {
            const string fontDirectory = @"C:\Windows\Fonts";
            var parser = new FontMetadataParser();

            List<FontMetadata> fontMetadatas = new List<FontMetadata>();
            DirectoryInfo fontDirectoryInfo = new DirectoryInfo(fontDirectory);
            foreach (FileInfo fileInfo in fontDirectoryInfo.EnumerateFiles())
            {
                fontMetadatas.AddRange(await parser.Parse(fileInfo.FullName));
            }
            var fontFamilies = fontMetadatas.Select(item => item.FontFamily).Distinct().Order().ToList();

            Assert.IsTrue(fontMetadatas.Count > 0);
            FontMetadata consolas = fontMetadatas.Find(item => item.FontFamily == "Consolas");
            Assert.IsNotNull(consolas);
            Assert.IsFalse(consolas.IsSymbolFont);
            FontMetadata wingdings = fontMetadatas.Find(item => item.FontFamily == "Wingdings");
            Assert.IsNotNull(wingdings);
            Assert.IsTrue(wingdings.IsSymbolFont);
        }
    }
}
