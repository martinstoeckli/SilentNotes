// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.App;
using Android.Content;
using CommunityToolkit.Mvvm.Messaging;
using Java.Net;
using SilentNotes.Platforms.Services;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.Platforms
{
    /// <summary>
    /// Handles application lifecycle events for the Android platform.
    /// Possible events are listed here: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle
    /// </summary>
    internal class ApplicationEventHandler : ApplicationEventHandlerBase
    {
        internal void OnCreate(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnCreate() " + GetId(activity));

            // Workaround: Android soft keyboard hides the lower part of the content,
            // see https://learn.microsoft.com/en-us/dotnet/maui/android/platform-specifics/soft-keyboard-input-mode
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.UseWindowSoftInputModeAdjust(
                Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>(),
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);

            // Inform services about the new main activity.
            Ioc.Instance.GetService<IAppContextService>().Initialize(activity);
        }

        internal void OnResume(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnResume() " + GetId(activity));
            var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
            synchronizationService.StopAutoSynchronization(Ioc.Instance);

            if (IsStartedByOAuthRedirectIndent(synchronizationService))
            {
                var startStep = new HandleOAuthRedirectStep();
                _ = startStep.RunStory(synchronizationService.ManualSynchronization, Ioc.Instance, synchronizationService.ManualSynchronization.StoryMode);
            }
        }

        internal void OnPause(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnPause() " + GetId(activity));
            WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());
        }

        internal void OnStop(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnPause() " + GetId(activity));
            var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
            synchronizationService.AutoSynchronizeAtShutdown(Ioc.Instance);
        }

        internal void OnDestroy(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnDestroy() " + GetId(activity));
            WeakReferenceMessenger.Default.Send<ClosePageMessage>(new ClosePageMessage());
            Ioc.Instance.GetService<IActivityResultAwaiter>().RedirectedOnDestroy();
        }

        internal void OnActivityResult(Activity activity, int requestCode, Result resultCode, Intent data)
        {
            Ioc.Instance.GetService<IActivityResultAwaiter>().RedirectedOnActivityResult(requestCode, resultCode, data);
        }

        private bool IsStartedByOAuthRedirectIndent(ISynchronizationService synchronizationService)
        {
            return synchronizationService.ManualSynchronization?.OauthRedirectUrl != null;
        }

        private static string GetId(Activity activity)
        {
            if (activity is MainActivity mainActivity)
                return mainActivity.Id.ToString();
            return null;
        }
    }
}
