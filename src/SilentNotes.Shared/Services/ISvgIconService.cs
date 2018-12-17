// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

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
        /// <returns>A string containing the SVG vector graphic.</returns>
        string LoadIcon(string id);

        /// <summary>
        /// Loads only the path of a given vector graphic icon without the SVG element. This can be
        /// useful, if the SVG HTML element requires attributes like class or style="fill:#a8a8a8".
        /// </summary>
        /// <param name="id">Id of the vector graphic.</param>
        /// <returns>A string containing the SVG vector graphic path.</returns>
        string LoadIconSvgPath(string id);
    }
}
