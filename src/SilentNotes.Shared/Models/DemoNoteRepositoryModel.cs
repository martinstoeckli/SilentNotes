// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
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

            Notes.Add(new NoteModel
            {
                Id = new Guid("57f1b162-85e7-4402-8731-05b9c06c6915"),
                NoteType = NoteType.Text,
                BackgroundColorHex = "#fbf4c1",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:25:16.6787213Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:52:39.8746458Z"),
                Tags = new List<string> { "Shopping" },
                HtmlContent = @"<h1>Opening hours</h1><h3>Post office</h3><p>Monday - Friday 9:00-12:00 / 13:00-17:00</p><p>Saturday 9:00-12:00</p><h3>Supermarket</h3><p>Monday - Sunday 7:00-22:00</p><h3>Ticket shop</h3><p>Monday - Friday 8:00-18:30</p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("62099c1c-e0b5-418f-bd5f-722f72fc8d77"),
                NoteType = NoteType.Checklist,
                BackgroundColorHex = "#d0f8f9",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:25:16.6787213Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:52:39.8746458Z"),
                Tags = new List<string> { "Shopping" },
                HtmlContent = @"<h1>🛒 Shopping list</h1><p class='done'>Milk</p><p class='done'>Toast</p><p>Sun cream</p><p>Garbage bags</p><h1>Bakery</h1><p>Croissants</p><p>Marmelade</p><p class='disabled'>Torte</p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("a2b16ab9-9f7f-4389-916f-f2ef9a2f3a3a"),
                NoteType = NoteType.Text,
                BackgroundColorHex = "#fdd8bb",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:44:14.5098885Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T13:28:48.6114923Z"),
                HtmlContent = @"<h1>Borrowed / loaned</h1><p>📗 'The Black Magician Trilogy' ➜ from aunt Maggie</p><p>📀 'The golden compass' ➜ from Tim</p><p>📕 'Harry potter 5' ➜ to Dawn Cook</p></p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("265d5706-8e46-4aee-93d4-424718ed13dd"),
                NoteType = NoteType.Checklist,
                BackgroundColorHex = "#fbf4c1",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:51:49.7504749Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:51:34.2026179Z"),
                HtmlContent = @"<h1>🚴‍ Bike tour</h1><p class='done'>Tent</p><p class='done'>Sleeping bag</p><p>Maps</p><p>Change €-$</p><p>Spare tire / repair kit</p><p>Swimsuit</p><p>Sun cream</p><p>Towel</p><p>Power adapter</p><p>Gas cooker</p><p>Mess kit</p><p>Detergent</p><p>Spare clothes</p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("c35de588-b92f-49a4-a19e-319f045619f5"),
                NoteType = NoteType.Text,
                BackgroundColorHex = "#d9f8c8",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:51:49.7504749Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:51:34.2026179Z"),
                HtmlContent = @"<h1>🎧 Radio Songs</h1><ol><li>Spider's web (Katie Melua)</li><li>Seven mountains (77 Bombay Street)</li><li>Waiting for A.M. (Glen Of Guinness)</li></ol>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("33f00756-6c8b-416d-abe4-2e27d9f58615"),
                NoteType = NoteType.Text,
                BackgroundColorHex = "#fbf4c1",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:35:52.9190418Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:54:18.333087Z"),
                Tags = new List<string> { "Shopping" },
                HtmlContent = @"<h1>Printer refill</h1><p>brother DCP-9020CDW</p><p>Cartridge <strong>TN-241* </strong><em>(1400 pages)</em></p><p>Cartridge <strong>TN-245*</strong> <em>(2200 pages)</em></p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("8f9e539d-172c-41e8-99ab-60effec84284"),
                NoteType = NoteType.Text,
                BackgroundColorHex = "#d0f8f9",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2018-12-18T12:53:09.3660557Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T13:25:55.8335803Z"),
                Tags = new List<string> { "Recipes", "Cooking" },
                HtmlContent = @"<h1>Recipe Yellow Split Pea Soup</h1><pre><code>1.25 l    Water              put in pressure cooker.
200 g     Yellow beans       add to cooker.
2         Bouillon cube      crumble and add to cooker.
          Herbs              as much as you like.</code></pre><p></p><pre><code>0.25      Celery
2         Carrots            grate vegetables and add to cooker.</code></pre><p>Bring the soup to boil, stir well so that no beans are 'sticking' to the bottom of the cooker, only now close the pressure cooker.</p><p></p><pre><code>Time         30 min
Temperature  4 out of 10</code></pre><p>Mix the soup with a blender and add some cream if you like.</p>"
            });

            Notes.Add(new NoteModel
            {
                Id = new Guid("a5076c29-0503-4d06-b1d0-4759868be7c6"),
                NoteType = NoteType.Text,
                BackgroundColorHex = "#fdd8bb",
                InRecyclingBin = false,
                CreatedAt = DateTime.Parse("2017-09-10T09:25:16.6787213Z"),
                ModifiedAt = DateTime.Parse("2018-12-18T12:52:42.8746458Z"),
                HtmlContent = @"<h1>Fun</h1><h3>Old enough</h3><p>The son asks his father: ""Can I borrow your car, I'm old enough?"". The father answers: ""Yes you are, but the car is not"".</p>",
            });

            DeletedNotes.Add(new Guid("fae40c63-d850-4b78-a8bd-609893d2983b"));
        }
    }
#endif
}
