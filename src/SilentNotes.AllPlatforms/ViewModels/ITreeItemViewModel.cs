// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Describes a view model which can be bound to a treeview.
    /// </summary>
    public interface ITreeItemViewModel
    {
        /// <summary>
        /// Gets the title of the tree node.
        /// The setter is only available because MudBlazor requires it to bind to this property.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node can be expanded or not.
        /// </summary>
        bool CanExpand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is expanded or not.
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is selected or not.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets the parent of the tree node, or null if it is a root node.
        /// </summary>
        public ITreeItemViewModel Parent { get; }

        /// <summary>
        /// Gets a list of child nodes. The list can be empty if no children exist for this node.
        /// The setter is only available because MudBlazor requires it to bind to this property.
        /// </summary>
        public HashSet<ITreeItemViewModel> Children { get; set; }

        /// <summary>
        /// Populates the <see cref="Children"/> list, if not already loaded.
        /// </summary>
        /// <returns>Task for async calls.</returns>
        public Task LazyLoadChildren();
    }

    /// <summary>
    /// Extension methods for <see cref="ITreeItemViewModel"/>.
    /// </summary>
    public static class ITreeItemViewModelExtensions
    {
        /// <summary>
        /// Enumerates all parents of <paramref name="treeItem"/>, beginning with the direct parent
        /// and ending with the root node.
        /// </summary>
        /// <param name="treeItem">The tree node from which to start the search.</param>
		/// <param name="includeMyself">If true the <paramref name="treeItem"/> will be included in the result.</param>
        /// <returns>Enumeration of parents of the node.</returns>
        public static IEnumerable<ITreeItemViewModel> EnumerateAnchestorsRecursive(this ITreeItemViewModel treeItem, bool includeMyself)
        {
            if (treeItem == null)
                yield break;

            if (includeMyself)
                yield return treeItem;

            ITreeItemViewModel parent = treeItem.Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        /// <summary>
        /// Enumerates all direct and indirect children of <paramref name="treeItem"/>.
        /// </summary>
        /// <param name="treeItem">The tree node from which to start the search.</param>
		/// <param name="includeMyself">If true the <paramref name="treeItem"/> will be included in the result.</param>
        /// <returns>Enumeration of children of the node.</returns>
        public static IEnumerable<ITreeItemViewModel> EnumerateSiblingsRecursive(this ITreeItemViewModel treeItem, bool includeMyself)
        {
            if (treeItem == null)
                yield break;

            if (includeMyself)
                yield return treeItem;

            foreach (var child in treeItem.Children)
            {
                yield return child;
                var grandChildren = EnumerateSiblingsRecursive(child, false);
                foreach (var grandChild in grandChildren)
                    yield return grandChild;
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="treeItem"/> is the root (has no parent).
        /// </summary>
        /// <param name="treeItem">The tree node to check.</param>
        /// <returns>Returns true if the ode is a leaf, false if it is a branch.</returns>
        public static bool IsRoot(this ITreeItemViewModel treeItem)
        {
            if (treeItem == null)
                return false;

            return treeItem.Parent == null;
        }

        /// <summary>
        /// Determines whether the <paramref name="treeItem"/> is a leaf (has no children).
        /// </summary>
        /// <param name="treeItem">The tree node to check.</param>
        /// <returns>Returns true if the ode is a leaf, false if it is a branch.</returns>
        public static bool IsLeaf(this ITreeItemViewModel treeItem)
        {
            if (treeItem == null)
                return false;

            return !treeItem.Children.Any();
        }

        /// <summary>
        /// Tries to expands the <paramref name="treeItem"/>, by first lazy loading the children
        /// and afterwards setting the correct expanded state.
        /// </summary>
        /// <param name="treeItem">The tree node to expand.</param>
        /// <returns>Task for async call.</returns>
        public static async Task Expand(this ITreeItemViewModel treeItem)
        {
            if ((treeItem == null) || (treeItem.IsExpanded))
                return;

            await treeItem.LazyLoadChildren();
            treeItem.CanExpand = treeItem.Children.Any();
            treeItem.IsExpanded = true;
        }
    }
}
