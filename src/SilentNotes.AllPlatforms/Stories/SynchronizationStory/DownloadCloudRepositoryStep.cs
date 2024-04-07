// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It downloads the
    /// repository from the cloud storage.
    /// </summary>
    internal class DownloadCloudRepositoryStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override async Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            System.Diagnostics.Debug.WriteLine("** " + nameof(DownloadCloudRepositoryStep) + " " + uiMode.ToString());

            try
            {
                var cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();

                SerializeableCloudStorageCredentials credentials = model.Credentials;
                ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);

                // The repository can be cached for this story, download the repository only once.
                if (model.BinaryCloudRepository == null)
                {
                    model.BinaryCloudRepository = await cloudStorageClient.DownloadFileAsync(NoteRepositoryModel.RepositoryFileName, credentials);
                }
                return ToResult(new ExistsTransferCodeStep());
            }
            catch (Exception ex)
            {
                if (uiMode.HasFlag(StoryMode.BusyIndicator))
                    serviceProvider.GetService<IFeedbackService>().SetBusyIndicatorVisible(false, true);

                // Keep the current page open and show the error message
                return ToResult(ex);
            }
        }
    }
}
