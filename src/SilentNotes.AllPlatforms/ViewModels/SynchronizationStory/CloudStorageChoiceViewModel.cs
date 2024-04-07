// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the available cloud storages to the user.
    /// </summary>
    public class CloudStorageChoiceViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISynchronizationService _synchronizationService;
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageChoiceViewModel"/> class.
        /// </summary>
        public CloudStorageChoiceViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _synchronizationService = _serviceProvider.GetService<ISynchronizationService>();
            _cloudStorageClientFactory = _serviceProvider.GetService<ICloudStorageClientFactory>();

            ServiceChoices = new List<CloudStorageChoiceItemViewModel>();
            ChooseCommand = new RelayCommand<object>(Choose);

            ListChoices(ServiceChoices);
        }

        /// <summary>
        /// Initializes the list of available cloud storage services.
        /// </summary>
        /// <param name="choices">List to fill.</param>
        private void ListChoices(List<CloudStorageChoiceItemViewModel> choices)
        {
            foreach (string cloudStorageId in _cloudStorageClientFactory.EnumerateCloudStorageIds())
            {
                CloudStorageMetadata metadata = _cloudStorageClientFactory.GetCloudStorageMetadata(cloudStorageId);
                choices.Add(new CloudStorageChoiceItemViewModel
                {
                    CloudStorageId = cloudStorageId,
                    Title = metadata.Title,
                    Icon = metadata.AssetImageName,
                });
            }
        }

        /// <summary>
        /// Gets a list of all available storage services.
        /// </summary>
        public List<CloudStorageChoiceItemViewModel> ServiceChoices { get; private set; }

        /// <summary>
        /// Gets the command when the user selects an item.
        /// </summary>
        public ICommand ChooseCommand { get; private set; }

        private async void Choose(object value)
        {
            SynchronizationStoryModel storyModel = _synchronizationService.ManualSynchronization;
            storyModel.Credentials = new SerializeableCloudStorageCredentials
            {
                CloudStorageId = value.ToString()
            };

            var nextStep = new ShowCloudStorageAccountStep();
            await nextStep.RunStoryAndShowLastFeedback(storyModel, _serviceProvider, storyModel.StoryMode);
        }

        /// <summary>
        /// A single item of the <see cref="ServiceChoices"/> collection.
        /// </summary>
        public class CloudStorageChoiceItemViewModel
        {
            /// <summary>
            /// Gets or sets the id of the cloud storage client.
            /// </summary>
            public string CloudStorageId { get; set; }

            /// <summary>
            /// Gets or sets the title to display.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the filename of the icon without directory.
            /// </summary>
            public string Icon { get; set; }
        }
    }
}
