// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Describes a wallpaper which is shown as background.
    /// </summary>
    public class WallpaperModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WallpaperModel"/> class.
        /// </summary>
        /// <param name="id">See the <see cref="Id"/> property.</param>
        /// <param name="image">Sets the <see cref="Image"/> property.</param>
        public WallpaperModel(string id, string image)
        {
            Id = id;
            Image = image;
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
    }
}
