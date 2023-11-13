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
        // Always replace the last entry of the WebView history, so that the browser doesn't
        // maintain a history on its own.
        private const bool ReplaceWebviewHistoryAlways = true;

        // Never do a force load, because this will recreate all scoped services and would show the
        // splash screen until the page is reloaded.
        private const bool ForceLoadNever = false;

        private readonly NavigationManager _navigationManager;
        private readonly IBrowserHistoryService _browserHistoryService;
        private readonly string _homeRoute;
        private IDisposable _eventHandlerDisposable;
        private bool _clearHistoryAtNextLocationChanging;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager to wrap.</param>
        /// <param name="browserHistoryService">The browser history service, which is not scoped
        /// like the <paramref name="navigationManager"/> and therefore can keep the browser history
        /// across Android app restarts.</param>
        public NavigationService(NavigationManager navigationManager, IBrowserHistoryService browserHistoryService, string homeRoute = "/")
        {
            System.Diagnostics.Debug.WriteLine("*** Scoped NavigationService create " + Id);
            _navigationManager = navigationManager;
            _browserHistoryService = browserHistoryService;
            _homeRoute = homeRoute;
            _eventHandlerDisposable = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);

            // Initialize the start page in the browser history, because there is no navigation
            // on the startup of the application yet.
            _browserHistoryService.UpdateHistoryOnNavigation(_homeRoute, string.Empty);
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
        public void NavigateTo(string uri, bool removeCurrentFromHistory = false)
        {
            if (removeCurrentFromHistory)
                _browserHistoryService.RemoveCurrent();
            NavigateToOrReload(uri);
        }

        /// <inheritdoc/>
        public bool CanNavigateBack
        {
            get { return !string.IsNullOrEmpty(_browserHistoryService.PreviousLocation); }
        }

        /// <inheritdoc/>
        public void NavigateBack()
        {
            string lastLocation = _browserHistoryService.PreviousLocation;
            if (lastLocation != null)
                NavigateTo(lastLocation);
        }

        /// <inheritdoc/>
        public void NavigateReload()
        {
            string currentLocation = _browserHistoryService.CurrentLocation;
            if (currentLocation != null)
                NavigateTo(currentLocation);
        }

        /// <inheritdoc/>
        public void NavigateHome()
        {
            _clearHistoryAtNextLocationChanging = true;
            NavigateToOrReload(_homeRoute);
        }

        private void NavigateToOrReload(string uri)
        {
            NavigationDirection direction = _browserHistoryService.DetermineDirection(uri, _navigationManager.BaseUri);

            switch (direction)
            {
                case NavigationDirection.Next:
                case NavigationDirection.Back:
                    _navigationManager.NavigateTo(uri, ForceLoadNever, ReplaceWebviewHistoryAlways);
                    break;
                case NavigationDirection.Reload:
                    // Only a "forceReload" would reliably reload the new content of the page.
                    // Since we don't want to use "forceReload" (see const ForceLoadNever), we call
                    // a route which immediately redirects to our target route.
                    string forceLoadRoute = "/forceload/" + System.Web.HttpUtility.UrlEncode(uri);
                    _navigationManager.NavigateTo(forceLoadRoute, ForceLoadNever, ReplaceWebviewHistoryAlways);
                    break;
            }
        }

        /// <inheritdoc/>
        private ValueTask LocationChangingHandler(LocationChangingContext context)
        {
            if (context.TargetLocation.StartsWith("/forceload/"))
                return ValueTask.CompletedTask;

            // Update our own browser history
            NavigationDirection direction = _browserHistoryService.UpdateHistoryOnNavigation(context.TargetLocation, _navigationManager.BaseUri);
            if (_clearHistoryAtNextLocationChanging)
            {
                _clearHistoryAtNextLocationChanging = false;
                _browserHistoryService.Clear(true);
            }

            // Inform current page about being closed, when navigating to another page
            if ((direction == NavigationDirection.Next) || (direction == NavigationDirection.Back))
            {
                WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());
                WeakReferenceMessenger.Default.Send<ClosePageMessage>(new ClosePageMessage());
            }
            return ValueTask.CompletedTask;
        }
    }
}
