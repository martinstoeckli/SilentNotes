// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Views
{
    /// <summary>
    /// Stores the toggle states of all available text format styles.
    /// </summary>
    internal class FormatToggleStates
    {
        public bool Heading1 { get; set; }

        public bool Heading2 { get; set; }

        public bool Heading3 { get; set; }

        public bool Bold { get; set; }

        public bool Italic { get; set; }

        public bool Underline { get; set; }

        public bool Strike { get; set; }

        public bool Codeblock { get; set; }

        public bool Blockquote { get; set; }

        public bool Bulletlist { get; set; }

        public bool Orderedlist { get; set; }

        /// <summary>
        /// Loads the bool array which was passed from JavaScript, after the cursor position of the
        /// editor changed.
        /// </summary>
        /// <param name="states">Array of all format states at the current cursor position.</param>
        public void LoadFromJsArray(bool[] states)
        {
            Heading1 = states[0];
            Heading2 = states[1];
            Heading3 = states[2];
            Bold = states[3];
            Italic = states[4];
            Underline = states[5];
            Strike = states[6];
            Codeblock = states[7];
            Blockquote = states[8];
            Bulletlist = states[9];
            Orderedlist = states[10];
        }
    }
}
