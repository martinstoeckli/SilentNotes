// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the cloud storage settings to the user.
    /// </summary>
    public class CloudStorageAccountViewModel : ViewModelBase
    {
        private readonly IStoryBoardService _storyBoardService;
        private readonly CloudStorageCredentialsRequirements _credentialsRequirements;
        private readonly IFeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageAccountViewModel"/> class.
        /// </summary>
        public CloudStorageAccountViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService,
            IFeedbackService feedbackService,
            ICloudStorageClientFactory cloudStorageClientFactory,
            SerializeableCloudStorageCredentials model)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService ?? throw new ArgumentNullException(nameof(storyBoardService));
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
            Model = model;

            _credentialsRequirements = cloudStorageClientFactory.GetByKey(Model.CloudStorageId).CredentialsRequirements;
            CloudServiceName = cloudStorageClientFactory.GetCloudStorageMetadata(Model.CloudStorageId).Title;

            GoBackCommand = new RelayCommand(GoBack);
            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
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

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand CancelCommand { get; private set; }

        private void Cancel()
        {
            _storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository);
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand OkCommand { get; private set; }

        private async void Ok()
        {
            _feedbackService.ShowBusyIndicator(true);
            _storyBoardService.ActiveStory?.StoreToSession(SynchronizationStorySessionKey.CloudStorageCredentials, Model);
            await (_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.ExistsCloudRepository)
                   ?? Task.CompletedTask);
        }

        /// <inheritdoc />
        public string CloudServiceName { get; private set; }

        /// <inheritdoc />
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string Url
        {
            get { return Model.Url; }
            set { SetPropertyAndModified(Model.Url, value, (string v) => Model.Url = v); }
        }

        /// <inheritdoc />
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string Username
        {
            get { return Model.Username; }
            set { SetPropertyAndModified(Model.Username, value, (string v) => Model.Username = v); }
        }

        /// <inheritdoc />
        [VueDataBinding(VueBindingMode.TwoWay)]
        public SecureString Password
        {
            get { return Model.Password; }

            set
            {
                SecureString oldValue = Model.Password;
                if (!SecureStringExtensions.AreEqual(oldValue, value))
                {
                    Model.Password?.Clear();
                    Model.Password = value;
                    OnPropertyChanged(nameof(Password));
                    Modified = true;
                }
            }
        }

        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool Secure
        {
            get { return Model.Secure; }
            set { SetPropertyAndModified(Model.Secure, value, (bool v) => Model.Secure = v); }
        }

        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool AcceptInvalidCertificate
        {
            get { return Model.AcceptInvalidCertificate; }
            set { SetPropertyAndModified(Model.AcceptInvalidCertificate, value, (bool v) => Model.AcceptInvalidCertificate = v); }
        }

        /// <summary>
        /// Gets a value indicating whether a url must be entered.
        /// </summary>
        public bool NeedsUrl
        {
            get { return _credentialsRequirements.HasRequirement(CloudStorageCredentialsRequirements.Url); }
        }

        /// <summary>
        /// Gets a value indicating whether a user name must be entered.
        /// </summary>
        public bool NeedsUsername
        {
            get { return _credentialsRequirements.HasRequirement(CloudStorageCredentialsRequirements.Username); }
        }

        /// <summary>
        /// Gets a value indicating whether a password must be entered.
        /// </summary>
        public bool NeedsPassword
        {
            get { return _credentialsRequirements.HasRequirement(CloudStorageCredentialsRequirements.Password); }
        }

        /// <summary>
        /// Gets a value indicating whether the secure flag must be entered.
        /// </summary>
        public bool NeedsSecureFlag
        {
            get { return _credentialsRequirements.HasRequirement(CloudStorageCredentialsRequirements.Secure); }
        }

        /// <summary>
        /// Gets a value indicating whether unsafe certificates can be accepted.
        /// </summary>
        public bool NeedsAcceptUnsafeCertificate
        {
            get { return _credentialsRequirements.HasRequirement(CloudStorageCredentialsRequirements.AcceptUnsafeCertificate); }
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal SerializeableCloudStorageCredentials Model { get; private set; }
    }
}
