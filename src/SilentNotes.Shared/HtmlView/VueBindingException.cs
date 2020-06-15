// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.HtmlView
{
    public class VueBindingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingException"/> class.
        /// </summary>
        public VueBindingException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public VueBindingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">An inner exception which lead to this exception.</param>
        public VueBindingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
