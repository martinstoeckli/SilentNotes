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
        /// <returns>Enumeration of parents of the node.</returns>
        public static IEnumerable<ITreeItemViewModel> EnumerateAnchestorsRecursive(this ITreeItemViewModel treeItem)
        {
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
        /// <returns>Enumeration of children of the node.</returns>
        public static IEnumerable<ITreeItemViewModel> EnumerateSiblingsRecursive(this ITreeItemViewModel treeItem)
        {
            foreach (var child in treeItem.Children)
            {
                yield return child;
                var grandChildren = EnumerateSiblingsRecursive(child);
                foreach (var grandChild in grandChildren)
                    yield return grandChild;
            }
        }
    }
}
