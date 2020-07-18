// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Description of a keyboard shortcut.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "The Key* constants are self explanatory.")]
    public class VueBindingShortcut
    {
        public const string KeyEnter = "Enter";
        public const string KeyTab = "Tab";
        public const string KeySpace = " ";
        public const string KeyArrowDown = "ArrowDown";
        public const string KeyArrowLeft = "ArrowLeft";
        public const string KeyArrowRight = "ArrowRight";
        public const string KeyArrowUp = "ArrowUp";
        public const string KeyEnd = "End";
        public const string KeyHome = "Home";
        public const string KeyPageDown = "PageDown";
        public const string KeyPageUp = "PageUp";
        public const string KeyBackspace = "Backspace";
        public const string KeyDelete = "Delete";
        public const string KeyEscape = "Escape";
        public const string KeyHelp = "Help";

        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingShortcut"/> class.
        /// </summary>
        /// <param name="key">Sets the <see cref="Key"/> property. You can use one of the predefined
        /// Key* constants or other JavaScript key codes.</param>
        /// <param name="commandName">Sets the <see cref="CommandName"/> property.</param>
        public VueBindingShortcut(string key, string commandName)
        {
            Key = key;
            CommandName = commandName;
        }

        /// <summary>
        /// Gets or sets the name of the key to which the app should listen. This key must match
        /// one of the JavaScript key codes, see: https://developer.mozilla.org/de/docs/Web/API/KeyboardEvent/key/Key_Values
        /// some often used key codes are listed in the Key* constants.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the control key must be pressed (default is false).
        /// </summary>
        public bool Ctrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the shift key must be pressed (default is false).
        /// </summary>
        public bool Shift { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alt key must be pressed (default is false).
        /// </summary>
        public bool Alt { get; set; }

        /// <summary>
        /// Gets or sets the name of the ICommand from the viewmodel which should be executed.
        /// </summary>
        public string CommandName { get; set; }
    }
}
