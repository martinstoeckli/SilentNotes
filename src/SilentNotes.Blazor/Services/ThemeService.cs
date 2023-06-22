// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MudBlazor;
using MudBlazor.Utilities;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IThemeService"/> interface.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private const int DefaultWallpaper = 0;
        private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeService"/> class.
        /// </summary>
        /// <param name="settingsService">A settings service, which can get the current theme.</param>
        /// <param name="environmentService">A environment service, which can get device information.</param>
        public ThemeService(ISettingsService settingsService, IEnvironmentService environmentService)
        {
            _settingsService = settingsService;
            _environmentService = environmentService;

            Theme = new MudTheme()
            {
                Palette = new PaletteLight()
                {
                    //AppbarBackground = new MudColor("#323232"),
                },
                PaletteDark = new PaletteDark()
                {
                    //AppbarBackground = new MudColor("#323232"),
                },
            };
            Wallpapers = new List<WallpaperModel>();
            FillWallpapers(Wallpapers);
        }

        /// <inheritdoc/>
        public bool IsDarkMode
        {
            get
            {
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                switch (settings.ThemeMode)
                {
                    case ThemeMode.Auto:
                        return _environmentService.InDarkMode;
                    case ThemeMode.Dark:
                        return true;
                    case ThemeMode.Light:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(settings.ThemeMode));
                }
            }

            set
            {
                // Unused, MudBlazor requires a setter for binding
            }
        }

        /// <inheritdoc/>
        public MudTheme Theme { get; set; }

        /// <inheritdoc/>
        public Action RefreshGui { get; set; }

        private void FillWallpapers(List<WallpaperModel> themes)
        {
            themes.Add(new WallpaperModel("cork", "cork.jpg"));
            themes.Add(new WallpaperModel("forest", "forest.jpg"));
            themes.Add(new WallpaperModel("stone", "stone.jpg"));
            themes.Add(new WallpaperModel("blackstone", "blackstone.jpg"));
            themes.Add(new WallpaperModel("smarties", "smarties.jpg"));
            themes.Add(new WallpaperModel("grass", "grass.jpg"));
            themes.Add(new WallpaperModel("paper", "paper.jpg"));
            themes.Add(new WallpaperModel("sky", "sky.jpg"));
            themes.Add(new WallpaperModel("water", "water.jpg"));
            themes.Add(new WallpaperModel("sand", "sand.jpg"));
            themes.Add(new WallpaperModel("stars", "stars.jpg"));
        }

        ///// <inheritdoc/>
        //public bool DarkMode
        //{
        //    get
        //    {
        //        SettingsModel settings = _settingsService.LoadSettingsOrDefault();
        //        switch (settings.ThemeMode)
        //        {
        //            case ThemeMode.Auto:
        //                //return _environmentService.InDarkMode;
        //            case ThemeMode.Dark:
        //                return true;
        //            case ThemeMode.Light:
        //                return false;
        //            default:
        //                throw new ArgumentOutOfRangeException(nameof(settings.ThemeMode));
        //        }
        //    }
        //}

        ///// <inheritdoc/>
        //public string CssClassDark
        //{
        //    get { return DarkMode ? "dark" : string.Empty; }
        //}

        /// <inheritdoc/>
        public string CssBackground
        {
            get
            {
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                if (settings.UseSolidColorTheme)
                {
                    return string.Format("background-color: {0};", settings.ColorForSolidTheme);
                }
                else if (settings.UseWallpaper)
                {
                    int wallpaperIndex = FindWallpaperIndexOrDefault(settings.SelectedWallpaper);
                    if (wallpaperIndex >= 0)
                        return string.Format("background-image: url(wallpapers/{0});", Wallpapers[wallpaperIndex].Image);
                }
                return string.Empty;
            }
        }

        /// <inheritdoc/>
        public List<WallpaperModel> Wallpapers { get; private set; }

        /// <inheritdoc/>
        public int FindWallpaperIndexOrDefault(string themeId)
        {
            int result = Wallpapers.FindIndex(item => string.Equals(item.Id, themeId));
            return result >= 0 ? result : DefaultWallpaper;
        }
    }
}
