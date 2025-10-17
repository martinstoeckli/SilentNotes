// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Android.Content;
using Android.Text;
using SilentNotes.Services;
using Uri = Android.Net.Uri;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="ISharingService"/> interface for the Android platform.
    /// </summary>
    internal class SharingService : ISharingService
    {
        private readonly IAppContextService _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ISharingService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        public SharingService(IAppContextService appContextService)
        {
            _appContext = appContextService;
        }

        /// <inheritdoc/>
        public Task ShareHtmlText(string htmlText, string plainText)
        {
            Intent shareIntent = new Intent(Intent.ActionSend);
            shareIntent.SetType("text/html");
            shareIntent.PutExtra(Intent.ExtraSubject, "Note from SilentNotes");
            shareIntent.PutExtra(Intent.ExtraText, plainText);
            shareIntent.PutExtra(Intent.ExtraHtmlText, htmlText);

            // Following statement may work for some receiving apps, but in an email client, tags
            // like lists are removed completely:
            // shareIntent.PutExtra(Intent.ExtraText, Html.FromHtml(htmlText, FromHtmlOptions.ModeLegacy));

            Intent chooserIntent = Intent.CreateChooser(shareIntent, "Share");
            chooserIntent.AddFlags(ActivityFlags.NewTask);
            _appContext.RootActivity.StartActivity(chooserIntent);
            return Task.CompletedTask;
        }
    }
}
