// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using AndroidX.Concurrent.Futures;
//using AndroidX.Work;
using Google.Common.Util.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;
//using SilentNotes.StoryBoards;
//using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.Services
{
    // todo:
    /// <summary>
    /// Implementation of the <see cref="IAutoSynchronizationService"/> interface for the Android platform.
    /// It makes use of the Android WorkManager because this allows the synchronization to run a
    /// short time longer than the app itself and therefore won't be killed when closing the app.
    /// </summary>
    internal class AutoSynchronizationService : AutoSynchronizationServiceBase, IAutoSynchronizationService
    {
        /// <inheritdoc/>
        public override Task SynchronizeAtShutdown()
        {
            Debug.WriteLine("*** AutoSynchronizationService.SynchronizeAtShutdown()");
            return Task.CompletedTask;
            //// Still running from startup?
            //if (IsRunning)
            //    return Task.CompletedTask;

            //IsRunning = true; // cannot be reset to false (no return to GUI thread), wait on stop() of next startup
            //IAppContextService appContext = Ioc.Default.GetService<IAppContextService>();
            //ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            //IInternetStateService internetStateService = Ioc.Default.GetService<IInternetStateService>();
            //IRepositoryStorageService repositoryStorageService = Ioc.Default.GetService<IRepositoryStorageService>();

            //if (!ShouldSynchronize(internetStateService, settingsService))
            //    return Task.CompletedTask;

            //// If there are no modifications since the last synchronization, we can spare this step
            //repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
            //long currentFingerprint = localRepository.GetModificationFingerprint();
            //if (currentFingerprint == LastSynchronizationFingerprint)
            //    return Task.CompletedTask;

            //// Force a reload at next startup because the sync will be done with an independend storage service.
            //repositoryStorageService.ClearCache();

            //Constraints constraints = new Constraints.Builder().SetRequiredNetworkType(NetworkType.Connected).Build();
            //Data inputData = new Data.Builder().PutLong(nameof(LastSynchronizationFingerprint), LastSynchronizationFingerprint).Build();

            //OneTimeWorkRequest workRequest = new OneTimeWorkRequest.Builder(typeof(ListenableSynchronizationWorker))
            //    .AddTag(ListenableSynchronizationWorker.TAG)
            //    .SetConstraints(constraints)
            //    .SetInputData(inputData)
            //    .Build();

            //WorkManager workManager = WorkManager.GetInstance(appContext.Context);
            //workManager
            //    .BeginUniqueWork(ListenableSynchronizationWorker.TAG, ExistingWorkPolicy.Replace, workRequest)
            //    .Enqueue();

            //// Don't await the synchronization, it can run even after the app was closed
            //return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            //IsRunning = false;
            //IAppContextService appContext = Ioc.Default.GetService<IAppContextService>();
            //WorkManager workManager = WorkManager.GetInstance(appContext.Context);
            //workManager.CancelAllWorkByTag(ListenableSynchronizationWorker.TAG);
        }

        ///// <summary>
        ///// Worker class which starts the synchronization story in the background and can be used
        ///// by the Android <see cref="WorkManager"/>.
        ///// </summary>
        //private class ListenableSynchronizationWorker : ListenableWorker, CallbackToFutureAdapter.IResolver
        //{
        //    public const string TAG = "f03a330e-8dbe-4244-ae50-947dfdaf6a4a";
        //    private Context _appContext;

        //    /// <inheritdoc/>
        //    public ListenableSynchronizationWorker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        //    {
        //    }

        //    /// <inheritdoc/>
        //    public ListenableSynchronizationWorker(Context appContext, WorkerParameters workerParams) : base(appContext, workerParams)
        //    {
        //        _appContext = appContext;
        //    }

        //    /// <inheritdoc/>
        //    public override IListenableFuture StartWork()
        //    {
        //        return CallbackToFutureAdapter.GetFuture(this);
        //    }

        //    #region IResolver

        //    /// <inheritdoc/>
        //    public Java.Lang.Object AttachCompleter(CallbackToFutureAdapter.Completer p0)
        //    {
        //        Task.Run(async () =>
        //        {
        //            try
        //            {
        //                if (_appContext != null)
        //                    await RunSilentSynchronizationStory();
        //            }
        //            catch (Exception)
        //            {
        //                // always signal success to the workmanager
        //            }
        //            p0.Set(Result.InvokeSuccess());
        //        });
        //        return TAG;
        //    }

        //    #endregion

        //    /// <summary>
        //    /// Here the actual work is done. This method should work self-sufficient and should not
        //    /// depend on the running app, because it can run even after the app was closed.
        //    /// </summary>
        //    /// <returns>An asynchronous task.</returns>
        //    private async Task RunSilentSynchronizationStory()
        //    {
        //        // Create an environment which is independend of the app.
        //        ServiceCollection services = new ServiceCollection();
        //        Startup.RegisterServices(services);
        //        StartupShared.RegisterCloudStorageClientFactory(services);
        //        ServiceProvider serviceProvider = services.BuildServiceProvider();
        //        IAppContextService appContextService = serviceProvider.GetService<IAppContextService>();
        //        appContextService.InitializeWithContextOnly(_appContext);

        //        StoryBoardStepResult result = await SynchronizationStoryBoard.RunSilent(
        //            serviceProvider.GetService<ISettingsService>(),
        //            serviceProvider.GetService<ILanguageService>(),
        //            serviceProvider.GetService<ICloudStorageClientFactory>(),
        //            serviceProvider.GetService<ICryptoRandomService>(),
        //            serviceProvider.GetService<IRepositoryStorageService>(),
        //            serviceProvider.GetService<INoteRepositoryUpdater>());
        //    }
        //}
   }
}