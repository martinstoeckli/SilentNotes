using System;
using System.Collections.Generic;
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
        public void NoDuplicatesOnDeletedNotes()
        {
            Guid note1Id = new Guid("10000000000000000000000000000000");
            Guid note2Id = new Guid("20000000000000000000000000000000");
            Guid note3Id = new Guid("30000000000000000000000000000000");

            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            serverRepo.DeletedNotes.Add(note1Id);
            serverRepo.DeletedNotes.Add(note2Id);
            serverRepo.DeletedNotes.Add(note3Id);
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            clientRepo.DeletedNotes.Add(note2Id); // deleted in both repos
            clientRepo.Notes.Add(new NoteModel { Id = note3Id }); // not yet deleted

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(3, result.DeletedNotes.Count);
            Assert.IsTrue(result.DeletedNotes.Contains(note1Id));
            Assert.IsTrue(result.DeletedNotes.Contains(note2Id));
            Assert.IsTrue(result.DeletedNotes.Contains(note3Id));
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
        public void OuterJoinTakesNewNotesFromClient()
        {
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            NoteModel note101 = new NoteModel();
            NoteModel note102 = new NoteModel();
            NoteModel note103 = new NoteModel();
            clientRepo.Notes.Add(note101); // first note only on client
            clientRepo.Notes.Add(note102);
            clientRepo.Notes.Add(note103); // last note only on client
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            NoteModel note202 = note102.Clone();
            serverRepo.Notes.Add(note202);

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(3, result.Notes.Count);
            Assert.AreEqual(note101.Id, result.Notes[0].Id);
            Assert.AreEqual(note102.Id, result.Notes[1].Id);
            Assert.AreEqual(note103.Id, result.Notes[2].Id);
        }

        [Test]
        public void OuterJoinTakesNewNotesFromServer()
        {
            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            NoteModel note102 = new NoteModel();
            clientRepo.Notes.Add(note102);
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            NoteModel note201 = new NoteModel(); // first note only on server
            NoteModel note202 = note102.Clone();
            NoteModel note203 = new NoteModel(); // last note only on server
            serverRepo.Notes.Add(note201);
            serverRepo.Notes.Add(note202);
            serverRepo.Notes.Add(note203);

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(3, result.Notes.Count);
            Assert.AreEqual(note201.Id, result.Notes[0].Id);
            Assert.AreEqual(note202.Id, result.Notes[1].Id);
            Assert.AreEqual(note203.Id, result.Notes[2].Id);
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
        public void MergeSafes()
        {
            Guid safe1Id = new Guid("10000000000000000000000000000000");
            Guid safe2Id = new Guid("20000000000000000000000000000000");
            Guid safe3Id = new Guid("30000000000000000000000000000000");
            Guid safe4Id = new Guid("40000000000000000000000000000000");
            Guid safe5Id = new Guid("50000000000000000000000000000000");
            Guid safe6Id = new Guid("60000000000000000000000000000000");
            DateTime newerDate = new DateTime(2008, 08, 08);
            DateTime middleDate = new DateTime(2006, 06, 06);

            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            SafeModel safeS1 = new SafeModel { Id = safe2Id, SerializeableKey = "s1", ModifiedAt = newerDate };
            SafeModel safeS2 = new SafeModel { Id = safe4Id, SerializeableKey = "s2", ModifiedAt = middleDate };
            SafeModel safeS3 = new SafeModel { Id = safe6Id, SerializeableKey = "s3", ModifiedAt = middleDate };
            serverRepo.Safes.AddRange(new[] { safeS1, safeS2, safeS3 });
            AddNotesWithSafeIds(serverRepo, new[] { safe2Id, safe4Id, safe6Id } );

            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            SafeModel safeC1 = new SafeModel { Id = safe5Id, SerializeableKey = "c1", ModifiedAt = middleDate };
            SafeModel safeC2 = new SafeModel { Id = safe4Id, SerializeableKey = "c2", ModifiedAt = newerDate };
            SafeModel safeC3 = new SafeModel { Id = safe2Id, SerializeableKey = "c3", ModifiedAt = middleDate };
            SafeModel safeC4 = new SafeModel { Id = safe1Id, SerializeableKey = "c4", ModifiedAt = middleDate };
            SafeModel safeC5 = new SafeModel { Id = safe3Id, SerializeableKey = "c5", ModifiedAt = middleDate };
            clientRepo.Safes.AddRange(new[] { safeC1, safeC2, safeC3, safeC4, safeC5 });
            AddNotesWithSafeIds(clientRepo, new[] { safe5Id, safe4Id, safe2Id, safe1Id, safe3Id });

            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);
            SafeListModel safes = result.Safes;

            Assert.AreEqual(6, safes.Count);
            Assert.AreEqual(safe5Id, safes[0].Id); Assert.AreEqual("c1", safes[0].SerializeableKey);
            Assert.AreEqual(safe2Id, safes[1].Id); Assert.AreEqual("s1", safes[1].SerializeableKey);
            Assert.AreEqual(safe1Id, safes[2].Id); Assert.AreEqual("c4", safes[2].SerializeableKey);
            Assert.AreEqual(safe3Id, safes[3].Id); Assert.AreEqual("c5", safes[3].SerializeableKey);
            Assert.AreEqual(safe4Id, safes[4].Id); Assert.AreEqual("c2", safes[4].SerializeableKey);
            Assert.AreEqual(safe6Id, safes[5].Id); Assert.AreEqual("s3", safes[5].SerializeableKey);
        }

        private static void AddNotesWithSafeIds(NoteRepositoryModel repo, IEnumerable<Guid> safeIds)
        {
            foreach (Guid safeId in safeIds)
                repo.Notes.Add(new NoteModel { SafeId = safeId });
        }

        [Test]
        public void ChooseLastModifiedNoteWorksCorrectly()
        {
            NoteModel note1 = new NoteModel();
            NoteModel note2 = new NoteModel();

            // Newer ModifiedAt wins
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MetaModifiedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 01);
            note2.MetaModifiedAt = null;
            Assert.AreSame(note1, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MetaModifiedAt));

            note1.ModifiedAt = new DateTime(2000, 06, 01);
            note1.MetaModifiedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MetaModifiedAt = null;
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MetaModifiedAt));

            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MetaModifiedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MetaModifiedAt = null;
            Assert.AreSame(note1, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MetaModifiedAt));

            // MetaModifiedAt is ignored when ModifiedAt is different
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MetaModifiedAt = new DateTime(2001, 06, 30);
            note2.ModifiedAt = new DateTime(2000, 06, 30);
            note2.MetaModifiedAt = new DateTime(2001, 06, 15);
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MetaModifiedAt));

            // Newer MetaModifiedAt wins
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MetaModifiedAt = new DateTime(2001, 06, 15);
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MetaModifiedAt = new DateTime(2001, 06, 30);
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MetaModifiedAt));

            // Non null MaintainedAt wins
            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MetaModifiedAt = new DateTime(2001, 06, 15);
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MetaModifiedAt = null;
            Assert.AreSame(note1, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MetaModifiedAt));

            note1.ModifiedAt = new DateTime(2000, 06, 15);
            note1.MetaModifiedAt = null;
            note2.ModifiedAt = new DateTime(2000, 06, 15);
            note2.MetaModifiedAt = new DateTime(2001, 06, 15);
            Assert.AreSame(note2, NoteRepositoryMerger.ChooseLastModified(note1, note2, item => item.ModifiedAt, item => item.MetaModifiedAt));
        }

        /// <summary>
        /// If the order of the remote repository is newer, it usually wins. Though, if a note was
        /// pinned on the client and went to the top, it should stick there, even if the order of
        /// the remote repository has precedence.
        /// </summary>
        [Test]
        public void KeepPinnedNotesToTheTop()
        {
            NoteRepositoryModel serverRepo = new NoteRepositoryModel();
            NoteModel note101 = new NoteModel { IsPinned = true };
            NoteModel note102 = new NoteModel();
            NoteModel note103 = new NoteModel();
            NoteModel note104 = new NoteModel();
            serverRepo.Notes.Add(note101);
            serverRepo.Notes.Add(note102);
            serverRepo.Notes.Add(note103);
            serverRepo.Notes.Add(note104);

            NoteRepositoryModel clientRepo = new NoteRepositoryModel();
            NoteModel note201 = note101.Clone(); // IsPinned == true
            NoteModel note202 = note102.Clone();
            NoteModel note203 = note103.Clone();
            NoteModel note204 = note104.Clone();
            note204.IsPinned = true;
            note204.RefreshModifiedAt();
            clientRepo.Notes.Add(note204); // User pinned note so it went to the top
            clientRepo.Notes.Add(note201); // Was already pinned
            clientRepo.Notes.Add(note203); // The new order of 202/203 where changed, but the remote repo has a newer order
            clientRepo.Notes.Add(note202);

            // Take order of server, but keep pinned to the top
            NoteRepositoryMerger merger = new NoteRepositoryMerger();
            serverRepo.OrderModifiedAt = new DateTime(2000, 01, 02); // newer
            clientRepo.OrderModifiedAt = new DateTime(2000, 01, 01);
            NoteRepositoryModel result = merger.Merge(clientRepo, serverRepo);

            Assert.AreEqual(note204.Id, result.Notes[0].Id);
            Assert.AreEqual(note201.Id, result.Notes[1].Id);
            Assert.AreEqual(note202.Id, result.Notes[2].Id);
            Assert.AreEqual(note203.Id, result.Notes[3].Id);
        }
    }
}
