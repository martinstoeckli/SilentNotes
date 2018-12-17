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
    /// View model to present the available merge possibilities to the user.
    /// </summary>
    public class MergeChoiceViewModel : ViewModelBase
    {
        private readonly IStoryBoardService _storyBoardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeChoiceViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public MergeChoiceViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService)
            : base(navigationService, languageService, svgIconService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService;

            UseMergedRepositoryCommand = new RelayCommand(UseMergedRepository);
            UseLocalRepositoryCommand = new RelayCommand(UseLocalRepository);
            UseCloudRepositoryCommand = new RelayCommand(UseCloudRepository);
            GoBackCommand = new RelayCommand(GoBack);
            CancelCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// Gets the command to merge the repositories.
        /// </summary>
        public ICommand UseMergedRepositoryCommand { get; private set; }

        private void UseMergedRepository()
        {
            _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StoreMergedRepositoryAndQuit.ToInt());
        }

        /// <summary>
        /// Gets the command to use the cloud repository.
        /// </summary>
        public ICommand UseCloudRepositoryCommand { get; private set; }

        private void UseCloudRepository()
        {
            _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit.ToInt());
        }

        /// <summary>
        /// Gets the command to use the local repository.
        /// </summary>
        public ICommand UseLocalRepositoryCommand { get; private set; }

        private void UseLocalRepository()
        {
            _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt());
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand GoBackCommand { get; private set; }

        private void GoBack()
        {
            Cancel();
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        /// <summary>
        /// Gets the command to cancel the synchronization.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        private void Cancel()
        {
            _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository.ToInt());
        }
    }
}
