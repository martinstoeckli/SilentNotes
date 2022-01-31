// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Can shorten HTML content, so that only the begin of a long document is kept, but the HTML
    /// tag hierarchy keeps intact.
    /// </summary>
    public class HtmlShortener
    {
        private static char[] _tagMarkupCharacters = { '<', '>', '/', ' ', '\t' };
        private static Regex _findTagsRegex;

        // Sorted list of tags which cannot have child nodes see: https://developer.mozilla.org/en-US/docs/Glossary/empty_element
        private static string[] _emptyTagNames = { "area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source", "track", "wbr" };

        // Sorted list of tags which are affecting the maximum number of tags / maximum length
        private static string[] _relevantTagNames = { "blockquote", "dd", "div", "dt", "h1", "h2", "h3", "h4", "li", "p", "pre", "table" };

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlShortener"/> class.
        /// </summary>
        public HtmlShortener()
        {
            MinimumLengthForShortening = 400;
            WantedTagNumber = 8;
            WantedLength = 400;
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
        /// Truncates a long HTML document, so that it is possible to show only the start of the
        /// document. The resulting content keeps the tag structure like lists, but removes elements
        /// if a certain length is reached.
        /// </summary>
        /// <param name="content">Html content to shorten.</param>
        /// <returns>Shortened HTML content or the original content if it was short enough.</returns>
        public string Shorten(string content)
        {
            if ((content == null) || (content.Length <= MinimumLengthForShortening))
                return content;

            Stack<TagInfo> branchToLastItem = new Stack<TagInfo>();
            IEnumerator<Match> tagEnumerator = EnumerateTags(content).GetEnumerator();
            int numberOfFoundTags = 0;
            int lengthOfFoundTags = 0;
            TagInfo lastItem = null;
            FindLastItemInsideLimits(ref lastItem, branchToLastItem, tagEnumerator, ref numberOfFoundTags, ref lengthOfFoundTags);

            return BuildShortenedContent(content, lastItem, branchToLastItem);
        }

        /// <summary>
        /// Searches the HTML content and enumerates all found opening/closing HTML tags.
        /// </summary>
        /// <param name="content">The HTML content to search.</param>
        /// <returns>Found matches of tags.</returns>
        private static IEnumerable<Match> EnumerateTags(string content)
        {
            Regex htmlTagRegex = GetOrCreateRegex();
            Match foundTag = htmlTagRegex.Match(content, 0);
            while (foundTag.Success)
            {
                yield return foundTag;
                foundTag = htmlTagRegex.Match(content, foundTag.Index + foundTag.Length);
            }
        }

        private void FindLastItemInsideLimits(
            ref TagInfo result,
            Stack<TagInfo> branchToLastItem,
            IEnumerator<Match> tagEnumerator,
            ref int numberOfFoundTags,
            ref int lengthOfFoundTags)
        {
            while ((numberOfFoundTags < WantedTagNumber) && (lengthOfFoundTags < WantedLength) && tagEnumerator.MoveNext())
            {
                Match foundTag = tagEnumerator.Current;
                if (string.IsNullOrEmpty(foundTag.Value))
                    continue;

                string tagName = GetLowerCaseTagName(foundTag.Value);
                TagType tagType = GetTagType(foundTag.Value);

                if (IsEmptyTag(tagName) || (tagType == TagType.OpeningAndClosing))
                {
                    if (IsRelevantTag(tagName))
                    {
                        numberOfFoundTags++;
                    }
                }
                else if (tagType == TagType.Opening)
                {
                    var child = new TagInfo
                    {
                        Name = tagName,
                        StartTag = foundTag,
                    };
                    branchToLastItem.Push(child);
                }
                else if (tagType == TagType.Closing)
                {
                    if (branchToLastItem.Count == 0)
                        continue;

                    TagInfo lastTag = branchToLastItem.Peek();
                    if (string.Equals(tagName, lastTag.Name))
                    {
                        result = branchToLastItem.Pop();
                        result.EndTag = foundTag;

                        if (IsRelevantTag(tagName))
                        {
                            numberOfFoundTags++;
                            lengthOfFoundTags += result.ContentLength;
                        }
                    }
                }
            }
        }

        private string BuildShortenedContent(string content, TagInfo lastItem, Stack<TagInfo> branchToLastItem)
        {
            if (lastItem == null)
                return content;

            string partToLastItem = content.Substring(0, lastItem.StartTagPosition + lastItem.TagLength);
            if (branchToLastItem.Count == 0)
                return partToLastItem;

            StringBuilder result = new StringBuilder(partToLastItem);
            while (branchToLastItem.Count > 0)
            {
                TagInfo itemToClose = branchToLastItem.Pop();
                result.Append("</");
                result.Append(itemToClose.Name);
                result.Append(">");
            }
            return result.ToString();
        }

        private static string GetLowerCaseTagName(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return string.Empty;

            string tagContent = tag.Trim(_tagMarkupCharacters);
            int position = tagContent.IndexOfAny(_tagMarkupCharacters);
            if (position > 0)
                tagContent = tagContent.Remove(position);
            return tagContent.ToLowerInvariant();
        }

        /// <summary>
        /// Gets a value indicating whether the tag should be counted when limiting by
        /// <see cref="WantedTagNumber"/>.
        /// </summary>
        /// <param name="tagName">Name of the tag which is countable or not.</param>
        /// <returns>Returns true if the tag should be counted, otherwise false.</returns>
        private static bool IsRelevantTag(string tagName)
        {
            return Array.BinarySearch(_relevantTagNames, tagName) >= 0;
        }

        private static bool IsEmptyTag(string tagName)
        {
            return Array.BinarySearch(_emptyTagNames, tagName) >= 0;
        }

        private static TagType GetTagType(string tag)
        {
            tag = tag.Replace(" ", "");
            if (tag.StartsWith("</"))
                return TagType.Closing;
            else if (tag.EndsWith("/>"))
                return TagType.OpeningAndClosing;
            else if (tag.StartsWith("<!"))
                return TagType.DocType;
            else
                return TagType.Opening;
        }

        private static Regex GetOrCreateRegex()
        {
            return _findTagsRegex ?? (_findTagsRegex = new Regex("<[^>]+>", RegexOptions.Compiled)); // Any HTML tag
        }

        private class TagInfo
        {
            public string Name { get; set; }

            public Match StartTag { get; set; }

            public Match EndTag { get; set; }

            public int StartTagPosition
            {
                get { return StartTag.Index; }
            }

            public int ContentLength
            {
                get { return (EndTag != null) ? EndTag.Index - StartTag.Index - StartTag.Length : 0; }
            }

            public int TagLength
            {
                get { return (EndTag != null) ? EndTag.Index + EndTag.Length - StartTag.Index : 0; }
            }
        }

        private enum TagType
        {
            Opening,

            Closing,

            OpeningAndClosing,

            DocType,
        }
    }
}
