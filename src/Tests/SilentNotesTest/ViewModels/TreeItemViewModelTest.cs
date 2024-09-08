using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestClass]
    public class TreeItemViewModelTest
    {
        [TestMethod]
        public void EnumerateAnchestorsRecursive_ListsAnchestorsInDecreasingOrder()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);
            var node111 = new TestViewModel("aaa", node11);
            var node12 = new TestViewModel("ab", node1);
            var node2 = new TestViewModel("b", rootNode);

            var anchestors = node11.EnumerateAnchestorsRecursive(false).ToList();
            Assert.AreEqual(2, anchestors.Count);
            Assert.AreEqual(node1, anchestors[0]);
            Assert.AreEqual(rootNode, anchestors[1]);
        }

        [TestMethod]
        public void EnumerateAnchestorsRecursive_ReturnsItself()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);

            var anchestors = node11.EnumerateAnchestorsRecursive(true).ToList();
            Assert.AreEqual(3, anchestors.Count);
            Assert.AreEqual(node11, anchestors[0]);
            Assert.AreEqual(node1, anchestors[1]);
        }

        [TestMethod]
        public void EnumerateSiblingsRecursive_ListsAnchestorsInRecursiveOrder()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);
            var node111 = new TestViewModel("aaa", node11);
            var node12 = new TestViewModel("ab", node1);
            var node2 = new TestViewModel("b", rootNode);

            var siblings = rootNode.EnumerateSiblingsRecursive(false).ToList();
            Assert.AreEqual(5, siblings.Count);
            Assert.AreEqual(node1, siblings[0]);
            Assert.AreEqual(node11, siblings[1]);
            Assert.AreEqual(node111, siblings[2]);
            Assert.AreEqual(node12, siblings[3]);
            Assert.AreEqual(node2, siblings[4]);
        }

        [TestMethod]
        public void EnumerateSiblingsRecursive_ReturnsItself()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);

            var siblings = rootNode.EnumerateSiblingsRecursive(true).ToList();
            Assert.AreEqual(3, siblings.Count);
            Assert.AreEqual(rootNode, siblings[0]);
            Assert.AreEqual(node1, siblings[1]);
        }

        [TestMethod]
        public void IsRoot_WorksCorrectly()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);

            Assert.IsTrue(rootNode.IsRoot());
            Assert.IsFalse(node1.IsRoot());
            Assert.IsFalse(node11.IsRoot());
        }

        [TestMethod]
        public void IsLeaf_WorksCorrectly()
        {
            var rootNode = new TestViewModel("root", null);
            var node1 = new TestViewModel("a", rootNode);
            var node11 = new TestViewModel("aa", node1);

            Assert.IsFalse(rootNode.IsLeaf());
            Assert.IsFalse(node1.IsLeaf());
            Assert.IsTrue(node11.IsLeaf());
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
