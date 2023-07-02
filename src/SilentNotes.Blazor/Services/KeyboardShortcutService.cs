// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using MudBlazor;
using Toolbelt.Blazor.HotKeys2;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IKeyboardShortcutService"/> interface.
    /// </summary>
    public class KeyboardShortcutService : IKeyboardShortcutService
    {
        private readonly HotKeys _hotKeys;
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Implementation of the <see cref="KeyboardShortcutService"/> class.
        /// </summary>
        /// <param name="hotKeys">An instance of the HotKeys2 addin from the IOC.</param>
        /// <param name="navigationService">A navigation service from the IOC.</param>
        public KeyboardShortcutService(HotKeys hotKeys, INavigationService navigationService)
        {
            _hotKeys = hotKeys;
            _navigationService = navigationService;
        }

        /// <inheritdoc/>
        public IKeyboardShortcuts CreateShortcuts()
        {
            HotKeysContext context = _hotKeys.CreateContext();
            return new KeyboardShortcuts(context, _navigationService);
        }

        /// <summary>
        /// Implementation of the <see cref="IKeyboardShortcuts"/> interface.
        /// </summary>
        private class KeyboardShortcuts : IKeyboardShortcuts, IDisposable
        {
            private readonly INavigationService _navigationService;
            private HotKeysContext _context;

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyboardShortcuts"/> class.
            /// </summary>
            /// <param name="context">The wrapped hotkey context.</param>
            /// <param name="navigationService">The navigation service.</param>
            public KeyboardShortcuts(HotKeysContext context, INavigationService navigationService)
            {
                _context = context;
                _navigationService = navigationService;
            }

            /// <inheritdoc/>
            public IKeyboardShortcuts Add(ModCode modifiers, Code code, Func<MudBaseButton> button)
            {
                _context.Add(modifiers, code, () => SimulateButtonClick(button), string.Empty, Exclude.None);
                return this;
            }

            /// <inheritdoc/>
            public IKeyboardShortcuts Add(ModCode modifiers, Code code, Func<MudToggleIconButton> button)
            {
                _context.Add(modifiers, code, () => SimulateButtonClick(button), string.Empty, Exclude.None);
                return this;
            }

            /// <inheritdoc/>
            public IKeyboardShortcuts Add(ModCode modifiers, Code code, string href)
            {
                _context.Add(modifiers, code, () => _navigationService.NavigateTo(href), string.Empty, Exclude.None);
                return this;
            }

            public IKeyboardShortcuts Add(ModCode modifiers, Code code, Action action)
            {
                _context.Add(modifiers, code, action, string.Empty, Exclude.None);
                return this;
            }

            private async ValueTask SimulateButtonClick(Func<MudBaseButton> button)
            {
                MudBaseButton btn = button();

                // Before the button is clicked it gets the focus, so that other controls will
                // update their ViewModels with the current input.
                await btn.FocusAsync();

                if (!string.IsNullOrEmpty(btn.Href))
                {
                    string route = btn.Href.TrimStart('/');
                    _navigationService.NavigateTo(route);
                }
                else
                {
                    await btn.OnClick.InvokeAsync();
                }
            }

            private async ValueTask SimulateButtonClick(Func<MudToggleIconButton> button)
            {
                MudToggleIconButton btn = button();
                await btn.Toggle();
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }
    }
}
