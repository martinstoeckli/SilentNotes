using System.Xml.Linq;
using SilentNotes.Services;

namespace SilentNotes.Pwa.Services
{
    /// <summary>
    /// Implementation of the <see cref="IXmlFileService"/> interface, which reads/writes to an
    /// IndexedDb of a PWA.
    /// </summary>
    public class IndexedDbXmlFileService : IXmlFileService
    {
        /// <inheritdoc/>
        public bool TryLoad(string filePath, out XDocument xml)
        {
            try
            {
                xml = null;
                return false;
                //return true;
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
            return false;
        }

        /// <inheritdoc/>
        public bool Exists(string filePath)
        {
            return false;
        }
    }
}
