// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implements the <see cref="INavigationService"/> interface by wrapping the <see cref="NavigationManager"/>.
    /// </summary>
    internal class NavigationService: INavigationService, IDisposable
    {
        private readonly NavigationManager _navigationManager;
        private readonly IBrowserHistoryService _browserHistoryService;
        private IDisposable _eventHandlerDisposable;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager to wrap.</param>
        /// <param name="browserHistoryService">The browser history service, which is not scoped
        /// like the <paramref name="navigationManager"/> and therefore can keep the browser history
        /// across Android app restarts.</param>
        public NavigationService(NavigationManager navigationManager, IBrowserHistoryService browserHistoryService)
        {
            System.Diagnostics.Debug.WriteLine("*** Scoped NavigationService create " + Id);
            _navigationManager = navigationManager;
            _browserHistoryService = browserHistoryService;
            _eventHandlerDisposable = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);
        }

        public Guid Id { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("*** Scoped NavigationService dispose " + Id);
            _eventHandlerDisposable?.Dispose();
            _eventHandlerDisposable = null;
        }

        /// <inheritdoc/>
        public void NavigateTo(string uri, HistoryModification historyModification = HistoryModification.Add)
        {
            if (historyModification == HistoryModification.ReplaceCurrent)
                _browserHistoryService.RemoveCurrent();

            // Always replace the last entry of the WebView history, so that the browser doesn't
            // maintains a history on its own. Instead we will update our own controllable history.
            // This allows to navigate to arbitrary history entries and prevents the Windows back
            // key to interfere with the navigation.
            bool replaceWebviewHistory = true;
            _navigationManager.NavigateTo(uri, false, replaceWebviewHistory);
        }

        /// <inheritdoc/>
        public bool CanNavigateBack
        {
            get { return !string.IsNullOrEmpty(_browserHistoryService.LastLocation); }
        }

        /// <inheritdoc/>
        public void NavigateBack()
        {
            string lastLocation = _browserHistoryService.LastLocation;
            if (lastLocation != null)
                NavigateTo(lastLocation);
        }

        /// <inheritdoc/>
        public void NavigateHome()
        {
            _browserHistoryService.Clear();
            NavigateTo(Routes.NoteRepository);
        }

        /// <inheritdoc/>
        public void Reload()
        {
            bool forceLoad = true;
            _navigationManager.NavigateTo(_browserHistoryService.CurrentLocation, forceLoad, true);
        }

        /// <inheritdoc/>
        private ValueTask LocationChangingHandler(LocationChangingContext context)
        {
            // Inform current page before navigating to the next page
            WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());
            WeakReferenceMessenger.Default.Send<ClosePageMessage>(new ClosePageMessage());

            // Update our own browser history
            _browserHistoryService.UpdateHistoryOnNavigation(context.TargetLocation, _navigationManager.BaseUri);
            return ValueTask.CompletedTask;
        }
    }
}
