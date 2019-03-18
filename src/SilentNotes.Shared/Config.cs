// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
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
            RunningMode = RunningModes.Production;
#if (ENV_DEMO && DEBUG)
            RunningMode = RunningModes.Demo;
#endif
        }

        /// <summary>
        /// Gets the mode of the application.
        /// Make sure this value is set to <see cref="RunningModes.Production"/> to deploy the application.
        /// </summary>
        public static RunningModes RunningMode { get; internal set; }

        /// <summary>
        /// Gets the filename without path, used to store the note repository
        /// </summary>
        public static string RepositoryFileName
        {
            get
            {
                switch (RunningMode)
                {
                    case RunningModes.Production:
                        return "silentnotes_repository.silentnotes";
                    case RunningModes.Demo:
                        return "silentnotes_repository_demo.silentnotes";
                    case RunningModes.UnitTest:
                        return "silentnotes_repository_unittest.silentnotes";
                    default:
                        throw new ArgumentOutOfRangeException("Config.RunningMode");
                }
            }
        }

        /// <summary>
        /// Gets the filename without path, used to store the user settings
        /// </summary>
        public static string UserSettingsFileName
        {
            get
            {
                switch (RunningMode)
                {
                    case RunningModes.Production:
                        return "silentnotes_user_settings.config";
                    case RunningModes.Demo:
                        return "silentnotes_user_settings_demo.config";
                    case RunningModes.UnitTest:
                        return "silentnotes_user_settings_unittest.config";
                    default:
                        throw new ArgumentOutOfRangeException("Config.RunningMode");
                }
            }
        }

        /// <summary>
        /// Enumeration of all known running modes.
        /// </summary>
        public enum RunningModes
        {
            Production,
            Demo,
            UnitTest,
        }
    }
}
