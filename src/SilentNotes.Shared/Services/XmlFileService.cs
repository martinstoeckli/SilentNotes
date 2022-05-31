// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using SilentNotes.Workers;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IXmlFileService"/> interface.
    /// </summary>
    public class XmlFileService : IXmlFileService
    {
        /// <inheritdoc/>
        public bool TryLoad(string filePath, out XDocument xml)
        {
            try
            {
                xml = XDocument.Load(filePath);
                return true;
            }
            catch (Exception)
            {
                xml = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TrySerializeAndSave(string filePath, object serializeableObject)
        {
            try
            {
                var writer = new AtomicFileWriter { MinValidFileSize = "</silentnotes>".Length };
                writer.Write(
                    filePath,
                    (xmlStream) => {
                        XmlUtils.SerializeToXmlStream(serializeableObject, xmlStream, Encoding.UTF8);
                    });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool Exists(string filePath)
        {
            var writer = new AtomicFileWriter { MinValidFileSize = "</silentnotes>".Length };
            writer.CompletePendingWrite(filePath);
            return File.Exists(filePath);
        }
    }
}