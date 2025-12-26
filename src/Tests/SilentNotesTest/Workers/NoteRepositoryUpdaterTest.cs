using System;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class NoteRepositoryUpdaterTest
    {
        [TestMethod]
        public void IsTooNewForThisAppWorksCorrectly()
        {
            INoteRepositoryUpdater updater = new NoteRepositoryUpdater();
            NoteRepositoryModel repository = new NoteRepositoryModel();

            // Accept lower revisions
            repository.Revision = NoteRepositoryModel.NewestSupportedRevision - 1;
            XDocument repositoryXml = XmlUtils.SerializeToXmlDocument(repository);
            Assert.IsFalse(updater.IsTooNewForThisApp(repositoryXml));

            // Accept equal revisions
            repository.Revision = NoteRepositoryModel.NewestSupportedRevision;
            repositoryXml = XmlUtils.SerializeToXmlDocument(repository);
            Assert.IsFalse(updater.IsTooNewForThisApp(repositoryXml));

            // Reject higher revisions
            repository.Revision = NoteRepositoryModel.NewestSupportedRevision + 1;
            repositoryXml = XmlUtils.SerializeToXmlDocument(repository);
            Assert.IsTrue(updater.IsTooNewForThisApp(repositoryXml));
        }

        [TestMethod]
        public void Update_SetsNewestVersionNumber()
        {
            // Set revision too small
            NoteRepositoryModel repository = new NoteRepositoryModel { Revision = NoteRepositoryModel.CurrentSavingRevision - 1 };
            XDocument repositoryXml = XmlUtils.SerializeToXmlDocument(repository);

            int newestSupportedRevision = NoteRepositoryModel.CurrentSavingRevision + 1;
            INoteRepositoryUpdater updater = new NoteRepositoryUpdater(newestSupportedRevision);
            Assert.IsTrue(updater.Update(repositoryXml));
            Assert.AreEqual(NoteRepositoryModel.CurrentSavingRevision.ToString(), repositoryXml.Root.Attribute("revision").Value);

            // Update does not use newest supported revision if higher than current saving revision.
            Assert.AreNotEqual(newestSupportedRevision.ToString(), repositoryXml.Root.Attribute("revision").Value);
        }

        [TestMethod]
        public void UpdatesFrom1To2Correctly()
        {
            INoteRepositoryUpdater updater = new NoteRepositoryUpdater();
            XDocument xml = XDocument.Parse(Version1Repository);

            bool result = updater.Update(xml);
            NoteRepositoryModel repository = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(xml);

            Assert.IsTrue(result);
            Assert.IsNotNull(repository);
            Assert.AreEqual(new Guid("093b917a-f69f-4dd3-91b7-ad175fe0a4c1"), repository.Id);
            Assert.AreEqual(3, repository.Notes.Count);
            Assert.AreEqual(new Guid("a2b16ab9-9f7f-4389-916f-f2ef9a2f3a3a"), repository.Notes[0].Id);
            string noteContent = repository.Notes[0].HtmlContent;
            Assert.IsTrue(noteContent.Contains("<h1>Borrowed</h1>"));
            Assert.IsTrue(noteContent.Contains("<p>• &#39;The Black Magician Trilogy&#39;"));
            Assert.AreEqual(new Guid("70a25de4-2141-4164-aefc-b9b2624a112c"), repository.Notes[1].Id);
            noteContent = repository.Notes[1].HtmlContent;
            Assert.IsFalse(noteContent.Contains("<h1>")); // no title
            Assert.IsTrue(noteContent.Contains("<p>- Milk"));
            noteContent = repository.Notes[2].HtmlContent;
            Assert.IsTrue(noteContent.Contains("&lt;field&gt;"));

            Assert.AreEqual(1, repository.DeletedNotes.Count);
            Assert.AreEqual(new Guid("fae40c63-d850-4b78-a8bd-609893d2983b"), repository.DeletedNotes[0].Id);

            Assert.AreEqual(NoteRepositoryModel.CurrentSavingRevision, repository.Revision);
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

    <note id='6cd3f087-3d2f-4687-b573-4a4a57382ed2' background_color='#fbf4c1' in_recycling_bin='true' created_at='2018-11-01T13:31:07.357739Z' modified_at='2018-11-02T14:03:13.5417007Z' auto_word_wrap='true'>
      <text>text</text>
      <title>ShowCase</title>
      <content>&lt;field dropdown_lookup_table='...' constraintField='...' constraintValue='...' /&gt;

&lt;field&gt;
    &lt;dropdown_lookup table='...'&gt;
        &lt;constraints&gt;
            &lt;constraint column='' value='' /&gt;
        &lt;/constraints&gt;
    &lt;/dropdown_lookup&gt;
&lt;/field&gt;</content>
    </note>

  </notes>
  <deleted_notes>
    <deleted_note>fae40c63-d850-4b78-a8bd-609893d2983b</deleted_note>
  </deleted_notes>
</silentnotes>";
    }
}
