// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SilentNotes.Controllers;
using SilentNotes.Services;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step is a possible entry point to the <see cref="SynchronizationStoryBoard"/>. It
    /// shows the dialog to choose a cloud storage target.
    /// </summary>
    public class ShowCloudStorageChoiceStep : SynchronizationStoryBoardStepBase
    {
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowCloudStorageChoiceStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public ShowCloudStorageChoiceStep(int stepId, IStoryBoard storyBoard, INavigationService navigationService)
            : base(stepId, storyBoard)
        {
            _navigationService = navigationService;
        }

        /// <inheritdoc/>
        public override Task Run()
        {
            if (StoryBoard.Mode.ShouldUseGui())
                _navigationService.Navigate(ControllerNames.CloudStorageChoice);
            return GetCompletedDummyTask();
        }
    }
}
