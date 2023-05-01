// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
    /// <summary>
    /// Model for triggers of startup notifications (a message which can be shown to the user when
    /// the app starts). The trigger can be saved in the settings and determines when a
    /// notification is shown.
    /// </summary>
    public class NotificationTriggerModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTriggerModel"/> class.
        /// </summary>
        public NotificationTriggerModel()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the id of the startup notification.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the notification trigger starts counting the time.
        /// </summary>
        [XmlAttribute(AttributeName = "created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC when the notification was shown, or null if it was not
        /// yet shown.
        /// </summary>
        [XmlIgnore]
        public DateTime? ShownAt { get; set; }

        [XmlAttribute(AttributeName = "shown_at")]
        public DateTime ShownAtSerializeable
        {
            get { return ShownAt.Value; }
            set { ShownAt = value; }
        }

        public bool ShownAtSerializeableSpecified { get { return ShownAt != null; } } // Serialize only when set

        /// <summary>
        /// Checks whether the notification trigger has expired and the notification should be shown.
        /// If the notification has already been shown, it returns false.
        /// </summary>
        /// <param name="now">The current time in UTC.</param>
        /// <param name="queueTime">The time to wait starting with <see cref="CreatedAt"/>.</param>
        /// <returns>Returns true if the notification should be shown, otherwise false.</returns>
        public bool IsDue(DateTime now, TimeSpan queueTime)
        {
            return (ShownAt == null) && ((now - CreatedAt) >= queueTime);
        }
    }
}
