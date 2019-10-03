// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeService"/> class.
        /// </summary>
        /// <param name="settingsService"></param>
        public ThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            Themes = new List<ThemeModel>();
            FillThemes(Themes);
        }

        private void FillThemes(List<ThemeModel> themes)
        {
            themes.Add(new ThemeModel("cork", "cork.jpg", "#e7c59f"));
            themes.Add(new ThemeModel("forest", "forest.jpg", "#497a2b"));
            themes.Add(new ThemeModel("stone", "stone.jpg", "#948e97"));
            themes.Add(new ThemeModel("grass", "grass.jpg", "#90d84c"));
            themes.Add(new ThemeModel("paper", "paper.jpg", "#fffcf9"));
            themes.Add(new ThemeModel("sky", "sky.jpg", "#6fa7d6"));
            themes.Add(new ThemeModel("water", "water.jpg", "#41a7d0"));
            themes.Add(new ThemeModel("sand", "sand.jpg", "#f3cb98"));
            themes.Add(new ThemeModel("stars", "stars.jpg", "#000518"));
            themes.Add(new ThemeModel("meadow", "meadow.jpg", "#1f6002"));
        }

        /// <inheritdoc/>
        public List<ThemeModel> Themes { get; private set; }

        /// <inheritdoc/>
        public ThemeModel SelectedTheme
        {
            get
            {
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                return FindThemeOrDefault(settings.SelectedTheme);
            }
        }

        /// <inheritdoc/>
        public ThemeModel FindThemeOrDefault(string themeId)
        {
            ThemeModel result = Themes.Find(item => string.Equals(item.Id, themeId));
            return result ?? (result = Themes[DefaultTheme]);
        }
    }
}
