// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes
{
    /// <summary>
    /// Message to inform the current page to store its unsaved data.
    /// </summary>
    public class StoreUnsavedDataMessage
    {
    }

    /// <summary>
    /// Message to inform the current page that it will be closed in a moment.
    /// </summary>
    public class ClosePageMessage
    {
    }

    /// <summary>
    /// Message to inform the current page that a call to StateHasChanged is necessary.
    /// </summary>
    public class RedrawCurrentPageMessage

    {
    }

    /// <summary>
    /// Message to inform the application MainLayout page that a call to StateHasChanged is necessary.
    /// </summary>
    public class RedrawMainPageMessage
    {
    }

    /// <summary>
    /// Message to inform the current page that a synchroinization has been completed.
    /// </summary>
    public class ReloadAfterSyncMessage
    {
    }

    /// <summary>
    /// Message to inform the current page that the system back button was pressed.
    /// </summary>
    public class BackButtonPressedMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether the listener handled the press event.
        /// </summary>
        public bool Handled { get; set; }
    }

    /// <summary>
    /// Message to inform the application that the main layout has finished first rendering.
    /// At this moment the services are available form IOC, even if they are scoped services.
    /// </summary>
    public class MainLayoutReady
    {
    }
}
