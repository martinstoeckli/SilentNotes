// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
using System.Xml.Linq;
using SilentNotes.Models;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Implementation of the <see cref="INoteRepositoryUpdater"/> interface.
    /// </summary>
    public class NoteRepositoryUpdater : INoteRepositoryUpdater
    {
        private readonly int _newestSupportedRevision;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryUpdater"/> class.
        /// </summary>
        public NoteRepositoryUpdater()
            : this(NoteRepositoryModel.NewestSupportedRevision)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryUpdater"/> class.
        /// For testing purposes only.
        /// </summary>
        /// <param name="newestSupportedRevision">Replaces the <see cref="NoteRepositoryModel.NewestSupportedRevision"/>
        /// for testing.</param>
        internal NoteRepositoryUpdater(int newestSupportedRevision)
        {
            _newestSupportedRevision = newestSupportedRevision;
        }

        /// <inheritdoc/>
        public bool IsTooNewForThisApp(XDocument repository)
        {
            XElement root = repository.Root;
            XAttribute revisionAttribute = root.Attribute("revision");
            int repositoryRevision = int.Parse(revisionAttribute.Value);
            return repositoryRevision > _newestSupportedRevision;
        }

        /// <inheritdoc/>
        public bool Update(XDocument repository)
        {
            XElement root = repository.Root;
            XAttribute revisionAttribute = root.Attribute("revision");
            int oldRevision = int.Parse(revisionAttribute.Value);

            // Check for necessary update steps (nothing to do from 2 to 4)
            if (oldRevision <= 1)
            {
                UpdateRepositoryFrom1To2(root);
            }

            bool updated = oldRevision < NoteRepositoryModel.CurrentSavingRevision;
            if (updated)
                root.SetAttributeValue("revision", NoteRepositoryModel.CurrentSavingRevision);
            return updated;
        }

        private void UpdateRepositoryFrom1To2(XElement root)
        {
            StringBuilder sb = new StringBuilder();

            XElement notes = root.Element("notes");
            foreach (XElement note in notes.Elements())
            {
                sb.Clear();
                XElement titleElement = note.Element("title");
                string title = titleElement?.Value;
                XElement contentElement = note.Element("content");
                string content = contentElement?.Value;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    title = System.Net.WebUtility.HtmlEncode(title);
                    sb.Append("<h1>");
                    sb.Append(title);
                    sb.Append("</h1>");
                }
                if (!string.IsNullOrWhiteSpace(content))
                {
                    content = System.Net.WebUtility.HtmlEncode(content);
                    content = content.Replace("\n", "</p><p>");
                    sb.Append("<p>");
                    sb.Append(content);
                    sb.Append("</p>");
                }

                if (titleElement != null)
                    titleElement.Remove();
                if (contentElement != null)
                    contentElement.Remove();
                note.Add(new XElement("html_content", sb.ToString()));
            }
        }
    }
}
