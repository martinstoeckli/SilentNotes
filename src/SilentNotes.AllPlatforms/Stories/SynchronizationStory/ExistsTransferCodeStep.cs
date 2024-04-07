// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It checks whether a
    /// transfer code is alredady stored in the settings.
    /// </summary>
    internal class ExistsTransferCodeStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            System.Diagnostics.Debug.WriteLine("** " + nameof(ExistsTransferCodeStep) + " " + uiMode.ToString());

            var settingsService = serviceProvider.GetService<ISettingsService>();
            SettingsModel settings = settingsService.LoadSettingsOrDefault();

            // Execute step
            if (settings.HasTransferCode)
            {
                return ToTask(ToResult(new DecryptCloudRepositoryStep()));
            }
            else
            {
                return ToTask(ToResult(new ShowTransferCodeStep()));
            }
        }
    }
}
