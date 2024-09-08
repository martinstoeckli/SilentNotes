using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class WebviewUtilsTest
    {
        [TestMethod]
        public void EscapeJavaScriptStringReturnsEmptyStringForNull()
        {
            Assert.AreEqual(string.Empty, WebviewUtils.EscapeJavaScriptString(null));
            Assert.AreEqual(string.Empty, WebviewUtils.EscapeJavaScriptString(string.Empty));
        }

        [TestMethod]
        public void IsExternalUrlWorksWithNullOrEmpty()
        {
            Assert.IsFalse(WebviewUtils.IsExternalUri(null));
            Assert.IsFalse(WebviewUtils.IsExternalUri(string.Empty));
        }

        [TestMethod]
        public void IsExternalUrlWorksWithKnownProtocols()
        {
            Assert.IsTrue(WebviewUtils.IsExternalUri("http://example.com"));
            Assert.IsTrue(WebviewUtils.IsExternalUri("https://www.example.com"));
            Assert.IsTrue(WebviewUtils.IsExternalUri("ftp://example.com/text.txt"));
            Assert.IsTrue(WebviewUtils.IsExternalUri("ftps://example.com/text.txt"));
            Assert.IsTrue(WebviewUtils.IsExternalUri("mailto://name@example.com"));
            Assert.IsTrue(WebviewUtils.IsExternalUri("news://example.com"));
            Assert.IsTrue(WebviewUtils.IsExternalUri("tel:+41 00 000 000"));
        }

        [TestMethod]
        public void IsExternalUrlRejectsInvalidFormat()
        {
            Assert.IsFalse(WebviewUtils.IsExternalUri("https://"));
        }

        [TestMethod]
        public void IsExternalUrlWorksCaseInsensitive()
        {
            Assert.IsTrue(WebviewUtils.IsExternalUri("HttP://example.com/test"));
        }

        [TestMethod]
        public void IsExternalUrlRejectsWrongUrls()
        {
            Assert.IsFalse(WebviewUtils.IsExternalUri("webview://click#http"));
        }
    }
}
