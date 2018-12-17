using System;
using System.Xml.Linq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class NoteRepositoryUpdaterTest
    {
        [Test]
        public void NewestSupportedVersionIsCorrect()
        {
            INoteRepositoryUpdater updater = new NoteRepositoryUpdater();
            Assert.AreEqual(2, updater.NewestSupportedRevision);
        }

        [Test]
        public void IsTooNewForThisAppWorksCorrectly()
        {
            INoteRepositoryUpdater updater = new NoteRepositoryUpdater();
            int supportedVersion = updater.NewestSupportedRevision;
            NoteRepositoryModel repository = new NoteRepositoryModel();

            repository.Revision = supportedVersion - 1;
            XDocument repositoryXml = XmlUtils.SerializeToXmlDocument(repository);
            Assert.IsFalse(updater.IsTooNewForThisApp(repositoryXml));

            repository.Revision = supportedVersion;
            repositoryXml = XmlUtils.SerializeToXmlDocument(repository);
            Assert.IsFalse(updater.IsTooNewForThisApp(repositoryXml));

            repository.Revision = supportedVersion + 1;
            repositoryXml = XmlUtils.SerializeToXmlDocument(repository);
            Assert.IsTrue(updater.IsTooNewForThisApp(repositoryXml));
        }

        [Test]
        public void UpdatesFrom1To2Correctly()
        {
            INoteRepositoryUpdater updater = new NoteRepositoryUpdater();
            XDocument xml = XDocument.Parse(Version1Repository);

            bool result = updater.Update(xml);
            NoteRepositoryModel repository = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(xml);

            Assert.IsTrue(result);
            Assert.IsNotNull(repository);
            Assert.AreEqual(new Guid("093b917a-f69f-4dd3-91b7-ad175fe0a4c1"), repository.Id);
            Assert.AreEqual(2, repository.Notes.Count);
            Assert.AreEqual(new Guid("a2b16ab9-9f7f-4389-916f-f2ef9a2f3a3a"), repository.Notes[0].Id);
            string noteContent = repository.Notes[0].HtmlContent;
            Assert.IsTrue(noteContent.Contains("<h1>Borrowed</h1>"));
            Assert.IsTrue(noteContent.Contains("<p>• 'The Black Magician Trilogy'"));
            Assert.AreEqual(new Guid("70a25de4-2141-4164-aefc-b9b2624a112c"), repository.Notes[1].Id);
            noteContent = repository.Notes[1].HtmlContent;
            Assert.IsFalse(noteContent.Contains("<h1>")); // no title
            Assert.IsTrue(noteContent.Contains("<p>- Milk"));
            Assert.AreEqual(1, repository.DeletedNotes.Count);
            Assert.AreEqual(new Guid("fae40c63-d850-4b78-a8bd-609893d2983b"), repository.DeletedNotes[0]);

            Assert.AreEqual(NoteRepositoryModel.NewestSupportedRevision, repository.Revision);
        }

        private const string Version1Repository =
@"<?xml version='1.0' encoding='utf-8'?>
<silentnotes xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' id='093b917a-f69f-4dd3-91b7-ad175fe0a4c1' revision='1' order_modified_at='2017-09-10T10:19:25.585917Z'>
  <notes>
    <note id='a2b16ab9-9f7f-4389-916f-f2ef9a2f3a3a' background_color='#fdd8bb' in_recycling_bin='false' created_at='2017-09-10T09:44:14.5098885Z' modified_at='2017-09-10T10:19:32.5617538Z' auto_word_wrap='true'>
      <title>Borrowed</title>
      <content>• 'The Black Magician Trilogy' ➜ from aunt Maggie
• 'The golden compass' ➜ from Tim
• 'Harry potter 5' ➜ from Dawn Cook</content>
    </note>
    <note id='70a25de4-2141-4164-aefc-b9b2624a112c' background_color='#fbf4c1' in_recycling_bin='false' created_at='2017-09-10T09:39:12.8056002Z' modified_at='2017-09-10T09:44:00.8563593Z' auto_word_wrap='true'>
      <content>- Milk
- Toast
- Sun cream
- Garbage bags</content>
    </note>
  </notes>
  <deleted_notes>
    <deleted_note>fae40c63-d850-4b78-a8bd-609893d2983b</deleted_note>
  </deleted_notes>
</silentnotes>";
    }
}
