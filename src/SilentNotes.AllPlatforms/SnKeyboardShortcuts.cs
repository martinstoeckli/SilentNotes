// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace SilentNotes
{
    /// <summary>
    /// Adds keyboard shortcut / hotkeys support for Blazor Hybrid applications.
    /// It requires a small javascript file "sn-keyboard-shortcuts.js", which should be added to
    /// the "wwwroot" directory and must be declared in the index.html file.
    /// <example>
    /// index.html:
    /// <code>
    /// &lt;script src=&quot;sn-keyboard-shortcuts.js&quot;&gt;&lt;/script&gt;
    /// </code>
    /// 
    /// razor page @code section:
    /// <code>
    /// protected override async Task OnAfterRenderAsync(bool firstRender)
    /// {
    ///     if (firstRender)
    ///     {
    ///         _shortcuts = new SnKeyboardShortcuts();
    ///         await _shortcuts.InitializeAsync(JSRuntime);
    ///         _shortcuts.AddShortcut(new SnKeyboardShortcut("c", true), CopyAction);
    ///     }
    /// }
    /// 
    /// protected override void OnBeforeNavigatingToOtherPage()
    /// {
    ///     _shortcuts?.Dispose();
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class SnKeyboardShortcuts : IDisposable
    {
        private readonly bool _platformSupportsShortcuts = false;
        private bool _disposed = false;
        private IJSRuntime _jsRuntime;
        private DotNetObjectReference<SnKeyboardShortcuts> _dotnetModule;
        private Dictionary<SnKeyboardShortcut, Action> _shortcuts;
        private KeyValuePair<SnKeyboardShortcut, Func<bool>> _closeMenuShortcut;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnKeyboardShortcuts"/> class.
        /// </summary>
        public SnKeyboardShortcuts()
        {
#if WINDOWS
            _platformSupportsShortcuts = true;
#endif
            _shortcuts = new Dictionary<SnKeyboardShortcut, Action>();
            _closeMenuShortcut = new KeyValuePair<SnKeyboardShortcut, Func<bool>>(null, null);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SnKeyboardShortcuts"/> class.
        /// </summary>
        ~SnKeyboardShortcuts()
        {
            Dispose(false);
        }

        public ValueTask InitializeAsync(IJSRuntime jsRuntime)
        {
            if (!_platformSupportsShortcuts)
                return ValueTask.CompletedTask;
            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));
            if (_jsRuntime != null)
                throw new Exception("Initialization is already done.");

            _jsRuntime = jsRuntime;
            _dotnetModule = DotNetObjectReference.Create(this);
            return _jsRuntime.InvokeVoidAsync("SnKeyboardShortcuts.initialize", _dotnetModule);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Method invoked when either <see cref="Dispose()"/> or the finalizer is called.
        /// </summary>
        /// <param name="disposing">Is true if called by <see cref="Dispose()"/>, false if called
        /// by the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!_platformSupportsShortcuts)
                return;

            if (!_disposed)
            {
                _disposed = true;
                try
                {
                    // Remove js event listener.
                    _jsRuntime?.InvokeVoidAsync("SnKeyboardShortcuts.dispose");
                }
                catch (Exception)
                {
                    // We tried our best
                }
                _jsRuntime = null;
                _dotnetModule?.Dispose();
                _dotnetModule = null;
            }
        }

        /// <summary>
        /// Called from the javascript to update the content of the note.
        /// </summary>
        [JSInvokable("SnKeyboardShortcutsEvent")]
        public Task<bool> SnKeyboardShortcutsEvent(KeyboardEventArgs ev)
        {
            if (_disposed)
                return Task.FromResult(false);

            SnKeyboardShortcut eventShortcut = SnKeyboardShortcut.FromEvent(ev);
            if (SnKeyboardShortcut.Equals(eventShortcut, _closeMenuShortcut.Key))
            {
                Func<bool> closeMenuHandler = _closeMenuShortcut.Value;
                if (closeMenuHandler())
                    return Task.FromResult(true);
            }

            if (_shortcuts.TryGetValue(eventShortcut, out Action handler))
            {
                handler?.Invoke();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// Defines a new keyboard shortcut for the razor page.
        /// </summary>
        /// <param name="shortcut">Describes the state of the keys to trigger the shortcut.</param>
        /// <param name="handler">The handler assigned to this shortcut.</param>
        /// <returns>Returns itself for a fluent declaration of shortcuts.</returns>
        public SnKeyboardShortcuts AddShortcut(SnKeyboardShortcut shortcut, Action handler)
        {
            if (shortcut == null)
                throw new ArgumentNullException(nameof(shortcut));

            _shortcuts.Add(shortcut, handler);
            return this;
        }

        /// <summary>
        /// Defines a keyboard shortcut for the razor page, whose <paramref name="handler"/> can
        /// close open menus and dialogs (usually the Escape key). This handler is called before
        /// any other shortcut handlers.
        /// </summary>
        /// <param name="shortcut">Describes the state of the keys to trigger the shortcut (Escape).</param>
        /// <param name="handler">The handler assigned to this shortcut, it should return "true"
        /// if something has been closed to prevent further actions, otherwise false.</param>
        /// <returns>Returns itself for a fluent declaration of shortcuts.</returns>
        public SnKeyboardShortcuts SetCloseMenuShortcut(SnKeyboardShortcut shortcut, Func<bool> handler)
        {
            if (shortcut == null)
                throw new ArgumentNullException(nameof(shortcut));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _closeMenuShortcut = new KeyValuePair<SnKeyboardShortcut, Func<bool>>(shortcut, handler);
            return this;
        }

        /// <summary>
        /// Simulates a click on the <paramref name="button"/>. As with the user doing the click,
        /// the button first gets the focus and then invokes the OnClick. By moving the focus,
        /// other input elements like textboxes can update their ViewModels, before the click.
        /// </summary>
        /// <param name="button">The button to click.</param>
        /// <returns>Awaitable task.</returns>
        public static async Task SimulateClickAsync(MudBaseButton button)
        {
            // Before the button is clicked it gets the focus, so that other controls will
            // update their ViewModels with the current input.
            await button.FocusAsync();
            await button.OnClick.InvokeAsync();
        }
    }

    public class SnKeyboardShortcut : IEquatable<SnKeyboardShortcut>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnKeyboardShortcut"/> class.
        /// </summary>
        /// <param name="key">Sets the <see cref="Key"/> property, correct values one can get from
        /// the <see cref="SnKey"/> class and it must not be null.</param>
        /// <param name="ctrl">Sets the <see cref="Ctrl"/> property.</param>
        /// <param name="shift">Sets the <see cref="Shift"/> property.</param>
        /// <param name="alt">Sets the <see cref="Alt"/> property.</param>
        public SnKeyboardShortcut(string key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            Key = key;
            Ctrl = ctrl;
            Shift = shift;
            Alt = alt;
        }

        /// <summary>
        /// Gets the name of the key to which the app should listen. This key must match
        /// one of the JavaScript key codes. Some often used key codes are listed in the
        /// <see cref="SnKey"/> class.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets a value indicating whether the control key must be pressed (default is false).
        /// </summary>
        public bool Ctrl { get; }

        /// <summary>
        /// Gets a value indicating whether the shift key must be pressed (default is false).
        /// </summary>
        public bool Shift { get; }

        /// <summary>
        /// Gets a value indicating whether the alt key must be pressed (default is false).
        /// </summary>
        public bool Alt { get; }

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="SnKeyboardShortcut"/> class,
        /// taking the values from a keyboard event.
        /// </summary>
        /// <param name="ev">Keyboard event.</param>
        /// <returns>New shortcut instance.</returns>
        public static SnKeyboardShortcut FromEvent(KeyboardEventArgs ev)
        {
            return new SnKeyboardShortcut(ev.Key, ev.CtrlKey, ev.ShiftKey, ev.AltKey);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is SnKeyboardShortcut other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(SnKeyboardShortcut other)
        {
            return string.Equals(Key, other.Key, StringComparison.InvariantCultureIgnoreCase)
                && Ctrl == other.Ctrl
                && Shift == other.Shift
                && Alt == other.Alt;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Key.ToLowerInvariant().GetHashCode();
                hashCode = (hashCode * 397) + Ctrl.GetHashCode();
                hashCode = (hashCode * 397) + Shift.GetHashCode();
                hashCode = (hashCode * 397) + Alt.GetHashCode();
                return hashCode;
            }
        }
    }

    /// <summary>
    /// see: https://developer.mozilla.org/de/docs/Web/API/KeyboardEvent/key/Key_Values
    /// </summary>
    public static class SnKey
    {
        public const string Enter = "Enter";
        public const string Tab = "Tab";
        public const string Space = " ";
        public const string ArrowDown = "ArrowDown";
        public const string ArrowLeft = "ArrowLeft";
        public const string ArrowRight = "ArrowRight";
        public const string ArrowUp = "ArrowUp";
        public const string End = "End";
        public const string Home = "Home";
        public const string PageDown = "PageDown";
        public const string PageUp = "PageUp";
        public const string Backspace = "Backspace";
        public const string Delete = "Delete";
        public const string Escape = "Escape";
        public const string Help = "Help";
        public const string F1 = "F1";
        public const string F2 = "F2";
        public const string F3 = "F3";
        public const string F4 = "F4";
        public const string F5 = "F5";
        public const string F6 = "F6";
        public const string F7 = "F7";
        public const string F8 = "F8";
        public const string F9 = "F9";
        public const string F10 = "F10";
        public const string F11 = "F11";
        public const string F12 = "F12";
    }
}
