// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
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
        OauthCloudStorageService,
        OauthState,
        OauthRedirectUrl,
    }

    /// <summary>
    /// Story for synchronization of the repository with the cloud.
    /// </summary>
    public class SynchronizationStoryBoard : StoryBoardBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationStoryBoard"/> class.
        /// </summary>
        /// <param name="mode">Sets the property <see cref="SilentMode"/>.</param>
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
                SynchronizationStoryStepId.IsCloudServiceSet.ToInt(), this, Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new ShowFirstTimeDialogStep(
                SynchronizationStoryStepId.ShowFirstTimeDialog.ToInt(), this, navigationService));
            RegisterStep(new ShowCloudStorageChoiceStep(
                SynchronizationStoryStepId.ShowCloudStorageChoice.ToInt(), this, navigationService));
            RegisterStep(new ShowCloudStorageAccountStep(
                SynchronizationStoryStepId.ShowCloudStorageAccount.ToInt(),
                this,
                navigationService,
                Ioc.GetOrCreate<INativeBrowserService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new HandleOAuthRedirectStep(
                SynchronizationStoryStepId.HandleOAuthRedirect.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                navigationService,
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new DownloadCloudRepositoryStep(
                SynchronizationStoryStepId.DownloadCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new ExistsTransferCodeStep(
                SynchronizationStoryStepId.ExistsTransferCode.ToInt(), this, Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new ShowTransferCodeStep(
                SynchronizationStoryStepId.ShowTransferCode.ToInt(), this, navigationService));
            RegisterStep(new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<INoteRepositoryUpdater>()));
            RegisterStep(new IsSameRepositoryStep(
                SynchronizationStoryStepId.IsSameRepository.ToInt(), this, Ioc.GetOrCreate<IRepositoryStorageService>()));
            RegisterStep(new ShowMergeChoiceStep(
                SynchronizationStoryStepId.ShowMergeChoice.ToInt(), this, navigationService));
            RegisterStep(new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreMergedRepositoryAndQuit.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new StoreCloudRepositoryToDeviceAndQuitStep(
                SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<IRepositoryStorageService>()));
            RegisterStep(new StopAndShowRepositoryStep(
                SynchronizationStoryStepId.StopAndShowRepository.ToInt(), this, navigationService, Ioc.GetOrCreate<IStoryBoardService>()));
        }

        /// <summary>
        /// This exception can be thrown, when a repository has a revision, which is supported only
        /// by more recent applications.
        /// </summary>
        public class UnsuportedRepositoryRevisionException : Exception
        {
        }
    }

    /// <summary>Extension methods for the enumeration.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Extension methods.")]
    public static class SynchronizationStoryStepIdExtensions
    {
        /// <summary>Conversion from enum to int.</summary>
        /// <param name="step">The step.</param>
        /// <returns>Integer of the step.</returns>
        [DebuggerStepThrough]
        public static int ToInt(this SynchronizationStoryStepId step)
        {
            return (int)step;
        }
    }

    /// <summary>Extension methods for the enumeration.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Extension methods.")]
    public static class SynchronisationStorySessionKeyExtensions
    {
        /// <summary>Conversion from enum to int.</summary>
        /// <param name="step">The step.</param>
        /// <returns>Integer of the step.</returns>
        [DebuggerStepThrough]
        public static int ToInt(this SynchronizationStorySessionKey step)
        {
            return (int)step;
        }
    }
}
