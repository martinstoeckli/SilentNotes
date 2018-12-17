// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using System.Web;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SilentNotes.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IHtmlView
    {
        private WebView _webView;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            _webView = webView;
            _webView.Settings.IsJavaScriptEnabled = true;
            _webView.Settings.IsIndexedDBEnabled = false;
            _webView.NavigationStarting += NavigatingStartingEventHandler;
            _webView.NavigationCompleted += NavigationCompletedEventHandler;

            Startup.InitializeApplication(this);
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            // Is triggered when the <see cref="MainPage"/> is loading.
            INavigationService navigation = Ioc.GetOrCreate<INavigationService>();
            navigation.Navigate(ControllerNames.NoteRepository, null);
        }

        private void NavigatingStartingEventHandler(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null)
            {
                OnNavigating(args.Uri.OriginalString);
                args.Cancel = true;
            }
        }

        private void NavigationCompletedEventHandler(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if ((NavigationCompleted != null) && args.IsSuccess)
                NavigationCompleted(sender, new EventArgs());
        }

        /// <inheritdoc/>
        public void LoadHtml(string html)
        {
            _webView.NavigateToString(html);
        }

        /// <inheritdoc/>
        public void ReplaceNode(string nodeId, string newHtml)
        {
            string encodedNewHtml = HttpUtility.JavaScriptStringEncode(newHtml, false);
            string script = string.Format("document.getElementById('{0}').outerHTML = \"{1}\";", nodeId, encodedNewHtml);
            ExecuteJavaScript(script);
        }

        /// <inheritdoc/>
        public async void ExecuteJavaScript(string script)
        {
            await _webView.InvokeScriptAsync("eval", new string[] { script });
        }

        /// <inheritdoc/>
        public async Task<string> ExecuteJavaScriptReturnString(string script)
        {
            var taskCompletion = new TaskCompletionSource<string>();
            await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    string res = await _webView.InvokeScriptAsync("eval", new string[] { script });
                    taskCompletion.SetResult(res);
                });
            string result = await taskCompletion.Task;
            return result;
        }

        /// <inheritdoc/>
        public event HtmlViewNavigatingEventHandler Navigating;

        private void OnNavigating(string url)
        {
            if (Navigating != null)
                Navigating(this, url);
        }

        /// <inheritdoc/>
        public event EventHandler NavigationCompleted;
    }
}
