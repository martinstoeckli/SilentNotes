using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestFixture]
    public class TreeItemViewModelTest
    {
        [Test]
        public void EnumerateAnchestorsRecursive_ListsAnchestorsInDecreasingOrder()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);
            var node111 = new TestViewModel("aaa", node11);
            var node12 = new TestViewModel("ab", node1);
            var node2 = new TestViewModel("b", rootNode);

            var anchestors = node11.EnumerateAnchestorsRecursive().ToList();
            Assert.AreEqual(2, anchestors.Count);
            Assert.AreEqual(node1, anchestors[0]);
            Assert.AreEqual(rootNode, anchestors[1]);
        }

        [Test]
        public void EnumerateSiblingsRecursive_ListsANchestorsInRecursiveOrder()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);
            var node111 = new TestViewModel("aaa", node11);
            var node12 = new TestViewModel("ab", node1);
            var node2 = new TestViewModel("b", rootNode);

            var siblings = rootNode.EnumerateSiblingsRecursive().ToList();
            Assert.AreEqual(5, siblings.Count);
            Assert.AreEqual(node1, siblings[0]);
            Assert.AreEqual(node11, siblings[1]);
            Assert.AreEqual(node111, siblings[2]);
            Assert.AreEqual(node12, siblings[3]);
            Assert.AreEqual(node2, siblings[4]);
        }

        private class TestViewModel : TreeItemViewModelBase<string>
        {
            public TestViewModel(string model, ITreeItemViewModel parent)
                : base(model, parent)
            {
            }
        }
    }
}
