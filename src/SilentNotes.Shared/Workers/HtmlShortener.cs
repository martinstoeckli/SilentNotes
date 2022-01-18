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

            TagTreeItem rootTag = new TagTreeItem();
            IEnumerator<Match> tagEnumerator = EnumerateTags(content).GetEnumerator();
            BuildTagTree(rootTag, tagEnumerator);

            int numberOfFoundTags = 0;
            int lengthOfMarkedTags = 0;
            MarkRetainedTags(rootTag, ref numberOfFoundTags, ref lengthOfMarkedTags);

            List<TagTreeItem> tagsToRemove = new List<TagTreeItem>();
            FindTagsToRemove(tagsToRemove, rootTag);

            if (tagsToRemove.Count > 0)
                return RemoveTags(content, tagsToRemove);
            else
                return content;
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

        /// <summary>
        /// Recursively walks the HTML tags and builds a tree of <see cref="TagTreeItem"/> objects.
        /// </summary>
        /// <param name="parent">The current element of the tree which should be extended.</param>
        /// <param name="tagEnumerator">The enumerator which can find the tags.</param>
        private void BuildTagTree(TagTreeItem parent, IEnumerator<Match> tagEnumerator)
        {
            while (tagEnumerator.MoveNext())
            {
                Match foundTag = tagEnumerator.Current;
                if (string.IsNullOrEmpty(foundTag.Value))
                    continue;

                string tagName = GetLowerCaseTagName(foundTag.Value);
                if (IsEmptyTag(tagName))
                {
                    TagTreeItem child = new TagTreeItem
                    {
                        Name = tagName,
                        StartTag = foundTag,
                        EndTag = null,
                    };
                    parent.Children.Add(child);
                }
                else if (IsOpeningTag(foundTag.Value))
                {
                    TagTreeItem child = new TagTreeItem
                    {
                        Name = tagName,
                        StartTag = foundTag,
                    };
                    parent.Children.Add(child);
                    BuildTagTree(child, tagEnumerator);
                }
                else
                {
                    if (string.Equals(tagName, parent.Name))
                    {
                        parent.EndTag = foundTag;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Walks the tree of tags, and sets the <see cref="TagTreeItem.Retained"/> to true, for so
        /// many tags as are inside the limits of <see cref="WantedTagNumber"/> and <see cref="WantedLength"/>.
        /// It assumes that all <see cref="TagTreeItem.Retained"/> are preset to false.
        /// </summary>
        /// <param name="tag">Decide for this tag.</param>
        /// <param name="numberOfMarkedTags">The sum of already marked tags.</param>
        /// <param name="lengthOfMarkedTags">The sum of the length of the already marked tags.</param>
        private void MarkRetainedTags(TagTreeItem tag, ref int numberOfMarkedTags, ref int lengthOfMarkedTags)
        {
            for (int index = 0; index < tag.ChildrenCount; index++)
            {
                TagTreeItem childTag = tag.Children[index];
                bool isRelevant = IsRelevantTag(childTag.Name);
                if (isRelevant)
                {
                    if ((numberOfMarkedTags < WantedTagNumber) && (lengthOfMarkedTags < WantedLength))
                    {
                        childTag.Retained = true;
                        numberOfMarkedTags++;
                        lengthOfMarkedTags += childTag.ContentLength;
                        MarkRetainedTags(childTag, ref numberOfMarkedTags, ref lengthOfMarkedTags);
                    }
                }
                else
                {
                    childTag.Retained = true;
                    MarkRetainedTags(childTag, ref numberOfMarkedTags, ref lengthOfMarkedTags);
                }
            }
        }

        /// <summary>
        /// Recursively walks the tag tree and fills <paramref name="result"/> with tags which can
        /// be removed to shorten the content. Only removeable top level tags are included, without
        /// their child tags.
        /// </summary>
        /// <param name="result">Receives the tags to remove.</param>
        /// <param name="tag">Check this tag and recursively its children.</param>
        private static void FindTagsToRemove(List<TagTreeItem> result, TagTreeItem tag)
        {
            for (int index = 0; index < tag.ChildrenCount; index++)
            {
                TagTreeItem childTag = tag.Children[index];
                if (childTag.Retained)
                {
                    FindTagsToRemove(result, childTag);
                }
                else
                {
                    result.Add(childTag);
                }
            }
        }

        private static string RemoveTags(string content, List<TagTreeItem> tagsToRemove)
        {
            // Remove the range of the characters covered by the tag, working from the end to
            // the begin of the content.
            StringBuilder result = new StringBuilder(content);
            for (int index = tagsToRemove.Count - 1; index >= 0; index--)
            {
                TagTreeItem tagToRemove = tagsToRemove[index];
                result.Remove(tagToRemove.StartTagPosition, tagToRemove.TagLength);
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

        private static bool IsOpeningTag(string tag)
        {
            return !tag.StartsWith("</");
        }

        private static Regex GetOrCreateRegex()
        {
            return _findTagsRegex ?? (_findTagsRegex = new Regex("<[^>]+>", RegexOptions.Compiled)); // Any HTML tag
        }

        private class TagTreeItem
        {
            private List<TagTreeItem> _children;

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

            public bool Retained { get; set; }

            public List<TagTreeItem> Children
            {
                get { return _children ?? (_children = new List<TagTreeItem>()); }
            }

            /// <summary>
            /// Gets the number of children. By using <see cref="ChildrenCount"/> instead of
            /// Children.Count we can avoid creation of unnecessary lists (lazy creation).
            /// </summary>
            public int ChildrenCount 
            { 
                get { return _children != null ? _children.Count : 0; }
            }
        }
    }
}
