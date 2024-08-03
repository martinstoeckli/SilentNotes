import { initializeChecklist, registerIsShoppingModeActive, toggleFormat, isFormatActive, searchAndHighlight, selectNextWhileTyping, selectNext, selectPrevious, moveChecklist, sortChecklistPendingToTop, sortChecklistAlphabetical, setCheckStateForAllToTodo, setCheckStateForAllToDone, setCheckStateForAllToDisabled } from '../prose-mirror-bundle.js';

var _page;
var _editor;
var _shoppingModeActive;
var _isBundlingNoteContentChanges;

// Initializes the prosemirror editor
export function initialize(dotnetPage, editorContainer, shoppingModeActive) {
    _page = dotnetPage;
    _editor = initializeChecklist(editorContainer);
    _isBundlingNoteContentChanges = false;

    _editor.on('selectionUpdate', onSelectionUpdate);
    _editor.on('update', onUpdate);

    _shoppingModeActive = shoppingModeActive;
    registerIsShoppingModeActive(function () { return _shoppingModeActive; });
    return _editor;
}

// Finalizes the prosemirror editor
export function finalize() {
    _editor?.off('selectionUpdate', onSelectionUpdate);
    _editor?.off('update', onUpdate);
    _page = null;
    _editor = null;
}

function onSelectionUpdate(editor) {
    onNoteContentChanged();
}

function onUpdate(editor) {
    onNoteContentChanged();
}

// By setting the content after loading the page, we can avoid that the content has to be
// declared pre rendered as javascript and therefore would occupy memory twice.
export function setNoteContent(text) {
    try {
        _editor?.chain().setMeta('addToHistory', false).setContent(text).scrollToTop().run();
    }
    catch (ex) {
        _editor?.setEditable(false);
    }
}

export function setEditable(editable) {
    _editor?.setEditable(editable);
}

export function setShoppingModeActive(shoppingModeActive) {
    _shoppingModeActive = shoppingModeActive;
}

export function toggleFormatAndRefresh(formatName, formatParameter) {
    toggleFormat(_editor, formatName, formatParameter);
    onActiveFormatStateChanged();
}

function onActiveFormatStateChanged() {
    var states = [
        isFormatActive(_editor, 'heading', { level: 1 }),
        isFormatActive(_editor, 'heading', { level: 2 }),
        isFormatActive(_editor, 'heading', { level: 3 }),
        isFormatActive(_editor, 'bold'),
        isFormatActive(_editor, 'italic'),
        isFormatActive(_editor, 'underline'),
        isFormatActive(_editor, 'strike'),
        isFormatActive(_editor, 'codeblock'),
        isFormatActive(_editor, 'blockquote'),
        isFormatActive(_editor, 'bulletlist'),
        isFormatActive(_editor, 'orderedlist')
    ];
    _page.invokeMethodAsync('RefreshActiveFormatState', states);
}

function onNoteContentChanged() {
    if (!_isBundlingNoteContentChanges) {
        var noteContent = _editor?.getHTML();
        _page?.invokeMethodAsync('SetNoteContent', noteContent);
    }
}

/* Collects all calls to onNoteContentChanged() and bundles them to a single call. */
function bundleNoteContentChanged(delegate) {
    try {
        _isBundlingNoteContentChanges = true;
        delegate();
    }
    finally {
        _isBundlingNoteContentChanges = false;
    }
    onNoteContentChanged();
}

export function undo() {
    _editor?.commands.undo();
}

export function redo() {
    _editor?.commands.redo();
}

export function search(searchPattern) {
    searchAndHighlight(_editor, searchPattern);
    selectNextWhileTyping(_editor);
}

export function findNext() {
    selectNext(_editor);
}

export function findPrevious() {
    selectPrevious(_editor);
}

export function moveChecklistItem(upwards, singleStep) {
    moveChecklist(_editor, upwards, singleStep);
}

export function sortPendingToTop() {
    bundleNoteContentChanged(sortChecklistPendingToTop(_editor));
}

export function sortAlphabetical() {
    bundleNoteContentChanged(sortChecklistAlphabetical(_editor));
}

export function setForAllToTodo() {
    bundleNoteContentChanged(setCheckStateForAllToTodo(_editor));
}

export function setForAllToDone() {
    bundleNoteContentChanged(setCheckStateForAllToDone(_editor));
}

export function setForAllToDisabled() {
    bundleNoteContentChanged(setCheckStateForAllToDisabled(_editor));
}
