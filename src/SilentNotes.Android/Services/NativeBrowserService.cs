// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.Content;
using Android.Net;
using AndroidX.Browser.CustomTabs;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="INativeBrowserService"/> interface for the Android platform.
    /// </summary>
    public class NativeBrowserService : INativeBrowserService
    {
        private readonly Context _applicationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeBrowserService"/> class.
        /// </summary>
        /// <param name="applicationContext">The Android application context.</param>
        public NativeBrowserService(Context applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <inheritdoc/>
        public void OpenWebsite(string url)
        {
            Uri webpage = Uri.Parse(url);
            Intent intent = new Intent(Intent.ActionView, webpage);
            _applicationContext.StartActivity(intent);
        }

        /// <inheritdoc/>
        public void OpenWebsiteInApp(string url)
        {
            // Use the Android custom tabs to display the webpage inside the app.
            CustomTabsIntent.Builder builder = new CustomTabsIntent.Builder();
            CustomTabsIntent customTabsIntent = builder.Build();
            customTabsIntent.LaunchUrl(_applicationContext, Uri.Parse(url));
        }
    }
}