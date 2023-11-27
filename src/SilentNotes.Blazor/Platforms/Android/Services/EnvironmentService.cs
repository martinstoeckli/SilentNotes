// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;
using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IEnvironmentService"/> for the Android platform.
    /// </summary>
    internal class EnvironmentService : IEnvironmentService, IKeepScreenOn, IScreenshots
    {
        private readonly IAppContextService _appContext;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        public EnvironmentService(IAppContextService appContextService)
        {
            Os = SilentNotes.Services.OperatingSystem.Android;
            _appContext = appContextService;
        }

        /// <inheritdoc/>
        public SilentNotes.Services.OperatingSystem Os { get; private set; }

        /// <inheritdoc/>
        public bool InDarkMode
        {
            get
            {
                UiMode nightModeFlags = _appContext.Context.Resources.Configuration.UiMode & UiMode.NightMask;
                return nightModeFlags == UiMode.NightYes;
            }
        }

        /// <inheritdoc/>
        public IKeepScreenOn KeepScreenOn
        {
            get { return this; }
        }

        /// <inheritdoc/>
        void IKeepScreenOn.Start(System.TimeSpan duration)
        {
            Debug.WriteLine("*** IKeepScreenOn.Start");

            // If a timer is already active, deactivate it.
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;

            // Set the activity flag to keep the screen on.
            _appContext.RootActivity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            // (Re)start the timer
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Delay(duration, _cancellationTokenSource.Token).ContinueWith(StoppedAfterTimeout);
        }

        private void StoppedAfterTimeout(Task task)
        {
            Debug.WriteLine(string.Format("*** IKeepScreenOn.StoppedAfterTimeout WasCanceled={0}", task.IsCanceled));
            if (task.IsCanceled)
                return;

            // Timer was not cancelled, so timeout was reached and KeepScreenOn should be stopped.
            _cancellationTokenSource = null;
            _appContext.RootActivity.RunOnUiThread(() =>
            {
                _appContext.RootActivity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn); // must be called on UI thread
                WeakReferenceMessenger.Default.Send(new KeepScreenOnChangedMessage());
            });
        }

        /// <inheritdoc/>
        void IKeepScreenOn.Stop()
        {
            Debug.WriteLine("*** IKeepScreenOn.Stop");
            if (_cancellationTokenSource != null)
            {
                // If a timer is active, deactivate it.
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

                _appContext.RootActivity.RunOnUiThread(() =>
                {
                    _appContext.RootActivity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
                });
            }
        }

        /// <inheritdoc/>
        bool IKeepScreenOn.IsActive
        {
            get { return _cancellationTokenSource != null; }
        }

        /// <inheritdoc/>
        public IScreenshots Screenshots
        {
            get { return this; }
        }

        /// <inheritdoc/>
        bool IScreenshots.PreventScreenshots
        {
            set
            {
                if (value)
                    _appContext.RootActivity.Window.AddFlags(WindowManagerFlags.Secure);
                else
                    _appContext.RootActivity.Window.ClearFlags(WindowManagerFlags.Secure);
            }
        }
    }
}