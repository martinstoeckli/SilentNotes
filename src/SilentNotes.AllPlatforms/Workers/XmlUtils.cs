// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Helper class for serializing models to XML and back.
    /// </summary>
    public class XmlUtils
    {
        /// <summary>
        /// Uses the DotNet serialization framework to serialize an object to a stream.
        /// The object should be tagged with the [Xml...] attributes. Make sure, that only valid
        /// strings are serialized, test is with <see cref="SanitizeXmlString"/> if necessary.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="outputStream">The xml is written to this stream.
        /// The stream is closed automatically.</param>
        /// <param name="encoding">The optional encoding, which is used for writing. The default
        /// encoding is "utf-8"</param>
        public static void SerializeToXmlStream(object obj, Stream outputStream, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Indent = true,
                Encoding = encoding
            };

            using (XmlWriter xmlWriter = XmlWriter.Create(outputStream, settings))
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(xmlWriter, obj);
            }
        }

        /// <summary>
        /// Uses the DotNet serialization framework to serialize an object to an array of bytes.
        /// The object should be tagged with the [Xml...] attributes.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="encoding">The optional encoding, which is used for writing. The default
        /// encoding is "utf-8"</param>
        /// <returns>A new created byte array, containing the serialized object.</returns>
        public static byte[] SerializeToXmlBytes(object obj, Encoding encoding = null)
        {
            byte[] result;
            using (MemoryStream stream = new MemoryStream())
            {
                SerializeToXmlStream(obj, stream, encoding);
                result = stream.ToArray();
            }
            return result;
        }

        /// <summary>
        /// Mainly used for unit-testing, it uses the DotNet serialization framework to serialize
        /// an object to an XDocument.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Serialized XDocument</returns>
        internal static XDocument SerializeToXmlDocument(object obj)
        {
            XDocument result = new XDocument();
            using (XmlWriter xmlWriter = result.CreateWriter())
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(xmlWriter, obj);
            }
            return result;
        }

        /// <summary>
        /// Mainly used for unit-testing, it uses the DotNet serialization framework to serialize
        /// an object to a string. Because strings are always unicode, the encoding "utf-16" is
        /// used. If you want to store the xml to a file, please use <see cref="SerializeToXmlStream(object, Stream, Encoding)"/>
        /// instead, so the encoding can be set correctly.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>String containing the Xml of the serialized object.</returns>
        internal static string SerializeToString(object obj)
        {
            Encoding encoding = Encoding.Unicode;
            byte[] bytes = SerializeToXmlBytes(obj, encoding);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Creates an XDocument from an array of bytes, containing the XML content.
        /// </summary>
        /// <param name="bytes">Xml file content as byte array.</param>
        /// <returns>Loaded XDocument.</returns>
        public static XDocument LoadFromXmlBytes(byte[] bytes)
        {
            XDocument result;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                result = XDocument.Load(stream);
            }
            return result;
        }

        /// <summary>
        /// Uses the DotNet serialization framework to deserialize an object from an XML document.
        /// The object should be tagged with the [Xml...] attributes.
        /// </summary>
        /// <typeparam name="T">Type of the object to create.</typeparam>
        /// <param name="xml">Read the xml from this XML document.</param>
        /// <returns>New deserialized object.</returns>
        public static T DeserializeFromXmlDocument<T>(XDocument xml)
        {
            T result;
            using (XmlReader xmlReader = xml.CreateReader())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                result = (T)serializer.Deserialize(xmlReader);
            }
            return result;
        }

        /// <summary>
        /// Removes all characters, which would cause errors when loading with the XmlDeserializer,
        /// </summary>
        /// <param name="xml">String to sanitize.</param>
        /// <returns>Sanitized string.</returns>
        public static string SanitizeXmlString(string xml)
        {
            if (xml == null)
                return null;

            // Remove invalid utf-8 encoding
            Encoding sanitizerEncoding = Encoding.GetEncoding(
                "utf-8",
                new EncoderReplacementFallback(string.Empty),
                new DecoderReplacementFallback(string.Empty));
            byte[] xmlBytes = sanitizerEncoding.GetBytes(xml);
            string result = sanitizerEncoding.GetString(xmlBytes);

            // Remove illegal control characters
            StringBuilder sb = new StringBuilder(result.Length);
            foreach (char letter in result)
            {
                if (IsAcceptedXmlCharacter(letter))
                    sb.Append(letter);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Checks whether a given character is allowed in an XML document. This is a relaxed
        /// version of <see cref="IsLegalXmlChar(int)"/>, because the strict rules would not allow
        /// to store emojis.
        /// </summary>
        /// <param name="letter">Character to test.</param>
        /// <returns>Returns true if the character is valid, otherwise false.</returns>
        private static bool IsAcceptedXmlCharacter(char letter)
        {
            return (
                (letter >= 0x20 && letter < 0xFFFE) // first check most common case
                || (letter > 0xFFFF)
                || (letter == 0x9)
                || (letter == 0xA)
                || (letter == 0xD));
        }

        /// <summary>
        /// Checks whether a given character is allowed according to XML 1.0.
        /// See also https://www.w3.org/TR/REC-xml/#charsets
        /// </summary>
        /// <param name="character">Character to test.</param>
        /// <returns>Returns true if the character is valid, otherwise false.</returns>
        private static bool IsLegalXmlChar(int character)
        {
            return (
                 (character == 0x9) // == '\t' == 9
                 || (character == 0xA) // == '\n' == 10
                 || (character == 0xD) // == '\r' == 13
                 || (character >= 0x20 && character <= 0xD7FF)
                 || (character >= 0xE000 && character <= 0xFFFD)
                 || (character >= 0x10000 && character <= 0x10FFFF));
        }
    }
}
