// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step is an end point of the <see cref="SynchronizationStoryBoard"/>. It keeps the
    /// cloud repository and stores it to the local device.
    /// </summary>
    internal class StoreCloudRepositoryToDeviceAndQuitStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            try
            {
                var repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
                var languageService = serviceProvider.GetService<ILanguageService>();

                repositoryStorageService.TrySaveRepository(model.CloudRepository);
                return ToTask(ToResult(new StopAndShowRepositoryStep(), languageService["sync_success"], null));
            }
            catch (Exception ex)
            {
                if (uiMode.HasFlag(StoryMode.BusyIndicator))
                    serviceProvider.GetService<IFeedbackService>().SetBusyIndicatorVisible(false, true);

                // Keep the current page open and show the error message
                return ToTask(ToResult(ex));
            }
        }
    }
}
