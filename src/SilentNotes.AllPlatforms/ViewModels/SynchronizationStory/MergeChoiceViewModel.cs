// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the available merge possibilities to the user.
    /// </summary>
    public class MergeChoiceViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStoryBoardService _storyBoardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeChoiceViewModel"/> class.
        /// </summary>
        public MergeChoiceViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _storyBoardService = serviceProvider.GetService<IStoryBoardService>();

            UseMergedRepositoryCommand = new RelayCommand(UseMergedRepository);
            UseLocalRepositoryCommand = new RelayCommand(UseLocalRepository);
            UseCloudRepositoryCommand = new RelayCommand(UseCloudRepository);
            CancelCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// Gets the command to merge the repositories.
        /// </summary>
        public ICommand UseMergedRepositoryCommand { get; private set; }

        private void UseMergedRepository()
        {

            //_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StoreMergedRepositoryAndQuit);
        }

        /// <summary>
        /// Gets the command to use the cloud repository.
        /// </summary>
        public ICommand UseCloudRepositoryCommand { get; private set; }

        private void UseCloudRepository()
        {
            //_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit);
        }

        /// <summary>
        /// Gets the command to use the local repository.
        /// </summary>
        public ICommand UseLocalRepositoryCommand { get; private set; }

        private void UseLocalRepository()
        {
            //_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit);
        }

        /// <summary>
        /// Gets the command to cancel the synchronization.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        private async void Cancel()
        {
            var nextStep = new StopAndShowRepositoryStep();
            await nextStep.RunStory(_storyBoardService.SynchronizationStory, _serviceProvider, Stories.StoryMode.Gui);
        }
    }
}
