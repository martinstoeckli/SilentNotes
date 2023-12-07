// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. http://mozilla.org/MPL/2.0/.
var _page = null;

async function callDotnetSnKeyboardShortcutsEvent(ev) {
    if (!_page || !ev || ev.isComposing || ev.keyCode === 229)
        return;

    var params = {
        key: ev.key,
        code: ev.keyCode.toString(),
        location: ev.location,
        repeat: ev.repeat,
        ctrlKey: ev.ctrlKey,
        shiftKey: ev.shiftKey,
        altKey: ev.altKey,
        metaKey: ev.metaKey,
        type: ev.type
    };
    var handled = await _page.invokeMethodAsync('SnKeyboardShortcutsEvent', params);
//    if (handled) {
//        ev.preventDefault();
//    }
}

window.SnKeyboardShortcuts = {
    initialize: function (dotnetPage) {
        _page = dotnetPage;
        window.document.addEventListener('keydown', callDotnetSnKeyboardShortcutsEvent);
    },

    dispose: function () {
        _page = null;
        window.document.removeEventListener('keydown', callDotnetSnKeyboardShortcutsEvent)
    }
}