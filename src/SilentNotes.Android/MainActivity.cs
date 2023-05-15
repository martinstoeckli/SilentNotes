﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Webkit;
using CommunityToolkit.Mvvm.DependencyInjection;
using Java.IO;
using SilentNotes.Android.Services;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.Android
{
    /// <summary>
    /// The main activity of the Android app. Because this is a single page app, it is the only
    /// activity showing a window.
    /// </summary>
    [Activity(
        Label = "SilentNotes",
        Icon = "@drawable/ic_launcher",
        Theme = "@style/Theme.App.Starting",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        Exported = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity, IHtmlView
    {
        private WebView _webView;
        private Navigation _lastNavigation;
        private string _actionSendParameter;

        /// <inheritdoc/>
        protected override void OnCreate(Bundle bundle)
        {
            AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

#if DEBUG
            // Allow debugging of JavaScript inside of the WebView.
            // Debugging can be activated in Chrome by calling "chrome://inspect"
            WebView.SetWebContentsDebuggingEnabled(true);
#endif

            // Initialize the Ioc and make the new main window available to the services.
            Startup.InitializeApplication();
            Ioc.Default.GetService<IAppContextService>().Initialize(this);

            ConsumeActionSendIntentParameter(Intent);

            // Prevent notes from being visible in list of recent apps and screenshots
            ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            SettingsModel settings = settingsService.LoadSettingsOrDefault();
            if (settings.PreventScreenshots)
                Ioc.Default.GetService<IEnvironmentService>().Screenshots.PreventScreenshots = true;

            // Load main window of single page application.
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            // Prepare the webview
            _webView = FindViewById<WebView>(Resource.Id.webView);
            _webView.SetWebViewClient(new HybridWebViewClient(
                (url) => OnNavigating(url),
                () => OnNavigationCompleted()));

            WebSettings webSettings = _webView.Settings;
            webSettings.JavaScriptEnabled = true;
            webSettings.BlockNetworkLoads = true; // only local content allowed
            webSettings.AllowFileAccess = false; // no local files but from the asset directory
            webSettings.CacheMode = CacheModes.NoCache; // is already local content
            webSettings.JavaScriptCanOpenWindowsAutomatically = false; // same as default
            webSettings.SetSupportMultipleWindows(false); // same as default
            webSettings.TextZoom = 100; // Ignores system font size, so the app controls the font size
            webSettings.SaveFormData = false;
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            Ioc.Default.GetService<IActivityResultAwaiter>().RedirectedOnDestroy();
            base.OnDestroy();
        }

        /// <inheritdoc/>
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // After reading the parameters from the passed intent, we forget about it and keep
            // the old intent running.
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

        private static bool CanStartupWithLastNavigation(Navigation navigation)
        {
            var allowedStartupNavigations = new[] { ControllerNames.NoteRepository, ControllerNames.Note, ControllerNames.Checklist, ControllerNames.Settings, ControllerNames.Info };
            return allowedStartupNavigations.Contains(navigation?.ControllerId);
        }

        /// <inheritdoc/>
        protected override void OnStop()
        {
            // We do not await the synchronization, it runs in a background service which can stay
            // alive a bit longer than the app itself.
            IAutoSynchronizationService syncService = Ioc.Default.GetService<IAutoSynchronizationService>();
            syncService.SynchronizeAtShutdown();
            base.OnStop();
        }

        /// <inheritdoc/>
        protected override void OnPause()
        {
            base.OnPause();
            try
            {
                INavigationService navigationService = Ioc.Default.GetService<INavigationService>();
                _lastNavigation = navigationService?.CurrentNavigation;
                navigationService.CurrentController?.StoreUnsavedData();
                navigationService.CurrentController?.Dispose();

                // Make sure that all open safes are closed.
                IRepositoryStorageService repositoryService = Ioc.Default.GetService<IRepositoryStorageService>();
                repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel repositoryModel);
                repositoryModel?.Safes.ForEach(safe => safe.Close());
            }
            catch (Exception)
            {
                // No exception should escape here.
            }
        }

        /// <summary>
        /// The OnStart() and OnNewIntent() methods have no guaranteed order, so we do all the
        /// work for starting up the app here, this is guaranteed to be called after them.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            INavigationService navigationService = Ioc.Default.GetService<INavigationService>();
            IStoryBoardService storyBoardService = Ioc.Default.GetService<IStoryBoardService>();
            INotificationService notificationService = Ioc.Default.GetService<INotificationService>();
            IAutoSynchronizationService autoSynchronizationService = Ioc.Default.GetService<IAutoSynchronizationService>();
            autoSynchronizationService.Stop();

            if (!string.IsNullOrEmpty(_actionSendParameter))
            {
                // Create new note and show it
                IRepositoryStorageService repositoryStorageService = Ioc.Default.GetService<IRepositoryStorageService>();
                ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();

                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
                NoteModel note = new NoteModel
                {
                    BackgroundColorHex = settingsService.LoadSettingsOrDefault().DefaultNoteColorHex,
                    HtmlContent = _actionSendParameter,
                };
                noteRepository.Notes.Insert(0, note);
                repositoryStorageService.TrySaveRepository(noteRepository);

                _actionSendParameter = null; // create the note only once
                navigationService.Navigate(new Navigation(ControllerNames.Note, ControllerParameters.NoteId, note.Id.ToString()));
            }
            else if (IsStartedByOAuthRedirectIndent(storyBoardService))
            {
                if (storyBoardService.ActiveStory is SynchronizationStoryBoard synchronizationStory)
                {
                    storyBoardService.ActiveStory.ContinueWith(SynchronizationStoryStepId.HandleOAuthRedirect);
                }
            }
            else
            {
                // Normal startup
                if (CanStartupWithLastNavigation(_lastNavigation))
                    navigationService.Navigate(_lastNavigation);
                else
                    navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
                autoSynchronizationService.SynchronizeAtStartup(); // no awaiting, run in background
                notificationService.ShowNextNotification();
            }
        }

        /// <inheritdoc/>
        public override void OnBackPressed()
        {
            // We tell the webview to close any open dropdowns like the menu, if there is nothing
            // to close, the function will signal an application close which we intercept in OnNavigating.
            ExecuteJavaScript("closeDropdownOrSignalBackPressed();");
        }

        /// <inheritdoc/>
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            Ioc.Default.GetService<IActivityResultAwaiter>().RedirectedOnActivityResult(
                requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }

        /// <inheritdoc/>
        public void LoadHtml(string html)
        {
            IBaseUrlService baseUrl = Ioc.Default.GetService<IBaseUrlService>();
            _webView.LoadDataWithBaseURL(baseUrl.HtmlBase, html, "text/html", "UTF-8", null);
        }

        /// <inheritdoc/>
        public void ReplaceNode(string nodeId, string newHtml)
        {
            string encodedNewHtml = WebviewUtils.EscapeJavaScriptString(newHtml);
            string script = string.Format("document.getElementById('{0}').outerHTML = \"{1}\";", nodeId, encodedNewHtml);
            ExecuteJavaScript(script);
        }

        /// <inheritdoc/>
        public void ExecuteJavaScript(string script)
        {
            _webView.EvaluateJavascript(script, null);
        }

        /// <inheritdoc/>
        public async Task<string> ExecuteJavaScriptReturnString(string script)
        {
            var taskCompletion = new TaskCompletionSource<string>();
            WebviewValueCallback callback = new WebviewValueCallback(taskCompletion);
            _webView.EvaluateJavascript(script, callback);
            string result = await taskCompletion.Task;
            return result;
        }

        /// <inheritdoc/>
        public void SetBackgroundColor(System.Drawing.Color backgroundColor)
        {
            _webView.SetBackgroundColor(Color.Argb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B));
        }

        /// <inheritdoc/>
        public event HtmlViewNavigatingEventHandler Navigating;

        private void OnNavigating(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            // Intercept the javascript signal for the BackPressed event raised in OnBackPressed.
            if (url.EndsWith("BackPressed"))
            {
                // Delegate the back pressed to the current controller
                INavigationService navigation = Ioc.Default.GetService<INavigationService>();
                navigation.CurrentController.OnGoBackPressed(out bool handled);

                // If not handled by the controller, close the application
                if (!handled)
                    base.OnBackPressed(); // close application
                return;
            }

            if (Navigating != null)
                Navigating(this, url);
        }

        /// <inheritdoc/>
        public event EventHandler NavigationCompleted;

        private void OnNavigationCompleted()
        {
            if (NavigationCompleted != null)
                NavigationCompleted(_webView, new EventArgs());
        }

        private bool IsStartedByOAuthRedirectIndent(IStoryBoardService storyBoardService)
        {
            return (storyBoardService.ActiveStory != null) &&
                storyBoardService.ActiveStory.Session.TryLoad(SynchronizationStorySessionKey.OauthRedirectUrl, out string _);
        }

        private class WebviewValueCallback : Java.Lang.Object, IValueCallback
        {
            private readonly TaskCompletionSource<string> _taskCompletion;

            /// <summary>
            /// Initializes a new instance of the <see cref="WebviewValueCallback"/> class.
            /// </summary>
            /// <param name="taskCompletion">An object which can signal the completion of the task.</param>
            public WebviewValueCallback(TaskCompletionSource<string> taskCompletion)
            {
                _taskCompletion = taskCompletion;
            }

            public void OnReceiveValue(Java.Lang.Object value)
            {
                string decodedValue = DecodeJsonString(value.ToString());
                _taskCompletion.SetResult(decodedValue);
            }

            /// <summary>
            /// Decodes a JSON encoded string.
            /// </summary>
            /// <param name="jsonString">JSON containing a single string value.</param>
            /// <returns>Decoded string.</returns>
            private static string DecodeJsonString(string jsonString)
            {
                JsonReader reader = new JsonReader(new StringReader(jsonString));
                try
                {
                    reader.Lenient = true; // Must set lenient to parse single values

                    JsonToken token = reader.Peek();
                    if (token == JsonToken.Null)
                        return null;
                    else if (token == JsonToken.String)
                        return reader.NextString();
                    throw new Exception("DecodeJsonString expects a string in the Json, but it did not contain a string.");
                }
                finally
                {
                    reader.Close();
                }
            }
        }
    }
}
