// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace SilentNotes.Services
{
    /// <summary>
    /// The SvgService provides available vector graphics, e.g. for icons.
    /// Those graphics are returned as &lt;svg&gt;...&lt;/svg&gt; HTML tags and can be placed
    /// inside an HTML page.
    /// </summary>
    public interface ISvgIconService
    {
        ///// <summary>
        ///// Indexer property, shortcut for <see cref="LoadIcon(string, IEnumerable{KeyValuePair{string, string}})"/>.
        ///// </summary>
        ///// <param name="id">Id of the vector graphic.</param>
        ///// <returns>A string containing the SVG vector graphic.</returns>
        //MarkupString this[string id] { get; }

        ///// <summary>
        ///// Loads a given vector graphic icon.
        ///// The icons are usually without color information, but they can be styled with the CSS
        ///// styles "fill" and "stroke" of the SVG element.
        ///// </summary>
        ///// <param name="id">Id of the vector graphic.</param>
        ///// <param name="attributes">Optional attributes, which can be added to the icon, so it can
        ///// be customized.</param>
        ///// <returns>A string containing the SVG vector graphic.</returns>
        //MarkupString LoadIcon(string id, IEnumerable<KeyValuePair<string, string>> attributes = null);

        /// <summary>
        /// Gets the svg of an icon specified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <returns>A string containing the SVG vector graphic.</returns>
        String this[string id] { get; }

        /// <summary>
        /// Like <see cref="EmbedLinkableSvg(string)"/> it loads a given vector graphic icons, which
        /// can be defined in a hidden div container and linked to with <see cref="GetSvgLink(string, int)"/>.
        /// </summary>
        /// <param name="ids">List of ids of required icons.</param>
        /// <returns>A string containing the SVG vector graphics.</returns>
        MarkupString EmbedLinkableSvgs(IEnumerable<string> ids);

        /// <summary>
        /// Loads a given vector graphic icon, which can be defined in a hidden div container and
        /// linked to with <see cref="GetSvgLink(string, int)"/>.
        /// The icons are usually without color information, but they can be styled with the CSS
        /// styles "fill" and "stroke" of the SVG element.
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <returns>A string containing the SVG vector graphic.</returns>
        MarkupString EmbedLinkableSvg(string id);

        /// <summary>
        /// Creates a link to a vector graphic icon, which was defined by <see cref="EmbedLinkableSvg(string)"/>.
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <param name="size">An optional size for the icon (width and height).</param>
        /// <returns></returns>
        string GetSvgLink(string id, int size = 24);

        MarkupString EmbedSvgLink(string id, int size = 24);

        /// <summary>
        /// Loads a given vector graphic icon and encodes it, so it can be used inside a CSS url().
        /// The icons are usually without color information, but they can be styled with the CSS
        /// styles "fill" and "stroke" of the SVG element.
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <param name="attributes">Optional attributes, which can be added to the icon, so it can
        /// be customized.</param>
        /// <returns>A string containing the SVG url, like: url("data:image/svg+xml,...").</returns>
        //string LoadIconAsCssUrl(string id, IEnumerable<KeyValuePair<string, string>> attributes = null);
    }
}
