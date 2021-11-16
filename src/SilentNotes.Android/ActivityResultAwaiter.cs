// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SilentNotes.Android
{
    /// <summary>
    /// This class offers awaitable methods to start an activity and wait for its result.
    /// This way we can circumvent the Android OnActivityResult event and its difficult handling.
    /// </summary>
    public class ActivityResultAwaiter : IDisposable
    {
        private static int _requestCode = 497652123;
        private readonly List<StartedActivityInfo> _startedActivityInfos;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityResultAwaiter"/> class.
        /// </summary>
        /// <param name="starterActivity">Sets the <see cref="StarterActivity"/> property.</param>
        public ActivityResultAwaiter(Activity starterActivity)
        {
            StarterActivity = starterActivity;
            _startedActivityInfos = new List<StartedActivityInfo>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ActivityResultAwaiter"/> class.
        /// </summary>
        ~ActivityResultAwaiter()
        {
            Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
                _disposed = true;
        }

        /// <summary>
        /// Gets or sets the Activity which should start the new activity, this can be the
        /// root/main activity of the app.
        /// </summary>
        private Activity StarterActivity { get; set; }

        /// <summary>
        /// Instead of calling "StarterActivity.StartActivityForResult()" directly, we call this
        /// awaitable method, which takes care about waiting for the Android event. If the Intent
        /// is never finished, the method would not return, but usually this results in a cancel
        /// result. If the instance was disposed meanwhile, a cancel result will be returned.
        /// </summary>
        /// <param name="intentToStart">New indent we want to start, this could for example be a
        /// file dialog intent.</param>
        /// <returns>Returns the result of the started and awaited intent, or null in case that
        /// the awaiter was disposed meanwhile.</returns>
        public async Task<ActivityResult> StartActivityAndWaitForResult(Intent intentToStart)
        {
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
                StarterActivity.StartActivityForResult(intentToStart, activityInfo.RequestCode);
                activityInfo.WaitHandle.WaitOne();
                if (!_disposed)
                    return activityInfo.Result;
                else
                    return new ActivityResult(Result.Canceled, null);
            });

            // Start the waiting
            waiterTask.Start();
            return await waiterTask;
        }

        /// <summary>
        /// The StarterActivity should overwrite the "OnActivityResult()" method and redirect its
        /// parameter directly to this method. Afterwards it can call base.OnActivityResult().
        /// </summary>
        /// <param name="requestCode">The requestCode passed to the StarterActivity.</param>
        /// <param name="resultCode">The resultCode passed to the StarterActivity.</param>
        /// <param name="data">The data passed to the StarterActivity.</param>
        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
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
        /// A class holding all relevant information about the result of the finished activity.
        /// </summary>
        public class ActivityResult
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