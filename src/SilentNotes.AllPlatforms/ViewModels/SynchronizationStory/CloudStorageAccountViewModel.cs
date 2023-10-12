// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the cloud storage settings to the user.
    /// </summary>
    public class CloudStorageAccountViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISynchronizationService _synchronizationService;
        private readonly CloudStorageCredentialsRequirements _credentialsRequirements;
        private readonly IFeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageAccountViewModel"/> class.
        /// </summary>
        public CloudStorageAccountViewModel(
            IServiceProvider serviceProvider,
            SerializeableCloudStorageCredentials model)
        {
            _serviceProvider = serviceProvider;
            _synchronizationService = serviceProvider.GetService<ISynchronizationService>();
            _feedbackService = serviceProvider.GetRequiredService<IFeedbackService>();

            Model = model;

            ICloudStorageClientFactory cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();
            _credentialsRequirements = cloudStorageClientFactory.GetByKey(Model.CloudStorageId).CredentialsRequirements;
            CloudServiceName = cloudStorageClientFactory.GetCloudStorageMetadata(Model.CloudStorageId).Title;

            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private async void Ok()
        {
            _feedbackService.SetBusyIndicatorVisible(true, true);
            SynchronizationStoryModel storyModel = _synchronizationService.ManualSynchronization;
            var nextStep = new ExistsCloudRepositoryStep();
            await nextStep.RunStory(storyModel, _serviceProvider, storyModel.StoryMode);
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        private async void Cancel()
        {
            var nextStep = new StopAndShowRepositoryStep();
            await nextStep.RunStory(_synchronizationService.ManualSynchronization, _serviceProvider, _synchronizationService.ManualSynchronization.StoryMode);
        }

        /// <inheritdoc />
        public string CloudServiceName { get; private set; }

        /// <inheritdoc />
        public string Url
        {
            get { return Model.Url; }
            set { SetPropertyAndModified(Model.Url, value, (string v) => Model.Url = v); }
        }

        /// <inheritdoc />
        public string Username
        {
            get { return Model.Username; }
            set { SetPropertyAndModified(Model.Username, value, (string v) => Model.Username = v); }
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
                    Model.Password?.Clear();
                    Model.Password = value;
                    OnPropertyChanged(nameof(Password));
                    Modified = true;
                }
            }
        }

        public bool Secure
        {
            get { return Model.Secure; }
            set { SetPropertyAndModified(Model.Secure, value, (bool v) => Model.Secure = v); }
        }

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
