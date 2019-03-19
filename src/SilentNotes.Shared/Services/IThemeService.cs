// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Can get informations about the current theme.
    /// </summary>
    public interface IThemeService
    {
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
