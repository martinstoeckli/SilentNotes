// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using MudBlazor;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Can get informations about the current theme.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets or sets a value indicating whether the app should show its dark theme.
        /// </summary>
        bool IsDarkMode { get; set; }

        /// <summary>
        /// Gets or sets the current theme.
        /// </summary>
        MudTheme Theme { get; set; }

        /// <summary>
        /// Gets or sets an action which can redraw the GUI to react to changes of the theme.
        /// </summary>
        Action RedrawTheme { get; set; }

        /// <summary>
        /// Gets a value indicating whether the app should show its dark theme.
        /// </summary>
        bool DarkMode { get; }

        /// <summary>
        /// Gets the keyword "dark" or "" depending on <see cref="DarkMode"/>, which can be used
        /// inside the Html class attributes, to refer to the Css dark class.
        /// </summary>
        string CssClassDark { get; }

        /// <summary>
        /// Gets a Css attribute string if a solid background is defined or an empty string otherwise.
        /// Example: background-color: #121212;
        /// </summary>
        string CssBackgroundColor { get; }

        /// <summary>
        /// Gets a Css attribute string like:
        ///   background-image: url(images/image.png);
        /// </summary>
        string CssBackgroundImage { get; }

        /// <summary>
        /// Gets a list of all available themes.
        /// </summary>
        List<ThemeModel> Themes { get; }

        /// <summary>
        /// Gets the active theme selected by the user, or the default theme.
        /// </summary>
        /// <returns>Active theme.</returns>
        ThemeModel SelectedTheme { get; }

        /// <summary>
        /// Searches for the theme with a given <paramref name="themeId"/>. If no such theme can
        /// be found, the default theme is returned.
        /// </summary>
        /// <param name="themeId">Id of the theme to search for.</param>
        /// <returns>Found theme or default theme.</returns>
        ThemeModel FindThemeOrDefault(string themeId);
    }
}
