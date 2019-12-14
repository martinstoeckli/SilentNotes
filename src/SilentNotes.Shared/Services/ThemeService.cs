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
        /// <param name="settingsService">A settings service, which can get the current theme.</param>
        public ThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            Themes = new List<ThemeModel>();
            FillThemes(Themes);
        }

        private void FillThemes(List<ThemeModel> themes)
        {
            themes.Add(new ThemeModel("cork", "cork.jpg", "#7d6346"));
            themes.Add(new ThemeModel("forest", "forest.jpg", "#5e9a4c"));
            themes.Add(new ThemeModel("stone", "stone.jpg", "#373033"));
            themes.Add(new ThemeModel("blackstone", "blackstone.jpg", "#323232"));
            themes.Add(new ThemeModel("grass", "grass.jpg", "#335726"));
            themes.Add(new ThemeModel("paper", "paper.jpg", "#fcf7f4"));
            themes.Add(new ThemeModel("sky", "sky.jpg", "#4f718d"));
            themes.Add(new ThemeModel("water", "water.jpg", "#2f637b"));
            themes.Add(new ThemeModel("sand", "sand.jpg", "#7c6951"));
            themes.Add(new ThemeModel("stars", "stars.jpg", "#000518"));
            themes.Add(new ThemeModel("meadow", "meadow.jpg", "#2b4426"));
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
