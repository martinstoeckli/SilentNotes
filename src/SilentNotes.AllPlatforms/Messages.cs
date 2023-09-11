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
    public class StateHasChangedMessage
    {
    }

    /// <summary>
    /// Message to inform the application MainLayout page that a call to StateHasChanged is necessary.
    /// </summary>
    public class GlobalStateHasChangedMessage
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
}
