// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Views
{
    /// <summary>
    /// Stores the informations for the link dialog.
    /// </summary>
    internal class LinkState
    {
        /// <summary>
        /// Gets or sets the link url before starting the dialog, to check whether the link changed.
        /// </summary>
        public string OldLinkUrl { get; set; }

        /// <summary>
        /// Gets or sets the link url.
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// Gets or sets the link title before starting the dialog, to check whether the link changed.
        /// </summary>
        public string OldLinkTitle { get; set; }

        /// <summary>
        /// Gets or sets the link title.
        /// </summary>
        public string LinkTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the link dialog is visible.
        /// </summary>
        public bool IsDialogOpen { get; set; }
    }
}
