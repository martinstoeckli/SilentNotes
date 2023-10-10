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

namespace SilentNotes.Platforms
{
    /// <summary>
    /// Handles application lifecycle events for the Windows platform.
    /// Possible events are listed here: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle
    /// </summary>
    internal class ApplicationEventHandler : ApplicationEventHandlerBase
    {
        internal void OnClosed(Microsoft.UI.Xaml.Window window, WindowEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnClosed()");
            WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());

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

            if ((e.Kind == ExtendedActivationKind.Protocol) &&
                (e.Data is ProtocolActivatedEventArgs activatedEventArgs))
            {
                var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
                if (synchronizationService.CurrentStory != null)
                {
                    BringWindowToFront();
                    synchronizationService.CurrentStory.OauthRedirectUrl = activatedEventArgs.Uri.AbsoluteUri;
                    var handleOAuthRedirectStep = new HandleOAuthRedirectStep();
                    _ = handleOAuthRedirectStep.RunStory(synchronizationService.CurrentStory, Ioc.Instance, synchronizationService.CurrentStory.StoryMode);
                }
            }
        }

        private static void BringWindowToFront()
        {
            var mainWindow = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView);
            var mainWindowHandle = mainWindow.GetWindowHandle();
            SetForegroundWindow(mainWindowHandle);
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
