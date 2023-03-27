// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            IFeedbackService feedbackService = Ioc.Default.GetService<IFeedbackService>();
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
        /// Executes the parts of the story which can be run silently without UI in a background
        /// service. It can be called when the app is starting up or shutting down. If successful,
        /// the story returns the <see cref="SynchronizationStoryStepId.StopAndShowRepository"/>
        /// step.
        /// </summary>
        /// <remarks>
        /// This allows to execute the synchronization in an Android background service, which can
        /// stay alive a short time longer than the app itself.
        /// </remarks>
        public static async Task<StoryBoardStepResult> RunSilent()
        {
            StoryBoardStepResult result;
            IStoryBoardSession session = new StoryBoardSession();
            ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            ILanguageService languageService = Ioc.Default.GetService<ILanguageService>();
            ICloudStorageClientFactory cloudStorageFactory = Ioc.Default.GetService<ICloudStorageClientFactory>();
            ICryptoRandomService cryptoRandomService = Ioc.Default.GetService<ICryptoRandomService>();
            IRepositoryStorageService repositoryStorageService = Ioc.Default.GetService<IRepositoryStorageService>();
            INoteRepositoryUpdater noteRepositoryUpdater = Ioc.Default.GetService<INoteRepositoryUpdater>();

            // Steps which do not lead to a successful synchronisation without user interaction are ignored
            result = IsCloudServiceSetStep.RunSilent(session, settingsService);
            if (result.NextStepIs(SynchronizationStoryStepId.ExistsCloudRepository))
            {
                result = await ExistsCloudRepositoryStep.RunSilent(StoryBoardMode.Silent, session, settingsService, languageService, cloudStorageFactory);
                if (result.NextStepIs(SynchronizationStoryStepId.DownloadCloudRepository))
                {
                    result = await DownloadCloudRepositoryStep.RunSilent(session, cloudStorageFactory);
                    if (result.NextStepIs(SynchronizationStoryStepId.ExistsTransferCode))
                    {
                        result = ExistsTransferCodeStep.RunSilent(settingsService);
                        if (result.NextStepIs(SynchronizationStoryStepId.DecryptCloudRepository))
                        {
                            result = DecryptCloudRepositoryStep.RunSilent(session, settingsService, languageService, noteRepositoryUpdater);
                            if (result.NextStepIs(SynchronizationStoryStepId.IsSameRepository))
                            {
                                result = IsSameRepositoryStep.RunSilent(session, repositoryStorageService);
                                if (result.NextStepIs(SynchronizationStoryStepId.StoreMergedRepositoryAndQuit))
                                {
                                    result = await StoreMergedRepositoryAndQuitStep.RunSilent(session, settingsService, languageService, cryptoRandomService, repositoryStorageService, cloudStorageFactory);
                                }
                            }
                        }
                    }
                }
                else if (result.NextStepIs(SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit))
                {
                    result = await StoreLocalRepositoryToCloudAndQuitStep.RunSilent(session, settingsService, languageService, cryptoRandomService, repositoryStorageService, cloudStorageFactory);
                }
            }
            return result;
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
