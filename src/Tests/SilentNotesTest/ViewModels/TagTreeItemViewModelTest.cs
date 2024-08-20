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
            Assert.AreEqual("01Jan", rootNode.Children[0].Title);
            Assert.AreEqual("02Feb", rootNode.Children[1].Title);
            Assert.AreEqual("03Mar", rootNode.Children[2].Title);
            Assert.AreEqual("04Apr", rootNode.Children[3].Title);
            Assert.AreEqual("05May", rootNode.Children[4].Title);
            Assert.AreEqual("06Jun", rootNode.Children[5].Title);
            Assert.AreEqual("07Jul", rootNode.Children[6].Title);
            Assert.AreEqual("08Aug", rootNode.Children[7].Title);
            Assert.AreEqual("09Sep", rootNode.Children[8].Title);
            Assert.AreEqual("10Oct", rootNode.Children[9].Title);
            Assert.AreEqual("11Nov", rootNode.Children[10].Title);
            Assert.AreEqual("12Dec", rootNode.Children[11].Title);

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
