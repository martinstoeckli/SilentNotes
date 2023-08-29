// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
//using SilentNotes.Controllers;
//using SilentNotes.Models;
//using SilentNotes.StoryBoards;
//using SilentNotes.StoryBoards.SynchronizationStory;
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
            await Task.CompletedTask;
            //IsRunning = true;
            //try
            //{
            //    IFeedbackService feedbackService = Ioc.Default.GetService<IFeedbackService>();
            //    ILanguageService languageService = Ioc.Default.GetService<ILanguageService>();
            //    ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            //    IInternetStateService internetStateService = Ioc.Default.GetService<IInternetStateService>();
            //    IRepositoryStorageService repositoryStorageService = Ioc.Default.GetService<IRepositoryStorageService>();
            //    ICloudStorageClientFactory cloudStorageFactory = Ioc.Default.GetService<ICloudStorageClientFactory>();
            //    ICryptoRandomService cryptoRandomService = Ioc.Default.GetService<ICryptoRandomService>();
            //    INoteRepositoryUpdater noteRepositoryUpdater = Ioc.Default.GetService<INoteRepositoryUpdater>();
            //    INavigationService navigationService = Ioc.Default.GetService<INavigationService>();

            //    if (!ShouldSynchronize(internetStateService, settingsService))
            //        return;

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
            //}
            //finally
            //{
            //    IsRunning = false;
            //}
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
#if DEBUG
            System.Diagnostics.Debug.WriteLine("*** AutoSynchronizationService.SynchronizeAtShutdown() " + Id);
#endif
            await Task.CompletedTask;
            //// Still running from startup?
            //if (IsRunning)
            //    return;

            //IsRunning = true;
            //try
            //{
            //    ILanguageService languageService = Ioc.Default.GetService<ILanguageService>();
            //    ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            //    IInternetStateService internetStateService = Ioc.Default.GetService<IInternetStateService>();
            //    IRepositoryStorageService repositoryStorageService = Ioc.Default.GetService<IRepositoryStorageService>();
            //    ICloudStorageClientFactory cloudStorageFactory = Ioc.Default.GetService<ICloudStorageClientFactory>();
            //    ICryptoRandomService cryptoRandomService = Ioc.Default.GetService<ICryptoRandomService>();
            //    INoteRepositoryUpdater noteRepositoryUpdater = Ioc.Default.GetService<INoteRepositoryUpdater>();

            //    if (!ShouldSynchronize(internetStateService, settingsService))
            //        return;

            //    // If there are no modifications since the last synchronization, we can spare this step
            //    repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
            //    long currentFingerprint = localRepository.GetModificationFingerprint();
            //    if (currentFingerprint == LastSynchronizationFingerprint)
            //        return;

            //    StoryBoardStepResult stepResult = await SynchronizationStoryBoard.RunSilent(
            //        settingsService,
            //        languageService,
            //        cloudStorageFactory,
            //        cryptoRandomService,
            //        repositoryStorageService,
            //        noteRepositoryUpdater);
            //}
            //finally
            //{
            //    IsRunning = false;
            //}
#if DEBUG
            System.Diagnostics.Debug.WriteLine("*** AutoSynchronizationService.SynchronizeAtShutdown() finished");
#endif
        }

        /// <inheritdoc/>
        public abstract void Stop();

        /// <inheritdoc/>
        public bool IsRunning { get; set; }

        /// <inheritdoc/>
        public long LastSynchronizationFingerprint { get; set; }

        ///// <summary>
        ///// Checks whether the synchronization should and can be done or not.
        ///// </summary>
        ///// <param name="internetStateService">An internet state service.</param>
        ///// <param name="settingsService">A settings service.</param>
        ///// <returns>Returns true if the synchronization should be done, otherwise false.</returns>
        //protected static bool ShouldSynchronize(IInternetStateService internetStateService, ISettingsService settingsService)
        //{
        //    if (!internetStateService.IsInternetConnected())
        //        return false;

        //    AutoSynchronizationMode syncMode = settingsService.LoadSettingsOrDefault().AutoSyncMode;
        //    switch (syncMode)
        //    {
        //        case AutoSynchronizationMode.Never:
        //            return false;
        //        case AutoSynchronizationMode.CostFreeInternetOnly:
        //            return internetStateService.IsInternetCostFree();
        //        case AutoSynchronizationMode.Always:
        //            return true;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(syncMode));
        //    }
        //}
    }
}
