// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SilentNotes.Views
{
    /// <summary>
    /// Base class for all razor pages.
    /// </summary>
    public abstract class PageBase: ComponentBase, IAsyncDisposable
    {
        private bool _disposed = false;
        private bool _closed = false;
        private DotNetObjectReference<PageBase> _dotNetModule = null;
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
                this, async (recipient, message) => await TriggerOnClosingPageAsync());
            WeakReferenceMessenger.Default.Register<StateHasChangedMessage>(
                this, (recipient, message) => StateHasChanged());
        }

        /// <summary>
        /// Gets a unique id of the page instance. It can be used for debugging purposes.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            Debug.WriteLine(string.Format("*** {0}.DisposeAsync {1}", GetType().Name, Id));
            if (!_disposed)
            {
                _disposed = true;
                await TriggerOnClosingPageAsync();
                _dotNetModule?.Dispose();
            }
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

        private async ValueTask TriggerOnClosingPageAsync()
        {
            if (_closed)
                return;
            _closed = true;

            // Deconnect message listening
            WeakReferenceMessenger.Default.Unregister<StoreUnsavedDataMessage>(this);
            WeakReferenceMessenger.Default.Unregister<ClosePageMessage>(this);
            WeakReferenceMessenger.Default.Unregister<StateHasChangedMessage>(this);

            _keyboardShortcuts?.Dispose();
            OnClosingPage();
            await OnClosingPageAsync();
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
        /// Method invoked when the page is to be closed. This method is guaranteed to be called
        /// only once per page. It can e.g. be used to disconnect events.
        /// </summary>
        protected virtual ValueTask OnClosingPageAsync()
        {
            Debug.WriteLine(string.Format("*** {0}.OnClosingPageAsync {1}", GetType().Name, Id));
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Gets or creates a keyboard-shortcut handler. This reference is lazy created and will be
        /// cleaned up automatically.
        /// </summary>
        protected SnKeyboardShortcuts KeyboardShortcuts
        {
            get { return _keyboardShortcuts ?? (_keyboardShortcuts = new SnKeyboardShortcuts()); }
        }

        /// <summary>
        /// Gets a reference to the dotnet module of the page, which can be used by JS functions to
        /// call C# methods. This reference is lazy created and will be cleaned up automatically.
        /// </summary>
        protected DotNetObjectReference<PageBase> DotNetModule
        {
            get { return _dotNetModule ?? (_dotNetModule = DotNetObjectReference.Create(this)); }
        }
    }
}
