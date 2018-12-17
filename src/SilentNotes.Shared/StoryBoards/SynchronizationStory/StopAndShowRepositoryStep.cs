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
    /// This step is an end point of the <see cref="SynchronizationStoryBoard"/>. It aborts the
    /// story and goes back to the repository overview.
    /// </summary>
    public class StopAndShowRepositoryStep : StoryBoardStepBase
    {
        private readonly INavigationService _navigationService;
        private readonly IStoryBoardService _storyBoardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StopAndShowRepositoryStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public StopAndShowRepositoryStep(int stepId, IStoryBoard storyBoard, INavigationService navigationService, IStoryBoardService storyBoardService)
            : base(stepId, storyBoard)
        {
            _navigationService = navigationService;
            _storyBoardService = storyBoardService;
        }

        /// <inheritdoc/>
        public override Task Run()
        {
            StoryBoard.ClearSession();
            _storyBoardService.ActiveStory = null;
            _navigationService.Navigate(ControllerNames.NoteRepository);
            return GetCompletedDummyTask();
        }
    }
}
