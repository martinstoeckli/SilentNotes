// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.Stories.PullPushStory
{
    /// <summary>
    /// This step belongs to the "PullPushStoryBoard". It downloads the repository from the cloud storage.
    /// </summary>
    internal class DownloadCloudRepositoryStep : SilentNotes.Stories.SynchronizationStory.DownloadCloudRepositoryStep
    {
        /// <inheritdoc/>
        public override async Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            uiMode = StoryMode.Toasts;
            var languageService = serviceProvider.GetService<ILanguageService>();

            // Reuse SynchronizationStory
            var result = await base.RunStep(model, serviceProvider, uiMode);

            // Instead of reimplementing the whole story, we demand a manual sync in case of a problem.
            if (result.NextStep is (SilentNotes.Stories.SynchronizationStory.ExistsTransferCodeStep))
                return ToResult(new SilentNotes.Stories.PullPushStory.DecryptCloudRepositoryStep(), null, null);
            else
                return ToResult(null, languageService["pushpull_error_need_sync_first"], null);
        }
    }
}
