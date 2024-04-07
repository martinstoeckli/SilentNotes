// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It shows the dialog to
    /// enter the transfer code.
    /// </summary>
    internal class ShowTransferCodeStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            System.Diagnostics.Debug.WriteLine("** " + nameof(ShowTransferCodeStep) + " " + uiMode.ToString());

            if (uiMode.HasFlag(StoryMode.Dialogs))
            {
                var navigation = serviceProvider.GetService<INavigationService>();
                navigation.NavigateTo(RouteNames.TransferCodePrompt);
            }
            return ToTask(ToResultEndOfStory());
        }
    }
}
