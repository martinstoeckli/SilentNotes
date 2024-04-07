// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.Stories.SynchronizationStory
{
    internal class IsCloudServiceSetStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            System.Diagnostics.Debug.WriteLine("** " + nameof(IsCloudServiceSetStep) + " " + uiMode.ToString());

            var settingsService = serviceProvider.GetService<ISettingsService>();
            SettingsModel settings = settingsService.LoadSettingsOrDefault();

            if (settings.HasCloudStorageClient)
            {
                model.Credentials = settings.Credentials;
                return ToTask(ToResult(new ExistsCloudRepositoryStep()));
            }
            else
            {
                return ToTask(ToResult(new ShowFirstTimeDialogStep()));
            }
        }
    }
}
