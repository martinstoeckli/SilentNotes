// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.DependencyInjection;
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
                ? Ioc.Default.GetService<IFeedbackService>()
                : new DummyFeedbackService();
            INavigationService navigationService = mode.ShouldUseGui()
                ? Ioc.Default.GetService<INavigationService>()
                : new DummyNavigationService();

            RegisterStep(new IsCloudServiceSetStep(
                SynchronizationStoryStepId.IsCloudServiceSet, this, Ioc.Default.GetService<ISettingsService>()));
            RegisterStep(new ShowFirstTimeDialogStep(
                SynchronizationStoryStepId.ShowFirstTimeDialog, this, navigationService));
            RegisterStep(new ShowCloudStorageChoiceStep(
                SynchronizationStoryStepId.ShowCloudStorageChoice, this, navigationService));
            RegisterStep(new ShowCloudStorageAccountStep(
                SynchronizationStoryStepId.ShowCloudStorageAccount,
                this,
                navigationService,
                Ioc.Default.GetService<INativeBrowserService>(),
                Ioc.Default.GetService<ICryptoRandomService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
            RegisterStep(new HandleOAuthRedirectStep(
                SynchronizationStoryStepId.HandleOAuthRedirect,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                feedbackService,
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
            RegisterStep(new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                feedbackService,
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
            RegisterStep(new DownloadCloudRepositoryStep(
                SynchronizationStoryStepId.DownloadCloudRepository,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                feedbackService,
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
            RegisterStep(new ExistsTransferCodeStep(
                SynchronizationStoryStepId.ExistsTransferCode, this, Ioc.Default.GetService<ISettingsService>()));
            RegisterStep(new ShowTransferCodeStep(
                SynchronizationStoryStepId.ShowTransferCode,
                this,
                navigationService,
                feedbackService));
            RegisterStep(new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                feedbackService,
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<INoteRepositoryUpdater>()));
            RegisterStep(new IsSameRepositoryStep(
                SynchronizationStoryStepId.IsSameRepository, this, Ioc.Default.GetService<IRepositoryStorageService>()));
            RegisterStep(new ShowMergeChoiceStep(
                SynchronizationStoryStepId.ShowMergeChoice,
                this,
                navigationService,
                feedbackService));
            RegisterStep(new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreMergedRepositoryAndQuit,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                feedbackService,
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<ICryptoRandomService>(),
                Ioc.Default.GetService<IRepositoryStorageService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
            RegisterStep(new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                feedbackService,
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<ICryptoRandomService>(),
                Ioc.Default.GetService<IRepositoryStorageService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
            RegisterStep(new StoreCloudRepositoryToDeviceAndQuitStep(
                SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                feedbackService,
                Ioc.Default.GetService<IRepositoryStorageService>()));
            RegisterStep(new StopAndShowRepositoryStep(
                SynchronizationStoryStepId.StopAndShowRepository,
                this,
                feedbackService,
                navigationService,
                Ioc.Default.GetService<IStoryBoardService>()));
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
