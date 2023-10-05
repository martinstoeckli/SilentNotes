// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.Services
{
    // todo:
    /// <summary>
    /// base class for implementations of the <see cref="IAutoSynchronizationService"/> interface.
    /// </summary>
    public abstract class AutoSynchronizationServiceBase : IAutoSynchronizationService, IDisposable
    {
        public AutoSynchronizationServiceBase()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("*** Singleton service create " + Id);
#endif
        }

        /// <inheritdoc/>
        public virtual async Task SynchronizeAtStartup()
        {
            IsRunning = true;
            try
            {
                IFeedbackService feedbackService = Ioc.Instance.GetService<IFeedbackService>();
                //    ILanguageService languageService = Ioc.Instance.GetService<ILanguageService>();
                ISettingsService settingsService = Ioc.Instance.GetService<ISettingsService>();
                IInternetStateService internetStateService = Ioc.Instance.GetService<IInternetStateService>();
                //    IRepositoryStorageService repositoryStorageService = Ioc.Instance.GetService<IRepositoryStorageService>();
                //    ICloudStorageClientFactory cloudStorageFactory = Ioc.Instance.GetService<ICloudStorageClientFactory>();
                //    ICryptoRandomService cryptoRandomService = Ioc.Instance.GetService<ICryptoRandomService>();
                //    INoteRepositoryUpdater noteRepositoryUpdater = Ioc.Instance.GetService<INoteRepositoryUpdater>();
                //    INavigationService navigationService = Ioc.Instance.GetService<INavigationService>();

                if (!ShouldSynchronize(internetStateService, settingsService))
                    return;

                //    repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                //    long oldFingerprint = localRepository.GetModificationFingerprint();

                //    // Do the synchronization with the cloud storage in a background thread
                //    StoryBoardStepResult stepResult = await Task.Run(async () =>
                //    {
                //        return await SynchronizationStoryBoard.RunSilent(
                //            settingsService,
                //            languageService,
                //            cloudStorageFactory,
                //            cryptoRandomService,
                //            repositoryStorageService,
                //            noteRepositoryUpdater);
                //    }).ConfigureAwait(true); // Come back to the UI thread

                //    // Memorize fingerprint of the synchronized respository
                //    repositoryStorageService.LoadRepositoryOrDefault(out localRepository);
                //    long newFingerprint = localRepository.GetModificationFingerprint();
                //    LastSynchronizationFingerprint = newFingerprint;

                //    await SynchronizationStoryBoard.ShowFeedback(stepResult, feedbackService, languageService);

                //    // Reload active page, but only if notes are visible and the repository differs
                //    if (oldFingerprint != newFingerprint)
                //    {
                //        navigationService.RepeatNavigationIf(
                //            new[] { ControllerNames.NoteRepository, ControllerNames.Note });
                //    }
            }
            finally
            {
                IsRunning = false;
            }
            await Task.CompletedTask;
        }

#if DEBUG
        public Guid Id { get; } = Guid.NewGuid();
#endif

        /// <inheritdoc/>
        public void Dispose()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("*** Scoped service dispose " + Id);
#endif
        }

        /// <inheritdoc/>
        public virtual async Task SynchronizeAtShutdown()
        {
            System.Diagnostics.Debug.WriteLine("*** AutoSynchronizationService.SynchronizeAtShutdown() " + Id);

            await Task.CompletedTask;
            // Still running from startup?
            if (IsRunning)
                return;

            IsRunning = true;
            try
            {
                ISynchronizationService synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
                ISettingsService settingsService = Ioc.Instance.GetService<ISettingsService>();
                IInternetStateService internetStateService = Ioc.Instance.GetService<IInternetStateService>();
                IRepositoryStorageService repositoryStorageService = Ioc.Instance.GetService<IRepositoryStorageService>();

                if (!ShouldSynchronize(internetStateService, settingsService))
                    return;

                // If there are no modifications since the last synchronization, we can spare this step
                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                long currentFingerprint = localRepository.GetModificationFingerprint();
                if (currentFingerprint == LastSynchronizationFingerprint)
                    return;

                // todo:
                //synchronizationService.CurrentStory = new SynchronizationStoryModel();
                var synchronizationStory = new IsCloudServiceSetStep();
                await synchronizationStory.RunStory(synchronizationService.CurrentStory, Ioc.Instance, StoryMode.Silent);
            }
            finally
            {
                IsRunning = false;
            }
            System.Diagnostics.Debug.WriteLine("*** AutoSynchronizationService.SynchronizeAtShutdown() finished");
        }

        /// <inheritdoc/>
        public virtual void Stop()
        {
            IsRunning = false;
        }

        /// <inheritdoc/>
        public bool IsRunning { get; set; }

        /// <inheritdoc/>
        public long LastSynchronizationFingerprint { get; set; }

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
    }
}
