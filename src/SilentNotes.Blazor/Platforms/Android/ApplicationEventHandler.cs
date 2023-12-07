// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
using Android.App;
using Android.Content;
using CommunityToolkit.Mvvm.Messaging;
using Java.Net;
using SilentNotes.Models;
using SilentNotes.Platforms.Services;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;
using SilentNotes.ViewModels;

namespace SilentNotes.Platforms
{
    /// <summary>
    /// Handles application lifecycle events for the Android platform.
    /// Possible events are listed here: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle
    /// </summary>
    internal class ApplicationEventHandler : ApplicationEventHandlerBase
    {
        private string _actionSendParameter;
        private NoteModel _newNoteFromActionSend;

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

            // Check for arguments passed by the ActionSendActivity.
            ConsumeActionSendIntentParameter(activity.Intent);
        }

        /// <inheritdoc/>
        internal void OnNewIntent(Intent intent)
        {
            // After reading the parameters from the passed intent, we forget about it and keep
            // the old intent running. This is called if the app was closed with the back key and
            // was not completely closed.
            ConsumeActionSendIntentParameter(intent);
        }

        /// <summary>
        /// Checks whether the intent was created by an <see cref="ActionSendActivity"/> and stores
        /// its parameter to the variable <see cref="_actionSendParameter"/>, so it can later be
        /// used to start up the app.
        /// </summary>
        /// <param name="intent">The active indent.</param>
        private void ConsumeActionSendIntentParameter(Intent intent)
        {
            bool isStartedWithActionSend = intent.HasExtra(ActionSendActivity.NoteHtmlParam);
            bool isAlreadyHandled = intent.Flags.HasFlag(ActivityFlags.LaunchedFromHistory);

            if (isStartedWithActionSend && !isAlreadyHandled)
            {
                _actionSendParameter = intent.GetStringExtra(ActionSendActivity.NoteHtmlParam);
                intent.RemoveExtra(ActionSendActivity.NoteHtmlParam);
            }
            else
            {
                _actionSendParameter = null;
            }
        }

        /// <summary>
        /// The OnStart() and OnNewIntent() methods have no guaranteed order, so we do all the
        /// work for starting up the app here, this is guaranteed to be called after them.
        /// </summary>
        internal void OnResume(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnResume() " + GetId(activity));
            var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
            synchronizationService.StopAutoSynchronization(Ioc.Instance);

            if (!string.IsNullOrEmpty(_actionSendParameter))
            {
                _newNoteFromActionSend = CreateNewNoteFromSendParameter(Ioc.Instance, _actionSendParameter);
                _actionSendParameter = null; // create the note only once
            }
            else if (IsStartedByOAuthRedirectIndent(synchronizationService))
            {
                var startStep = new HandleOAuthRedirectStep();
                _ = startStep.RunStory(synchronizationService.ManualSynchronization, Ioc.Instance, synchronizationService.ManualSynchronization.StoryMode);
            }
        }

        private NoteModel CreateNewNoteFromSendParameter(IServiceProvider serviceProvider, string actionSendParameter)
        {
            var repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
            var settingsService = serviceProvider.GetService<ISettingsService>();
            repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);

            NoteModel note = new NoteModel
            {
                BackgroundColorHex = settingsService.LoadSettingsOrDefault().DefaultNoteColorHex,
                HtmlContent = _actionSendParameter,
            };

            int insertionIndex = noteRepository.Notes.IndexToInsertNewNote(NoteInsertionMode.AtTop);
            noteRepository.Notes.Insert(insertionIndex, note);
            repositoryStorageService.TrySaveRepository(noteRepository);
            return note;
        }

        internal void OnPause(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnPause() " + GetId(activity));
            WeakReferenceMessenger.Default.Send(new StoreUnsavedDataMessage(MessageSender.ApplicationEventHandler));
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
            WeakReferenceMessenger.Default.Send(new ClosePageMessage());
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

        protected override Task OnMainLayoutReady()
        {
            if (_newNoteFromActionSend != null)
            {
                WeakReferenceMessenger.Default.Send(new BringNoteIntoViewMessage(_newNoteFromActionSend.Id, false));
                _newNoteFromActionSend = null;
            }
            return base.OnMainLayoutReady();
        }

        private static string GetId(Activity activity)
        {
            if (activity is MainActivity mainActivity)
                return mainActivity.Id.ToString();
            return null;
        }
    }
}
