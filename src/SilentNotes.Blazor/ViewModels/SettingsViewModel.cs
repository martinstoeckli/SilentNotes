using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilentNotes.Models;

namespace SilentNotes.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(SettingsModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the a solid color should be used instead of a
        /// background image.
        /// </summary>
        public bool UseSolidColorTheme
        {
            get { return Model.UseSolidColorTheme; }
            set { SetPropertyAndModified(Model.UseSolidColorTheme, value, (bool v) => Model.UseSolidColorTheme = v); }
        }

        /// <summary>
        /// Gets or sets the solid background color for the theme background. It depends on
        /// <see cref="UseSolidColorTheme"/> whether this value is respected.
        /// </summary>
        public string ColorForSolidThemeHex
        {
            get { return Model.ColorForSolidTheme; }
            set { SetPropertyAndModified(Model.ColorForSolidTheme, value, (string v) => Model.ColorForSolidTheme = v); }
        }

        private SettingsModel Model { get; }
    }
}
