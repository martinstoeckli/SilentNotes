using System;
using System.Collections.Generic;
using MudBlazor;
using SilentNotes.ViewModels;

namespace SilentNotes.Views
{
    /// <summary>
    /// Wrapper class around the <see cref="ITreeItemViewModel"/> interface.
    /// Using this class allows to keep the MudBlazor <see cref="TreeItemData{T}"/> class in the
    /// view only, the viewmodel can work with the interface without dependencies. The TreeView
    /// object of MudBlazor requires to use the <see cref="TreeItemData{T}"/> class to work.
    /// </summary>
    public class MudTreeItemWrapper : TreeItemData<ITreeItemViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MudTreeItemWrapper"/> class.
        /// </summary>
        /// <param name="viewModel">The viewmodel to wrap.</param>
        /// <param name="parent">Sets the <see cref="Parent"/> property.</param>
        public MudTreeItemWrapper(ITreeItemViewModel viewModel, MudTreeItemWrapper parent)
            : base(viewModel)
        {
            Parent = parent;
            MirrorChildren();
        }

#if (DEBUG)
        string InstanceId => Value.InstanceId;
#endif

        /// <inheritdoc/>
        public override string Text
        {
            get { return Value.Title; }
            set { Value.Title = value; }
        }

        /// <summary>
        /// Gets the parent node, or null if it is a root node.
        /// </summary>
        public MudTreeItemWrapper Parent { get; }

        /// <inheritdoc/>
        public override IReadOnlyCollection<ITreeItemData<ITreeItemViewModel>> Children
        {
            get { return base.Children ?? (base.Children = new List<TreeItemData<ITreeItemViewModel>>()); }
            set { base.Children = value; }
        }

        /// <inheritdoc/>
        public override bool Expandable
        {
            get { return Value.CanExpand; }
            set { Value.CanExpand = value; }
        }

        /// <inheritdoc/>
        public override bool Expanded
        {
            get { return Value.IsExpanded; }
            set { Value.IsExpanded = value; }
        }

        /// <inheritdoc/>
        public override bool Selected
        {
            get { return Value.IsSelected; }
            set { Value.IsSelected = value; }
        }

        /// <summary>
        /// Searches the tree for a given wrapped <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The wrapped object to search for.</param>
        /// <param name="recursive">If false only direct children are searched, if true all
        /// descendants are searched</param>
        /// <returns>Found object or null if no such object could be found.</returns>
        public MudTreeItemWrapper FindChildByValue(ITreeItemViewModel value, bool recursive)
        {
            // Ask in base class without auto creating the list
            if (base.Children == null)
                return null;

            // Search in direct children
            ITreeItemData<ITreeItemViewModel> foundChild = Children.FirstOrDefault(child => Object.ReferenceEquals(child.Value, value));

            // Search recursive
            int childIndex = 0;
            while ((foundChild == null) && recursive && (childIndex < Children.Count))
            {
                var child = (MudTreeItemWrapper)Children.ElementAt(childIndex);
                foundChild = child.FindChildByValue(value, recursive);
                childIndex++;
            }
            return foundChild as MudTreeItemWrapper;
        }

        /// <summary>
        /// Populates the <see cref="Children"/> list, if not already loaded.
        /// </summary>
        /// <returns>Returns true if childrens had to be loaded, false when they where already loaded.</returns>
        public async Task<bool> LazyLoadChildren()
        {
            bool didLoad = await Value.LazyLoadChildren();
            MirrorChildren();
            return didLoad;
        }

        /// <summary>
        /// Mirrors the child list with the child list of the wrapped object.
        /// </summary>
        public void MirrorChildren()
        {
            Children = Value.Children.Select(child => new MudTreeItemWrapper(child, this)).ToList();
        }
    }
}
