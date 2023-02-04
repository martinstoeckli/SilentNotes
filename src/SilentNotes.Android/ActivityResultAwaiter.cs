// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace SilentNotes.Android
{
    /// <summary>
    /// This interface offers awaitable methods to start an activity and wait for its result.
    /// This way we can circumvent the Android OnActivityResult event and its difficult handling.
    /// </summary>
    internal interface IActivityResultAwaiter
    {
        /// <summary>
        /// Instead of calling "StarterActivity.StartActivityForResult()" directly, we call this
        /// awaitable method, which takes care about waiting for the Android event. If the Intent
        /// is never finished, the method would not return, but usually this results in a cancel
        /// result. If the instance was disposed meanwhile, a cancel result will be returned.
        /// </summary>
        /// <param name="starterActivity">The activity which should start the new intent.</param>
        /// <param name="intentToStart">New indent we want to start, this could for example be a
        /// file dialog intent.</param>
        /// <returns>Returns the result of the started and awaited intent, or null in case that
        /// the awaiter was disposed meanwhile.</returns>
        Task<ActivityResult> StartActivityAndWaitForResult(Activity starterActivity, Intent intentToStart);

        /// <summary>
        /// The StarterActivity should overwrite the OnDestroy() method and call this method to signal
        /// that the application is not ready to handle the results of previously started activities.
        /// </summary>
        void RedirectedOnDestroy();

        /// <summary>
        /// The StarterActivity should overwrite the "OnActivityResult()" method and redirect its
        /// parameter directly to this method. Afterwards it can call base.OnActivityResult().
        /// </summary>
        /// <param name="requestCode">The requestCode passed to the StarterActivity.</param>
        /// <param name="resultCode">The resultCode passed to the StarterActivity.</param>
        /// <param name="data">The data passed to the StarterActivity.</param>
        void RedirectedOnActivityResult(int requestCode, Result resultCode, Intent data);
    }

    /// <summary>
    /// A class holding all relevant information about the result of the started and finished activity.
    /// </summary>
    internal class ActivityResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityResult"/> class.
        /// </summary>
        /// <param name="resultCode"></param>
        public ActivityResult(Result resultCode, Intent data)
        {
            ResultCode = resultCode;
            Data = data;
        }

        /// <summary>
        /// Gets the result code of the finished activity.
        /// </summary>
        public Result ResultCode { get; }

        /// <summary>
        /// Gets the data of the finished activity.
        /// </summary>
        public Intent Data { get; }
    }

    /// <summary>
    /// Implementation of the <see cref="IActivityResultAwaiter"/> interface.
    /// </summary>
    internal class ActivityResultAwaiter: IActivityResultAwaiter
    {
        private static int _requestCode = 497652123;
        private readonly List<StartedActivityInfo> _startedActivityInfos;
        private bool _isListening;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityResultAwaiter"/> class.
        /// </summary>
        public ActivityResultAwaiter()
        {
            _startedActivityInfos = new List<StartedActivityInfo>();
        }

        /// <inheritdoc/>
        public async Task<ActivityResult> StartActivityAndWaitForResult(Activity starterActivity, Intent intentToStart)
        {
            _isListening = true;
            unchecked
            {
                _requestCode++;
            }
            StartedActivityInfo activityInfo = new StartedActivityInfo
            {
                RequestCode = _requestCode,
                WaitHandle = new ManualResetEvent(false)
            };
            _startedActivityInfos.Add(activityInfo);

            // Prepare the waiting task
            Task<ActivityResult> waiterTask = new Task<ActivityResult>(() =>
            {
                // Start the activity
                starterActivity.StartActivityForResult(intentToStart, activityInfo.RequestCode);
                activityInfo.WaitHandle.WaitOne();

                if (_isListening)
                    return activityInfo.Result;
                else
                    return new ActivityResult(Result.Canceled, null);
            });

            // Start the waiting
            waiterTask.Start();
            return await waiterTask;
        }

        /// <inheritdoc/>
        public void RedirectedOnDestroy()
        {
            _isListening = false;
        }

        /// <inheritdoc/>
        public void RedirectedOnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Lets see if we have started an activity with this request code.
            StartedActivityInfo activityInfo = _startedActivityInfos.Find(item => item.RequestCode == requestCode);
            if (activityInfo != null)
            {
                _startedActivityInfos.Remove(activityInfo);

                // Prepare the result and stop the waiting task.
                activityInfo.Result = new ActivityResult(resultCode, data);
                activityInfo.WaitHandle.Set();
            }
        }

        /// <summary>
        /// Private class holding the context of a started activity.
        /// </summary>
        private class StartedActivityInfo
        {
            public int RequestCode { get; set; }

            public ManualResetEvent WaitHandle { get; set; }

            public ActivityResult Result { get; set; }
        }
    }
}