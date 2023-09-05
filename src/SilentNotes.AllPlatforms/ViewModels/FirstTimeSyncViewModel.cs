// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the info dialog to the user.
    /// </summary>
    public class FirstTimeSyncViewModel : ViewModelBase
    {
        //private readonly IStoryBoardService _storyBoardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstTimeSyncViewModel"/> class.
        /// </summary>
        public FirstTimeSyncViewModel()
        {
            ContinueCommand = new RelayCommand(Continue);
        }

        /// <summary>
        /// Gets the command when the user presses the continue button.
        /// </summary>
        public ICommand ContinueCommand { get; private set; }

        private async void Continue()
        {
            await Task.CompletedTask;
            //await (_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageChoice)
            //    ?? Task.CompletedTask);
        }
    }
}
