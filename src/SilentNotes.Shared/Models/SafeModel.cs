// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
    /// <summary>
    /// A safe can be used to encrypt one or more notes.
    /// </summary>
    public class SafeModel
    {
        private Guid _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeModel"/> class.
        /// </summary>
        public SafeModel()
        {
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = CreatedAt;
        }

        /// <summary>
        /// Gets or sets the id of the safe.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get { return (_id != Guid.Empty) ? _id : (_id = Guid.NewGuid()); }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the time in UTC, when the note was first created.
        /// </summary>
        [XmlAttribute(AttributeName = "created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the password was last updated.
        /// </summary>
        [XmlAttribute(AttributeName = "modified_at")]
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the serializable <see cref="Key"/>.
        /// </summary>
        [XmlElement("key")]
        public string SerializeableKey { get; set; }

        /// <summary>
        /// Sets the <see cref="ModifiedAt"/> property to the current UTC time.
        /// </summary>
        public void RefreshModifiedAt()
        {
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Makes a deep copy of the safe.
        /// </summary>
        /// <returns>Copy of the note.</returns>
        public SafeModel Clone()
        {
            SafeModel result = new SafeModel();
            CloneTo(result);
            return result;
        }

        /// <summary>
        /// Makes <paramref name="target"/> a deep copy of the safe.
        /// </summary>
        /// <param name="target">Copy all properties to this note.</param>
        public void CloneTo(SafeModel target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.Id = this.Id;
            target.SerializeableKey = this.SerializeableKey;
            target.CreatedAt = this.CreatedAt;
            target.ModifiedAt = this.ModifiedAt;
        }
    }
}
