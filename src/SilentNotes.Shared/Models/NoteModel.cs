// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
    /// <summary>
    /// Serializeable model of a single note.
    /// </summary>
    public class NoteModel
    {
        private Guid _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteModel"/> class.
        /// </summary>
        public NoteModel()
        {
            _id = Guid.Empty;
            BackgroundColorHex = SettingsModel.StartDefaultNoteColorHex;
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = CreatedAt;
        }

        /// <summary>
        /// Gets or sets the id of the note.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get
            {
                // Lazy creation
                if (Guid.Empty == _id)
                    _id = Guid.NewGuid();
                return _id;
            }

            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the formatted html content of the note.
        /// </summary>
        [XmlElement(ElementName = "html_content")]
        public string HtmlContent { get; set; }

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

        /// <summary>
        /// Makes <paramref name="targetNote"/> a deep copy of the note.
        /// </summary>
        /// <param name="targetNote">Copy all properties to this note.</param>
        public void CloneTo(NoteModel targetNote)
        {
            if (targetNote == null)
                throw new ArgumentNullException(nameof(targetNote));

            targetNote.Id = this.Id;
            targetNote.HtmlContent = this.HtmlContent;
            targetNote.BackgroundColorHex = this.BackgroundColorHex;
            targetNote.InRecyclingBin = this.InRecyclingBin;
            targetNote.CreatedAt = this.CreatedAt;
            targetNote.ModifiedAt = this.ModifiedAt;
        }
    }
}
