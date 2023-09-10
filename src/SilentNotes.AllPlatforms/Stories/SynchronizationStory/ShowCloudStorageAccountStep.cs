// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using SilentNotes.Services;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It shows the dialog to
    /// enter credentials for the cloud storage. In case of an OAuth2 login, it opens the external
    /// browser and waits for the confirmation of the open auth login.
    /// </summary>
    internal class ShowCloudStorageAccount : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override ValueTask<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            if (uiMode == StoryMode.Gui)
            {
                //var navigation = serviceProvider.GetService<INavigationService>();
                //navigation.NavigateTo(Routes.CloudStorageChoice);
            }
            return FromResult(null);
        }
    }
}
