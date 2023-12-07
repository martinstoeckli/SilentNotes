using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestFixture]
    public class TagTreeItemViewModelTest
    {
        [Test]
        public async Task LoadChildren_ExpandsWithCorrectTags()
        {
            var notes = new List<NoteViewModelReadOnly>();
            notes.Add(CreateNote(new string[] { "01Jan", "02Feb" }));
            notes.Add(CreateNote(new string[] { "02Feb", "03Mar" }));
            notes.Add(CreateNote(new string[] { "02Feb", "03Mar", "04Apr" }));
            notes.Add(CreateNote(new string[] { "04Apr", "05May" }));
            notes.Add(CreateNote(new string[] { "06Jun", "07Jul" }));
            notes.Add(CreateNote(new string[] { "07Jul", "08Aug" }));
            notes.Add(CreateNote(new string[] { "06Jun", "08Aug", "09Sep" }));
            notes.Add(CreateNote(new string[] { "10Oct", "11Nov" }));
            notes.Add(CreateNote(new string[] { "12Dec" }));

            TagTreeItemViewModel rootNode = new TagTreeItemViewModel(null, null, notes);
            await rootNode.LazyLoadChildren();

            Assert.AreEqual(12, rootNode.Children.Count);

            // todo: enable check for order as soon as MudBlazor replaces the HashSet with a ICollection, see: https://github.com/MudBlazor/MudBlazor/pull/4556
            Assert.IsTrue(rootNode.Children.Any(child => "01Jan" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "02Feb" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "03Mar" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "04Apr" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "05May" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "06Jun" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "07Jul" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "08Aug" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "09Sep" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "10Oct" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "11Nov" == child.Title));
            Assert.IsTrue(rootNode.Children.Any(child => "12Dec" == child.Title));

            // Jan just is in group with Feb
            var nodeJan = rootNode.Children.Where(node => node.Title == "01Jan").First();
            await nodeJan.LazyLoadChildren();
            Assert.AreEqual(1, nodeJan.Children.Count);
            Assert.IsTrue(nodeJan.Children.Any(child => "02Feb" == child.Title));

            // Feb is in group with Jan, Mar and Apr
            var nodeFeb = rootNode.Children.Where(node => node.Title == "02Feb").First();
            await nodeFeb.LazyLoadChildren();
            Assert.AreEqual(3, nodeFeb.Children.Count);
            Assert.IsTrue(nodeFeb.Children.Any(child => "01Jan" == child.Title));
            Assert.IsTrue(nodeFeb.Children.Any(child => "03Mar" == child.Title));
            Assert.IsTrue(nodeFeb.Children.Any(child => "04Apr" == child.Title));

            // Feb && Apr is only in group with Mar
            var nodeFebApr = nodeFeb.Children.Where(node => node.Title == "04Apr").First();
            await nodeFebApr.LazyLoadChildren();
            Assert.AreEqual(1, nodeFebApr.Children.Count);
            Assert.IsTrue(nodeFebApr.Children.Any(child => "03Mar" == child.Title));

            // Jan && Feb do not have any other nodes in same group
            var nodeJanFeb = nodeJan.Children.Where(node => node.Title == "02Feb").First();
            await nodeJanFeb.LazyLoadChildren();
            Assert.AreEqual(0, nodeJanFeb.Children.Count);
        }

        private NoteViewModelReadOnly CreateNote(IEnumerable<string> tags)
        {
            return new NoteViewModelReadOnly(new NoteModel { Tags = tags.ToList() }, null, null, null, null, null);
        }
    }
}
