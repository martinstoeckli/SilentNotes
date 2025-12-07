// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Text;
using SilentNotes.Models;
using static SilentNotes.Workers.CompressUtils;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Imports and exports notes in the Joplin (*.jex) file format.
    /// </summary>
    public class JexImporterExporter
    {
        private enum ModelType
        {
            Note = 1,
            Folder = 2,
            Setting = 3,
            Resource = 4,
            Tag = 5,
            NoteTag = 6,
            Search = 7,
            Alarm = 8,
            MasterKey = 9,
            ItemChange = 10,
            NoteResource = 11,
            ResourceLocalState = 12,
            Revision = 13,
            Migration = 14,
            SmartFilter = 15,
            Command = 16,
        }

        public bool TryReadRepositoryFromJexFile(Stream jexFileStream, out NoteRepositoryModel repository)
        {
            List<JexFileEntry> jexFileEntries;
            if (TryReadFromJexFile(jexFileStream, out jexFileEntries))
            {
                repository = new NoteRepositoryModel();
                return true;
            }
            else
            {
                repository = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to read a JEX archive file.
        /// </summary>
        /// <param name="jexFileStream">The open stream of a JEX archive file.</param>
        /// <param name="jexFileEntries">Retrieves a list of entries, one for each contained note
        /// file.</param>
        /// <returns>Returns true if the file could be read, false if the file does not contain a
        /// JEX archive or is invalid.</returns>
        public bool TryReadFromJexFile(Stream jexFileStream, out List<JexFileEntry> jexFileEntries)
        {
            jexFileEntries = new List<JexFileEntry>();

            try
            {
                TarReader tarReader = new TarReader(jexFileStream);
                TarEntry jexArchiveEntry;
                while ((jexArchiveEntry = tarReader.GetNextEntry()) != null)
                {
                    // Curently resource files like png images are ignored
                    bool isMdFile = string.Equals(".md", Path.GetExtension(jexArchiveEntry.Name), StringComparison.InvariantCultureIgnoreCase);
                    bool isEmptyDataStream = jexArchiveEntry.DataStream == null;
                    if (!isMdFile || isEmptyDataStream)
                        continue;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        jexArchiveEntry.DataStream.CopyTo(ms);
                        string content = Encoding.UTF8.GetString(ms.ToArray());

                        if (TryReadFromArchiveEntry(content, out JexFileEntry jexFileEntry))
                        {
                            string metaDataType = jexFileEntry.MetaData["type_"];
                            switch (ToModelType(metaDataType))
                            {
                                case ModelType.Note:
                                    jexFileEntries.Add(jexFileEntry);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to read a single file from a JEX archive.
        /// </summary>
        /// <param name="archiveEntryContent">The content of a single file from a JEX archive.</param>
        /// <param name="jexFileEntry">Retrieves the interpreted info of the file.</param>
        /// <returns>Returns true if the content could be read, false if the content is invalid.</returns>
        internal bool TryReadFromArchiveEntry(string archiveEntryContent, out JexFileEntry jexFileEntry)
        {
            const string contentMetaDelimiter = "\n\n";
            if (archiveEntryContent.Contains('\r'))
                archiveEntryContent = archiveEntryContent.Replace("\r", "");

            int delimiterPos = archiveEntryContent.LastIndexOf(contentMetaDelimiter);
            bool foundDelimiter = (delimiterPos >= 0);
            string metaPart = foundDelimiter
                ? archiveEntryContent.Substring(delimiterPos + 2)
                : archiveEntryContent; // There is no content, only a meta part

            if (TryReadMetadata(metaPart, out Dictionary<string, string> metaData))
            {
                string contentPart = foundDelimiter
                    ? archiveEntryContent.Substring(0, delimiterPos)
                    : null; // There is no content, only a meta part

                jexFileEntry = new JexFileEntry(contentPart, metaData);
                return true;
            }
            else
            {
                jexFileEntry = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to read the metadata added at the end of the *.md file.
        /// </summary>
        /// <param name="metaDataContent">Part of the file containing the metadata.</param>
        /// <param name="metaData">Retrieves a dictionary of metadata values.</param>
        /// <returns>Returns true if the metadata could be read, false if the metadata is invalid.</returns>
        private bool TryReadMetadata(string metaDataContent, out Dictionary<string, string> metaData)
        {
            const string LineDelimiter = "\n";
            const string KeyValueDelimiter = ":";
            metaData = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            try
            {
                string[] lines = metaDataContent.Split(LineDelimiter);
                if (lines.Length == 0)
                    return false;

                foreach (string line in lines)
                {
                    int delimiterPos = line.IndexOf(KeyValueDelimiter);
                    if (delimiterPos < 1)
                        return false; // Invalid metadata

                    string keyPart = line.Substring(0, delimiterPos).Trim();
                    string valuePart = line.Substring(delimiterPos + 1).Trim();
                    metaData[keyPart] = valuePart;
                }

                // Check for mandatory keys
                return metaData.ContainsKey("id") && metaData.ContainsKey("type_");
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ModelType ToModelType(string modelTypeFromMetadata)
        {
            int modelTypeNumber = int.Parse(modelTypeFromMetadata);
            return (ModelType)modelTypeNumber;
        }

        /// <summary>
        /// Describes a single file in the jex archive, which describes a single note.
        /// </summary>
        public class JexFileEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="JexFileEntry"/> class.
            /// </summary>
            /// <param name="content">Sets the <see cref="Content"/> property.</param>
            /// <param name="metaData">Sets the <see cref="MetaData"/> property.</param>
            public JexFileEntry(string content, Dictionary<string, string> metaData)
            {
                Content = content;
                MetaData = metaData;
            }

            /// <summary>
            /// Gets the content of the item, e.g. the markdown of a note. Some items types like
            /// TYPE_NOTE_TAG do not have a content, so this property will be null.
            /// </summary>
            public string Content { get; }

            /// <summary>
            /// Gets a dictionary of metadata information.
            /// </summary>
            public Dictionary<string, string> MetaData { get; }
        }
    }
}
