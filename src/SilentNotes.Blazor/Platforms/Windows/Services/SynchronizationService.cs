// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="ISynchronizationService"/> interface for the Windows platform.
    /// </summary>
    internal class SynchronizationService : SynchronizationServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationService"/> class.
        /// </summary>
        /// <param name="synchronizationState">A singleton storing the current state of the synchronization.</param>
        public SynchronizationService(ISynchronizationState synchronizationState)
            : base(synchronizationState)
        {
        }

        /// <inheritdoc/>
        public override async Task AutoSynchronizeAtShutdown(IServiceProvider serviceProvider)
        {
            System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtShutdown()");
            if (IsWaitingForOAuthRedirect)
                return;

            // Still running from startup?
            if (!_synchronizationState.TryStartSynchronizationState(SynchronizationType.AtShutdown))
                return;

            try
            {
                IInternetStateService internetStateService = serviceProvider.GetService<IInternetStateService>();
                ISettingsService settingsService = serviceProvider.GetService<ISettingsService>();
                IRepositoryStorageService repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();

                if (!ShouldSynchronize(internetStateService, settingsService))
                    return;

                // If there are no modifications since the last synchronization, we can spare this step
                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                long currentFingerprint = localRepository.GetModificationFingerprint();
                if (currentFingerprint == LastSynchronizationFingerprint)
                    return;

                System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtShutdown() start");

                var result = await RunSilent(
                    serviceProvider.GetService<ISettingsService>(),
                    serviceProvider.GetService<ILanguageService>(),
                    serviceProvider.GetService<ICloudStorageClientFactory>(),
                    serviceProvider.GetService<ICryptoRandomService>(),
                    serviceProvider.GetService<IRepositoryStorageService>(),
                    serviceProvider.GetService<INoteRepositoryUpdater>());

                if ((result != null) && (!result.HasError))
                    _synchronizationState.UpdateLastFinishedSynchronization();
            }
            finally
            {
                _synchronizationState.StopSynchronizationState();
            }
        }

        /// <inheritdoc/>
        public override void StopAutoSynchronization(IServiceProvider serviceProvider)
        {
            _synchronizationState.StopSynchronizationState();
        }
    }
}
