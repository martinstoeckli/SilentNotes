using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.ViewModels;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class NoteMoverTest
    {
        [Test]
        public void GetNotePositions_MoveStepDownwards_RespectsGapsInFilter()
        {
            var allNotes = PrepareTestNotes();
            var filteredNotes = new ObservableCollection<NoteViewModelReadOnly> { allNotes[1], allNotes[3] };
            NoteViewModelReadOnly selectedNote = allNotes[1];

            var positions = NoteMover.GetNotePositions(allNotes, filteredNotes, selectedNote, false, true);
            Assert.AreEqual(1, positions.OldAllNotesPos);
            Assert.AreEqual(0, positions.OldFilteredNotesPos);
            Assert.AreEqual(3, positions.NewAllNotesPos);
            Assert.AreEqual(1, positions.NewFilteredNotesPos);
        }

        [Test]
        public void GetNotePositions_MoveStepUpwards_RespectsGapsInFilter()
        {
            var allNotes = PrepareTestNotes();
            var filteredNotes = new ObservableCollection<NoteViewModelReadOnly> { allNotes[1], allNotes[3] };
            NoteViewModelReadOnly selectedNote = allNotes[3];

            var positions = NoteMover.GetNotePositions(allNotes, filteredNotes, selectedNote, true, true);
            Assert.AreEqual(3, positions.OldAllNotesPos);
            Assert.AreEqual(1, positions.OldFilteredNotesPos);
            Assert.AreEqual(1, positions.NewAllNotesPos);
            Assert.AreEqual(0, positions.NewFilteredNotesPos);
        }

        [Test]
        public void GetNotePositions_MoveStepsUpwards_MovesToBeginOfVisibleList()
        {
            var allNotes = PrepareTestNotes();
            allNotes[0].IsPinned = true;
            allNotes[1].IsPinned = true;
            allNotes[2].IsPinned = true;
            var filteredNotes = new ObservableCollection<NoteViewModelReadOnly> { allNotes[1], allNotes[3] };
            NoteViewModelReadOnly selectedNote = allNotes[3];

            var positions = NoteMover.GetNotePositions(allNotes, filteredNotes, selectedNote, true, false);
            Assert.AreEqual(3, positions.OldAllNotesPos);
            Assert.AreEqual(1, positions.OldFilteredNotesPos);
            Assert.AreEqual(1, positions.NewAllNotesPos);
            Assert.AreEqual(0, positions.NewFilteredNotesPos);
        }

        [Test]
        public void GetNotePositions_MoveStepsDownwards_MovesToEndOfVisibleList()
        {
            var allNotes = PrepareTestNotes();
            var filteredNotes = new ObservableCollection<NoteViewModelReadOnly> { allNotes[1], allNotes[3] };
            NoteViewModelReadOnly selectedNote = allNotes[1];

            var positions = NoteMover.GetNotePositions(allNotes, filteredNotes, selectedNote, false, false);
            Assert.AreEqual(1, positions.OldAllNotesPos);
            Assert.AreEqual(0, positions.OldFilteredNotesPos);
            Assert.AreEqual(3, positions.NewAllNotesPos);
            Assert.AreEqual(filteredNotes.Count-1, positions.NewFilteredNotesPos);
        }

        [Test]
        public void AdjustPinStatusAfterMoving_PinsNoteIfDraggedAboveLastPinnedNote()
        {
            // Note 2 was dragged above Note 3
            var allNotes = PrepareTestNotes();
            allNotes[0].IsPinned = true;
            allNotes[1].IsPinned = true;
            allNotes[2].IsPinned = false;
            allNotes[3].IsPinned = true;

            NoteMover.AdjustPinStatusAfterMoving(allNotes, 2);
            Assert.IsTrue(allNotes[2].IsPinned);
        }

        [Test]
        public void AdjustPinStatusAfterMoving_UnpinsNoteIfDraggedBelowFirstUnpinnedNote()
        {
            // Note2 was dragged below Note1
            var allNotes = PrepareTestNotes();
            allNotes[0].IsPinned = true;
            allNotes[1].IsPinned = false;
            allNotes[2].IsPinned = true;

            NoteMover.AdjustPinStatusAfterMoving(allNotes, 2);
            Assert.IsFalse(allNotes[2].IsPinned);
        }

        [Test]
        public void AdjustPinStatusAfterMoving_DoesNotChangePinStateIfNotNecessary()
        {
            var allNotes = PrepareTestNotes();
            allNotes[0].IsPinned = false;
            allNotes[1].IsPinned = false;
            allNotes[2].IsPinned = false;

            NoteMover.AdjustPinStatusAfterMoving(allNotes, 0);
            Assert.IsFalse(allNotes[0].IsPinned);

            allNotes = PrepareTestNotes();
            allNotes[0].IsPinned = true;
            allNotes[1].IsPinned = true;
            allNotes[2].IsPinned = true;

            NoteMover.AdjustPinStatusAfterMoving(allNotes, 0);
            Assert.IsTrue(allNotes[0].IsPinned);
        }

        private static List<NoteViewModelReadOnly> PrepareTestNotes()
        {
            var result = new List<NoteViewModelReadOnly>();
            result.Add(CreateNoteViewModel(new NoteModel { Id = new Guid("00000000-0000-0000-0000-000000000000"), HtmlContent = "0" }));
            result.Add(CreateNoteViewModel(new NoteModel { Id = new Guid("11111111-1111-1111-1111-111111111111"), HtmlContent = "1" }));
            result.Add(CreateNoteViewModel(new NoteModel { Id = new Guid("22222222-2222-2222-2222-222222222222"), HtmlContent = "2" }));
            result.Add(CreateNoteViewModel(new NoteModel { Id = new Guid("33333333-3333-3333-3333-333333333333"), HtmlContent = "3" }));
            result.Add(CreateNoteViewModel(new NoteModel { Id = new Guid("44444444-4444-4444-4444-444444444444"), HtmlContent = "4" }));
            result.Add(CreateNoteViewModel(new NoteModel { Id = new Guid("55555555-5555-5555-5555-555555555555"), HtmlContent = "5" }));
            return result;
        }

        private static NoteViewModelReadOnly CreateNoteViewModel(NoteModel model)
        {
            return new NoteViewModelReadOnly(model, null, null, null, CommonMocksAndStubs.SafeKeyService(), null, null);
        }
    }
}
