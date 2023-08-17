﻿// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using SilentNotes.Views;

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
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NavigationService"/> class.
        /// </summary>
        public void Dispose()
        {
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
        public void NavigateTo(string uri, bool forceLoad = false, bool replace = false)
        {
            _navigationManager.NavigateTo(uri, forceLoad, replace);
        }

        /// <inheritdoc/>
        public void NavigateBack()
        {
            // This navigation will be intercepted in the LocationChangingHandler.
            NavigateTo(RouteBack, false, false);
        }

        /// <inheritdoc/>
        public void Reload()
        {
            _navigationManager.NavigateTo(_navigationManager.Uri, true, true);
        }

        /// <summary>
        /// Intercept navigation events, to implement routes with special meaning.
        /// 1) "/back": This route uses JavaScript to navigate back to the last page, adjusting the browser history.
        /// </summary>
        /// <param name="context">The event arguments.</param>
        /// <returns>A task for async calls.</returns>
        private async ValueTask LocationChangingHandler(LocationChangingContext context)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("*** LocationChangedEvent: '{0}'", context.TargetLocation));
            if (IsRoute(context, RouteBack))
            {
                context.PreventNavigation();
                await _jsRuntime.InvokeVoidAsync("history.back"); // Call javascript to navigate back
            }
            else
            {
                PageBase.InvokeStoreUnsavedData();
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