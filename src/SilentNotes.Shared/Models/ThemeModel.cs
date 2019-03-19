// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Describes a theme which controls the apperance of the application.
    /// </summary>
    public class ThemeModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeModel"/> class.
        /// </summary>
        /// <param name="id">See the <see cref="Id"/> property.</param>
        /// <param name="image">Sets the <see cref="Image"/> property.</param>
        /// <param name="imageTint">Sets the <see cref="ImageTint"/> property.</param>
        public ThemeModel(string id, string image, string imageTint)
        {
            Id = id;
            Image = image;
            ImageTint = imageTint;
        }

        /// <summary>
        /// Gets the identificator of the theme, used for serialization.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets a relative file without path to the background texture image.
        /// Example: "cork.jpg"
        /// </summary>
        public string Image { get; private set; }

        /// <summary>
        /// Gets a hex color representing the <see cref="Image"/> to avoid flickering when
        /// navigating between pages. Example: "#00ff00"
        /// </summary>
        public string ImageTint { get; private set; }
    }
}
