// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Enumeration of all modes, a story board can be started with.
    /// </summary>
    public enum StoryBoardMode
    {
        /// <summary>Shows the complete user interface, inclusive user dialogs and messages.</summary>
        Gui,

        /// <summary>Runs in silent mode, doesn't show any dialogs. If the story fails, it does it
        /// silently without informing the user, or if a feedback service is not a dummy it can show
        /// a toast. This mode is appropriate for stories running while the app is closing.</summary>
        Silent,
    }

    /// <summary>
    /// Extension methods for the <see cref="StoryBoardMode"/> enumeration.
    /// </summary>
    public static class StoryBoardModeExtensions
    {
        /// <summary>
        /// Checks whether this mode should show dialogs for user input.
        /// </summary>
        /// <param name="mode">Mode to check.</param>
        /// <returns>Returns true if if requires the gui, otherwise false.</returns>
        public static bool ShouldUseGui(this StoryBoardMode mode)
        {
            return mode == StoryBoardMode.Gui;
        }
    }
}
