// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SilentNotes.Views
{
    /// <summary>
    /// Base class for all razor pages.
    /// </summary>
    public abstract class PageBase: ComponentBase, IDisposable
    {
        private bool _disposed = false;
        private bool _closed = false;
        private DotNetObjectReference<PageBase> _dotnetModule = null;
        private SnKeyboardShortcuts _keyboardShortcuts = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageBase"/> class.
        /// </summary>
        public PageBase()
            :base()
        {
            Debug.WriteLine(string.Format("*** {0}.Create {1}", GetType().Name, Id));

            WeakReferenceMessenger.Default.Register<StoreUnsavedDataMessage>(
                this, (recipient, message) => OnStoringUnsavedData());
            WeakReferenceMessenger.Default.Register<ClosePageMessage>(
                this, (recipient, message) => TriggerOnClosingPage());
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PageBase"/> class.
        /// </summary>
        ~PageBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a unique id of the page instance. It can be used for debugging purposes.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

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
            Debug.WriteLine(string.Format("*** {0}.Dispose {1}", GetType().Name, Id));

            if (!_disposed)
            {
                _disposed = true;
                TriggerOnClosingPage();
                //_dotnetModule?.Dispose();
                OnDisposing();
            }
        }

        /// <summary>
        /// Method invoked when either <see cref="Dispose()"/> or the finalizer is called.
        /// This method is called only once. Consider using <see cref="OnClosingPage"/> instead.
        /// </summary>
        protected virtual void OnDisposing()
        {
            Debug.WriteLine(string.Format("*** {0}.OnDisposing {1}", GetType().Name, Id));
        }

        /// <summary>
        /// Method invoked when the page should store its data. This method can be called repeatedly,
        /// the implementing class should be able to work with consecutive calls and thus be able to
        /// check whether its data is modified or not.
        /// </summary>
        protected virtual void OnStoringUnsavedData()
        {
            Debug.WriteLine(string.Format("*** {0}.OnStoringUnsavedData {1}", GetType().Name, Id));
        }

        private void TriggerOnClosingPage()
        {
            if (_closed)
                return;
            _closed = true;

            // Deconnect message listening
            WeakReferenceMessenger.Default.Unregister<StoreUnsavedDataMessage>(this);
            WeakReferenceMessenger.Default.Unregister<ClosePageMessage>(this);

            _keyboardShortcuts?.Dispose();
            OnClosingPage();
        }

        /// <summary>
        /// Method invoked when the page is to be closed. This method is guaranteed to be called
        /// only once per page. It can e.g. be used to disconnect events.
        /// </summary>
        protected virtual void OnClosingPage()
        {
            Debug.WriteLine(string.Format("*** {0}.OnClosingPage {1}", GetType().Name, Id));
        }

        /// <summary>
        /// Gets or creates a keyboard-shortcut handler. This reference is lazy created and will be
        /// cleaned up automatically.
        /// </summary>
        protected SnKeyboardShortcuts KeyboardShortcuts
        {
            get { return _keyboardShortcuts ?? (_keyboardShortcuts = new SnKeyboardShortcuts()); }
        }

        ///// <summary>
        ///// Gets a reference to the dotnet module of the page, which can be used by JS functions to
        ///// call C# methods. This reference is lazy created and will be cleaned up automatically.
        ///// </summary>
        //protected DotNetObjectReference<PageBase> DotNetModule
        //{
        //    get { return _dotnetModule ?? (_dotnetModule = DotNetObjectReference.Create(this)); }
        //}
    }
}
