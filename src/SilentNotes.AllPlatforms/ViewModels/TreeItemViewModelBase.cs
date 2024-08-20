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
#if (DEBUG)
        public string InstanceId { get; } = Guid.NewGuid().ToString();
#endif

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
            CanExpand = true;
            IsExpanded = false;
            Children = new List<ITreeItemViewModel>();
            Parent?.Children.Add(this);
        }

        /// <inheritdoc/>
        public virtual string Title { get; set; }

        /// <inheritdoc/>
        public virtual bool CanExpand { get; set; }

        /// <inheritdoc/>
        public virtual bool IsExpanded { get; set; }

        /// <inheritdoc/>
        public virtual bool IsSelected { get; set; }

        /// <inheritdoc/>
        public ITreeItemViewModel Parent { get; }

        /// <inheritdoc/>
        public List<ITreeItemViewModel> Children { get; set; }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        public TModel Model { get; }

        /// <inheritdoc/>
        public async Task<bool> LazyLoadChildren()
        {
            if (!_areChildrenLoaded)
            {
                _areChildrenLoaded = true;
                await LoadChildren();
                return true;
            }
            return false;
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
    }
}
