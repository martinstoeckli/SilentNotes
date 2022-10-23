// Copyright © 2018 Martin Stoeckli.
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
using Android.Views;
using Android.Webkit;
using Java.IO;
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
        Theme = "@style/MainTheme.SplashScreen",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        Exported = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity, IHtmlView
    {
        private ActivityResultAwaiter _activityResultAwaiter;
        private WebView _webView;
        private Navigation _lastNavigation;
        private string _actionSendParameter;

        /// <inheritdoc/>
        protected override void OnCreate(Bundle bundle)
        {
#if DEBUG
            // Allow debugging of JavaScript inside of the WebView.
            // Debugging can be activated in Chrome by calling "chrome://inspect"
            WebView.SetWebContentsDebuggingEnabled(true);
#endif

            // Clear the splash screen theme, which is declared as attribute of the activity.
            SetTheme(Resource.Style.MainTheme);

            _activityResultAwaiter = new ActivityResultAwaiter(this);
            Startup.InitializeApplication(this, _activityResultAwaiter);
            ConsumeActionSendIntentParameter(Intent);

            // Prevent notes from being visible in list of recent apps and screenshots
            Window.AddFlags(WindowManagerFlags.Secure);

            // Load main window of single page application.
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            // Prepare the webview
            _webView = FindViewById<WebView>(Resource.Id.webView);
            _webView.SetWebViewClient(new HybridWebViewClient(
                (url) => OnNavigating(url),
                () => OnNavigationCompleted()));

            WebSettings settings = _webView.Settings;
            settings.JavaScriptEnabled = true;
            settings.BlockNetworkLoads = true; // only local content allowed
            settings.AllowFileAccess = false; // no local files but from the asset directory
            settings.SetPluginState(WebSettings.PluginState.Off); // no plugins allowed
            settings.CacheMode = CacheModes.NoCache; // is already local content
            settings.JavaScriptCanOpenWindowsAutomatically = false; // same as default
            settings.SetSupportMultipleWindows(false); // same as default
            settings.TextZoom = 100; // Ignores system font size, so the app controls the font size
            settings.SaveFormData = false;
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            _activityResultAwaiter.Dispose();
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
            // The synchronization continues when we do not await it, even if another app became
            // active in the meantime. As long as the user doesn't swipe away the app from the
            // "recent apps", it can finish the job, that's exactly what we need.
            // Tested with Android 5.0, 8.1
            IAutoSynchronizationService syncService = Ioc.GetOrCreate<IAutoSynchronizationService>();
            syncService.SynchronizeAtShutdown();
            base.OnStop();
        }

        /// <inheritdoc/>
        protected override void OnPause()
        {
            base.OnPause();
            try
            {
                INavigationService navigationService = Ioc.GetOrCreate<INavigationService>();
                _lastNavigation = navigationService?.CurrentNavigation;
                navigationService.CurrentController?.StoreUnsavedData();
                navigationService.CurrentController?.Dispose();

                // Make sure that all open safes are closed.
                IRepositoryStorageService repositoryService = Ioc.GetOrCreate<IRepositoryStorageService>();
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

            INavigationService navigationService = Ioc.GetOrCreate<INavigationService>();
            IStoryBoardService storyBoardService = Ioc.GetOrCreate<IStoryBoardService>();

            if (!string.IsNullOrEmpty(_actionSendParameter))
            {
                // Create new note and show it
                IRepositoryStorageService repositoryStorageService = Ioc.GetOrCreate<IRepositoryStorageService>();
                ISettingsService settingsService = Ioc.GetOrCreate<ISettingsService>();

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
                    // Create a copy of the active story, which uses the Ioc of this new process
                    storyBoardService.ActiveStory = new SynchronizationStoryBoard(synchronizationStory);
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

                IAutoSynchronizationService syncService = Ioc.GetOrCreate<IAutoSynchronizationService>();
                syncService.SynchronizeAtStartup(); // no awaiting, run in background
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
            _activityResultAwaiter.OnActivityResult(requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }

        /// <inheritdoc/>
        public void LoadHtml(string html)
        {
            IBaseUrlService baseUrl = Ioc.GetOrCreate<IBaseUrlService>();
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
                INavigationService navigation = Ioc.GetOrCreate<INavigationService>();
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
                storyBoardService.ActiveStory.TryLoadFromSession(SynchronizationStorySessionKey.OauthRedirectUrl, out string _);
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
