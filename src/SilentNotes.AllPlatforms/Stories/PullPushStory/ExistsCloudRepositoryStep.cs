// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.Stories.PullPushStory
{
    /// <summary>
    /// This step belongs to the "PullPushStoryBoard". It checks whether a repository exists in the
    /// cloud storage.
    /// </summary>
    internal class ExistsCloudRepositoryStep : SilentNotes.Stories.SynchronizationStory.ExistsCloudRepositoryStep
    {
        /// <inheritdoc/>
        public override async Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            if (!(model is PullPushStoryModel))
                throw new Exception("Story requires a model of type " + nameof(PullPushStoryModel));

            uiMode = StoryMode.Toasts;
            var settingsService = serviceProvider.GetService<ISettingsService>();
            var languageService = serviceProvider.GetService<ILanguageService>();

            // Instead of reimplementing the whole story, we require a previous manual sync.
            SettingsModel settings = settingsService.LoadSettingsOrDefault();
            if (!settings.HasCloudStorageClient || !settings.HasTransferCode)
            {
                return ToResult(null, languageService["pushpull_error_need_sync_first"], null);
            }

            // Internally reuse SynchronizationStory
            model.Credentials = settings.Credentials;
            var result = await base.RunStep(model, serviceProvider, uiMode);

            // Instead of reimplementing the whole story, we demand a manual sync in case of a problem.
            if (result.NextStep is SilentNotes.Stories.SynchronizationStory.DownloadCloudRepositoryStep)
                return ToResult(new SilentNotes.Stories.PullPushStory.DownloadCloudRepositoryStep(), null, null);
            else
                return ToResult(null, languageService["pushpull_error_need_sync_first"], null);
        }
    }
}
