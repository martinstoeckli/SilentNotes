// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using System.Web;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Webkit;
using Java.IO;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
using SilentNotes.Services;

namespace SilentNotes.Android
{
    /// <summary>
    /// The main activity of the Android app. Because this is a single page app, it is the only
    /// activity showing a window.
    /// </summary>
    [Activity(Label = "SilentNotes", Icon = "@drawable/icon", Theme = "@style/MainTheme.SplashScreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity, IHtmlView
    {
        private WebView _webView;

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

            Ioc.Reset(); // Theoretically the OnCreate can be called repeatedly without an OnDestroy between
            Startup.InitializeApplication(this);

            INavigationService navigation = Ioc.GetOrCreate<INavigationService>();
            navigation.Navigate(ControllerNames.NoteRepository);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            Ioc.Reset();
            base.OnDestroy();
        }

        /// <inheritdoc/>
        protected override void OnStop()
        {
            INavigationService navigationService = Ioc.GetOrCreate<INavigationService>();
            navigationService.CurrentController?.StoreUnsavedData();
            base.OnStop();
        }

        /// <inheritdoc/>
        public override void OnBackPressed()
        {
            // We tell the webview to close any open dropdowns like the menu, if there is nothing
            // to close, the function will signal an application close which we intercept in OnNavigating.
            ExecuteJavaScript("closeDropdownOrSignalBackPressed();");
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
            string encodedNewHtml = HttpUtility.JavaScriptStringEncode(newHtml, false);
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
        public event HtmlViewNavigatingEventHandler Navigating;

        private void OnNavigating(string url)
        {
            // Intercept the javascript signal for the BackPressed event raised in OnBackPressed.
            if (url.EndsWith("BackPressed"))
            {
                // Delegate the back pressed to the current controller
                INavigationService navigation = Ioc.GetOrCreate<INavigationService>();
                bool handled = false;
                navigation.CurrentController.OnGoBackPressed(out handled);

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
