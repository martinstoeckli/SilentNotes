// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Security;
using VanillaCloudStorageClient;

namespace SilentNotes.Views
{
    /// <summary>
    /// A MudBlazor compatible converter between a SecureString and String.
    /// </summary>
    /// <remarks>
    /// Ideally a conversion between String and SecureString would never happen, but for this to
    /// work we would need UI controls which support SecureString. In HTML no such controls exist,
    /// so we cannot entirely avoid the conversion. We do the second best and get the string from
    /// the input control and store it in a disposable SecureString.
    /// </remarks>
    public class SecureStringConverter : MudBlazor.Converter<SecureString, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecureStringConverter"/> class.
        /// </summary>
        public SecureStringConverter()
        {
            SetFunc = SecureStringExtensions.SecureStringToString;
            GetFunc = SecureStringExtensions.StringToSecureString;
        }
    }
}
