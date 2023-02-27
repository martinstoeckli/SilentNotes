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
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotes.Android
{
    /// <summary>
    /// Intercepts URLs of the scheme "ch.martinstoeckli.silentnotes://...". This scheme is used
    /// to react to OAuth2 answers.
    /// </summary>
    [Activity(Label = "CloudStorageRedirectActivity", NoHistory = true, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "ch.martinstoeckli.silentnotes",
        DataPath = "/oauth2redirect")]
    public class CloudStorageRedirectActivity : Activity
    {
        /// <inheritdoc/>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            string redirectUrl = Intent.Data.ToString();
            IStoryBoardService storyBoardService = new StoryBoardService();
            if (storyBoardService.ActiveStory != null)
                storyBoardService.ActiveStory.Session.Store(SynchronizationStorySessionKey.OauthRedirectUrl, redirectUrl);

            // Clear the activity holding the custom tab and return to already running main activity.
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);

            // Stop the redirect activity, its job is already done.
            Finish();
        }
    }
}