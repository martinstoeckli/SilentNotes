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
