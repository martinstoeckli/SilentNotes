// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Android.Webkit;

namespace SilentNotes.Android
{
    /// <summary>
    /// Subclassing the WebViewClient allows to intercept load events and customization of the WebView.
    /// </summary>
    internal class HybridWebViewClient : WebViewClient
    {
        private readonly Action<string> _navigatingAction;
        private readonly Action _navigationCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="HybridWebViewClient"/> class.
        /// </summary>
        /// <param name="navigatingAction">A delegeate which is called when a navigating event occures.</param>
        /// <param name="navigationCompleted">A delegate which is called when a navigation event ends.</param>
        public HybridWebViewClient(Action<string> navigatingAction, Action navigationCompleted)
        {
            _navigatingAction = navigatingAction;
            _navigationCompleted = navigationCompleted;
        }

        /// <inheritdoc/>
        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            // Intercept all links and never do the default loading.
            OnNavigating(request?.Url?.ToString());
            return true;
        }

#pragma warning disable CS0672 // Member overrides obsolete member
        public override bool ShouldOverrideUrlLoading(WebView webView, string url)
#pragma warning restore CS0672
        {
            // Intercept all links and never do the default loading.
            OnNavigating(url);
            return true;
        }

        private void OnNavigating(string url)
        {
            _navigatingAction(url);
        }

        /// <inheritdoc/>
        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            _navigationCompleted();
        }
    }
}
