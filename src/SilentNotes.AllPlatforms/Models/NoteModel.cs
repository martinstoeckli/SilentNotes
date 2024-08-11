﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
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

#if (DEBUG)
        public readonly string InstanceId = Guid.NewGuid().ToString();
#endif

        /// <summary>
        /// Can be used instead of null, and acts as replacement for a note which does not exist.
        /// </summary>
        public readonly static NoteModel NotFound = new NoteModel { Id=Guid.Empty };

        private Guid _id;
        private string _htmlContent;
        private DateTime? _metaModifiedAt;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteModel"/> class.
        /// </summary>
        public NoteModel()
        {
            BackgroundColorHex = SettingsModel.StartDefaultNoteColorHex;
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = CreatedAt;
            MetaModifiedAt = null;
            HtmlContent = string.Empty;
            Tags = new List<string>();
        }

        /// <summary>
        /// Makes <paramref name="target"/> a deep copy of the note.
        /// </summary>
        /// <remarks>
        /// If the instance is the same as the target, no copy is performed.
        /// </remarks>
        /// <param name="target">Copy all properties to this note.</param>
        public void CloneTo(NoteModel target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (Object.ReferenceEquals(this, target))
                return;

            target.Id = this.Id;
            target.NoteType = this.NoteType;
            target.HtmlContent = this.HtmlContent;
            target.Tags.Clear();
            target.Tags.AddRange(this.Tags);
            target.BackgroundColorHex = this.BackgroundColorHex;
            target.InRecyclingBin = this.InRecyclingBin;
            target.CreatedAt = this.CreatedAt;
            target.ModifiedAt = this.ModifiedAt;
            target.MetaModifiedAt = this.MetaModifiedAt;
            target.SafeId = this.SafeId;
            target.ShoppingModeActive = this.ShoppingModeActive;
            target.IsPinned = this.IsPinned;
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
        /// Gets or sets a list of tags (labels) associated with this note.
        /// </summary>
        /// <remarks>
        /// The tags are independend of the content, they rather should be seen like directory
        /// information. This means they are not encrypted with the note and can be renamed
        /// without making the note the newer one, see also <see cref="MetaModifiedAt"/>.
        /// </remarks>
        [XmlArray("tags")]
        [XmlArrayItem("tag")]
        public List<string> Tags { get; set; }

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
        /// Gets or sets a value indicating whether the note is pinned.
        /// </summary>
        [XmlAttribute(AttributeName = "note_pinned")]
        public bool IsPinned { get; set; }

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
        /// Gets or sets the time in UTC, when the meta data of the note was last updated.
        /// Keeping the timestamp of content and meta data separate, allows e.g. to rename a given
        /// tag in all notes, without making it the more recent note (which would overwrite the
        /// actually newer note content).
        /// </summary>
        [XmlIgnore]
        public DateTime? MetaModifiedAt
        {
            get
            {
                // If the ModifiedAt property is newer, then the MetaModifiedAt is irrelevant.
                if (_metaModifiedAt.HasValue && _metaModifiedAt <= ModifiedAt)
                    _metaModifiedAt = null;
                return _metaModifiedAt;
            }

            set { _metaModifiedAt = value; }
        }

        [XmlAttribute(AttributeName = "meta_modified_at")]
        public DateTime MetaModifiedAtSerializeable
        {
            get { return MetaModifiedAt.Value; }
            set { MetaModifiedAt = value; }
        }

        public bool MetaModifiedAtSerializeableSpecified { get { return MetaModifiedAt != null; } } // Serialize only when set

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
        /// Sets the <see cref="MetaModifiedAt"/> property to the current UTC time.
        /// </summary>
        public void RefreshMetaModifiedAt()
        {
            MetaModifiedAt = DateTime.UtcNow;
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