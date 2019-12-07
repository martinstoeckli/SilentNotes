using System;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class NoteRepositoryMergerTest
    {
        [Test]
        public void TakeNewNotesFromBoth()
        {
            NoteRepositoryModel repo1 = new NoteRepositoryModel();
            repo1.Notes.Add(new NoteModel());
            NoteRepositoryModel repo2 = new NoteRepositoryModel();
            repo2.Notes.Add(new NoteModel());

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(repo1, repo2);

            Assert.AreEqual(2, result.Notes.Count);
            Assert.IsTrue(result.Notes.ContainsById(repo1.Notes[0].Id));
            Assert.IsTrue(result.Notes.ContainsById(repo2.Notes[0].Id));
        }

        [Test]
        public void UpdateNewerNotes()
        {
            NoteModel note1 = new NoteModel { HtmlContent = "11", ModifiedAt = new DateTime(2000, 01, 01) };
            NoteModel note2 = new NoteModel { HtmlContent = "22", ModifiedAt = new DateTime(2000, 02, 02) };
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            clientRepo.Notes.Add(note1);
            clientRepo.Notes.Add(note2);
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            NoteModel note3 = new NoteModel { HtmlContent = "33", Id = note1.Id, ModifiedAt = new DateTime(2000, 01, 08) }; // newer, should win
            NoteModel note4 = new NoteModel { HtmlContent = "44", Id = note2.Id, ModifiedAt = new DateTime(2000, 01, 28) }; // older, should loose
            serverRepo.Notes.Add(note3);
            serverRepo.Notes.Add(note4);

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(2, result.Notes.Count);
            Assert.AreEqual("33", result.Notes[0].HtmlContent);
            Assert.AreEqual("22", result.Notes[1].HtmlContent);
        }

        [Test]
        public void RemoveNotesDeletedOnTheClient()
        {
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            NoteModel note1 = new NoteModel();
            NoteModel note2 = new NoteModel();
            NoteModel note3 = new NoteModel();
            serverRepo.Notes.Add(note1);
            serverRepo.Notes.Add(note2);
            serverRepo.Notes.Add(note3);
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            clientRepo.DeletedNotes.Add(note2.Id);
            clientRepo.DeletedNotes.Add(note1.Id);
            clientRepo.DeletedNotes.Add(note3.Id);

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(0, result.Notes.Count);
            Assert.AreEqual(3, result.DeletedNotes.Count);
            Assert.IsTrue(result.DeletedNotes.Contains(note1.Id));
            Assert.IsTrue(result.DeletedNotes.Contains(note2.Id));
            Assert.IsTrue(result.DeletedNotes.Contains(note3.Id));
        }

        [Test]
        public void RemoveNotesDeletedOnTheServer()
        {
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            NoteModel note = new NoteModel();
            clientRepo.Notes.Add(note);
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            serverRepo.DeletedNotes.Add(note.Id);

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(0, result.Notes.Count);
            Assert.AreEqual(1, result.DeletedNotes.Count);
            Assert.IsTrue(result.DeletedNotes.Contains(note.Id));
        }

        [Test]
        public void AlwaysCreateNewestRevision()
        {
            NoteRepositoryModel repo1 = new NoteRepositoryModel();
            repo1.Revision = 0;
            NoteRepositoryModel repo2 = new NoteRepositoryModel();
            repo2.Revision = 1;

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(repo1, repo2);
            Assert.AreEqual(NoteRepositoryModel.NewestSupportedRevision, result.Revision);
        }

        [Test]
        public void UseOrderCorrectly()
        {
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            NoteModel note101 = new NoteModel();
            NoteModel note102 = new NoteModel();
            NoteModel note103 = new NoteModel();
            NoteModel note104 = new NoteModel();
            NoteModel note105 = new NoteModel();
            clientRepo.Notes.Add(note101);
            clientRepo.Notes.Add(note102);
            clientRepo.Notes.Add(note103);
            clientRepo.Notes.Add(note104);
            clientRepo.Notes.Add(note105);
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            NoteModel note201 = new NoteModel();
            NoteModel note202 = note103.Clone();
            NoteModel note203 = new NoteModel();
            NoteModel note204 = note102.Clone();
            NoteModel note205 = new NoteModel();
            NoteModel note206 = note104.Clone();
            serverRepo.Notes.Add(note201);
            serverRepo.Notes.Add(note202);
            serverRepo.Notes.Add(note203);
            serverRepo.Notes.Add(note204);
            serverRepo.Notes.Add(note205);
            serverRepo.Notes.Add(note206);

            // Take order of client
            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            clientRepo.OrderModifiedAt = new DateTime(2000, 01, 02); // newer
            serverRepo.OrderModifiedAt = new DateTime(2000, 01, 01); // older
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(8, result.Notes.Count);
            Assert.AreEqual(note101.Id, result.Notes[0].Id);
            Assert.AreEqual(note201.Id, result.Notes[1].Id);
            Assert.AreEqual(note102.Id, result.Notes[2].Id);
            Assert.AreEqual(note205.Id, result.Notes[3].Id);
            Assert.AreEqual(note103.Id, result.Notes[4].Id);
            Assert.AreEqual(note203.Id, result.Notes[5].Id);
            Assert.AreEqual(note104.Id, result.Notes[6].Id);
            Assert.AreEqual(note105.Id, result.Notes[7].Id);

            // Take order of server
            clientRepo.OrderModifiedAt = new DateTime(2000, 01, 01); // older
            serverRepo.OrderModifiedAt = new DateTime(2000, 01, 02); // newer
            result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(8, result.Notes.Count);
            Assert.AreEqual(note201.Id, result.Notes[0].Id);
            Assert.AreEqual(note101.Id, result.Notes[1].Id);
            Assert.AreEqual(note202.Id, result.Notes[2].Id);
            Assert.AreEqual(note203.Id, result.Notes[3].Id);
            Assert.AreEqual(note204.Id, result.Notes[4].Id);
            Assert.AreEqual(note205.Id, result.Notes[5].Id);
            Assert.AreEqual(note206.Id, result.Notes[6].Id);
            Assert.AreEqual(note105.Id, result.Notes[7].Id);
        }

        [Test]
        public void UseOrderCorrectlyIfNoNewNotes()
        {
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            NoteModel note102 = new NoteModel();
            NoteModel note104 = new NoteModel();
            clientRepo.Notes.Add(note102);
            clientRepo.Notes.Add(note104);
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            NoteModel note202 = note102.Clone();
            NoteModel note204 = note104.Clone();
            serverRepo.Notes.Add(note204);
            serverRepo.Notes.Add(note202);

            // Take order of client
            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            clientRepo.OrderModifiedAt = new DateTime(2000, 01, 02); // newer
            serverRepo.OrderModifiedAt = new DateTime(2000, 01, 01); // older
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(2, result.Notes.Count);
            Assert.AreEqual(note102.Id, result.Notes[0].Id);
            Assert.AreEqual(note104.Id, result.Notes[1].Id);
        }

        [Test]
        public void ChooseLastModifiedNoteWorksCorrectly()
        {
            NoteModel note1 = new NoteModel();
            NoteModel note2 = new NoteModel();

            // Newer ModifiedAt wins
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MaintainedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 01);
            note2.MaintainedAt = null;
            Assert.AreSame(note1, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MaintainedAt));

            note1.ModifiedAt = new DateTime(2000, 06, 01);
            note1.MaintainedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MaintainedAt = null;
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MaintainedAt));

            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MaintainedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MaintainedAt = null;
            Assert.AreSame(note1, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MaintainedAt));

            // MaintainedAt is ignored when ModifiedAt is different
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MaintainedAt = new DateTime(2001, 06, 30);
            note2.ModifiedAt = new DateTime(2000, 06, 30);
            note2.MaintainedAt = new DateTime(2001, 06, 15);
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MaintainedAt));

            // Newer MaintainedAt wins
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MaintainedAt = new DateTime(2001, 06, 15);
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MaintainedAt = new DateTime(2001, 06, 30);
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MaintainedAt));

            // Non null MaintainedAt wins
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MaintainedAt = new DateTime(2001, 06, 15);
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MaintainedAt = null;
            Assert.AreSame(note1, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MaintainedAt));

            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MaintainedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MaintainedAt = new DateTime(2001, 06, 15);
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MaintainedAt));
        }
    }
}
