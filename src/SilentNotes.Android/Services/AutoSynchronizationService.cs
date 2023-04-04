// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using AndroidX.Concurrent.Futures;
using AndroidX.Lifecycle;
using AndroidX.Work;
using CommunityToolkit.Mvvm.DependencyInjection;
using Google.Common.Util.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Controllers;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IAutoSynchronizationService"/> interface for the Android platform.
    /// It makes use of the Android WorkManager because this allows the synchronization to run a
    /// short time longer than the app itself and therefore won't be killed when closing the app.
    /// </summary>
    internal class AutoSynchronizationService : AutoSynchronizationServiceBase, IAutoSynchronizationService
    {
        private const string DATA_KEY_SUCCEEDED = "succeeded";
        private const string DATA_KEY_OLD_FINGERPRINT = "oldfingerprint";
        private const string DATA_KEY_NEW_FINGERPRINT = "newfingerprint";

        private readonly ObserverHelper _observerHelper;
        private LiveData _observedLiveData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSynchronizationService"/> class.
        /// </summary>
        public AutoSynchronizationService() 
        {
            _observerHelper = new ObserverHelper(OnChanged);
        }

        /// <inheritdoc/>
        public override Task SynchronizeAtStartup()
        {
            IsRunning = true;
            IAppContextService appContext = Ioc.Default.GetService<IAppContextService>();
            ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            StopListening();

            var autoSyncMode = settingsService.LoadSettingsOrDefault().AutoSyncMode;
            if (autoSyncMode == AutoSynchronizationMode.Never)
            {
                IsRunning = false;
                return Task.CompletedTask;
            }

            NetworkType networkType = (autoSyncMode == AutoSynchronizationMode.CostFreeInternetOnly)
                ? NetworkType.Unmetered
                : NetworkType.Connected;
            Constraints constraints = new Constraints.Builder().SetRequiredNetworkType(networkType).Build();

            OneTimeWorkRequest workRequest = new OneTimeWorkRequest.Builder(typeof(ListenableSynchronizationWorker))
                .AddTag(ListenableSynchronizationWorker.TAG)
                .SetConstraints(constraints)
                .Build();

            WorkManager workManager = WorkManager.GetInstance(appContext.Context);
            workManager
                .BeginUniqueWork(ListenableSynchronizationWorker.TAG, ExistingWorkPolicy.Replace, workRequest)
                .Enqueue();

            // Attach observer so we complete the work in SynchronizationAtStartupFinished()
            LiveData liveDataForObserver = workManager.GetWorkInfosByTagLiveData(ListenableSynchronizationWorker.TAG);
            StartListening(liveDataForObserver);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Continues the synchronization at startup, after the worker has finished.
        /// </summary>
        /// <param name="succeeded">A value indicating whether the silent synchronization was successful.</param>
        /// <param name="oldFingerprint">The fingerprint of the repository before the synchronization.</param>
        /// <param name="newFingerprint">The fingerprint of the repository after the synchronization.</param>
        private void SynchronizationAtStartupFinished(bool succeeded, long oldFingerprint, long newFingerprint)
        {
            IRepositoryStorageService repositoryStorageService = Ioc.Default.GetService<IRepositoryStorageService>();
            IFeedbackService feedbackService = Ioc.Default.GetService<IFeedbackService>();
            ILanguageService languageService = Ioc.Default.GetService<ILanguageService>();
            INavigationService navigationService = Ioc.Default.GetService<INavigationService>();

            string message = succeeded
                ? languageService.LoadText("sync_success")
                : languageService.LoadText("sync_error_generic");
            feedbackService.ShowToast(message);

            // Memorize fingerprint of the synchronized respository
            LastSynchronizationFingerprint = newFingerprint;
            IsRunning = false;

            // Reload active page, but only if notes are visible and the repository differs
            if (oldFingerprint != newFingerprint)
            {
                repositoryStorageService.ClearCache();
                navigationService.RepeatNavigationIf(
                    new[] { ControllerNames.NoteRepository, ControllerNames.Note });
            }
        }

        /// <inheritdoc/>
        public override Task SynchronizeAtShutdown()
        {
            IAppContextService appContext = Ioc.Default.GetService<IAppContextService>();
            ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            IRepositoryStorageService repositoryStorageService = Ioc.Default.GetService<IRepositoryStorageService>();

            repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
            long currentFingerprint = localRepository.GetModificationFingerprint();
            repositoryStorageService.ClearCache();

            var autoSyncMode = settingsService.LoadSettingsOrDefault().AutoSyncMode;
            if (autoSyncMode == AutoSynchronizationMode.Never)
                return Task.CompletedTask;

            // If there are no modifications since the last synchronization, we can spare this step
            if (currentFingerprint == LastSynchronizationFingerprint)
                return Task.CompletedTask;

            IsRunning = true;
            Constraints constraints = (autoSyncMode == AutoSynchronizationMode.CostFreeInternetOnly)
                ? new Constraints.Builder().SetRequiredNetworkType(NetworkType.Unmetered).Build()
                : new Constraints.Builder().SetRequiredNetworkType(NetworkType.Connected).Build();
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
        public override void Stop()
        {
            IsRunning = false;
            IAppContextService appContext = Ioc.Default.GetService<IAppContextService>();
            WorkManager workManager = WorkManager.GetInstance(appContext.Context);
            workManager.CancelAllWorkByTag(ListenableSynchronizationWorker.TAG);
        }

        private void StartListening(LiveData liveDataToObserve)
        {
            _observedLiveData = liveDataToObserve;
            _observedLiveData.ObserveForever(_observerHelper);
        }

        private void StopListening()
        {
            _observedLiveData?.RemoveObserver(_observerHelper);
            _observedLiveData = null;
        }

        /// <summary>
        /// This event is called by the work manager observer.
        /// </summary>
        /// <param name="p0">Event arguments.</param>
        public void OnChanged(Java.Lang.Object p0)
        {
            if (p0 is JavaList javaList)
            {
                foreach (WorkInfo item in javaList)
                {
                    if (item.Tags.Contains(ListenableSynchronizationWorker.TAG))
                    {
                        WorkInfo.State state = item.GetState();
                        if (state.IsFinished)
                        {
                            StopListening();
                            bool succeeded = item.OutputData.GetBoolean(DATA_KEY_SUCCEEDED, false);
                            long oldFingerprint = item.OutputData.GetLong(DATA_KEY_OLD_FINGERPRINT, -1);
                            long newFingerprint = item.OutputData.GetLong(DATA_KEY_NEW_FINGERPRINT, -2);
                            SynchronizationAtStartupFinished(succeeded, oldFingerprint, newFingerprint);
                        }
                    }
                }
            }
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
                    Data.Builder outputDataBuilder = new Data.Builder();
                    try
                    {
                        if (_appContext != null)
                            await RunSilentSynchronizationStory(outputDataBuilder);
                    }
                    catch (Exception)
                    {
                        // always signal success to the workmanager
                    }
                    p0.Set(Result.InvokeSuccess(outputDataBuilder.Build()));
                });
                return TAG;
            }

            #endregion

            /// <summary>
            /// Here the actual work is done. This method should work self-sufficient and should not
            /// depend on the running app, because it can run after the app was closed.
            /// </summary>
            /// <returns>An asynchronous task.</returns>
            private async Task RunSilentSynchronizationStory(Data.Builder outputDataBuilder)
            {
                // Create an environment which is independend of the app.
                ServiceCollection services = new ServiceCollection();
                Startup.RegisterServices(services);
                StartupShared.RegisterCloudStorageClientFactory(services);
                ServiceProvider serviceProvider = services.BuildServiceProvider();

                IAppContextService appContextService = serviceProvider.GetService<IAppContextService>();
                appContextService.InitializeWithContextOnly(_appContext);

                NoteRepositoryModel localRepository;
                IRepositoryStorageService repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
                repositoryStorageService.LoadRepositoryOrDefault(out localRepository);
                long oldFingerprint = localRepository.GetModificationFingerprint();

                StoryBoardStepResult result = await SynchronizationStoryBoard.RunSilent(
                    serviceProvider.GetService<ISettingsService>(),
                    serviceProvider.GetService<ILanguageService>(),
                    serviceProvider.GetService<ICloudStorageClientFactory>(),
                    serviceProvider.GetService<ICryptoRandomService>(),
                    repositoryStorageService,
                    serviceProvider.GetService<INoteRepositoryUpdater>());

                repositoryStorageService.LoadRepositoryOrDefault(out localRepository);
                long newFingerprint = localRepository.GetModificationFingerprint();
                bool succeeded = result.NextStepIs(SynchronizationStoryStepId.StopAndShowRepository);

                outputDataBuilder.PutLong(DATA_KEY_OLD_FINGERPRINT, oldFingerprint);
                outputDataBuilder.PutLong(DATA_KEY_NEW_FINGERPRINT, newFingerprint);
                outputDataBuilder.PutBoolean(DATA_KEY_SUCCEEDED, succeeded);
            }
        }
    }
}