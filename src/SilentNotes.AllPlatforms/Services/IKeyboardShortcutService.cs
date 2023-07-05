// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using MudBlazor;
using Toolbelt.Blazor.HotKeys2;

namespace SilentNotes.Services
{
    /// <summary>
    /// Service which allows to set keyboard shortcuts.
    /// It makes use of the <see cref="Toolbelt.Blazor.HotKeys2.HotKeys"/> addin, and requires the
    /// :data to be added to the CSP header (e.g. script-src 'self' data:).
    /// </summary>
    public interface IKeyboardShortcutService
    {
        /// <summary>
        /// Creates a shortcut collection which defines the keyboard shortcuts of a given razor
        /// page. Dispose the generated collection in the <see cref="IDisposable.Dispose"/>
        /// method of the razor page.
        /// </summary>
        /// <returns>Shortcut collection.</returns>
        IKeyboardShortcuts CreateShortcuts();
    }

    /// <summary>
    /// The keyboard shortcut collection is created by the <see cref="IKeyboardShortcutService"/>.
    /// It can be created in the OnInitialized() or in the OnParametersSet() part of a razor page
    /// and should be disposed in the Dispose() of the page.
    /// </summary>
    public interface IKeyboardShortcuts : IDisposable
    {
        /// <summary>
        /// Defines a new keyboard shortcut for the razor page.
        /// </summary>
        /// <param name="modifiers">The flags of modifier keys, or <see cref="ModCode.None"/> if no
        /// modifier should be active.</param>
        /// <param name="code">The keyboard code to react to.</param>
        /// <param name="button">Simulates a click on this button. A Func is required, so that it
        /// is guaranteed that the @ref is assigned at the time of activating the shortcut.</param>
        /// <returns>Returns itself for a fluent declaration of shortcuts.</returns>
        IKeyboardShortcuts Add(ModCode modifiers, Code code, Func<MudBaseButton> button);

        /// <summary>
        /// Defines a new keyboard shortcut for the razor page.
        /// </summary>
        /// <param name="modifiers">The flags of modifier keys, or <see cref="ModCode.None"/> if no
        /// modifier should be active.</param>
        /// <param name="code">The keyboard code to react to.</param>
        /// <param name="button">Simulates a click on this button. A Func is required, so that it
        /// is guaranteed that the @ref is assigned at the time of activating the shortcut.</param>
        /// <returns>Returns itself for a fluent declaration of shortcuts.</returns>
        IKeyboardShortcuts Add(ModCode modifiers, Code code, Func<MudToggleIconButton> button);

        /// <summary>
        /// Defines a new keyboard shortcut for the razor page.
        /// </summary>
        /// <param name="modifiers">The flags of modifier keys, or <see cref="ModCode.None"/> if no
        /// modifier should be active.</param>
        /// <param name="code">The keyboard code to react to.</param>
        /// <param name="href">Assigns this navigation route to the shortcut.</param>
        /// <returns>Returns itself for a fluent declaration of shortcuts.</returns>
        IKeyboardShortcuts Add(ModCode modifiers, Code code, string href);

        /// <summary>
        /// Defines a new keyboard shortcut for the razor page.
        /// </summary>
        /// <param name="modifiers">The flags of modifier keys, or <see cref="ModCode.None"/> if no
        /// modifier should be active.</param>
        /// <param name="code">The keyboard code to react to.</param>
        /// <param name="action">Assigns this action to the shortcut.</param>
        /// <returns>Returns itself for a fluent declaration of shortcuts.</returns>
        IKeyboardShortcuts Add(ModCode modifiers, Code code, Action action);
    }
}
