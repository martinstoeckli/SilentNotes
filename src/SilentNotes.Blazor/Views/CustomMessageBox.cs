// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Reflection.Metadata;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SilentNotes.Views
{
    internal class CustomMessageBox : MudMessageBox
    {
        /// <summary>
        /// Gets or sets a parameter indicating whether the enter key is executing the primary
        /// action or not. A value "false" (default) will execute the primary action, "true" will
        /// set the focus on the cancel button.
        /// </summary>
        [Parameter]
        public bool ConservativeDefault { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Take focus from the primary action (last button)
            if (ConservativeDefault)
                UserAttributes.Add("DefaultFocus", DefaultFocus.FirstChild);
        }
    }
}
