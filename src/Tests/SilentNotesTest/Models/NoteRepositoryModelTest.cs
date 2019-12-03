using System;
using NUnit.Framework;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestFixture]
    public class NoteRepositoryModelTest
    {
        [Test]
        public void GetModificationFingerprintDetectsEqualityForEqualRepositories()
        {
            NoteRepositoryModel model1 = CreateNoteRepositoryModel();
            NoteRepositoryModel model2 = CreateNoteRepositoryModel();

            Assert.AreEqual(model1.GetModificationFingerprint(), model2.GetModificationFingerprint());
            Assert.AreEqual(model1.GetModificationFingerprint(), model1.GetModificationFingerprint());
        }

        [Test]
        public void GetModificationFingerprintDetectsOrderChanges()
        {
            NoteRepositoryModel model1 = CreateNoteRepositoryModel();
            NoteRepositoryModel model2 = CreateNoteRepositoryModel();
            model2.OrderModifiedAt = new DateTime(1984, 02, 21);

            Assert.AreNotEqual(model1.GetModificationFingerprint(), model2.GetModificationFingerprint());
        }

        [Test]
        public void GetModificationFingerprintDetectsRevisionUpdates()
        {
            NoteRepositoryModel model1 = CreateNoteRepositoryModel();
            NoteRepositoryModel model2 = CreateNoteRepositoryModel();
            model2.Revision = 8;

            Assert.AreNotEqual(model1.GetModificationFingerprint(), model2.GetModificationFingerprint());
        }

        [Test]
        public void GetModificationFingerprintDetectsNoteChanges()
        {
            NoteRepositoryModel model1 = CreateNoteRepositoryModel();
            NoteRepositoryModel model2 = CreateNoteRepositoryModel();
            model2.Notes[0].ModifiedAt = new DateTime(1984, 02, 21);

            Assert.AreNotEqual(model1.GetModificationFingerprint(), model2.GetModificationFingerprint());
        }

        [Test]
        public void GetModificationFingerprintDetectsDifferencesInDeletedNotes()
        {
            NoteRepositoryModel model1 = CreateNoteRepositoryModel();
            NoteRepositoryModel model2 = CreateNoteRepositoryModel();
            model1.DeletedNotes.Add(new Guid("db73989f-5d88-43f9-bae3-bdc1b9607479"));
            model2.DeletedNotes.Add(new Guid("0152d7da-397c-43ba-b586-e5ea6f8e7c4e"));

            Assert.AreNotEqual(model1.GetModificationFingerprint(), model2.GetModificationFingerprint());
        }

        [Test]
        public void ReuniteOpenSafesWorksCorrectly()
        {
            NoteRepositoryModel repository = CreateNoteRepositoryModel();
            byte[] testKey = new byte[0];

            SafeModel safe2002 = new SafeModel { CreatedAt = new DateTime(2002, 02, 02), Key = testKey };
            SafeModel safe2003 = new SafeModel { CreatedAt = new DateTime(2003, 03, 03), Key = testKey };
            SafeModel safe2001 = new SafeModel { CreatedAt = new DateTime(2001, 01, 01), Key = null }; // closed safe
            repository.Safes.AddRange(new[] { safe2002, safe2003, safe2001 });

            NoteModel note1 = new NoteModel { SafeId = safe2001.Id };
            NoteModel note2 = new NoteModel { SafeId = safe2002.Id };
            NoteModel note3 = new NoteModel { SafeId = safe2003.Id };
            NoteModel note4 = new NoteModel { SafeId = null }; // not in safe
            repository.Notes.AddRange(new[] { note1, note2, note3, note4 });

            int removedSafes = repository.ReuniteOpenSafes();
            Assert.AreEqual(1, removedSafes);

            // safe2003 is merged with safe2002
            Assert.AreEqual(2, repository.Safes.Count);
            Assert.AreEqual(safe2002, repository.Safes[0]);
            Assert.AreEqual(safe2001, repository.Safes[1]);

            // note3 is relinked to safe2002
            Assert.AreEqual(safe2001.Id, note1.SafeId); // unchanged to closed safe
            Assert.AreEqual(safe2002.Id, note2.SafeId); // unchanged to oldest safe
            Assert.AreEqual(safe2002.Id, note3.SafeId); // changed to oldest safe
            Assert.IsNull(note4.SafeId); // unchanged to no safe

            // note3 has maintained date set
            Assert.IsNull(note1.MaintainedAt);
            Assert.IsNull(note2.MaintainedAt);
            Assert.That(note3.MaintainedAt.Value, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(3)));
            Assert.IsNull(note4.MaintainedAt);
        }

        private static NoteRepositoryModel CreateNoteRepositoryModel()
        {
            NoteRepositoryModel model = new NoteRepositoryModel();
            model.Id = new Guid("3538c76a-eee9-4905-adcf-946f8b527c37");
            model.Revision = 2;
            model.OrderModifiedAt = new DateTime(2018, 02, 21);
            model.Notes.Add(new NoteModel { ModifiedAt = new DateTime(2018, 02, 22) });
            model.DeletedNotes.Add(new Guid("c84e7eb9-f671-4b9f-a7e7-a013a5e1cef7"));
            return model;
        }
    }
}
