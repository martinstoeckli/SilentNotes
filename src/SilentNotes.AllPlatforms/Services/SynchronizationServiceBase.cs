// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.Services
{
    /// <summary>
    /// Base class of all implementations of the <see cref="ISynchronizationService"/> interface.
    /// </summary>
    internal abstract class SynchronizationServiceBase : ISynchronizationService
    {
        protected readonly ISynchronizationState _synchronizationState;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationServiceBase"/> class.
        /// </summary>
        /// <param name="synchronizationState">A singleton storing the current state of the synchronization.</param>
        public SynchronizationServiceBase(ISynchronizationState synchronizationState)
        {
            _synchronizationState = synchronizationState;
        }

        /// <inheritdoc/>
        public async Task SynchronizeManually(IServiceProvider serviceProvider)
        {
            if (!_synchronizationState.TryStartSynchronizationState(SynchronizationType.Manually))
                return;

            try
            {
                // Ignore last fingerprint optimization, because it is triggered by the user
                LastSynchronizationFingerprint = 0;
                ManualSynchronization = new SynchronizationStoryModel
                {
                    StoryMode = StoryMode.BusyIndicator | StoryMode.Toasts | StoryMode.Messages | StoryMode.Dialogs,
                };

                var synchronizationStory = new IsCloudServiceSetStep();
                await synchronizationStory.RunStoryAndShowLastFeedback(ManualSynchronization, serviceProvider, ManualSynchronization.StoryMode);
            }
            finally
            {
                // Either the manually triggered sync finished or it shows a user dialog for input.
                // In both cases we want to remove the visual "IsRunning" indicator, and from a
                // user dialog no other synchronizations can be startet.
                _synchronizationState.StopSynchronizationState();
            }
        }

        /// <inheritdoc/>
        public async Task SynchronizeManuallyChangeCloudStorage(IServiceProvider serviceProvider)
        {
            if (!_synchronizationState.TryStartSynchronizationState(SynchronizationType.Manually))
                return;

            try
            {
                // Always start a new story, ignore last fingerprint, because it is triggered by the user
                LastSynchronizationFingerprint = 0;
                ManualSynchronization = new SynchronizationStoryModel
                {
                    StoryMode = StoryMode.BusyIndicator | StoryMode.Toasts | StoryMode.Messages | StoryMode.Dialogs,
                };

                var synchronizationStory = new ShowCloudStorageChoiceStep();
                await synchronizationStory.RunStoryAndShowLastFeedback(ManualSynchronization, serviceProvider, ManualSynchronization.StoryMode);
            }
            finally
            {
                // Either the manually triggered sync finished or it shows a user dialog for input.
                // In both cases we want to remove the visual "IsRunning" indicator, and from a
                // user dialog no other synchronizations can be startet.
                _synchronizationState.StopSynchronizationState();
            }
        }

        /// <inheritdoc/>
        public void FinishedManualSynchronization(IServiceProvider serviceProvider)
        {
            if (ManualSynchronization == null)
                return;
            ManualSynchronization = null;
            _synchronizationState.UpdateLastFinishedSynchronization();

            var repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
            if (repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel repositoryModel) == RepositoryStorageLoadResult.SuccessfullyLoaded)
                LastSynchronizationFingerprint = repositoryModel.GetModificationFingerprint();
        }

        /// <inheritdoc/>
        public virtual async Task AutoSynchronizeAtStartup(IServiceProvider serviceProvider)
        {
            System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtStartup()");
            if (IsWaitingForOAuthRedirect)
                return;

            if (!_synchronizationState.TryStartSynchronizationState(SynchronizationType.AtStartup))
                return;
            try
            {
                ILanguageService languageService = serviceProvider.GetService<ILanguageService>();
                ISettingsService settingsService = serviceProvider.GetService<ISettingsService>();
                IInternetStateService internetStateService = serviceProvider.GetService<IInternetStateService>();
                IRepositoryStorageService repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
                ICloudStorageClientFactory cloudStorageFactory = serviceProvider.GetService<ICloudStorageClientFactory>();
                ICryptoRandomService cryptoRandomService = serviceProvider.GetService<ICryptoRandomService>();
                INoteRepositoryUpdater noteRepositoryUpdater = serviceProvider.GetService<INoteRepositoryUpdater>();
                INavigationService navigation = serviceProvider.GetService<INavigationService>();

                // Check whether the synchronization should be done at all
                if (repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository) != RepositoryStorageLoadResult.SuccessfullyLoaded)
                    return;
                if (!ShouldSynchronize(internetStateService, settingsService))
                    return;

                System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtStartup() start");

                // Do the synchronization with the cloud storage in a background thread
                long oldFingerprint = localRepository.GetModificationFingerprint();
                var stepResult = await Task.Run(async () =>
                {
                    return await RunSilent(settingsService, languageService, cloudStorageFactory, cryptoRandomService, repositoryStorageService, noteRepositoryUpdater);
                }).ConfigureAwait(true); // Come back to the UI thread

                // Memorize fingerprint of the synchronized respository
                if (stepResult != null)
                {
                    repositoryStorageService.LoadRepositoryOrDefault(out localRepository);
                    long newFingerprint = localRepository.GetModificationFingerprint();
                    LastSynchronizationFingerprint = newFingerprint;
                    _synchronizationState.UpdateLastFinishedSynchronization();

                    if (stepResult.HasFeedback)
                        await new IsCloudServiceSetStep().ShowFeedback(stepResult, serviceProvider, StoryMode.Toasts | StoryMode.Messages);

                    // Reload active page, but only if the repository differs
                    if (oldFingerprint != newFingerprint)
                    {
                        navigation.NavigateReload();
                    }
                }

                System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtStartup() end");
            }
            finally
            {
                _synchronizationState.StopSynchronizationState();
            }
        }

        /// <inheritdoc/>
        public abstract Task AutoSynchronizeAtShutdown(IServiceProvider serviceProvider);

        /// <inheritdoc/>
        public abstract void StopAutoSynchronization(IServiceProvider serviceProvider);

        /// <inheritdoc/>
        public SynchronizationStoryModel ManualSynchronization { get; set; }

        /// <inheritdoc/>
        public bool IsWaitingForOAuthRedirect { get; set; }

        /// <summary>
        /// Gets or sets a fingerprint of the last synchronization, which allows to detect whether
        /// the local repository has changed since the last synchronization. The value can be
        /// updated/overwritten when the user does a manual synchronization and used to determine
        /// whether a synchronization at shutdown is necessary at all.
        /// </summary>
        protected long LastSynchronizationFingerprint { get; set; }

        /// <summary>
        /// Checks whether the synchronization should and can be done or not.
        /// </summary>
        /// <param name="internetStateService">An internet state service.</param>
        /// <param name="settingsService">A settings service.</param>
        /// <returns>Returns true if the synchronization should be done, otherwise false.</returns>
        protected static bool ShouldSynchronize(IInternetStateService internetStateService, ISettingsService settingsService)
        {
            if (!internetStateService.IsInternetConnected())
                return false;

            AutoSynchronizationMode syncMode = settingsService.LoadSettingsOrDefault().AutoSyncMode;
            switch (syncMode)
            {
                case AutoSynchronizationMode.Never:
                    return false;
                case AutoSynchronizationMode.CostFreeInternetOnly:
                    return internetStateService.IsInternetCostFree();
                case AutoSynchronizationMode.Always:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(syncMode));
            }
        }

        /// <summary>
        /// Executes the parts of the story which can be run silently without UI in a background
        /// service. It can be called when the app is starting up or shutting down. If successful,
        /// the story returns the result of the last step.
        /// </summary>
        /// <remarks>
        /// This allows to execute the synchronization in an Android background service, which can
        /// stay alive a short time longer than the app itself.
        /// </remarks>
        protected static async Task<StoryStepResult<SynchronizationStoryModel>> RunSilent(
            ISettingsService settingsService,
            ILanguageService languageService,
            ICloudStorageClientFactory cloudStorageFactory,
            ICryptoRandomService cryptoRandomService,
            IRepositoryStorageService repositoryStorageService,
            INoteRepositoryUpdater noteRepositoryUpdater)
        {
            StoryStepResult<SynchronizationStoryModel> result;
            var model = new SynchronizationStoryModel { StoryMode = StoryMode.Silent };

            // Create a service collection which is independend of the running app, so the story
            // can run in a background task
            var services = new ServiceCollection();
            services.AddSingleton(settingsService);
            services.AddSingleton(languageService);
            services.AddSingleton(cloudStorageFactory);
            services.AddSingleton(cryptoRandomService);
            services.AddSingleton(repositoryStorageService);
            services.AddSingleton(noteRepositoryUpdater);
            services.AddSingleton((IFeedbackService)(new DummyFeedbackService()));
            services.AddSingleton((INavigationService)(new DummyNavigationService()));
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Steps which do not lead to a successful synchronisation without user interaction are ignored
            var startStep = new IsCloudServiceSetStep();
            result = await startStep.RunStep(model, serviceProvider, model.StoryMode);
            if (result.NextStep is ExistsCloudRepositoryStep)
            {
                result = await result.NextStep.RunStep(model, serviceProvider, model.StoryMode);
                if (result.NextStep is DownloadCloudRepositoryStep)
                {
                    result = await result.NextStep.RunStep(model, serviceProvider, model.StoryMode);
                    if (result.NextStep is ExistsTransferCodeStep)
                    {
                        result = await result.NextStep.RunStep(model, serviceProvider, model.StoryMode);
                        if (result.NextStep is DecryptCloudRepositoryStep)
                        {
                            result = await result.NextStep.RunStep(model, serviceProvider, model.StoryMode);
                            if (result.NextStep is IsSameRepositoryStep)
                            {
                                result = await result.NextStep.RunStep(model, serviceProvider, model.StoryMode);
                                if (result.NextStep is StoreMergedRepositoryAndQuitStep)
                                {
                                    result = await result.NextStep.RunStep(model, serviceProvider, model.StoryMode);
                                    return result;
                                }
                            }
                        }
                    }
                }
                else if (result.NextStep is StoreLocalRepositoryToCloudAndQuitStep)
                {
                    result = await result.NextStep.RunStep(model, serviceProvider, model.StoryMode);
                    return result;
                }
            }
            return null;
        }
    }
}
