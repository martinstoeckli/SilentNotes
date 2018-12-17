// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the info dialog to the user.
    /// </summary>
    public class FirstTimeSyncViewModel : ViewModelBase
    {
        private readonly IStoryBoardService _storyBoardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstTimeSyncViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public FirstTimeSyncViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService)
            : base(navigationService, languageService, svgIconService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService;

            GoBackCommand = new RelayCommand(GoBack);
            ContinueCommand = new RelayCommand(Continue);
        }

        /// <summary>
        /// Gets the command when the user presses the continue button.
        /// </summary>
        public ICommand ContinueCommand { get; private set; }

        private async void Continue()
        {
            await _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageChoice.ToInt());
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand GoBackCommand { get; private set; }

        private async void GoBack()
        {
            await _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository.ToInt());
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }
    }
}
