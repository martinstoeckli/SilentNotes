// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It downloads the
    /// repository from the cloud storage.
    /// </summary>
    internal class DownloadCloudRepositoryStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override ValueTask<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            // todo:
            return CreateResultTaskEndOfStory();
        }
    }
}
