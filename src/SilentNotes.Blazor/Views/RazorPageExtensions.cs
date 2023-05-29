// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.AspNetCore.Components;

namespace SilentNotes.Views
{
    /// <summary>
    /// Extension methods for components on a Razor page.
    /// </summary>
    internal static class RazorPageExtensions
    {
        /// <summary>
        /// Inserts the <paramref name="cssClassName"/> only if a given <paramref name="condition"/>
        /// is met.
        /// <example><code>
        /// class="@this.CssClassIf("toggled", ViewModel.CheckIfToggled())"
        /// </code></example>
        /// </summary>
        /// <param name="page">The Razor page component to extend.</param>
        /// <param name="cssClassName">The class name to return if the condition is met.</param>
        /// <param name="condition">Only if this condition is met, the class is returned.</param>
        /// <returns>The CSS class name, or null if the condition is not met.</returns>
        public static string CssClassIf(this ComponentBase page, string cssClassName, bool condition)
        {
            return condition ? cssClassName : null;
        }
    }
}
