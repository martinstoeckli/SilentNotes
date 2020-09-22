// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Can shorten HTML content, so that only the begin of a long document is kept, but the HTML
    /// tag hierarchy keeps intact.
    /// </summary>
    public class HtmlShortener
    {
        private static char[] _tagCharacters = { '<', '>', '/', ' ', '\t' };
        private static string[] _visibleBlockTagNames = { "p", "h1", "h2", "h3", "li", "div", "dt", "dd", "blockquote", "pre", "table" };
        private readonly Regex _findTagsRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlShortener"/> class.
        /// </summary>
        public HtmlShortener()
        {
            MinimumLengthForShortening = 400;
            WantedTagNumber = 8;
            WantedLength = 400;
            _findTagsRegex = new Regex("<[^>]+>", RegexOptions.None); // Any HTML tag
        }

        /// <summary>
        /// Gets or sets the minimum characters an HTML must contain, so that is considered for
        /// shortening.
        /// </summary>
        public int MinimumLengthForShortening { get; set; }

        /// <summary>
        /// Gets or sets the number of visible block tags the shortened HTML should contain.
        /// Whatever is reached first, <see cref="WantedTagNumber"/> or <see cref="WantedLength"/>,
        /// will define the last considered tag.
        /// The resulting HTML can possibly be longer, because all tags must be closed correctly.
        /// </summary>
        public int WantedTagNumber { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of characters the shortened HTML should contain.
        /// Whatever is reached first, <see cref="WantedTagNumber"/> or <see cref="WantedLength"/>,
        /// will define the last considered tag.
        /// The resulting HTML can possibly be longer, because all tags must be closed correctly.
        /// </summary>
        public int WantedLength { get; set; }

        /// <summary>
        /// Truncates a long HTML document, so that only the start of the document can be displayed.
        /// The resulting content starts with the first visible block tag and is stripped of the
        /// enclosing html/body tag.
        /// </summary>
        /// <param name="content">Html content to shorten.</param>
        /// <returns>Shortened HTML contant or the original content if it was short enough.</returns>
        public string Shorten(string content)
        {
            if ((content == null) || (content.Length <= MinimumLengthForShortening))
                return content;

            int tagCount = 0;
            Match firstTag = null;
            Stack<string> tagNames = new Stack<string>();
            foreach (Match foundTag in EnumerateTags(content))
            {
                string tagName = GetTagName(foundTag.Value);
                if (IsVisibleBlockTag(tagName))
                {
                    if (IsOpeningTag(foundTag.Value))
                    {
                        tagNames.Push(tagName);
                        if (firstTag == null)
                            firstTag = foundTag;
                    }
                    else
                    {
                        if (tagName == tagNames.Peek())
                        {
                            tagNames.Pop();

                            if (tagNames.Count == 0)
                            {
                                tagCount++;
                                int length = foundTag.Index + foundTag.Length - firstTag.Index;
                                if (tagCount >= WantedTagNumber || length >= WantedLength)
                                {
                                    return content.Substring(firstTag.Index, length);
                                }
                            }
                        }
                    }
                }
            }
            return content;
        }

        private IEnumerable<Match> EnumerateTags(string content)
        {
            Match foundTag = _findTagsRegex.Match(content, 0);
            while (foundTag.Success)
            {
                yield return foundTag;
                foundTag = _findTagsRegex.Match(content, foundTag.Index + foundTag.Length);
            }
        }

        private static string GetTagName(string tag)
        {
            string tagContent = tag.Trim(_tagCharacters);
            int position = tagContent.IndexOfAny(_tagCharacters);
            if (position > 0)
                tagContent = tagContent.Remove(position);
            return tagContent.ToLowerInvariant();
        }

        private static bool IsVisibleBlockTag(string tagName)
        {
            return Array.IndexOf(_visibleBlockTagNames, tagName) >= 0;
        }

        private static bool IsOpeningTag(string tag)
        {
            return !tag.StartsWith("</");
        }
    }
}
