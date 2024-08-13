// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
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
        private const int DefaultWallpaper = 0; // cork
        private const string DefaultColorTheme = "Classic";
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

            // For now we fix the classic color theme, so users get their accustoment environment,
            // later this will become configurable.
            ColorThemes = new List<ColorThemeModel>();
            FillColorThemes(ColorThemes);
            SelectedColorTheme = ColorThemes.FindByNameOrDefault(DefaultColorTheme);
            Theme = CreateMudTheme(SelectedColorTheme);

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

        /// <summary>
        /// Gets a list of available color themes.
        /// </summary>
        private List<ColorThemeModel> ColorThemes { get; }

        /// <summary>
        /// Gets the currenctly selected color theme.
        /// </summary>
        private ColorThemeModel SelectedColorTheme { get; set; }

        /// <inheritdoc/>
        public string LightOrDarkClass 
        {
            get { return IsDarkMode ? "theme-dark" : "theme-light"; }
        }

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

        /// <inheritdoc/>
        public string CssNoteRepositoryBackground
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
        public string CssTheme
        {
            get
            {
                var colorSet = IsDarkMode ? SelectedColorTheme.DarkColors : SelectedColorTheme.LightColors;

                MudColor colorPrimary = colorSet.Primary;
                MudColor colorHeader = IsDarkMode ? colorPrimary.ColorLighten(0.1) : colorPrimary.ColorDarken(0.1);

                StringBuilder result = new StringBuilder();
                result.Append(":root {");
                result.Append("--theme-appbar: " + colorSet.AppBar + ";");
                result.Append("--theme-appbar-toggled: " + colorSet.AppBarToggled + ";");
                result.Append("--theme-appbar-secondary: " + colorSet.AppBarSecondary + ";");

                // Dynamically created colors
                result.Append("--theme-header: " + colorHeader + ";");
                result.Append("}");
                return result.ToString();
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

        private static MudTheme CreateMudTheme(ColorThemeModel colorTheme)
        {
            return new MudTheme()
            {
                PaletteLight = new PaletteLight()
                {
                    AppbarBackground = colorTheme.LightColors.AppBar,
                    Primary = colorTheme.LightColors.Primary,
                    Secondary = colorTheme.LightColors.Decoration,
                    Tertiary = colorTheme.LightColors.Secondary,
                },
                PaletteDark = new PaletteDark()
                {
                    AppbarBackground = colorTheme.DarkColors.AppBar,
                    Primary = colorTheme.DarkColors.Primary,
                    Secondary = colorTheme.DarkColors.Decoration,
                    Tertiary = colorTheme.DarkColors.Secondary,
                },
                Typography = new Typography()
                {
                    Default = new Default()
                    {
                        FontSize = "14px",
                    },
                },
            };
        }

        private void FillColorThemes(List<ColorThemeModel> themes)
        {
            themes.Add(new ColorThemeModel
            {
                Name = "Standard",
                LightColors = new ColorThemeModel.ColorSetModel
                {
                    AppBar = "#594AE2",
                    Primary = "#594AE2",
                    Secondary = "#1EC8A5",
                    Decoration = "#FF4081",
                },
                DarkColors = new ColorThemeModel.ColorSetModel
                {
                    AppBar = "#776be7",
                    Primary = "#776be7",
                    Secondary = "#1EC8A5",
                    Decoration = "#FF4081",
                },
            });

            themes.Add(new ColorThemeModel
            {
                Name = "Tropical",
                LightColors = new ColorThemeModel.ColorSetModel
                {
                    AppBar = "#f05638",
                    Primary = "#f05638",
                    Secondary = "#e31c5e",
                    Decoration = "#f0f028",
                },
                DarkColors = new ColorThemeModel.ColorSetModel
                {
                    AppBar = "#db4729",
                    Primary = "#db4729",
                    Secondary = "#e31c5e",
                    Decoration = "#dce763",
                },
            });

            themes.Add(new ColorThemeModel
            {
                Name = "Classic",
                LightColors = new ColorThemeModel.ColorSetModel
                {
                    AppBar = "#323232", // Kind of dark grey
                    AppBarToggled = "#82b7e3", // Blue matt SilentNotes brightened
                    AppBarSecondary = "#1f8fe9", // Same as Primary
                    Primary = "#1f8fe9", // Kind of blue
                    Secondary = "#1f8fe9", // Same as Primary
                    Decoration = "#f5d062", // Yellow SilentNotes darkened
                },
                DarkColors = new ColorThemeModel.ColorSetModel
                {
                    AppBar = "#121212", // Kind of dark grey
                    AppBarToggled = "#68a9de", // Blue matt SilentNotes
                    AppBarSecondary = "#1f8fe9", // Same as Primary
                    Primary = "#1f8fe9", // Kind of blue
                    Secondary = "#68a9de", // Blue matt SilentNotes
                    Decoration = "#ffe083", // Yellow banana SilentNotes
                },
            });
        }
    }
}
