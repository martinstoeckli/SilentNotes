// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Drawing;
using System.Globalization;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Extension methods for color values.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Determines whether the color is a dark color. A color is dark, if its brighness is
        /// below the middle of white and black.
        /// </summary>
        /// <param name="color">Color to test.</param>
        /// <returns>Returns true if the color is dark, otherwise false.</returns>
        public static bool IsDark(this System.Drawing.Color color)
        {
            float brightness = color.GetBrightness();
            return brightness < 0.5;
        }

        /// <summary>
        /// Converts a color in the HTML hex format to a color value.
        /// </summary>
        /// <param name="colorHex">HTML color of the form #rrggbb</param>
        /// <returns>Color value.</returns>
        public static System.Drawing.Color HexToColor(string colorHex)
        {
            colorHex = colorHex.Replace("#", string.Empty); // strip off # if it exists

            // Add alpha value if necessary
            bool hasAlpha = colorHex.Length > 6;
            if (!hasAlpha)
                colorHex = "ff" + colorHex;

            int colorInt = int.Parse(colorHex, NumberStyles.HexNumber);
            return System.Drawing.Color.FromArgb(colorInt);
        }
    }
}
