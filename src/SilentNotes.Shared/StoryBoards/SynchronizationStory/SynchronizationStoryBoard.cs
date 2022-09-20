// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// Story for synchronization of the repository with the cloud.
    /// </summary>
    public class SynchronizationStoryBoard : StoryBoardBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationStoryBoard"/> class.
        /// </summary>
        /// <param name="mode">Sets the property <see cref="StoryBoardBase.Mode"/>.</param>
        public SynchronizationStoryBoard(StoryBoardMode mode)
            : base(mode)
        {
            IFeedbackService feedbackService = mode.ShouldShowToasts()
                ? Ioc.GetOrCreate<IFeedbackService>()
                : new DummyFeedbackService();
            INavigationService navigationService = mode.ShouldUseGui()
                ? Ioc.GetOrCreate<INavigationService>()
                : new DummyNavigationService();

            RegisterStep(new IsCloudServiceSetStep(
                SynchronizationStoryStepId.IsCloudServiceSet, this, Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new ShowFirstTimeDialogStep(
                SynchronizationStoryStepId.ShowFirstTimeDialog, this, navigationService));
            RegisterStep(new ShowCloudStorageChoiceStep(
                SynchronizationStoryStepId.ShowCloudStorageChoice, this, navigationService));
            RegisterStep(new ShowCloudStorageAccountStep(
                SynchronizationStoryStepId.ShowCloudStorageAccount,
                this,
                navigationService,
                Ioc.GetOrCreate<INativeBrowserService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new HandleOAuthRedirectStep(
                SynchronizationStoryStepId.HandleOAuthRedirect,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new DownloadCloudRepositoryStep(
                SynchronizationStoryStepId.DownloadCloudRepository,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new ExistsTransferCodeStep(
                SynchronizationStoryStepId.ExistsTransferCode, this, Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new ShowTransferCodeStep(
                SynchronizationStoryStepId.ShowTransferCode,
                this,
                navigationService,
                feedbackService));
            RegisterStep(new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<INoteRepositoryUpdater>()));
            RegisterStep(new IsSameRepositoryStep(
                SynchronizationStoryStepId.IsSameRepository, this, Ioc.GetOrCreate<IRepositoryStorageService>()));
            RegisterStep(new ShowMergeChoiceStep(
                SynchronizationStoryStepId.ShowMergeChoice,
                this,
                navigationService,
                feedbackService));
            RegisterStep(new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreMergedRepositoryAndQuit,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new StoreCloudRepositoryToDeviceAndQuitStep(
                SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<IRepositoryStorageService>()));
            RegisterStep(new StopAndShowRepositoryStep(
                SynchronizationStoryStepId.StopAndShowRepository,
                this,
                feedbackService,
                navigationService,
                Ioc.GetOrCreate<IStoryBoardService>()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationStoryBoard"/> class, and
        /// adopts the session of another story board.
        /// </summary>
        /// <param name="otherStoryBoard">Copy the session variables from this story board.</param>
        public SynchronizationStoryBoard(SynchronizationStoryBoard otherStoryBoard)
            : this(otherStoryBoard.Mode)
        {
            _session = new Dictionary<Enum, object>(otherStoryBoard._session);
        }

        /// <summary>
        /// This exception can be thrown, when a repository has a revision, which is supported only
        /// by more recent applications.
        /// </summary>
        public class UnsuportedRepositoryRevisionException : Exception
        {
        }
    }

    /// <summary>
    /// Enumeration of all available step ids of the <see cref="SynchronizationStoryBoard"/>.
    /// </summary>
    public enum SynchronizationStoryStepId
    {
        IsCloudServiceSet,
        ShowCloudStorageChoice,
        ShowCloudStorageAccount,
        HandleOAuthRedirect,
        ExistsCloudRepository,
        DownloadCloudRepository,
        ExistsTransferCode,
        ShowTransferCode,
        DecryptCloudRepository,
        IsSameRepository,
        ShowMergeChoice,
        StoreLocalRepositoryToCloudAndQuit,
        StoreMergedRepositoryAndQuit,
        StoreCloudRepositoryToDeviceAndQuit,
        StopAndShowRepository,
        ShowFirstTimeDialog,
    }

    /// <summary>
    /// Enumeration of all available session keys of the <see cref="SynchronizationStoryBoard"/>.
    /// </summary>
    public enum SynchronizationStorySessionKey
    {
        CloudStorageCredentials,
        BinaryCloudRepository,
        UserEnteredTransferCode,
        CloudRepository,
        OauthState,
        OauthCodeVerifier,
        OauthRedirectUrl,
    }
}
