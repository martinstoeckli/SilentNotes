// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotes.Android
{
    /// <summary>
    /// Intercepts URLs of the scheme "ch.martinstoeckli.silentnotes://...". This scheme is used
    /// to react to OAuth2 answers.
    /// </summary>
    /// <remarks>
    /// The flags NoHistory and LaunchMode are essential, for the app to appear at the top again.
    /// </remarks>
    [Activity(Label = "CloudStorageRedirectActivity", NoHistory = true, LaunchMode = LaunchMode.SingleTask)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "ch.martinstoeckli.silentnotes")]
    public class CloudStorageRedirectActivity : Activity
    {
        /// <inheritdoc/>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Uri redirectUri = new Uri(Intent.Data.ToString()); // Convert Android.Net.Uri to System.Uri

            // Call the storage service
            IStoryBoardService storyBoardService = Ioc.GetOrCreate<IStoryBoardService>();
            IOauth2CloudStorageService oauthStorageService = storyBoardService.ActiveStory?.LoadFromSession<IOauth2CloudStorageService>(
                SynchronizationStorySessionKey.OauthCloudStorageService.ToInt());
            oauthStorageService?.HandleOauth2Redirect(redirectUri);

            // Stop the activity, its job is already done.
            Finish();
        }
    }
}