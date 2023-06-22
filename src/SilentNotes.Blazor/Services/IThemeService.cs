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
        Action RefreshGui { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the app should show its dark theme.
        ///// </summary>
        //bool DarkMode { get; }

        ///// <summary>
        ///// Gets the keyword "dark" or "" depending on <see cref="DarkMode"/>, which can be used
        ///// inside the Html class attributes, to refer to the Css dark class.
        ///// </summary>
        //string CssClassDark { get; }

        /// <summary>
        /// Gets a Css for a solid background or an attribute string like:
        /// <example>
        /// background-color: #121212;
        /// background-image: url(images/image.png);
        /// </example>
        /// </summary>
        string CssBackground { get; }

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
