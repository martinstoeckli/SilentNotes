// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace SilentNotes.Services
{
    /// <summary>
    /// The SvgService provides available vector graphics, e.g. for icons.
    /// Those graphics are returned as &lt;svg&gt;...&lt;/svg&gt; HTML tags and can be placed
    /// inside an HTML page.
    /// </summary>
    public interface ISvgIconService
    {
        /// <summary>
        /// Indexer property, shortcut for <see cref="LoadIcon(string)"/>.
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <returns>A string containing the SVG vector graphic.</returns>
        string this[string id] { get; }

        /// <summary>
        /// Loads a given vector graphic icon.
        /// The icons are usually without color information, but they can be styled with the CSS
        /// styles "fill" and "stroke" of the SVG element.
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <param name="attributes">Optional attributes, which can be added to the icon, so it can
        /// be customized.</param>
        /// <returns>A string containing the SVG vector graphic.</returns>
        string LoadIcon(string id, IEnumerable<KeyValuePair<string, string>> attributes = null);

        /// <summary>
        /// Loads a given vector graphic icon and encodes it, so it can be used inside a CSS url().
        /// The icons are usually without color information, but they can be styled with the CSS
        /// styles "fill" and "stroke" of the SVG element.
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <param name="attributes">Optional attributes, which can be added to the icon, so it can
        /// be customized.</param>
        /// <returns>A string containing the SVG url, like: url("data:image/svg+xml,...").</returns>
        string LoadIconAsCssUrl(string id, IEnumerable<KeyValuePair<string, string>> attributes = null);
    }
}
