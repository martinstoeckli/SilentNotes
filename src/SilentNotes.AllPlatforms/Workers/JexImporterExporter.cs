// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.Text;
using Markdig;
using SilentNotes.Models;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Imports and exports notes in the Joplin (*.jex) file format.
    /// </summary>
    public class JexImporterExporter
    {
        /// <summary>
        /// Generates a SilentNotes repository from previously loaded JexFileEntry objects, loaded
        /// with <see cref="TryReadFromJexFile(Stream, out List{JexFileEntry})"/>.
        /// </summary>
        /// <param name="jexFileEntries">The already loaded JexFileEntry objects.</param>
        /// <returns>A repository containing the imported notes.</returns>
        public NoteRepositoryModel CreateRepositoryFromJexFiles(List<JexFileEntry> jexFileEntries)
        {
            var result = new NoteRepositoryModel();

            // Extract tags
            Dictionary<Guid, string> tags = jexFileEntries
                .Where(item => item.ModelType == JexModelType.Tag)
                .ToDictionary(item => item.Id, item => item.Content);

            // Extract relations between notes and tags
            List<NoteIdTagIdPair> noteToTag = jexFileEntries
                .Where(item => item.ModelType == JexModelType.NoteTag)
                .Select(item => new NoteIdTagIdPair(Guid.Parse(item.MetaData["note_id"]), Guid.Parse(item.MetaData["tag_id"])))
                .ToList();
            noteToTag.RemoveAll(item => !tags.ContainsKey(item.TagId)); // Missing tags should not render import impossible

            // Create notes
            var noteEntries = jexFileEntries.Where(item => item.ModelType == JexModelType.Note);
            foreach (var noteEntry in noteEntries)
            {
                NoteModel noteModel = new NoteModel();
                noteModel.Id = noteEntry.Id;
                noteModel.HtmlContent = Markdown.ToHtml(noteEntry.Content);
                noteModel.CreatedAt = ExtractDateFromMetadata(noteEntry.MetaData, "created_time", null);
                noteModel.ModifiedAt = ExtractDateFromMetadata(noteEntry.MetaData, "updated_time", null);

                // Add tags
                var noteTags = noteToTag.Where(item => item.NoteId == noteModel.Id).Select(item => tags[item.TagId]);
                noteModel.Tags.AddRange(noteTags);
                result.Notes.Add(noteModel);
            }

            return result;
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
                        string archiveEntryContent = Encoding.UTF8.GetString(ms.ToArray());

                        if (TryReadFromArchiveEntry(archiveEntryContent, out JexFileEntry jexFileEntry))
                        {
                            switch (jexFileEntry.ModelType)
                            {
                                case JexModelType.Note:
                                case JexModelType.Tag:
                                case JexModelType.NoteTag:
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
            const string LineDelimiter = "\n";
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

                // Strip the title (first two lines)
                if (contentPart != null)
                {
                    for (int times = 0; times < 2; times++)
                    {
                        int titleDelimiterPos = contentPart.IndexOf(LineDelimiter);
                        if (titleDelimiterPos >= 0)
                            contentPart = contentPart.Remove(0, titleDelimiterPos + 1);
                    }
                }

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

        private static DateTime ExtractDateFromMetadata(Dictionary<string, string> metaData, string key, DateTime? defaultDate = null)
        {
            // Jex files store dates as ISO UTC string: 2025-12-07T09:32:16.690Z
            if (metaData.TryGetValue(key, out string dateText) &&
                DateTime.TryParse(dateText, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime result))
            {
                return result;
            }

            return defaultDate.HasValue ? defaultDate.Value : DateTime.UtcNow;
        }

        private class NoteIdTagIdPair
        {
            public NoteIdTagIdPair(Guid noteId, Guid tagId)
            {
                NoteId = noteId;
                TagId = tagId;
            }

            public Guid NoteId { get; }

            public Guid TagId { get; }
        }
    }

    /// <summary>
    /// Describes a single file in the jex archive, which describes a single note.
    /// </summary>
    [DebuggerDisplay("{Id} {ModelType}")]
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
            Id = Guid.Parse(MetaData["id"]);
            ModelType = ToModelType(MetaData["type_"]);
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

        /// <summary>
        /// Gets the id of the item.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the model type of the item.
        /// </summary>
        public JexModelType ModelType { get; }

        private static JexModelType ToModelType(string modelTypeFromMetadata)
        {
            int modelTypeNumber = int.Parse(modelTypeFromMetadata);
            return (JexModelType)modelTypeNumber;
        }
    }

    /// <summary>
    /// Enumeration of al possible model types, mirroring the source code of Joplin.
    /// See: https://github.com/laurent22/joplin/blob/8018f1269adc7ffabdecea8e81ab57c2a4da5307/packages/lib/BaseModel.ts
    /// </summary>
    public enum JexModelType
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
}
