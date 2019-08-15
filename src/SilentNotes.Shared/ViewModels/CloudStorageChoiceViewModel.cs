// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the available cloud storages to the user.
    /// </summary>
    public class CloudStorageChoiceViewModel : ViewModelBase
    {
        private readonly IStoryBoardService _storyBoardService;
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageChoiceViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public CloudStorageChoiceViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(navigationService, languageService, svgIconService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService;
            _cloudStorageClientFactory = cloudStorageClientFactory;

            ServiceChoices = new List<CloudStorageChoiceItemViewModel>();
            GoBackCommand = new RelayCommand(GoBack);
            ChooseCommand = new RelayCommand<string>(Choose);

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
                    Command = ChooseCommand,
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

        private async void Choose(string cloudStorageId)
        {
            SerializeableCloudStorageCredentials credentials = new SerializeableCloudStorageCredentials { CloudStorageId = cloudStorageId };
            _storyBoardService.ActiveStory?.StoreToSession(SynchronizationStorySessionKey.CloudStorageCredentials.ToInt(), credentials);
            await (_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageAccount.ToInt())
                ?? Task.CompletedTask);
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand GoBackCommand { get; private set; }

        private async void GoBack()
        {
            await (_storyBoardService.ActiveStory?.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository.ToInt())
                ?? Task.CompletedTask);
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        /// <summary>
        /// A single item of the <see cref="ServiceChoices"/> collection.
        /// </summary>
        public class CloudStorageChoiceItemViewModel
        {
            /// <summary>
            /// Gets or sets the command which is executed when selecting this item.
            /// </summary>
            public ICommand Command { get; set; }

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
