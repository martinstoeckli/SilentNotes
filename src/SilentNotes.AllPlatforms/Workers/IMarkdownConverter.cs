// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Workers
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
        string MarkdownToHtml(string markdown);

        /// <summary>
        /// Converts a text in HTML format to Markdown.
        /// </summary>
        /// <param name="html">Text in HTML format.</param>
        /// <returns>Converted Markdown text.</returns>
        string HtmlToMarkdown(string html);
    }
}
