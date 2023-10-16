// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using MudBlazor;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model for the treeview showing the tag filter in the side drawer.
    /// </summary>
    public class TagTreeItemViewModel : TreeItemViewModelBase<string>
    {
        private readonly IReadOnlyList<NoteViewModelReadOnly> _allNotes;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagTreeItemViewModel"/> class.
        /// </summary>
        /// <param name="model">The tag to display.</param>
        /// <param name="parent">The viewmodel of the parent node or null.</param>
        /// <param name="allNotes">A list of all available notes containing all tags.</param>
        public TagTreeItemViewModel(string model, TagTreeItemViewModel parent, IReadOnlyList<NoteViewModelReadOnly> allNotes)
            : base(model, parent)
        {
            _allNotes = allNotes;
        }

        /// <inheritdoc/>
        public override string Title
        {
            get { return Model; }
        }

        /// <inheritdoc/>
        protected override Task LoadChildren()
        {
            HashSet<string> parentTags = new HashSet<string>(
                this.EnumerateAnchestorsRecursive().Select(anchestor => anchestor.Title),
                StringComparer.InvariantCultureIgnoreCase);
            parentTags.Add(Model); // Include our own tag
            parentTags.Remove(null);

            // Find all groups of tags which contain all of the parentTags, and collect the
            // non-parentTags of those groups.
            HashSet<string> relatedTags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var intersectedTags = new List<string>();
            var nonIntersectedTags = new List<string>();
            var allNotesTags = _allNotes.Where(note => !note.InRecyclingBin).Select(note => note.Tags);
            foreach (List<string> noteTags in allNotesTags)
            {
                SplitIntoIntersectedAndNonintersected(noteTags, parentTags, intersectedTags, nonIntersectedTags);
                bool allParentTagsFound = intersectedTags.Count == parentTags.Count;
                if (allParentTagsFound)
                {
                    relatedTags.UnionWith(nonIntersectedTags);
                }
            }

            var result = relatedTags.ToList();
            result.Sort(StringComparer.InvariantCultureIgnoreCase);
            foreach (string relatedTag in result)
                new TagTreeItemViewModel(relatedTag, this, _allNotes);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Takes all items of <paramref name="tags"/> and checks whether they exist in <paramref name="parentTags"/>.
        /// </summary>
        /// <param name="tags">List to go through.</param>
        /// <param name="parentTags">All tags found in this hashset will go to the <paramref name="intersected"/>
        /// list, all others to the <paramref name="nonIntersected"/> list.</param>
        /// <param name="intersected">Found tags which are part of <paramref name="parentTags"/>.</param>
        /// <param name="nonIntersected">Found tags which are not part of <paramref name="parentTags"/>.</param>
        private void SplitIntoIntersectedAndNonintersected(IEnumerable<string> tags, HashSet<string> parentTags, List<string> intersected, List<string> nonIntersected)
        {
            intersected.Clear();
            nonIntersected.Clear();
            foreach (string tag in tags)
            {
                if (parentTags.Contains(tag))
                    intersected.Add(tag);
                else
                    nonIntersected.Add(tag);
            }
        }

        /// <summary>
        /// Clears the list of child nodes and allows to lazy load again.
        /// </summary>
        public void ResetChildren()
        {
            Children.Clear();
            _areChildrenLoaded = false;
        }
    }
}