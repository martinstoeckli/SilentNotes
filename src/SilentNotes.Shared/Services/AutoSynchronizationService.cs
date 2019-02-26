// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Controllers;
using SilentNotes.Models;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IAutoSynchronizationService"/> interface.
    /// </summary>
    public class AutoSynchronizationService : IAutoSynchronizationService
    {
        private readonly IInternetStateService _internetStateService;
        private readonly ISettingsService _settingsService;
        private readonly IRepositoryStorageService _repositoryStorageService;
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSynchronizationService"/> class.
        /// </summary>
        /// <param name="internetStateService">Service to check the internet connection.</param>
        /// <param name="settingsService">Service to get the auto sync mode. We need to be able to
        /// get the most current setting, the user can change this setting in the running app.</param>
        /// <param name="repositoryStorageService">Service to get the current repository.</param>
        /// <param name="navigationService">The navigation service.</param>
        public AutoSynchronizationService(
            IInternetStateService internetStateService,
            ISettingsService settingsService,
            IRepositoryStorageService repositoryStorageService,
            INavigationService navigationService)
        {
            _internetStateService = internetStateService;
            _settingsService = settingsService;
            _repositoryStorageService = repositoryStorageService;
            _navigationService = navigationService;
        }

        /// <inheritdoc/>
        public async Task SynchronizeAtStartup()
        {
            if (!ShouldSynchronize())
                return;

            _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
            long oldFingerprint = localRepository.GetModificationFingerprint();

            // Do the synchronization with the cloud storage in a background thread
            await Task.Run(async () =>
            {
                SynchronizationStoryBoard syncStory = new SynchronizationStoryBoard(true);
                await syncStory.Start();
            }).ConfigureAwait(true); // Come back to the UI thread

            // Memorize fingerprint of the synchronized respository
            _repositoryStorageService.LoadRepositoryOrDefault(out localRepository);
            long newFingerprint = localRepository.GetModificationFingerprint();
            LastSynchronizationFingerprint = newFingerprint;

            // Reload active page, but only if notes are visible and the repository differs
            if (oldFingerprint != newFingerprint)
            {
                _navigationService.RepeatNavigationIf(
                    new[] { ControllerNames.NoteRepository, ControllerNames.Note });
            }
        }

        /// <inheritdoc/>
        public async Task SynchronizeAtShutdown()
        {
            if (!ShouldSynchronize())
                return;

            // If there are no modifications since the last synchronization, we can spare this step
            _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
            long currentFingerprint = localRepository.GetModificationFingerprint();
            if (currentFingerprint == LastSynchronizationFingerprint)
                return;

            SynchronizationStoryBoard syncStory = new SynchronizationStoryBoard(true);
            await syncStory.Start();
        }

        /// <inheritdoc/>
        public long LastSynchronizationFingerprint { get; set; }

        /// <summary>
        /// Checks whether the synchronization should and can be done or not.
        /// </summary>
        /// <returns>Returns true if the synchronization should be done, otherwise false.</returns>
        private bool ShouldSynchronize()
        {
            if (!_internetStateService.IsInternetConnected())
                return false;

            AutoSynchronizationMode syncMode = _settingsService.LoadSettingsOrDefault().AutoSyncMode;
            switch (syncMode)
            {
                case AutoSynchronizationMode.Never:
                    return false;
                case AutoSynchronizationMode.CostFreeInternetOnly:
                    return _internetStateService.IsInternetCostFree();
                case AutoSynchronizationMode.Always:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(syncMode));
            }
        }
    }
}
