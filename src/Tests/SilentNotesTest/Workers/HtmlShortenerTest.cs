using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class HtmlShortenerTest
    {
        [Test]
        public void Shorten_KeepsShortContent()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 100, WantedTagNumber = 8 };
            string result = shortener.Shorten("<p>abc</p>");
            Assert.AreEqual("<p>abc</p>", result);

            result = shortener.Shorten(null);
            Assert.IsNull(result);
        }

        [Test]
        public void Shorten_TruncatesWantedTagNumber()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p><p>jkl</p><p>mno</p>");
            Assert.AreEqual("<p>abc</p><p>def</p>", result);
        }

        [Test]
        public void Shorten_TruncatesWantedLength()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 100000, WantedLength = 5 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p><p>jkl</p><p>mno</p>");
            Assert.AreEqual("<p>abc</p><p>def</p>", result);
        }

        [Test]
        public void Shorten_IsCaseInsensitive()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<p>abc</P><P>def</p><p>ghi</p><P>jkl</P>");
            Assert.AreEqual("<p>abc</P><P>def</p><p>ghi</p>", result);
        }

        [Test]
        public void Shorten_CanReturnSmallerContent()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 100 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p>");
            Assert.AreEqual("<p>abc</p><p>def</p><p>ghi</p>", result);
        }

        [Test]
        public void Shorten_CanReturnWholeContent()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p>");
            Assert.AreEqual("<p>abc</p><p>def</p><p>ghi</p>", result);
        }

        [Test]
        public void Shorten_KeepsEnclosingHtmlTag()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<!DOCTYPE html><html><head><meta charset='utf-8'></head><body><p>abc</p><p>def</p><p>ghi</p></body></html>");
            Assert.AreEqual("<!DOCTYPE html><html><head><meta charset='utf-8'></head><body><p>abc</p><p>def</p></body></html>", result);
        }

        [Test]
        public void Shorten_IgnoresUnrelevantTags()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc</br>def</p></br><p><strong>ghi</strong></p><p>jkl</p>");
            Assert.AreEqual("<p>abc</br>def</p></br><p><strong>ghi</strong></p>", result);
        }

        [Test]
        public void Shorten_WorksWithClasses()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p class='ql qt' spellcheck='false'>abc</p><p disabled>def</p><p>ghi</p>");
            Assert.AreEqual("<p class='ql qt' spellcheck='false'>abc</p><p disabled>def</p>", result);
        }

        [Test]
        public void Shorten_WorksWithNestedTags()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc<li>def</li></p><p><li><li>hi</li></li>ghi</p><p>klm</p>");
            Assert.AreEqual("<p>abc<li>def</li></p>", result);
        }

        [Test]
        public void Shorten_UnfinishedTagWorksCorrectly()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc<li>def</li><p>ghi</p><p>klm</p>");
            Assert.AreEqual("<p>abc<li>def</li>", result);
        }

        [Test]
        public void Shorten_ReducesItemsOfLongList()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<ul><li>111</li><li>222</li><li>333</li><li>444</li><li>555</li></ul>");
            Assert.AreEqual("<ul><li>111</li><li>222</li><li>333</li></ul>", result);
        }

        [Test]
        public void Shorten_ReducesItemsAcrossDistributedLists()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<ul><li>111</li><li>222</li></ul><ul><li>333</li><li>444</li><li>555</li></ul>");
            Assert.AreEqual("<ul><li>111</li><li>222</li></ul><ul><li>333</li></ul>", result);
        }

        [Test]
        public void Shorten_HandleSelfClosingItemsCorrectly()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 5 };
            string result = shortener.Shorten("<p>aaa</p><div/><p>bbb</p><br><p>ccc</p><br/><p>ddd</p><p>eee</p>");
            Assert.AreEqual("<p>aaa</p><div/><p>bbb</p><br><p>ccc</p><br/><p>ddd</p>", result);
        }
    }
}
