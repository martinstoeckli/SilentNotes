// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Drawing;
using System.Threading.Tasks;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.Workers;
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
            _webView.NewWindowRequested += NewWindowRequestedEventHandler;
            _webView.UnsupportedUriSchemeIdentified += UnsupportedUriSchemeIdentifiedEventHandler;

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
            // This event is called when a link is clicked or when JS tries to navigate.
            if (TryExtractOriginalUrl(args, out string url))
            {
                args.Cancel = true;
                OnNavigating(url);
            }
        }

        private void NavigationCompletedEventHandler(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if ((NavigationCompleted != null) && args.IsSuccess)
                NavigationCompleted(sender, new EventArgs());
        }

        private void NewWindowRequestedEventHandler(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            // This event is called when a link with an external target is clicked.
            args.Handled = true;
            if (TryExtractOriginalUrl(args, out string url))
                OnNavigating(url);
        }

        private void UnsupportedUriSchemeIdentifiedEventHandler(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            // This event is called when a link like mailto:// is called.
            args.Handled = true;
            if (TryExtractOriginalUrl(args, out string url))
                OnNavigating(url);
        }

        private static bool TryExtractOriginalUrl(WebViewNavigationStartingEventArgs args, out string url)
        {
            try
            {
                if (args?.Uri != null)
                {
                    url = args.Uri.OriginalString;
                    return true;
                }
            }
            catch
            {
                // It is possible that even calling the getter "args.Uri" already results in an
                // exception, e.g. when the event was triggered with a "tel://*" protocol.
            }
            url = null;
            return false;
        }

        private static bool TryExtractOriginalUrl(WebViewNewWindowRequestedEventArgs args, out string url)
        {
            try
            {
                if (args?.Uri != null)
                {
                    url = args.Uri.OriginalString;
                    return true;
                }
            }
            catch
            {
                // It is possible that even calling the getter "args.Uri" already results in an
                // exception, e.g. when the event was triggered with a "tel://*" protocol.
            }
            url = null;
            return false;
        }

        private static bool TryExtractOriginalUrl(WebViewUnsupportedUriSchemeIdentifiedEventArgs args, out string url)
        {
            try
            {
                if (args?.Uri != null)
                {
                    url = args.Uri.OriginalString;
                    return true;
                }
            }
            catch
            {
                // It is possible that even calling the getter "args.Uri" already results in an
                // exception, e.g. when the event was triggered with a "tel://*" protocol.
            }
            url = null;
            return false;
        }

        /// <inheritdoc/>
        public void LoadHtml(string html)
        {
            _webView.NavigateToString(html);
        }

        /// <inheritdoc/>
        public void ReplaceNode(string nodeId, string newHtml)
        {
            string encodedNewHtml = WebviewUtils.EscapeJavaScriptString(newHtml);
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
        public void SetBackgroundColor(Color backgroundColor)
        {
            _webView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
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
