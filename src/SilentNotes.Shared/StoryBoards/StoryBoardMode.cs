// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Enumeration of all modes, a story board can be started with.
    /// </summary>
    public enum StoryBoardMode
    {
        /// <summary>Shows the complete user interface, inclusive user dialogs and messages.</summary>
        GuiAndToasts,

        /// <summary>Shows messages but does not ask for user input or user interaction. This mode
        /// is appropriate for stories running while the app is starting up, but should run
        /// automatically without user interactions.</summary>
        ToastsOnly,

        /// <summary>Runs in silent mode, doesn't show any dialogs or messages. If the story fails,
        /// it does it silently without informing the user. This mode is appropriate for stories
        /// running while the app is closing.</summary>
        Silent,
    }

    /// <summary>
    /// Extension methods for the <see cref="StoryBoardMode"/> enumeration.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just extension methods for the same class.")]
    public static class StoryBoardModeExtensions
    {
        /// <summary>
        /// Checks whether this mode should show dialogs for user input.
        /// </summary>
        /// <param name="mode">Mode to check.</param>
        /// <returns>Returns true if if requires the gui, otherwise false.</returns>
        public static bool ShouldUseGui(this StoryBoardMode mode)
        {
            return mode == StoryBoardMode.GuiAndToasts;
        }

        /// <summary>
        /// Checks whether the story should show messages.
        /// </summary>
        /// <param name="mode">Mode to check.</param>
        /// <returns>Returns true if it should show messages, otherwise false.</returns>
        public static bool ShouldShowToasts(this StoryBoardMode mode)
        {
            return mode != StoryBoardMode.Silent;
        }

        /// <summary>
        /// Checks whether the story should run without any user interactions or messages.
        /// </summary>
        /// <param name="mode">Mode to check.</param>
        /// <returns>Returns true if it runs silently, otherwise false.</returns>
        public static bool ShouldRunSilently(this StoryBoardMode mode)
        {
            return mode == StoryBoardMode.Silent;
        }
    }
}
