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
        /// <summary>
        /// Initializes a new instance of the <see cref="StoreUnsavedDataMessage"/> class.
        /// Can be used for testing.
        /// </summary>
        internal StoreUnsavedDataMessage()
            : this(MessageSender.Unknown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreUnsavedDataMessage"/> class.
        /// </summary>
        /// <param name="sender">The origin from where the message is sent.</param>
        public StoreUnsavedDataMessage(MessageSender sender)
        {
            Sender = sender;
        }

        public MessageSender Sender { get; }
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
    /// Message to inform the current page that the system back button was pressed.
    /// </summary>
    public class BackButtonPressedMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether the listener handled the press event (e.g. by
        /// closing an open menu).
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets or sets a target for backwards navigation. A page should pass its BackRoute,
        /// a value of null will close the application.
        /// </summary>
        public string BackRoute { get; set; }
    }

    /// <summary>
    /// Message to inform the application that the main layout has finished first rendering.
    /// At this moment the services are available form IOC, even if they are scoped services.
    /// </summary>
    public class MainLayoutReadyMessage
    {
    }

    public class SynchronizationIsRunningChangedMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationIsRunningChangedMessage"/> class.
        /// </summary>
        /// <param name="isRunning">Sets the <see cref="IsRunning"/> property.</param>
        public SynchronizationIsRunningChangedMessage(bool isRunning)
        {
            IsRunning = isRunning;
        }

        /// <summary>
        /// Gets the new state after the cange.
        /// </summary>
        public bool IsRunning { get; }
    }

    /// <summary>
    /// Messager to signal that the app resumed after pausing on mobile devices.
    /// </summary>
    public class AfterResumeMessage
    {
        /// <summary>
        /// Gets or sets the time when app was going into pause mode.
        /// </summary>
        public DateTime LastPauseTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the safe was closed because of the pause timeout.
        /// </summary>
        public bool SafesClosed { get; set; }
    }

    /// <summary>
    /// This message can be used to signal that a given note should be brought into view.
    /// </summary>
    internal class BringNoteIntoViewMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BringNoteIntoViewMessage"/> class.
        /// </summary>
        /// <param name="noteId">Sets the <see cref="NoteId"/> property.</param>
        /// <param name="smooth">Sets the <see cref="Smooth"/> property.</param>
        public BringNoteIntoViewMessage(Guid noteId, bool smooth)
        {
            NoteId = noteId;
            Smooth = smooth;
        }

        /// <summary>
        /// Gets the id of the note to bring into view.
        /// </summary>
        public Guid NoteId { get; }

        /// <summary>
        /// Gets a value indicating whether the scrooling should be done smooth, or immediately.
        /// </summary>
        public bool Smooth { get; }
    }

    /// <summary>
    /// Enumeration of possible sources of a sent message.
    /// </summary>
    public enum MessageSender
    {
        Unknown,
        ApplicationEventHandler,
        NavigationManager,
        ViewModel,
    }
}
