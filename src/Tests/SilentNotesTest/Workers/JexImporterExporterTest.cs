using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SilentNotes;
using SilentNotes.Models;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class JexImporterExporterTest
    {
        [TestMethod]
        public void TryReadFromJexFile_FromRealFile()
        {
            List<JexFileEntry> jexFileEntries;
            bool success;
            using (var testDataStream = File.OpenRead(@"TestResources\joplintest.jex"))
            {
                var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
                success = jexImporter.TryReadFromJexFile(testDataStream, out jexFileEntries);
            }
            Assert.IsTrue(success);
            Assert.AreEqual(9, jexFileEntries.Count);
            Assert.AreEqual(4, jexFileEntries.Count(item => item.ModelType == JexModelType.Note));
            Assert.AreEqual(2, jexFileEntries.Count(item => item.ModelType == JexModelType.Tag));
            Assert.AreEqual(3, jexFileEntries.Count(item => item.ModelType == JexModelType.NoteTag));

            JexFileEntry note = jexFileEntries.Find(item => item.Id == Guid.Parse("51d5d24af4f242258d859f6056997791"));
            Assert.IsNotNull(note);
            Assert.IsTrue(note.Content.StartsWith("\n## Tables"));
            Assert.IsTrue(note.Content.EndsWith("Beer\n"));
        }

        [TestMethod]
        [Ignore]
        public void WriteToJexFile_ToRealFile()
        {
            var testData = CreateTestData();
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());

            using (var outputStream = new FileStream(@"D:\SilentNotes_unittest.jex", FileMode.Create))
            {
                jexImporter.WriteToJexFile(testData, outputStream);
            }
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_ReadsValidInput()
        {
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            JexFileEntry jexFileEntry;
            Assert.IsTrue(jexImporter.TryReadFromArchiveEntry("1. Welcome note!\n\n# First header!\n\n" + ValidNoteMetadata, out jexFileEntry)); // pass only metadata
            Assert.AreEqual("# First header!", jexFileEntry.Content);
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_ReadsEmptyContent()
        {
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            Assert.IsTrue(jexImporter.TryReadFromArchiveEntry(ValidNoteMetadata, out var jexFileEntry)); // pass only metadata
            Assert.IsNull(jexFileEntry.Content);
            Assert.IsTrue(jexFileEntry.MetaData.Count > 0);
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_ReadsInputWitEmptyTitle()
        {
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            const string inputWithEmptyTitle = "\n\nFirst line\nsecond line\n\n" + ValidNoteMetadata;
            Assert.IsTrue(jexImporter.TryReadFromArchiveEntry(inputWithEmptyTitle, out var jexFileEntry)); // pass only metadata
            Assert.AreEqual("First line\nsecond line", jexFileEntry.Content);
            Assert.AreEqual(Guid.Parse("51d5d24af4f242258d859f6056997791"), jexFileEntry.Id);
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_ReadsValidMetadata()
        {
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            Assert.IsTrue(jexImporter.TryReadFromArchiveEntry(ValidNoteMetadata, out var jexFileEntry));
            Assert.AreEqual(29, jexFileEntry.MetaData.Count);
            Assert.AreEqual("5d127c4376bb4b0ca496824216affdcc", jexFileEntry.MetaData["parent_id"]);
            Assert.AreEqual(string.Empty, jexFileEntry.MetaData["author"]);
            Assert.AreEqual("1", jexFileEntry.MetaData["type_"]);
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_RejectsEmptyMetadata()
        {
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            JexFileEntry jexFileEntry;
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("", out jexFileEntry));
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("", out jexFileEntry));
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("\n\n", out jexFileEntry));
        }

        [TestMethod]
        public void TryReadFromArchiveEntry_RejectsInvalidMetadata()
        {
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            JexFileEntry jexFileEntry;
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("id\nparent_id: 5d127c4376bb4b0ca496824216affdcc", out jexFileEntry)); // key without delimiter
            Assert.IsFalse(jexImporter.TryReadFromArchiveEntry("is_shared: 0", out jexFileEntry)); // missing mandatory keys
        }

        [TestMethod]
        public async Task CreateRepositoryFromJexFiles_WorksWithValidEntries()
        {
            var testData = CreateTestData();
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            var notes = await jexImporter.CreateRepositoryFromJexFileEntries(testData);

            Assert.AreEqual(3, notes.Count);

            var note1 = notes.FindById(ToJexId(new Guid("4c85ba38aea8400982b74e53f37e27db")));
            Assert.IsNotNull(note1);
            Assert.AreEqual(new DateTime(2025, 12, 07, 09, 32, 16, 750, DateTimeKind.Utc), note1.CreatedAt);
            Assert.AreEqual(new DateTime(2025, 12, 08, 09, 22, 25, 756, DateTimeKind.Utc), note1.ModifiedAt);
            Assert.AreEqual(1, note1.Tags.Count);
            Assert.AreEqual("caramel", note1.Tags[0]);
            Assert.AreEqual("<b>html</b>", note1.HtmlContent);

            var note2 = notes.FindById(ToJexId(new Guid("51d5d24af4f242258d859f6056997791")));
            Assert.IsNotNull(note2);
            Assert.AreEqual(2, note2.Tags.Count);
            Assert.AreEqual("candy", note2.Tags[0]);
            Assert.AreEqual("caramel", note2.Tags[1]);
            Assert.AreEqual("<b>html</b>", note2.HtmlContent);

            var note3 = notes.FindById(ToJexId(new Guid("b47f3cd7b1c943ba85085033e6b830b1")));
            Assert.IsNotNull(note3);
            Assert.AreEqual(0, note3.Tags.Count);
            Assert.AreEqual("<b>html</b>", note3.HtmlContent);
        }

        [TestMethod]
        public async Task CreateJexFilesFromRepository_WorksWithValidEntries()
        {
            var testData = CreateTestData();
            var jexImporter = new JexImporterExporter(CreateMarkdownConverterMock());
            List<NoteModel> testNotes = await jexImporter.CreateRepositoryFromJexFileEntries(testData);

            Guid repositoryId = Guid.NewGuid();
            List<JexFileEntry> jexFileEntries = await jexImporter.CreateJexFileEntriesFromRepository(repositoryId, testNotes);

            // Test number of entries by type
            Assert.AreEqual(1, jexFileEntries.Count(item => item.ModelType == JexModelType.Folder));
            Assert.AreEqual(3, jexFileEntries.Count(item => item.ModelType == JexModelType.Note));
            Assert.AreEqual(2, jexFileEntries.Count(item => item.ModelType == JexModelType.Tag));
            Assert.AreEqual(3, jexFileEntries.Count(item => item.ModelType == JexModelType.NoteTag));

            var originalNote1 = testData[0];
            var newNote1 = jexFileEntries.Find(item =>
                item.ModelType == JexModelType.Note && item.Id == ToJexId(originalNote1.Id));
            Assert.IsNotNull(newNote1);
            Assert.AreEqual(originalNote1.MetaData["created_time"], newNote1.MetaData["created_time"]);
            Assert.AreEqual(originalNote1.MetaData["updated_time"], newNote1.MetaData["updated_time"]);
            Assert.AreEqual(originalNote1.MetaData["type_"], newNote1.MetaData["type_"]);
            Assert.AreEqual(repositoryId.ToString("N"), newNote1.MetaData["parent_id"]);
            Assert.AreEqual("_markdown_", newNote1.Content);

            var originalTag1 = testData[3];
            var newTag1 = jexFileEntries.Find(item =>
                item.ModelType == JexModelType.Tag && item.Content == "caramel");
            Assert.IsNotNull(newTag1);
            Assert.AreEqual("36286186ec287b0a0957e9c44cf77db5", newTag1.MetaData["id"]); // Tag ids are predictable
            Assert.AreEqual(originalTag1.MetaData["type_"], newTag1.MetaData["type_"]);

            var originalRelation3 = testData[7];
            var newRelation1 = jexFileEntries.Find(item =>
                item.ModelType == JexModelType.NoteTag &&
                item.MetaData["note_id"] == newNote1.MetaData["id"] &&
                item.MetaData["tag_id"] == newTag1.MetaData["id"]);
            Assert.IsNotNull(newRelation1);
        }

        private static Guid ToJexId(Guid id)
        {
            return RelativeGuid.CreateRelativeGuid(id, JexImporterExporter.IdDistanceJex);
        }

        private static IMarkdownConverter CreateMarkdownConverterMock()
        {
            var result = new Mock<IMarkdownConverter>();
            result.Setup(m => m.HtmlToMarkdown(It.IsAny<string>())).ReturnsAsync("_markdown_");
            result.Setup(m => m.MarkdownToHtml(It.IsAny<string>())).ReturnsAsync("<b>html</b>");
            return result.Object;
        }

        /// <summary>
        /// Creates a list of jex file entries without reading a real jex file.
        /// </summary>
        /// <returns>List of jex file entries, representing the content of a jex file.</returns>
        private static List<JexFileEntry> CreateTestData()
        {
            // notes
            JexFileEntry note1 = new JexFileEntry("# First header!", new Dictionary<string, string>
            {
                { "id", "4c85ba38aea8400982b74e53f37e27db" },
                { "created_time", "2025-12-07T09:32:16.750Z" },
                { "updated_time", "2025-12-08T09:22:25.756Z" },
                { "type_", "1" },
            });

            JexFileEntry note2 = new JexFileEntry("## Tables\n\nAre available:", new Dictionary<string, string>
            {
                { "id", "51d5d24af4f242258d859f6056997791" },
                { "created_time", "2025-12-07T09:32:16.690Z" },
                { "updated_time", "2025-12-08T09:07:34.345Z" },
                { "type_", "1" },
            });

            JexFileEntry note3 = new JexFileEntry("\n\nWithout title", new Dictionary<string, string>
            {
                { "id", "b47f3cd7b1c943ba85085033e6b830b1" },
                { "created_time", "2025-12-09T13:26:03.829Z" },
                { "updated_time", "2025-12-09T13:26:30.684Z" },
                { "type_", "1" },
            });

            // tags
            JexFileEntry tag1 = new JexFileEntry("caramel", new Dictionary<string, string>
            {
                { "id", "3d35b54999694a26b9a441838689b17a" },
                { "created_time", "2025-12-08T09:01:09.810Z" },
                { "updated_time", "2025-12-08T09:01:09.810Z" },
                { "type_", "5" },
            });

            JexFileEntry tag2 = new JexFileEntry("candy", new Dictionary<string, string>
            {
                { "id", "98191efc65484f67ac3b125db71fba27" },
                { "created_time", "2025-12-08T09:12:19.622Z" },
                { "updated_time", "2025-12-08T09:12:19.622Z" },
                { "type_", "5" },
            });

            // note-tag relations
            JexFileEntry relation1 = new JexFileEntry(null, new Dictionary<string, string>
            {
                { "id", "dea62cc1266741b1bd4bc9b01863bdee" },
                { "note_id", "51d5d24af4f242258d859f6056997791" },
                { "tag_id", "98191efc65484f67ac3b125db71fba27" },
                { "type_", "6" },
            });

            JexFileEntry relation2 = new JexFileEntry(null, new Dictionary<string, string>
            {
                { "id", "3a9311fda94142da9cb8eb8ccc92b8a8" },
                { "note_id", "4c85ba38aea8400982b74e53f37e27db" },
                { "tag_id", "3d35b54999694a26b9a441838689b17a" },
                { "type_", "6" },
            });

            JexFileEntry relation3 = new JexFileEntry(null, new Dictionary<string, string>
            {
                { "id", "80ea0526a28e4ed3ae0a0be4f3948ee7" },
                { "note_id", "51d5d24af4f242258d859f6056997791" },
                { "tag_id", "3d35b54999694a26b9a441838689b17a" },
                { "type_", "6" },
            });

            // folder
            JexFileEntry folder1 = new JexFileEntry("Welcome!", new Dictionary<string, string>
            {
                { "id", "5d127c4376bb4b0ca496824216affdcc" },
                { "type_", "2" },
            });

            return new List<JexFileEntry>() { note1, note2, note3, tag1, tag2, relation1, relation2, relation3, folder1 };
        }

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
