using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;
using static SilentNotes.Workers.AtomicFileWriter;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class JexImporterExporterTest
    {
        [TestMethod]
        public void TestReadFromJexFile()
        {
            using (var jexFileStream = new FileStream(@"D:\Downloads\joplin.jex", FileMode.Open))
            {
                var jexImporter = new JexImporterExporter();
                bool success = jexImporter.TryReadFromJexFile(jexFileStream, out var jexFileEntries);
                Assert.IsTrue(success);
            }
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_ReadsValidInput()
        {
            var jexImporter = new JexImporterExporter();
            JexImporterExporter.JexFileEntry jexFileEntry;
            Assert.IsTrue(jexImporter.TryReadFromArchiveEntry(ValidNote, out jexFileEntry)); // pass only metadata
            Assert.AreEqual("Willkommen!\non line 2", jexFileEntry.Content);
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_ReadsEmptyContent()
        {
            var jexImporter = new JexImporterExporter();
            Assert.IsTrue(jexImporter.TryReadFromArchiveEntry(ValidNoteMetadata, out var jexFileEntry)); // pass only metadata
            Assert.IsNull(jexFileEntry.Content);
            Assert.IsTrue(jexFileEntry.MetaData.Count > 0);
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_ReadsValidMetadata()
        {
            var jexImporter = new JexImporterExporter();
            Assert.IsTrue(jexImporter.TryReadFromArchiveEntry(ValidNoteMetadata, out var jexFileEntry));
            Assert.AreEqual(29, jexFileEntry.MetaData.Count);
            Assert.AreEqual("5d127c4376bb4b0ca496824216affdcc", jexFileEntry.MetaData["parent_id"]);
            Assert.AreEqual(string.Empty, jexFileEntry.MetaData["author"]);
            Assert.AreEqual("1", jexFileEntry.MetaData["type_"]);
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_HandlesEmptyMetadata()
        {
            var jexImporter = new JexImporterExporter();
            JexImporterExporter.JexFileEntry jexFileEntry;
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("", out jexFileEntry));
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("", out jexFileEntry));
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("\n\n", out jexFileEntry));
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_HandlesInvalidMetadata()
        {
            var jexImporter = new JexImporterExporter();
            JexImporterExporter.JexFileEntry jexFileEntry;
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("id\nparent_id: 5d127c4376bb4b0ca496824216affdcc", out jexFileEntry)); // key without delimiter
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("is_shared: 0", out jexFileEntry)); // missing mandatory keys
        }

        const string ValidNote = "Willkommen!\non line 2\n\n" + ValidNoteMetadata;

        const string ValidNoteMetadata = @"id: 51d5d24af4f242258d859f6056997791
parent_id: 5d127c4376bb4b0ca496824216affdcc
created_time: 2025-12-07T09:32:16.690Z
updated_time: 2025-12-07T09:32:16.690Z
is_conflict: 0
latitude: 0.00000000
longitude: 0.00000000
altitude: 0.0000
author: 
source_url: 
is_todo: 0
todo_due: 0
todo_completed: 0
source: joplin-desktop
source_application: net.cozic.joplin-desktop
application_data: 
order: 1765099936690
user_created_time: 2025-12-07T09:32:16.690Z
user_updated_time: 2025-12-07T09:32:16.690Z
encryption_cipher_text: 
encryption_applied: 0
markup_language: 1
is_shared: 0
share_id: 
conflict_original_id: 
master_key_id: 
user_data: 
deleted_time: 0
type_: 1";
    }
}
