// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SilentNotesTest")]

namespace SilentNotes
{
    /// <summary>
    /// Application/solution wide static configs.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Initializes static members of the <see cref="Config"/> class.
        /// </summary>
        static Config()
        {
#if (RELEASE)
            RepositoryFileName = "silentnotes_repository.silentnotes";
            UserSettingsFileName = "silentnotes_user_settings.config";
#else
            RepositoryFileName = "silentnotes_repository_dev.silentnotes";
            UserSettingsFileName = "silentnotes_user_settings_dev.config";
#endif
        }

        /// <summary>
        /// Gets the filename without path, used to store the note repository
        /// </summary>
        public static string RepositoryFileName { get; private set; }

        /// <summary>
        /// Gets the filename without path, used to store the user settings
        /// </summary>
        public static string UserSettingsFileName { get; private set; }
    }
}
