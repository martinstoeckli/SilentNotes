// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Logging;

namespace SilentNotes.Views
{
    /// <summary>
    /// Base class for all razor pages.
    /// </summary>
    public abstract class PageBase: ComponentBase, IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageBase"/> class.
        /// </summary>
        public PageBase()
            :base()
        {
            WeakReferenceMessenger.Default.Register<StoreUnsavedDataMessage>(
                this, (recipient, message) => OnStoringUnsavedData());
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PageBase"/> class.
        /// </summary>
        ~PageBase()
        {
            Dispose(false);
        }

        ///// <summary>
        ///// Gets a logger for the page.
        ///// </summary>
        //[Inject]
        //protected ILogger<PageBase> Logger { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Method invoked when either <see cref="Dispose()"/> or the finalizer is called.
        /// </summary>
        /// <param name="disposing">Is true if called by <see cref="Dispose()"/>, false if called
        /// by the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                WeakReferenceMessenger.Default.Unregister<StoreUnsavedDataMessage>(this);
                OnDisposing();
            }
        }

        /// <summary>
        /// Method invoked when either <see cref="Dispose()"/> or the finalizer is called.
        /// This method is called only once.
        /// </summary>
        protected virtual void OnDisposing()
        {
        }

        /// <summary>
        /// Method invoked when the page should store its data. The implementing descendant should
        /// be able to work with consecutive calls and thus be able to check whether its data is
        /// modified.
        /// </summary>
        protected virtual void OnStoringUnsavedData()
        {
            Debug.WriteLine("*** PageBase.OnStoringUnsavedData()");
        }
    }
}
