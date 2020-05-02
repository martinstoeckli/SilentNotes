// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Collection of possible parameters for the controllers.
    /// </summary>
    public static class ControllerParameters
    {
        /// <summary>
        /// This parameter can be passed to the <see cref="NoteController"/> and contains the id of
        /// the note to display.
        /// </summary>
        public const string NoteId = "noteid";

        /// <summary>
        /// This parameter can be passed to the <see cref="NoteController"/> and contains text
        /// which was sent to or shared with SilentNotes by another application.
        /// </summary>
        public const string SendToSilentnotesText = "senttosilentnotes";
    }
}
