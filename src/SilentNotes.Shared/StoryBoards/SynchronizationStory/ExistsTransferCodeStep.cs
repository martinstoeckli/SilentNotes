// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It checks whether a
    /// transfer code is alredady stored in the settings.
    /// </summary>
    public class ExistsTransferCodeStep : SynchronizationStoryBoardStepBase
    {
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsTransferCodeStep"/> class.
        /// </summary>
        public ExistsTransferCodeStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ISettingsService settingsService)
            : base(stepId, storyBoard)
        {
            _settingsService = settingsService;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            StoryBoardStepResult result = RunSilent(_settingsService);
            if (result.HasNextStep)
                await StoryBoard.ContinueWith(result.NextStepId);
        }

        /// <summary>
        /// Executes the parts of the step which can be run silently without UI in a background service.
        /// </summary>
        public static StoryBoardStepResult RunSilent(
            ISettingsService settingsService)
        {
            SettingsModel settings = settingsService.LoadSettingsOrDefault();

            // Execute step
            if (settings.HasTransferCode)
            {
                return new StoryBoardStepResult(SynchronizationStoryStepId.DecryptCloudRepository);
            }
            else
            {
                return new StoryBoardStepResult(SynchronizationStoryStepId.ShowTransferCode);
            }
        }
    }
}
