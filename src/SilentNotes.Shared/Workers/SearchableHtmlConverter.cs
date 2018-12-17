// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Offers methods to convert HTML source code to plain text, so it can be searched.
    /// </summary>
    public class SearchableHtmlConverter
    {
        private const char TagReplacement = (char)160; // The Nbsp acts as replacement for Html tags
        private readonly Regex _stripTagsRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchableHtmlConverter"/> class.
        /// </summary>
        public SearchableHtmlConverter()
        {
            _stripTagsRegex = new Regex("<[^>]+>", RegexOptions.None); // Any HTML tag
        }

        /// <summary>
        /// Converts HTML souce code to plain text which can be used to search for content.
        /// All whitespaces will be normalized, that means sequences of whitespaces are replaced
        /// with a single space character. HTML tags are stripped away.
        /// </summary>
        /// <param name="html">HTML souce code to convert.</param>
        /// <param name="searchableText">The simplified text which can be searched.</param>
        /// <returns>A text which can be used for searching.</returns>
        public bool TryConvertHtml(string html, out string searchableText)
        {
            searchableText = string.Empty;
            if (string.IsNullOrWhiteSpace(html))
                return true;

            try
            {
                string strippedHtml = _stripTagsRegex.Replace(html, TagReplacement.ToString());
                string decodedHtml = HttpUtility.HtmlDecode(strippedHtml);
                searchableText = NormalizeWhitespaces(decodedHtml);
                return true;
            }
            catch (Exception)
            {
                searchableText = html;
                return false;
            }
        }

        /// <summary>
        /// Replaces groups of any whitespace characters with a single space character, and
        /// removes leading and trailing whitespaces.
        /// </summary>
        /// <param name="text">Text to normalize.</param>
        /// <returns>Normalized searchable text.</returns>
        public static string NormalizeWhitespaces(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            StringBuilder result = new StringBuilder();
            bool firstNonWitespacePassed = false;
            bool rememberToAddDelimiter = false;
            bool rememberToAddDelimiterWhenNonWhitespaceFollows = false;
            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (c != TagReplacement)
                    {
                        rememberToAddDelimiter = true;
                    }
                    else if (firstNonWitespacePassed)
                    {
                        rememberToAddDelimiterWhenNonWhitespaceFollows = true;
                    }
                }
                else
                {
                    firstNonWitespacePassed = true;
                    if (rememberToAddDelimiter || rememberToAddDelimiterWhenNonWhitespaceFollows)
                        result.Append(" ");
                    result.Append(c);
                    rememberToAddDelimiter = false;
                    rememberToAddDelimiterWhenNonWhitespaceFollows = false;
                }
            }
            if (rememberToAddDelimiter)
                result.Append(" ");
            return result.ToString();
        }
    }
}
