// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;
using Windows.ApplicationModel.Activation;
using Windows.Graphics;

namespace SilentNotes.Platforms
{
    /// <summary>
    /// Handles application lifecycle events for the Windows platform.
    /// Possible events are listed here: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle
    /// </summary>
    internal class ApplicationEventHandler : ApplicationEventHandlerBase
    {
        internal void OnWindowCreated(Microsoft.UI.Xaml.Window window)
        {
            AdjustWindowSizeInDemoMode(window);
        }

        internal void OnClosed(Microsoft.UI.Xaml.Window window, WindowEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnClosed()");
            WeakReferenceMessenger.Default.Send(new StoreUnsavedDataMessage(MessageSender.ApplicationEventHandler));

            // We need to wait for the end of the synchronization, otherwise the app exits before
            // the work is done.
            var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
            Task.Run(() => synchronizationService.AutoSynchronizeAtShutdown(Ioc.Instance)).Wait();
        }

        /// <summary>
        /// Is called when another application instance redirects to this instance.
        /// Example: The OAuth2 redirect starts a new instance and instead of starting a
        /// second application, the new instance redirects to the already running instance.
        /// </summary>
        internal void OnRedirected(object sender, AppActivationArguments e)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnRedirected()");

            BringWindowToFront();

            if ((e.Kind == ExtendedActivationKind.Protocol) &&
                (e.Data is ProtocolActivatedEventArgs activatedEventArgs))
            {
                // Continue manual synchronization, after the user confirmed the OAuth2 login.
                var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
                if (synchronizationService.ManualSynchronization != null)
                {
                    synchronizationService.ManualSynchronization.OauthRedirectUrl = activatedEventArgs.Uri.AbsoluteUri;
                    var handleOAuthRedirectStep = new HandleOAuthRedirectStep();
                    _ = handleOAuthRedirectStep.RunStoryAndShowLastFeedback(synchronizationService.ManualSynchronization, Ioc.Instance, synchronizationService.ManualSynchronization.StoryMode);
                }
            }
        }

        private static void BringWindowToFront()
        {
            var mainWindow = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView);
            var mainWindowHandle = mainWindow.GetWindowHandle();
            SetForegroundWindow(mainWindowHandle);
        }

        /// <summary>
        /// This method should set the size of the main window when the FORCE_WINDOW_SIZE condition
        /// is set in the Directory.Build.props, so that screenshots all have a preset size.
        /// </summary>
        private static void AdjustWindowSizeInDemoMode(Microsoft.UI.Xaml.Window window)
        {
#if (DEBUG && FORCE_WINDOW_SIZE)
            float density = window.GetDisplayDensity();

            // Set size to get 4:3 screenshots (800:600) on a unscaled screen.
            int width = (int)Math.Round(808.0 * density + 6);
            int height = (int)Math.Round(604.0 * density + 3);
            var size = new SizeInt32(width, height);

            var appWindow = window.GetAppWindow();
            appWindow.Resize(size);
#endif
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        //internal void OnLaunching(Microsoft.UI.Xaml.Application application, LaunchActivatedEventArgs args)
        //{
        //    System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnLaunching()");
        //}

        //internal void OnLaunched(Microsoft.UI.Xaml.Application window, LaunchActivatedEventArgs args)
        //{
        //    System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnLaunched()");
        //}

        //internal void OnActivated(Microsoft.UI.Xaml.Window window, WindowActivatedEventArgs args)
        //{
        //    System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnActivated() " + args.WindowActivationState);
        //}
    }
}
