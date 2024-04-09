// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
    /// <summary>
    /// Serializeable collection of colors, which can be used to style the application.
    /// </summary>
    public class ColorThemeModel
    {
        private ColorSetModel _lightColors;
        private ColorSetModel _darkColors;

        /// <summary>
        /// Gets or sets the name of the theme.
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a set of colors for the light theme.
        /// </summary>
        [XmlAttribute(AttributeName = "light_colors")]
        public ColorSetModel LightColors
        {
            get { return _lightColors ?? (_lightColors = new ColorSetModel()); }
            set { _lightColors = value; }
        }

        /// <summary>
        /// Gets or sets a set of colors for the dark theme.
        /// </summary>
        [XmlAttribute(AttributeName = "dark_colors")]
        public ColorSetModel DarkColors
        {
            get { return _darkColors ?? (_darkColors = new ColorSetModel()); }
            set { _darkColors = value; }
        }

        /// <summary>
        /// Describes a set of configurable colors.
        /// </summary>
        public class ColorSetModel
        {
            /// <summary>
            /// Gets or sets the primary color, used for titles, fab buttons, etc
            /// </summary>
            [XmlAttribute(AttributeName = "primary")]
            public string Primary { get; set; }

            /// <summary>
            /// Gets or sets the secondary color, e.g. for controls.
            /// </summary>
            [XmlAttribute(AttributeName = "secondary")]
            public string Secondary { get; set; }

            /// <summary>
            /// Gets or sets the color for decorations, e.g. for information icons.
            /// </summary>
            [XmlAttribute(AttributeName = "decoration")]
            public string Decoration { get; set; }

            /// <summary>
            /// Gets of sets the background color of the app bar.
            /// </summary>
            [XmlAttribute(AttributeName = "app_bar")]
            public string AppBar { get; set; }
        }
    }

    /// <summary>
    /// Extension methods to the <see cref="ColorThemeModel"/> class.
    /// </summary>
    public static class ColorThemeExtensions
    {
        /// <summary>
        /// Searches a list for a theme by name and returns the found result. If no theme was found it
        /// returns the first entry of the list.
        /// </summary>
        /// <param name="themeName">Name of the theme to search for. The search is case insensitive.</param>
        /// <returns>Found color theme, or default.</returns>
        public static ColorThemeModel FindByNameOrDefault(this IEnumerable<ColorThemeModel> items, string themeName)
        {
            return items.FirstOrDefault(
                item => string.Equals(item.Name, themeName, StringComparison.InvariantCultureIgnoreCase),
                items.FirstOrDefault());
        }
    }
}