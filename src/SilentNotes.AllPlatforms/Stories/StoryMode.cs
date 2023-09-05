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
    public enum StoryMode
    {
        /// <summary>Runs in silent mode, doesn't show any dialogs. If the story fails, it does it
        /// silently without informing the user.</summary>
        Silent,

        /// <summary>Shows messages and toasts, but doesn't ask for user input by opening dialogs.</summary>
        OnlyMessages,

        /// <summary>Shows the complete user interface, inclusive user dialogs and messages.</summary>
        Gui,
    }
}
