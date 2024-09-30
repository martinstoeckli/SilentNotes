// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using AndroidX.Concurrent.Futures;
using AndroidX.Work;
using Google.Common.Util.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="ISynchronizationService"/> interface for the Android platform.
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
        public override Task AutoSynchronizeAtShutdown(IServiceProvider serviceProvider)
        {
            System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtShutdown()");
            if (IsWaitingForOAuthRedirect)
                return Task.CompletedTask;

            // Still running from startup?
            if (!_synchronizationState.TryStartSynchronizationState(SynchronizationType.AtShutdown))
                return Task.CompletedTask;
            // todo: stom
            //if (IsStartupSynchronizationRunning)
            //    return Task.CompletedTask;
            IsStartupSynchronizationRunning = true; // cannot be reset to false (no return to GUI thread), wait on stop() of next startup

            IAppContextService appContext = serviceProvider.GetService<IAppContextService>();
            IInternetStateService internetStateService = serviceProvider.GetService<IInternetStateService>();
            ISettingsService settingsService = serviceProvider.GetService<ISettingsService>();
            IRepositoryStorageService repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();

            if (!ShouldSynchronize(internetStateService, settingsService))
                return Task.CompletedTask;

            // If there are no modifications since the last synchronization, we can spare this step
            repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
            long currentFingerprint = localRepository.GetModificationFingerprint();
            if (currentFingerprint == LastSynchronizationFingerprint)
                return Task.CompletedTask;

            System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtShutdown() start");

            // Force a reload at next startup because the sync will be done with an independend storage service.
            repositoryStorageService.ClearCache();

            Constraints constraints = new Constraints.Builder().SetRequiredNetworkType(NetworkType.Connected).Build();
            Data inputData = new Data.Builder().PutLong(nameof(LastSynchronizationFingerprint), LastSynchronizationFingerprint).Build();

            OneTimeWorkRequest workRequest = new OneTimeWorkRequest.Builder(typeof(ListenableSynchronizationWorker))
                .AddTag(ListenableSynchronizationWorker.TAG)
                .SetConstraints(constraints)
                .SetInputData(inputData)
                .Build();

            WorkManager workManager = WorkManager.GetInstance(appContext.Context);
            workManager
                .BeginUniqueWork(ListenableSynchronizationWorker.TAG, ExistingWorkPolicy.Replace, workRequest)
                .Enqueue();

            // Don't await the synchronization, it can run even after the app was closed
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void StopAutoSynchronization(IServiceProvider serviceProvider)
        {
            // todo: stom
            IsStartupSynchronizationRunning = false;
            IAppContextService appContext = serviceProvider.GetService<IAppContextService>();
            WorkManager workManager = WorkManager.GetInstance(appContext.Context);
            workManager.CancelAllWorkByTag(ListenableSynchronizationWorker.TAG);
        }

        /// <summary>
        /// Worker class which starts the synchronization story in the background and can be used
        /// by the Android <see cref="WorkManager"/>.
        /// </summary>
        private class ListenableSynchronizationWorker : ListenableWorker, CallbackToFutureAdapter.IResolver
        {
            public const string TAG = "f03a330e-8dbe-4244-ae50-947dfdaf6a4a";
            private Context _appContext;

            /// <inheritdoc/>
            public ListenableSynchronizationWorker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            /// <inheritdoc/>
            public ListenableSynchronizationWorker(Context appContext, WorkerParameters workerParams) : base(appContext, workerParams)
            {
                _appContext = appContext;
            }

            /// <inheritdoc/>
            public override IListenableFuture StartWork()
            {
                return CallbackToFutureAdapter.GetFuture(this);
            }

            #region IResolver

            /// <inheritdoc/>
            public Java.Lang.Object AttachCompleter(CallbackToFutureAdapter.Completer p0)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (_appContext != null)
                            await RunSilentSynchronizationStory();
                    }
                    catch (Exception)
                    {
                        // always signal success to the workmanager
                    }
                    p0.Set(Result.InvokeSuccess());
                });
                return TAG;
            }

            #endregion

            /// <summary>
            /// Here the actual work is done. This method should work self-sufficient and should not
            /// depend on the running app, because it can run even after the app was closed.
            /// </summary>
            /// <returns>An asynchronous task.</returns>
            private async Task RunSilentSynchronizationStory()
            {
                // Create an environment which is independend of the app.
                ServiceCollection services = new ServiceCollection();
                MauiProgram.RegisterSharedServices(services);
                MauiProgram.RegisterPlatformServices(services);
                ServiceProvider serviceProvider = services.BuildServiceProvider();
                try
                {
                    IAppContextService appContextService = serviceProvider.GetService<IAppContextService>();
                    appContextService.InitializeWithContextOnly(_appContext);

                    var result = await RunSilent(
                        serviceProvider.GetService<ISettingsService>(),
                        serviceProvider.GetService<ILanguageService>(),
                        serviceProvider.GetService<ICloudStorageClientFactory>(),
                        serviceProvider.GetService<ICryptoRandomService>(),
                        serviceProvider.GetService<IRepositoryStorageService>(),
                        serviceProvider.GetService<INoteRepositoryUpdater>());

                    System.Diagnostics.Debug.WriteLine("*** SynchronizationService.SynchronizeAtShutdown() end");
                }
                finally
                {
                    var synchronizationState = serviceProvider.GetService<ISynchronizationState>();
                    synchronizationState.StopSynchronizationState();
                }
            }
        }
    }
}
