using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class HtmlShortenerTest
    {
        [TestMethod]
        public void Shorten_KeepsShortContent()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 100, WantedTagNumber = 8 };
            string result = shortener.Shorten("<p>abc</p>");
            Assert.AreEqual("<p>abc</p>", result);

            result = shortener.Shorten(null);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Shorten_WorksWithMissingContent()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 0, WantedTagNumber = 8 };
            string result = shortener.Shorten("<p>abc</p>");
            Assert.AreEqual("<p>abc</p>", result);

            result = shortener.Shorten(string.Empty);
            Assert.AreEqual(string.Empty, result);

            result = shortener.Shorten(null);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Shorten_TruncatesWantedTagNumber()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p><p>jkl</p><p>mno</p>");
            Assert.AreEqual("<p>abc</p><p>def</p>", result);
        }

        [TestMethod]
        public void Shorten_TruncatesWantedLength()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 100000, WantedLength = 5 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p><p>jkl</p><p>mno</p>");
            Assert.AreEqual("<p>abc</p><p>def</p>", result);
        }

        [TestMethod]
        public void Shorten_IsCaseInsensitive()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<p>abc</P><P>def</p><p>ghi</p><P>jkl</P>");
            Assert.AreEqual("<p>abc</P><P>def</p><p>ghi</p>", result);
        }

        [TestMethod]
        public void Shorten_CanReturnSmallerContent()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 100 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p>");
            Assert.AreEqual("<p>abc</p><p>def</p><p>ghi</p>", result);
        }

        [TestMethod]
        public void Shorten_CanReturnWholeContent()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<p>abc</p><p>def</p><p>ghi</p>");
            Assert.AreEqual("<p>abc</p><p>def</p><p>ghi</p>", result);
        }

        [TestMethod]
        public void Shorten_KeepsEnclosingHtmlTag()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<!DOCTYPE html><html><head><meta charset='utf-8'></head><body><p>abc</p><p>def</p><p>ghi</p></body></html>");
            Assert.AreEqual("<!DOCTYPE html><html><head><meta charset='utf-8'></head><body><p>abc</p><p>def</p></body></html>", result);
        }

        [TestMethod]
        public void Shorten_IgnoresUnrelevantTags()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc</br>def</p></br><p><strong>ghi</strong></p><p>jkl</p>");
            Assert.AreEqual("<p>abc</br>def</p></br><p><strong>ghi</strong></p>", result);
        }

        [TestMethod]
        public void Shorten_WorksWithClasses()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p class='ql qt' spellcheck='false'>abc</p><p disabled>def</p><p>ghi</p>");
            Assert.AreEqual("<p class='ql qt' spellcheck='false'>abc</p><p disabled>def</p>", result);
        }

        [TestMethod]
        public void Shorten_WorksWithNestedTags()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc<li>def</li></p><p><li><li>hi</li></li>ghi</p><p>klm</p>");
            Assert.AreEqual("<p>abc<li>def</li></p>", result);
        }

        [TestMethod]
        public void Shorten_UnfinishedTagWorksCorrectly()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 2 };
            string result = shortener.Shorten("<p>abc<li>def</li><p>ghi</p><p>klm</p>");
            Assert.AreEqual("<p>abc<li>def</li><p>ghi</p></p>", result);
        }

        [TestMethod]
        public void Shorten_ReducesItemsOfLongList()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<ul><li>111</li><li>222</li><li>333</li><li>444</li><li>555</li></ul>");
            Assert.AreEqual("<ul><li>111</li><li>222</li><li>333</li></ul>", result);
        }

        [TestMethod]
        public void Shorten_ReducesItemsAcrossDistributedLists()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 3 };
            string result = shortener.Shorten("<ul><li>111</li><li>222</li></ul><ul><li>333</li><li>444</li><li>555</li></ul>");
            Assert.AreEqual("<ul><li>111</li><li>222</li></ul><ul><li>333</li></ul>", result);
        }

        [TestMethod]
        public void Shorten_HandleSelfClosingItemsCorrectly()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 5 };
            string result = shortener.Shorten("<p>aaa</p><div/><p>bbb</p><br><p>ccc</p><br/><p>ddd</p><p>eee</p>");
            Assert.AreEqual("<p>aaa</p><div/><p>bbb</p><br><p>ccc</p><br/><p>ddd</p>", result);
        }

        [TestMethod]
        public void DeactivateLinks_ReplacesAnchorTagsWithSpanTags()
        {
            HtmlShortener shortener = new HtmlShortener { MinimumLengthForShortening = 1, WantedTagNumber = 5 };
            string result = shortener.DisableLinks("<p>aaa <a href='https://sbb.ch'>SBB</a> bbb</p>");
            Assert.AreEqual("<p>aaa <span href='https://sbb.ch'>SBB</span> bbb</p>", result);

            // Empty anchor tag
            result = shortener.DisableLinks("<p>aaa <a></a> bbb</p>");
            Assert.AreEqual("<p>aaa <span></span> bbb</p>", result);

            // Uncommon formatted tag
            result = shortener.DisableLinks("< a  href = '#' ></ a >");
            Assert.AreEqual("< span  href = '#' ></ span >", result);

            // Without link tag
            result = shortener.DisableLinks("<p>aaa <div></div> bbb</p>");
            Assert.AreEqual("<p>aaa <div></div> bbb</p>", result);

            // Tag with child tag
            result = shortener.DisableLinks("<a href='#'><img src=''/></a>");
            Assert.AreEqual("<span href='#'><img src=''/></span>", result);
        }
    }
}
