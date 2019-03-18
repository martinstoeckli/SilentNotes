// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
#if (DEMO && DEBUG)
    /// <summary>
    /// A <see cref="NoteRepositoryModel"/> filled with demo data to make screenshots.
    /// </summary>
    [XmlRoot(ElementName = "silentnotes")]
    public class DemoNoteRepositoryModel : NoteRepositoryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoNoteRepositoryModel"/> class.
        /// </summary>
        public DemoNoteRepositoryModel()
        {
            Revision = NewestSupportedRevision;
            Id = new Guid("093b917a-f69f-4dd3-91b7-ad175fe0a4c1");
            OrderModifiedAt = DateTime.Parse("2018-12-18T13:25:20.8042714Z");

            Notes.Add(new NoteModel {
                Id = new Guid("d20dd71b-7cde-4894-8c69-1eaecf6930f6"),
                BackgroundColorHex = "#d0f8f9",
                InRecyclingBin = true,
                CreatedAt = DateTime.Parse("2017-09-10T09:59:01.6635731Z"),
                ModifiedAt = DateTime.Parse("2017-09-10T10:06:46.4459616Z"),
                HtmlContent = @"<h1>Windows10 tips</h1><p>*** Set background color ***

1) Open command line > 'cmd.exe'
2) Type > control /name Microsoft.Personalization /page pageWallpaper
3) User defined color rgb: 210 230 240
</p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("57f1b162-85e7-4402-8731-05b9c06c6915"),
                BackgroundColorHex = "#fbf4c1",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:25:16.6787213Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:52:39.8746458Z"),
                HtmlContent = @"<h1>Opening hours</h1><h3>Post office</h3><p>Monday - Friday 9:00-12:00 / 13:00-17:00</p><p>Saturday 9:00-12:00</p><h3>Supermarket</h3><p>Monday - Sunday 7:00-22:00</p><h3>Ticket shop</h3><p>Monday - Friday 8:00-18:30</p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("a2b16ab9-9f7f-4389-916f-f2ef9a2f3a3a"),
                BackgroundColorHex = "#fdd8bb",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:44:14.5098885Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T13:28:48.6114923Z"),
                HtmlContent = @"<h1>Borrowed / loaned</h1><p>📗 'The Black Magician Trilogy' ➜ from aunt Maggie</p><p>📀 'The golden compass' ➜ from Tim</p><p>📕 'Harry potter 5' ➜ to Dawn Cook</p></p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("c35de588-b92f-49a4-a19e-319f045619f5"),
                BackgroundColorHex = "#d0f8f9",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:51:49.7504749Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:51:34.2026179Z"),
                HtmlContent = @"<h1>Radio Songs</h1><ol><li>Spider's web (Katie Melua)</li><li>Seven mountains (77 Bombay Street)</li><li>Waiting for A.M. (Glen Of Guinness)</li></ol>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("33f00756-6c8b-416d-abe4-2e27d9f58615"),
                BackgroundColorHex = "#d9f8c8",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:35:52.9190418Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:54:18.333087Z"),
                HtmlContent = @"<h1>Printer refill</h1><p>brother DCP-9020CDW</p><p>Cartridge <strong>TN-241* </strong><em>(1400 pages)</em></p><p>Cartridge <strong>TN-245*</strong> <em>(2200 pages)</em></p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("8f9e539d-172c-41e8-99ab-60effec84284"),
                BackgroundColorHex = "#fbf4c1",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2018-12-18T12:53:09.3660557Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T13:25:55.8335803Z"),
                HtmlContent = @"<h1>Recipe Yellow Split Pea Soup</h1><pre class='ql-syntax' spellcheck='false'>1.25 l    Water              put in pressure cooker.
200 g     Yellow beans       add to cooker.
2         Bouillon cube      crumble and add to cooker.
          Herbs              as much as you like.
</pre><p><br></p><pre class='ql-syntax' spellcheck='false'>0.25      Celery
2         Carrots            grate vegetables and add to cooker.
</pre><p>Bring the soup to boil, stir well so that no beans are 'sticking' to the bottom of the cooker, only now close the pressure cooker.</p><p><br></p><pre class='ql-syntax' spellcheck='false'>Time         30 min
Temperature  4 out of 10
</pre><p>Mix the soup with a blender and add some cream if you like.</p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("70a25de4-2141-4164-aefc-b9b2624a112c"),
                BackgroundColorHex = "#d0f8f9",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:39:12.8056002Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T13:25:41.3363938Z"),
                HtmlContent = @"<h1>Shopping list</h1><ul><li>Milk</li><li>Toast</li><li>Sun cream</li><li>Garbage bags</li></ul>"
            });

            DeletedNotes.Add(new Guid("fae40c63-d850-4b78-a8bd-609893d2983b"));
        }
    }
#endif
}
