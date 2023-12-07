// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Stories
{
    /// <summary>
    /// Enumeration of all modes, a story board can be started with.
    /// </summary>
    [Flags]
    public enum StoryMode
    {
        /// <summary>Runs in silent mode, doesn't show any dialogs. If the story fails, it does it
        /// silently without informing the user.</summary>
        Silent = 0,

        /// <summary>Shows waiting circle / hour clock.</summary>
        BusyIndicator = 1,

        /// <summary>Shows toast messages.</summary>
        Toasts = 2,

        /// <summary>Shows messageboxes.</summary>
        Messages = 4,

        /// <summary>Shows pages for user input.</summary>
        Dialogs = 8,
    }
}
