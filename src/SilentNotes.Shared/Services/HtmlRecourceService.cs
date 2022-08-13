// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IHtmlRecourceService"/> interface.
    /// </summary>
    public class HtmlRecourceService : IHtmlRecourceService
    {
        /// <inheritdoc/>
        public string this[string id]
        { 
            get
            {
                switch (id)
                {
                    case "bootstrap-css": return "bootstrap5.min.css";
                    //case "bootstrap-js": return "bootstrap-bundle.js";
                    case "bootstrap-js": return "bootstrap5.bundle.min.js";
                    case "bootstrapcss": return "bootstrap.min.css";
                    case "bootstrapjs": return "bootstrap.bundle.min.js";
                    case "bootstrapautocompletejs": return "bootstrap-4-autocomplete.js";
                    case "vue-js": return "vue.min.js"; // v2.6.12 (v3 is not ECMAScript 5 compliant)
                    case "vuejs": return "vue.min.js"; // v2.6.12 (v3 is not ECMAScript 5 compliant)
                    case "jqueryjs": return "jquery-3.5.1.slim.min.js";
                    case "prosemirror": return "prose-mirror-bundle.js";
                    case "silentnotes-css": return "silentnotes2.css";
                    case "silentnotes-light-css": return "silentnotes2.light.css";
                    case "silentnotes-dark-css": return "silentnotes2.dark.css";
                    case "silentnotes-notecard-css": return "silentnotes2-notecard.css";
                    case "silentnotes-js": return "silentnotes.js";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
