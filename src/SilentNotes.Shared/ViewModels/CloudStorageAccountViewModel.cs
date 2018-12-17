// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Windows.Input;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the cloud storage settings to the user.
    /// </summary>
    public class CloudStorageAccountViewModel : ViewModelBase
    {
        private readonly IStoryBoardService _storyBoardService;
        private readonly IFeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageAccountViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public CloudStorageAccountViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService,
            IFeedbackService feedbackService,
            CloudStorageAccount model)
            : base(navigationService, languageService, svgIconService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService;
            _feedbackService = feedbackService;

            Model = model;
            GoBackCommand = new RelayCommand(GoBack);
            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand GoBackCommand { get; private set; }

        private async void GoBack()
        {
            await _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageChoice.ToInt());
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        private void Cancel()
        {
            _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository.ToInt());
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private async void Ok()
        {
            using (var busyIndicator = _feedbackService.ShowBusyIndicator())
            {
                _storyBoardService.ActiveStory?.StoreToSession(SynchronizationStorySessionKey.CloudStorageAccount.ToInt(), Model);
                await _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.ExistsCloudRepository.ToInt());
            }
        }

        /// <inheritdoc />
        public string CloudServiceName
        {
            get { return CloudStorageTypeExtensions.StorageTypeToString(Model.CloudType); }
        }

        /// <inheritdoc />
        public string Url
        {
            get { return Model.Url; }
            set { ChangePropertyIndirect(() => Model.Url, (string v) => Model.Url = v, value, true); }
        }

        /// <inheritdoc />
        public string Username
        {
            get { return Model.Username; }
            set { ChangePropertyIndirect(() => Model.Username, (string v) => Model.Username = v, value, true); }
        }

        /// <inheritdoc />
        public SecureString Password
        {
            get { return Model.Password; }

            set
            {
                SecureString oldValue = Model.Password;
                if (!SecureStringExtensions.AreEqual(oldValue, value))
                {
                    Model.Password = value;
                    OnPropertyChanged(nameof(Password));
                    Modified = true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the URL is fix, or if it can be edited by the user.
        /// </summary>
        public bool IsUrlFix
        {
            get { return Model.IsUrlReadonly; }
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal CloudStorageAccount Model { get; private set; }
    }
}
