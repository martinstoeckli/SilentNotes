using NUnit.Framework;
using SilentNotes.HtmlView;

namespace SilentNotesTest.HtmView
{
    [TestFixture]
    public class HtmlViewBindingsTest
    {
        [Test]
        public void IsExternalLinkUriAcceptsValidUrls()
        {
            Assert.IsTrue(HtmlViewBindings.IsExternalLinkUri("http://www.example.com"));
            Assert.IsTrue(HtmlViewBindings.IsExternalLinkUri("https://www.example.com"));
            Assert.IsTrue(HtmlViewBindings.IsExternalLinkUri("HTTP://www.example.com"));
            Assert.IsTrue(HtmlViewBindings.IsExternalLinkUri("mailto://me@example.com"));
        }

        [Test]
        public void IsExternalLinkUriRejectsWrongUrls()
        {
            Assert.IsFalse(HtmlViewBindings.IsExternalLinkUri(null));
            Assert.IsFalse(HtmlViewBindings.IsExternalLinkUri(""));
            Assert.IsFalse(HtmlViewBindings.IsExternalLinkUri("webview://click#http"));
            Assert.IsFalse(HtmlViewBindings.IsExternalLinkUri("tel://+41 00 000 000"));
        }
    }
}
