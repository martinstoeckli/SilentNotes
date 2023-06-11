// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Windows.Input;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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

        /// <summary>
        /// Calls the execute method of a command and closes the overflowmenu beforehand.
        /// This extension method can be used in an MudNavLink.OnClick handler, it makes sure that
        /// the cklicked menu is closed.
        /// </summary>
        /// <param name="command">The command to extend.</param>
        /// <param name="overflowMenu">Closes this overflow menu before executing the command.</param>
        public static void ExecuteAndCloseMenu(this ICommand command, MudMenu overflowMenu)
        {
            overflowMenu?.CloseMenu();
            command.Execute(null);
        }
    }

    /// <summary>
    /// Deactivate validation because it removes the error information.
    /// See: https://github.com/MudBlazor/MudBlazor/issues/4593
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MudTextFieldEx<T> : MudTextField<T>
    {
        protected override Task ValidateValue()
        {
            if (Validation == null)
                return Task.CompletedTask;
            return base.ValidateValue();
        }
    }
}
