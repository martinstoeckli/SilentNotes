// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes
{
    /// <summary>
    /// Defines methods for converting between Markdown and HTML formats.
    /// </summary>
    public interface IMarkdownConverter
    {
        /// <summary>
        /// Converts a text in Markdown format to HTML.
        /// </summary>
        /// <param name="markdown">Text in Markdown format.</param>
        /// <returns>Converted HTML text.</returns>
        ValueTask<string> MarkdownToHtml(string markdown);

        /// <summary>
        /// Converts a text in HTML format to Markdown.
        /// </summary>
        /// <param name="html">Text in HTML format.</param>
        /// <returns>Converted Markdown text.</returns>
        ValueTask<string> HtmlToMarkdown(string html);
    }

    /// <summary>
    /// Implementation of the <see cref="IMarkdownConverter"/> interface, using delegates to
    /// convert the Markdown.
    /// </summary>
    public class RelayMarkdownConverter : IMarkdownConverter
    {
        private readonly Func<string, ValueTask<string>> _htmlToMarkdown;
        private readonly Func<string, ValueTask<string>> _markdownToHtml;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayMarkdownConverter"/> class.
        /// </summary>
        /// <param name="htmlToMarkdown">Delegate which can convert from HTML to MarkDown.</param>
        /// <param name="markdownToHtml">Delegate which can convert from MarkDown to HTML.</param>
        public RelayMarkdownConverter(
            Func<string, ValueTask<string>> htmlToMarkdown,
            Func<string, ValueTask<string>> markdownToHtml)
        {
            _htmlToMarkdown = htmlToMarkdown;
            _markdownToHtml = markdownToHtml;
        }

        public ValueTask<string> HtmlToMarkdown(string html)
        {
            return _htmlToMarkdown(html);
        }

        public ValueTask<string> MarkdownToHtml(string markdown)
        {
            return _markdownToHtml(markdown);
        }
    }
}
