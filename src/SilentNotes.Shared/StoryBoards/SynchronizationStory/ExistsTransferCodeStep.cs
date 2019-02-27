// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public ExistsTransferCodeStep(
            int stepId,
            IStoryBoard storyBoard,
            ISettingsService settingsService)
            : base(stepId, storyBoard)
        {
            _settingsService = settingsService;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            SettingsModel settings = _settingsService.LoadSettingsOrDefault();

            // Execute step
            if (!string.IsNullOrWhiteSpace(settings.TransferCode))
            {
                await StoryBoard.ContinueWith(SynchronizationStoryStepId.DecryptCloudRepository.ToInt());
            }
            else
            {
                await StoryBoard.ContinueWith(SynchronizationStoryStepId.ShowTransferCode.ToInt());
            }
        }
    }
}
