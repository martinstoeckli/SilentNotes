// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Crypto;

namespace SilentNotes.Services
{
    /// <summary>
    /// Describes the interface of a cryptographically secure pseudo random number generator.
    /// </summary>
    public interface ICryptoRandomService : ICryptoRandomSource
    {
    }
}
