// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Xml.Serialization;
using SilentNotes.Crypto;

namespace SilentNotes.Models
{
    /// <summary>
    /// Serializeable model of a single note.
    /// </summary>
    public class NoteModel
    {
        /// <summary>The package name used for encryption, see <see cref="CryptoHeader.PackageName"/></summary>
        public const string CryptorPackageName = "SilentNote";
        private Guid _id;
        private string _htmlContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteModel"/> class.
        /// </summary>
        public NoteModel()
        {
            BackgroundColorHex = SettingsModel.StartDefaultNoteColorHex;
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = CreatedAt;
            HtmlContent = string.Empty;
        }

        /// <summary>
        /// Makes <paramref name="target"/> a deep copy of the note.
        /// </summary>
        /// <param name="target">Copy all properties to this note.</param>
        public void CloneTo(NoteModel target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.Id = this.Id;
            target.NoteType = this.NoteType;
            target.HtmlContent = this.HtmlContent;
            target.BackgroundColorHex = this.BackgroundColorHex;
            target.InRecyclingBin = this.InRecyclingBin;
            target.CreatedAt = this.CreatedAt;
            target.ModifiedAt = this.ModifiedAt;
            target.MaintainedAt = this.MaintainedAt;
            target.SafeId = this.SafeId;
            target.ShoppingModeActive = this.ShoppingModeActive;
        }

        /// <summary>
        /// Gets or sets the id of the note.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get { return (_id != Guid.Empty) ? _id : (_id = Guid.NewGuid()); }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the type of the note.
        /// </summary>
        [XmlAttribute(AttributeName = "note_type")]
        public NoteType NoteType { get; set; }

        /// <summary>
        /// Gets or sets the formatted html content of the note, or its encrypted representation.
        /// This property is never null, instead an empty string will be returned.
        /// </summary>
        [XmlElement(ElementName = "html_content")]
        public string HtmlContent 
        {
            get { return _htmlContent ?? (_htmlContent = string.Empty); }
            set { _htmlContent = value; }
        }

        /// <summary>
        /// Gets or sets the background color of the note as hex string, e.g. #ff0000
        /// </summary>
        [XmlAttribute(AttributeName = "background_color")]
        public string BackgroundColorHex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the note is deleted and is part of the
        /// recycling bin.
        /// </summary>
        [XmlAttribute(AttributeName = "in_recycling_bin")]
        public bool InRecyclingBin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the shopping mode is active or inactive. While
        /// the shopping mode is active, the note is read-only and the menu is inactive, so that
        /// one can use the note outdoor whithout fearing of unintentionally modifying the note.
        /// </summary>
        [XmlAttribute(AttributeName = "shopping_mode")]
        public bool ShoppingModeActive { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the note was first created.
        /// </summary>
        [XmlAttribute(AttributeName = "created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the note was last updated.
        /// </summary>
        [XmlAttribute(AttributeName = "modified_at")]
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the object was last updated by the system, instead of
        /// the user. This way the system can clean up deprecated functions, but does not interfere
        /// with more important user changes.
        /// </summary>
        [XmlIgnore]
        public DateTime? MaintainedAt { get; set; }

        [XmlAttribute(AttributeName = "maintained_at")]
        public DateTime MaintainedAtSerializeable
        {
            get { return MaintainedAt.Value; }
            set { MaintainedAt = value; }
        }
        public bool MaintainedAtSerializeableSpecified { get { return MaintainedAt != null && MaintainedAt > ModifiedAt; } } // Serialize only when set

        /// <summary>
        /// Clears the <see cref="MaintainedAt"/> property if it is obsolete, because the object was
        /// modified later.
        /// </summary>
        public void ClearMaintainedAtIfObsolete()
        {
            if ((MaintainedAt != null) && (MaintainedAt < ModifiedAt))
                MaintainedAt = null;
        }

        /// <summary>
        /// Gets or sets the safe which was used to encrypt the note, or null if it is not encrypted.
        /// </summary>
        [XmlElement(ElementName = "safe")]
        public Guid? SafeId { get; set; }
        public bool SafeIdSpecified { get { return SafeId != null; } } // Serialize only when set

        /// <summary>
        /// Sets the <see cref="ModifiedAt"/> property to the current UTC time.
        /// </summary>
        public void RefreshModifiedAt()
        {
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Makes a deep copy of the note.
        /// </summary>
        /// <returns>Copy of the note.</returns>
        public NoteModel Clone()
        {
            NoteModel result = new NoteModel();
            CloneTo(result);
            return result;
        }
    }
}
