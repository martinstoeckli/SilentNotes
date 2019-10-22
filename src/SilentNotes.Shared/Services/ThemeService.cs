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
            themes.Add(new ThemeModel("forest", "forest.jpg", "#3f6b30"));
            themes.Add(new ThemeModel("stone", "stone.jpg", "#6e6664"));
            themes.Add(new ThemeModel("grass", "grass.jpg", "#519b22"));
            themes.Add(new ThemeModel("paper", "paper.jpg", "#fcf7f4"));
            themes.Add(new ThemeModel("sky", "sky.jpg", "#7aacd1"));
            themes.Add(new ThemeModel("water", "water.jpg", "#40a5d1"));
            themes.Add(new ThemeModel("sand", "sand.jpg", "#caa976"));
            themes.Add(new ThemeModel("stars", "stars.jpg", "#000518"));
            themes.Add(new ThemeModel("meadow", "meadow.jpg", "#457931"));
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
