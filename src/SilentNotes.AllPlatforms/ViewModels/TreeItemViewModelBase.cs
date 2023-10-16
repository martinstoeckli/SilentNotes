// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Base class for nodes of a MudTreeView which implements the <see cref="ITreeItemViewModel"/>
    /// interface.
    /// </summary>
    public abstract class TreeItemViewModelBase<TModel>: ITreeItemViewModel
    {
        protected bool _areChildrenLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemViewModelBase{TModel}"/> class.
        /// </summary>
        /// <param name="model">The model to wrap.</param>
        /// <param name="parent">The viewmodel of the parent node or null. If the parent is not
        /// null, the new node will be added to the children of the parent node.</param>
        public TreeItemViewModelBase(TModel model, ITreeItemViewModel parent)
        {
            Model = model;
            Parent = parent;
            Children = new HashSet<ITreeItemViewModel>();

            if (Parent != null)
                Parent.Children.Add(this);
        }

        /// <inheritdoc/>
        public virtual string Title { get; set; }

        /// <inheritdoc/>
        public virtual bool IsExpanded { get; set; }

        /// <inheritdoc/>
        public virtual bool IsSelected { get; set; }

        /// <inheritdoc/>
        public ITreeItemViewModel Parent { get; }

        /// <inheritdoc/>
        public HashSet<ITreeItemViewModel> Children { get; set; }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        public TModel Model { get; }

        /// <inheritdoc/>
        public async Task LazyLoadChildren()
        {
            if (!_areChildrenLoaded)
            {
                _areChildrenLoaded = true;
                await LoadChildren();
            }
        }

        /// <summary>
        /// Populates the <see cref="Children"/> list. Overwrite this method if the data should be
        /// loaded on demand, it is called by <see cref="LazyLoadChildren"/> only when necessary.
        /// </summary>
        /// <returns></returns>
        protected virtual Task LoadChildren()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Bind this function to <see cref="MudTreeView.ServerData"/> if the data should be loaded
        /// on demand. It will call <see cref="LazyLoadChildren"/> to collect the expanded data.
        /// </summary>
        /// <param name="parentNode">The node to expand.</param>
        /// <returns>A hash set of child nodes.</returns>
        public static async Task<HashSet<ITreeItemViewModel>> LoadData(ITreeItemViewModel parentNode)
        {
            await parentNode.LazyLoadChildren();
            return parentNode.Children;
        }
    }
}
