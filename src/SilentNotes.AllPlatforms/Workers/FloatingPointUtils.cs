// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Globalization;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Helper functions for working with floating point values.
    /// </summary>
    public static class FloatingPointUtils
    {
        /// <summary>
        /// Formats a floating point value to an invariant string, so it can be used for
        /// culture neutral serialization. It always uses a decimal point as decimal separator,
        /// regardless of the current culture.
        /// </summary>
        /// <param name="value">Floating point value to format.</param>
        /// <param name="fmt">Optional format string.</param>
        /// <returns>Formatted value.</returns>
        public static string FormatInvariant(double value, string fmt = "0.###")
        {
            return value.ToString(fmt, CultureInfo.InvariantCulture);
        }
    }
}
