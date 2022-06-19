// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IEnvironmentService"/> for the Android platform.
    /// </summary>
    public class EnvironmentService : IEnvironmentService, IKeepScreenOn
    {
        private readonly Activity _rootActivity;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentService"/> class.
        /// </summary>
        /// <param name="os">Sets the <see cref="Os"/> property.</param>
        /// <param name="rootActivity">The context of the Android app.</param>
        public EnvironmentService(OperatingSystem os, Activity rootActivity)
        {
            Os = os;
            _rootActivity = rootActivity;
        }

        /// <inheritdoc/>
        public OperatingSystem Os { get; private set; }

        /// <inheritdoc/>
        public bool InDarkMode
        {
            get
            {
                UiMode nightModeFlags = _rootActivity.Resources.Configuration.UiMode & UiMode.NightMask;
                return nightModeFlags == UiMode.NightYes;
            }
        }

        /// <inheritdoc/>
        public IKeepScreenOn KeepScreenOn
        {
            get { return this; }
        }

        /// <inheritdoc/>
        void IKeepScreenOn.Start()
        {
            _rootActivity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        /// <inheritdoc/>
        void IKeepScreenOn.Stop()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _rootActivity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
        }

        /// <inheritdoc/>
        void IKeepScreenOn.StopAfter(System.TimeSpan duration)
        {
            // If a timer is already active, deactivate it.
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;

            // Create (renew) timer
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            Task.Delay(duration, token).ContinueWith(_ =>
            {
                _cancellationTokenSource = null;
                if (!token.IsCancellationRequested)
                    KeepScreenOn.Stop();
            });
        }
    }
}