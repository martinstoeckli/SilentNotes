// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Drawing;
using System.Threading.Tasks;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Allows to interact with a view containing a WebView control, to show Html pages.
    /// </summary>
    public interface IHtmlView
    {
        /// <summary>
        /// Loads and displays an html string as content.
        /// </summary>
        /// <param name="html">The html page to display</param>
        void LoadHtml(string html);

        /// <summary>
        /// Replaces a node in the currently loaded HTML document (like an AJAX call would do).
        /// </summary>
        /// <param name="nodeId">The node with this id will be replaced in the HTML dom.</param>
        /// <param name="newHtml">New Html code for the node.</param>
        void ReplaceNode(string nodeId, string newHtml);

        /// <summary>
        /// Executes Java script in the browser.
        /// </summary>
        /// <param name="script">Java script to execute.</param>
        void ExecuteJavaScript(string script);

        /// <summary>
        /// Executes Java script in the browser and returns the resulting string.
        /// </summary>
        /// <param name="script">Java script to execute.</param>
        /// <returns>Result of the called java script function.</returns>
        Task<string> ExecuteJavaScriptReturnString(string script);

        /// <summary>
        /// Sets the default background color of the webview, this is the color shown while loading
        /// an HTML page or when the css background color is transparent. This method is called as
        /// early as possible, to avoid the white flicker when using dark themes, which is visible
        /// until the html page has finished loading.
        /// </summary>
        /// <param name="backgroundColor">The default background color.</param>
        void SetBackgroundColor(Color backgroundColor);

        /// <summary>
        /// This event is called whenever the user clicks a link or the javascript triggers a
        /// navigation. This is the usual way to communicate from the (HTML) WebView to the viewmodel.
        /// </summary>
        event HtmlViewNavigatingEventHandler Navigating;

        /// <summary>
        /// This event is called whenever the page has successfully finished loading.
        /// </summary>
        event EventHandler NavigationCompleted;
    }

    /// <summary>
    /// Defines how the event handler of <see cref="IHtmlView.Navigating"/> must look like.
    /// </summary>
    /// <param name="sender">Sender which triggered the event.</param>
    /// <param name="uri">The uri which should be handled.</param>
    public delegate void HtmlViewNavigatingEventHandler(object sender, string uri);
}
