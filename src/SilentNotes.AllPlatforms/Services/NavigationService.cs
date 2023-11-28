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
        private IDisposable _eventHandlerDisposable;
        private string _currentLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager to wrap.</param>
        /// <param name="startRoute">The route of the first shown page.</param>
        public NavigationService(NavigationManager navigationManager, string startRoute = RouteNames.Home)
        {
            System.Diagnostics.Debug.WriteLine("*** Scoped NavigationService create " + Id);
            _navigationManager = navigationManager;
            _eventHandlerDisposable = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);

            // Initialize the start page in the browser history, because there is no navigation
            // on the startup of the application yet.
            _currentLocation = startRoute;
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
        public void NavigateTo(string uri)
        {
            _navigationManager.NavigateTo(uri, ForceLoadNever, ReplaceWebviewHistoryAlways);
        }

        /// <inheritdoc/>
        public void NavigateReload()
        {
            // Only a "forceReload" would reliably reload the new content of the page.
            // Since we don't want to use "forceReload" (see const ForceLoadNever), we call
            // a route which immediately redirects to our target route.
            string forceLoadRoute = RouteNames.Combine("/forceload", _currentLocation);
            _navigationManager.NavigateTo(forceLoadRoute, ForceLoadNever, ReplaceWebviewHistoryAlways);
        }

        /// <inheritdoc/>
        private ValueTask LocationChangingHandler(LocationChangingContext context)
        {
            if (context.TargetLocation.StartsWith("/forceload/"))
                return ValueTask.CompletedTask;

            string currentRoute = ExtractRouteName(_currentLocation, _navigationManager.BaseUri);
            string targetRoute = ExtractRouteName(context.TargetLocation, _navigationManager.BaseUri);
            bool isSameRoute = string.Equals(currentRoute, targetRoute, StringComparison.InvariantCultureIgnoreCase);

            _currentLocation = context.TargetLocation;

            if (!isSameRoute)
            {
                WeakReferenceMessenger.Default.Send(new StoreUnsavedDataMessage(MessageSender.NavigationManager));
                WeakReferenceMessenger.Default.Send(new ClosePageMessage());
            }
            return ValueTask.CompletedTask;
        }

        internal static string ExtractRouteName(string targetUri, string baseUri)
        {
            if (targetUri == null)
                throw new ArgumentNullException(nameof(targetUri));
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            string result = GetRelativeUri(targetUri, baseUri);
            int firstNonStartingDelimiter = result.IndexOf('/', 1);
            if (firstNonStartingDelimiter > 0)
                result = result.Remove(firstNonStartingDelimiter);
            return result;
        }

        internal static string GetRelativeUri(string targetUri, string baseUri)
        {
            string result = targetUri;
            baseUri = baseUri.TrimEnd('/');
            if (result.StartsWith(baseUri))
                result = result.Substring(baseUri.Length);
            return result;
        }
    }
}
