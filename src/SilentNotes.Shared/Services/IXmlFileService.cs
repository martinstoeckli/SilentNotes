// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Xml.Linq;

namespace SilentNotes.Services
{
    /// <summary>
    /// The xml file service can load and save XML files from/to the platform specific file system.
    /// Deserializing configs from a XDocument offers a convenient way to handle previous
    /// versions of the config. This way it is not necessary that the model contains deprecated
    /// properties and we can keep the model clean.
    /// </summary>
    public interface IXmlFileService
    {
        /// <summary>
        /// Tries to load an XML file from the file system.
        /// </summary>
        /// <remarks>
        /// Loading the file to an intermediate XDocument, instead of directly deserializing it to
        /// an object, has following advantages:
        /// 1) The XML can be updated to new different looking versions of the model if necessary.
        /// 2) The update can be done without cluttering the model class with deprecated properties,
        ///    so the model can be kept clean.
        /// 3) A valid XML can always be read, even if it does not match the belonging model, so it
        ///    can be inspected before deserializing.
        /// </remarks>
        /// <param name="filePath">The full path of the XML file to load.</param>
        /// <param name="xml">Receives the loaded XML document or null in case of an error.</param>
        /// <returns>Returns true if the file could be loaded, otherwise false.</returns>
        bool TryLoad(string filePath, out XDocument xml);

        /// <summary>
        /// Tries to save a serializeable object to the file system.
        /// </summary>
        /// <param name="filePath">The full path of the XML file to save.</param>
        /// <param name="serializeableObject">The object to serialize.</param>
        /// <returns>Returns true if the object could be saved, otherwise false.</returns>
        bool TrySerializeAndSave(string filePath, object serializeableObject);

        /// <summary>
        /// Checks whether the XML file exists at a given path.
        /// </summary>
        /// <param name="filePath">Full path to the XML file.</param>
        /// <returns>Returns true if the file exists, otherwise false.</returns>
        bool Exists(string filePath);
    }
}
