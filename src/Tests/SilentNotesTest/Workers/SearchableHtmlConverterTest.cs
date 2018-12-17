using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class SearchableHtmlConverterTest
    {
        [Test]
        public void TryConvertHtmlStripsTags()
        {
            string note = @"<h1>Hello Quillnote</h1><p>Das ist ein etwas längerer 🐢🐼👍🍒🥋💪⚕㊙🖐🏿 der sicher zu einem Zeilenumbruch führt. Natürlich gibt es dann auch extra Umbrüche, um zu testen wie diese aussehen. Der Text ist dermassen lang, dass er gar nicht ins Feld passt!!!</p><h3>begin</h3><pre class='ql-syntax' spellcheck='false'>Bla blah
</pre><blockquote>tuttouoweitu lksjflasjdf akljs flkjasf ksaljf laksjdflsadf</blockquote><p>aklsdfj lkasjfsajflkjsadf sugus und power</p>";
            Assert.IsTrue(new SearchableHtmlConverter().TryConvertHtml(note, out string res));

            string noteText = @"Hello Quillnote Das ist ein etwas längerer 🐢🐼👍🍒🥋💪⚕㊙🖐🏿 der sicher zu einem Zeilenumbruch führt. Natürlich gibt es dann auch extra Umbrüche, um zu testen wie diese aussehen. Der Text ist dermassen lang, dass er gar nicht ins Feld passt!!! begin Bla blah tuttouoweitu lksjflasjdf akljs flkjasf ksaljf laksjdflsadf aklsdfj lkasjfsajflkjsadf sugus und power";
            Assert.AreEqual(noteText, res);
        }

        [Test]
        public void TryConvertHtmlCorrectlyGroupsDifferentWhitespaces()
        {
            string note = @"<p> Hello 	
  world  </p>";
            Assert.IsTrue(new SearchableHtmlConverter().TryConvertHtml(note, out string res));
            Assert.AreEqual(" Hello world ", res);

            note = @" <p>Hello 	
  world</p>  ";
            Assert.IsTrue(new SearchableHtmlConverter().TryConvertHtml(note, out res));
            Assert.AreEqual(" Hello world ", res);
        }

        [Test]
        public void TryConvertHtmlCorrectlyHandlesBr()
        {
            string note = @"<p>Hello</p><p><br></p><p>world</p>";
            Assert.IsTrue(new SearchableHtmlConverter().TryConvertHtml(note, out string res));
            Assert.AreEqual("Hello world", res);
        }

        [Test]
        public void TryConvertHtmlCorrectlyHandlesEncodedHtml()
        {
            string note = @"<h1>H1</h1>
&lt;html lang='en'&gt;
&lt;head&gt;
&lt;/head&gt;

&lt;body&gt;
&lt;/body&gt;
&lt;/html&gt;";
            Assert.IsTrue(new SearchableHtmlConverter().TryConvertHtml(note, out string res));
            Assert.AreEqual("H1 <html lang='en'> <head> </head> <body> </body> </html>", res);
        }

        [Test]
        public void NormalizeWhitespacesLeavesTagsIntact()
        {
            string note = @"  Hello  <p> world  </p>  ";
            string res = SearchableHtmlConverter.NormalizeWhitespaces(note);
            Assert.AreEqual(" Hello <p> world </p> ", res);
        }

        [Test]
        public void NormalizeWhitespacesAcceptsNullAndSpace()
        {
            string res = SearchableHtmlConverter.NormalizeWhitespaces(null);
            Assert.AreEqual(string.Empty, res);

            res = SearchableHtmlConverter.NormalizeWhitespaces(string.Empty);
            Assert.AreEqual(string.Empty, res);

            res = SearchableHtmlConverter.NormalizeWhitespaces(" ");
            Assert.AreEqual(" ", res);
        }
    }
}
