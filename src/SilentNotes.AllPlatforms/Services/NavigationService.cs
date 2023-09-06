// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implements the <see cref="INavigationService"/> interface by wrapping the <see cref="NavigationManager"/>.
    /// </summary>
    internal class NavigationService: INavigationService, IDisposable
    {
        private const string RouteBack = "back";
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;
        private IDisposable _eventHandlerDisposable;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager to wrap.</param>
        public NavigationService(NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            System.Diagnostics.Debug.WriteLine("*** Scoped NavigationService create " + Id);
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
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
        public void InitializeVirtualRoutes()
        {
            if (_eventHandlerDisposable == null)
                _eventHandlerDisposable = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);
        }

        /// <inheritdoc/>
        public void NavigateTo(string uri, HistoryModification historyModification = HistoryModification.Add)
        {
            switch (historyModification) 
            {
                case HistoryModification.Add:
                    _navigationManager.NavigateTo(uri, false, false);
                    break;
                case HistoryModification.ReplaceLast:
                    _navigationManager.NavigateTo(uri, false, true);
                    break;
            }
        }

        /// <inheritdoc/>
        public async ValueTask NavigateBack()
        {
            await _jsRuntime.InvokeVoidAsync("history.back");
        }

        /// <inheritdoc/>
        public async ValueTask NavigateHome()
        {
            await _jsRuntime.InvokeVoidAsync("navigateHome");
        }

        /// <inheritdoc/>
        public async ValueTask Reload()
        {
            await _jsRuntime.InvokeVoidAsync("history.go");
            //_navigationManager.NavigateTo(_navigationManager.Uri, false, true);
        }

        /// <summary>
        /// Intercept navigation events, to implement routes with special meaning.
        /// 1) "/back": This route uses JavaScript to navigate back to the last page, adjusting the browser history.
        /// </summary>
        /// <param name="context">The event arguments.</param>
        /// <returns>A task for async calls.</returns>
        private async ValueTask LocationChangingHandler(LocationChangingContext context)
        {
            if (IsRoute(context, RouteBack))
            {
                context.PreventNavigation();
                await _jsRuntime.InvokeVoidAsync("history.back"); // Call javascript to navigate back
            }
            else
            {
                // Inform current page before navigating to the next page
                WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());
                WeakReferenceMessenger.Default.Send<ClosePageMessage>(new ClosePageMessage());
            }
        }

        private bool IsRoute(LocationChangingContext context, string route)
        {
            string baseUri = _navigationManager.BaseUri;
            string relativePath = context.TargetLocation;
            if (relativePath.StartsWith(baseUri, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring(baseUri.Length);
            }
            return string.Equals(route, relativePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
