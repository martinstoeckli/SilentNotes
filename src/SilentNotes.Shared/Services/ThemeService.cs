// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IThemeService"/> interface.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private const int DefaultTheme = 2;
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
            Themes = new List<ThemeModel>();
            FillThemes(Themes);
        }

        private void FillThemes(List<ThemeModel> themes)
        {
            themes.Add(new ThemeModel("cork", "cork.jpg"));
            themes.Add(new ThemeModel("forest", "forest.jpg"));
            themes.Add(new ThemeModel("stone", "stone.jpg"));
            themes.Add(new ThemeModel("blackstone", "blackstone.jpg"));
            themes.Add(new ThemeModel("smarties", "smarties.jpg"));
            themes.Add(new ThemeModel("grass", "grass.jpg"));
            themes.Add(new ThemeModel("paper", "paper.jpg"));
            themes.Add(new ThemeModel("sky", "sky.jpg"));
            themes.Add(new ThemeModel("water", "water.jpg"));
            themes.Add(new ThemeModel("sand", "sand.jpg"));
            themes.Add(new ThemeModel("stars", "stars.jpg"));
            themes.Add(new ThemeModel("meadow", "meadow.jpg"));
        }

        /// <inheritdoc/>
        public bool DarkMode
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
        }

        /// <inheritdoc/>
        public string CssClassDark
        {
            get { return DarkMode ? "dark" : string.Empty; }
        }

        /// <inheritdoc/>
        public string CssBackgroundColor 
        {
            get
            {
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                if (settings.UseSolidColorTheme)
                    return string.Format("background-color: {0};", settings.ColorForSolidTheme);
                else
                    return string.Empty;
            }
        }

        /// <inheritdoc/>
        public string CssBackgroundImage 
        {
            get
            {
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                return settings.UseSolidColorTheme
                    ? string.Empty
                    : string.Format("background-image: url({0});", SelectedTheme.Image);
            }
        }

        /// <inheritdoc/>
        public List<ThemeModel> Themes { get; private set; }

        /// <inheritdoc/>
        public ThemeModel SelectedTheme
        {
            get { return FindThemeOrDefault(_settingsService.LoadSettingsOrDefault().SelectedTheme); }
        }

        /// <inheritdoc/>
        public ThemeModel FindThemeOrDefault(string themeId)
        {
            ThemeModel result = Themes.Find(item => string.Equals(item.Id, themeId));
            return result ?? (result = Themes[DefaultTheme]);
        }
    }
}
