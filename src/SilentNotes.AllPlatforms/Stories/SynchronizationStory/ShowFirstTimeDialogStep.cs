// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;

namespace SilentNotes.Stories.SynchronizationStory
{
    internal class ShowFirstTimeDialogStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override ValueTask<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            if (uiMode == StoryMode.Gui)
            {
                var navigation = serviceProvider.GetService<INavigationService>();
                navigation.NavigateTo(Routes.FirstTimeSync);
            }
            return FromResultEndOfStory();
        }
    }
}
