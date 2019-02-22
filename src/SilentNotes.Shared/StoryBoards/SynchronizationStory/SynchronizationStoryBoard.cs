// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
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
        CloudStorageAccount,
        BinaryCloudRepository,
        UserEnteredTransferCode,
        CloudRepository,
        OauthCloudStorageService,
    }

    /// <summary>
    /// Story for synchronization of the repository with the cloud.
    /// </summary>
    public class SynchronizationStoryBoard : StoryBoardBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationStoryBoard"/> class.
        /// </summary>
        /// <param name="silentMode">Sets the property <see cref="SilentMode"/>.</param>
        public SynchronizationStoryBoard(bool silentMode)
            : base(silentMode)
        {
            IFeedbackService feedbackService = silentMode
                ? new DummyFeedbackService()
                : Ioc.GetOrCreate<IFeedbackService>();

            RegisterStep(new IsCloudServiceSetStep(
                SynchronizationStoryStepId.IsCloudServiceSet.ToInt(), this, Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new ShowFirstTimeDialogStep(
                SynchronizationStoryStepId.ShowFirstTimeDialog.ToInt(), this, Ioc.GetOrCreate<INavigationService>()));
            RegisterStep(new ShowCloudStorageChoiceStep(
                SynchronizationStoryStepId.ShowCloudStorageChoice.ToInt(), this, Ioc.GetOrCreate<INavigationService>()));
            RegisterStep(new ShowCloudStorageAccountStep(
                SynchronizationStoryStepId.ShowCloudStorageAccount.ToInt(),
                this,
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ICloudStorageServiceFactory>()));
            RegisterStep(new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICloudStorageServiceFactory>()));
            RegisterStep(new DownloadCloudRepositoryStep(
                SynchronizationStoryStepId.DownloadCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ICloudStorageServiceFactory>()));
            RegisterStep(new ExistsTransferCodeStep(
                SynchronizationStoryStepId.ExistsTransferCode.ToInt(), this, Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new ShowTransferCodeStep(
                SynchronizationStoryStepId.ShowTransferCode.ToInt(), this, Ioc.GetOrCreate<INavigationService>()));
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
                SynchronizationStoryStepId.ShowMergeChoice.ToInt(), this, Ioc.GetOrCreate<INavigationService>()));
            RegisterStep(new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreMergedRepositoryAndQuit.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageServiceFactory>()));
            RegisterStep(new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageServiceFactory>()));
            RegisterStep(new StoreCloudRepositoryToDeviceAndQuitStep(
                SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                feedbackService,
                Ioc.GetOrCreate<IRepositoryStorageService>()));
            RegisterStep(new StopAndShowRepositoryStep(
                SynchronizationStoryStepId.StopAndShowRepository.ToInt(), this, Ioc.GetOrCreate<INavigationService>(), Ioc.GetOrCreate<IStoryBoardService>()));
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
