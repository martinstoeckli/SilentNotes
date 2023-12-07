// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    internal static class ViewModelBaseExtensions
    {
        /// <summary>
        /// Gets the base font size [px] of the notes, from which the relative sizes are derrived.
        /// </summary>
        /// <param name="viewModel">The viewmodel to extend.</param>
        /// <param name="settingsService">A service which can read the settings.</param>
        /// <param name="overview">Value indicating whether the text is shown in the overview note list.</param>
        public static string GetNoteBaseFontSize(this ViewModelBase viewModel, ISettingsService settingsService, bool overview)
        {
            double defaultBaseFontSize = SettingsViewModel.ReferenceFontSize;
#if WINDOWS
            if (overview)
                defaultBaseFontSize = SettingsViewModel.ReferenceFontSize - 2;
#endif
            var settings = settingsService?.LoadSettingsOrDefault();
            double scale = settings != null ? settings.FontScale : 1.0;
            double fontSize = defaultBaseFontSize * scale;
            return FloatingPointUtils.FormatInvariant(fontSize);
        }
    }
}
