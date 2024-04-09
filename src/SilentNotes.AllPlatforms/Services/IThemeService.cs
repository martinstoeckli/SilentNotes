// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
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
        /// Gets the keyword "theme-light" or "theme-dark" depending on <see cref="IsDarkMode"/>,
        /// which is applied as css class to the MainLayout. Css definitions in app-colors.css can
        /// refer to this class.
        /// </summary>
        string LightOrDarkClass { get; }

        /// <summary>
        /// Gets a Css for a solid background or an attribute string like:
        /// <example>
        /// background-color: #121212;
        /// background-image: url(images/image.png);
        /// </example>
        /// </summary>
        string CssNoteRepositoryBackground { get; }

        /// <summary>
        /// Gets a list of all available wallpapers.
        /// </summary>
        List<WallpaperModel> Wallpapers { get; }

        /// <summary>
        /// Searches for the index of the wallpaper with a given <paramref name="wallpaperId"/>.
        /// If no such wallpaper can be found, the index of the default wallpaper is returned.
        /// </summary>
        /// <param name="wallpaperId">Id of the wallpaper to search for.</param>
        /// <returns>Found wallpaper index or index of default wallpaper.</returns>
        int FindWallpaperIndexOrDefault(string wallpaperId);
    }
}
