// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using SilentNotes.Workers;

namespace SilentNotes.Models
{
    /// <summary>
    /// Serializeable model of a note repository.
    /// Whenever changes to this model are done, take care of the <see cref="NewestSupportedRevision"/> constant.
    /// </summary>
    [XmlRoot(ElementName = "silentnotes")]
    public class NoteRepositoryModel
    {
        /// <summary>The highest revision of the repository which can be handled by this application.</summary>
        public const int NewestSupportedRevision = 7;

#if (DEBUG)
        public readonly string InstanceId = Guid.NewGuid().ToString();
#endif
#if (RELEASE)
        public const string RepositoryFileName = "silentnotes_repository.silentnotes";
#else
        public const string RepositoryFileName = "silentnotes_repository_dev.silentnotes";
#endif

        /// <summary>
        /// The invalid repository is a debuty repository, it shows that no valid repository could
        /// have been loaded. The invalid repository is never saved to disk and can never overwrite
        /// the original repository. It can be checked for equality with Object.ReferenceEquals().
        /// </summary>
        public static NoteRepositoryModel InvalidRepository = new NoteRepositoryModel();

        private Guid _id;
        private NoteListModel _notes;
        private List<Guid> _deletedNotes;
        private SafeListModel _safes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryModel"/> class.
        /// </summary>
        public NoteRepositoryModel()
        {
            OrderModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the id of the repository.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get { return (_id != Guid.Empty) ? _id : (_id = Guid.NewGuid()); }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the revision, which was used to create the repository.
        /// </summary>
        [XmlAttribute(AttributeName = "revision")]
        public int Revision { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the order of the notes was last changed.
        /// </summary>
        [XmlAttribute(AttributeName = "order_modified_at")]
        public DateTime OrderModifiedAt { get; set; }

        /// <summary>
        /// Sets the <see cref="OrderModifiedAt"/> property to the current UTC time.
        /// </summary>
        public void RefreshOrderModifiedAt()
        {
            OrderModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets a list of notes.
        /// </summary>
        [XmlArray("notes")]
        [XmlArrayItem("note")]
        public NoteListModel Notes
        {
            get { return _notes ?? (_notes = new NoteListModel()); }
            set { _notes = value; }
        }

        /// <summary>
        /// Gets or sets a list of ids of deleted notes.
        /// </summary>
        [XmlArray("deleted_notes")]
        [XmlArrayItem("deleted_note")]
        public List<Guid> DeletedNotes
        {
            get { return _deletedNotes ?? (_deletedNotes = new List<Guid>()); }
            set { _deletedNotes = value; }
        }

        /// <summary>
        /// Gets or sets a list of safes which are used to encrypt notes. Ideally at most one safe
        /// is used, but because of synchronisation between several devices, there can be more than
        /// one safe.
        /// </summary>
        [XmlArray("safes")]
        [XmlArrayItem("safe")]
        public SafeListModel Safes
        {
            get { return _safes ?? (_safes = new SafeListModel()); }
            set { _safes = value; }
        }

        /// <summary>
        /// Creates a distinct and sorted list of all tags of all notes in the repository.
        /// Tags from notes in the recycle bin are ignored.
        /// </summary>
        /// <returns>List of all tags.</returns>
        public List<string> CollectActiveTags()
        {
            var result = new List<string>();
            foreach (NoteModel note in Notes)
            {
                if (!note.InRecyclingBin)
                {
                    foreach (string tag in note.Tags)
                    {
                        if (!result.Contains(tag, StringComparer.InvariantCultureIgnoreCase))
                            result.Add(tag);
                    }
                }
            }
            result.Sort(StringComparer.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a fingerprint of the current repository, which can be used to determine, whether
        /// two repositories are different, or if a repository was modified in the meantime.
        /// Equal fingerprints mean unchanged repositories, different fingerprints indicate
        /// modifications in the repository.
        /// </summary>
        /// <remarks>
        /// This method is optimized for speed, so it does not consider the whole content of the
        /// repository, instead it uses the timestamps which would be used when merging.
        /// </remarks>
        /// <returns>A fingerprint representing the modification state.</returns>
        public long GetModificationFingerprint()
        {
            List<long> hashCodes = new List<long>();
            hashCodes.Add(Id.GetHashCode());
            hashCodes.Add(Revision);
            hashCodes.Add(OrderModifiedAt.GetHashCode());
            foreach (NoteModel note in Notes)
            {
                hashCodes.Add(note.ModifiedAt.GetHashCode());
                if (note.MetaModifiedAt != null)
                    hashCodes.Add(note.MetaModifiedAt.GetHashCode());
                hashCodes.Add(note.InRecyclingBin.GetHashCode());
            }
            foreach (Guid deletedNote in DeletedNotes)
            {
                hashCodes.Add(deletedNote.GetHashCode());
            }
            foreach (SafeModel safe in Safes)
            {
                hashCodes.Add(safe.ModifiedAt.GetHashCode());
                if (safe.MaintainedAt != null)
                    hashCodes.Add(safe.MaintainedAt.GetHashCode());
            }

            return ModificationDetector.CombineHashCodes(hashCodes);
        }

        public void RemoveUnusedSafes()
        {
            List<Guid> usedSafeIds = Notes.Where(note => note.SafeId != null).Select(note => note.SafeId.Value).ToList();
            Safes.RemoveAll(safe => !usedSafeIds.Contains(safe.Id));
        }
    }
}