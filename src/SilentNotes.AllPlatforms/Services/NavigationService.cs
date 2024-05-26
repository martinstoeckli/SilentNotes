﻿// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
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

        private readonly IScopedServiceProvider<NavigationManager> _navigationManagerProvider;
        private IDisposable _eventHandlerDisposable;
        private string _currentLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="navigationManagerProvider">The navigation manager to wrap.</param>
        /// <param name="startRoute">The route of the first shown page.</param>
        public NavigationService(IScopedServiceProvider<NavigationManager> navigationManagerProvider, string startRoute)
        {
            _navigationManagerProvider = navigationManagerProvider;
            _navigationManagerProvider.BeforeRegister += BeforeRegisterEventHandler;

            // Initialize the start page in the browser history, because there is no navigation
            // on the startup of the application yet.
            _currentLocation = startRoute;
        }

        public Guid Id { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public void Dispose()
        {
            _navigationManagerProvider.BeforeRegister -= BeforeRegisterEventHandler;
            _eventHandlerDisposable?.Dispose();
            _eventHandlerDisposable = null;
        }

        private void BeforeRegisterEventHandler(object sender, NavigationManager navigationManager)
        {
            _eventHandlerDisposable?.Dispose();
            _eventHandlerDisposable = navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);
        }

        /// <inheritdoc/>
        public void NavigateTo(string uri, bool reload = false)
        {
            if (!TryGetNavigationManager(out NavigationManager navigationManager))
                return;

            if (reload)
            {
                // Only a "forceReload" would reliably reload the new content of the page.
                // Since we don't want to use "forceReload" (see const ForceLoadNever), we call
                // a route which immediately redirects to our target route.
                uri = "/forceload/" + HexEncode(uri);
            }

            navigationManager.NavigateTo(uri, ForceLoadNever, ReplaceWebviewHistoryAlways);
        }

        /// <inheritdoc/>
        public void NavigateReload()
        {
            NavigateTo(_currentLocation, true);
        }

        private bool TryGetNavigationManager(out NavigationManager navigationManager)
        {
            navigationManager = _navigationManagerProvider.Get();
            return navigationManager != null;
        }

        private static string HexEncode(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToHexString(bytes);
        }

        /// <inheritdoc/>
        private ValueTask LocationChangingHandler(LocationChangingContext context)
        {
            if (context.TargetLocation.StartsWith("/forceload") ||
                !TryGetNavigationManager(out NavigationManager navigationManager))
                return ValueTask.CompletedTask;

            string currentRoute = ExtractRouteName(_currentLocation, navigationManager.BaseUri);
            string targetRoute = ExtractRouteName(context.TargetLocation, navigationManager.BaseUri);
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
