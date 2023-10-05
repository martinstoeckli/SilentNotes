// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to ask the user for a new transfer code.
    /// </summary>
    public class TransferCodePromptViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISynchronizationService _synchronizationService;
        private readonly IFeedbackService _feedbackService;
        private readonly ILanguageService _languageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferCodePromptViewModel"/> class.
        /// </summary>
        public TransferCodePromptViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _synchronizationService = serviceProvider.GetService<ISynchronizationService>();
            _feedbackService = serviceProvider.GetService<IFeedbackService>();
            _languageService = serviceProvider.GetService<ILanguageService>();

            // Initialize commands
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// Gets or sets the transfer code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private async void Ok()
        {
            bool codeIsValid = TransferCode.TrySanitizeUserInput(Code, out string sanitizedCode);
            if (codeIsValid)
            {
                _synchronizationService.CurrentStory.UserEnteredTransferCode = sanitizedCode;
                var nextStep = new DecryptCloudRepositoryStep();
                await nextStep.RunStory(_synchronizationService.CurrentStory, _serviceProvider, _synchronizationService.CurrentStory.StoryMode);
            }
            else
            {
                _feedbackService.ShowToast(_languageService["sync_error_transfercode"]);
            }
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        private async void Cancel()
        {
            var nextStep = new StopAndShowRepositoryStep();
            await nextStep.RunStory(_synchronizationService.CurrentStory, _serviceProvider, _synchronizationService.CurrentStory.StoryMode);
        }
    }
}
