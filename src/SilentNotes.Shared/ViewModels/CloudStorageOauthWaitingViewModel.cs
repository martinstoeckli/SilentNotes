// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the OpenAuth2 waiting dialog.
    /// </summary>
    public class CloudStorageOauthWaitingViewModel : ViewModelBase
    {
        private readonly IStoryBoardService _storyBoardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageOauthWaitingViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public CloudStorageOauthWaitingViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService,
            IFeedbackService feedbackService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService;
            feedbackService.ShowBusyIndicator(true);

            GoBackCommand = new RelayCommand(GoBack);
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand GoBackCommand { get; private set; }

        private async void GoBack()
        {
            await (_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageChoice)
                ?? Task.CompletedTask);
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }
    }
}
