import { initializeChecklist, registerIsShoppingModeActive, toggleFormat, isFormatActive, searchAndHighlight, selectNextWhileTyping, selectNext, selectPrevious, moveChecklist, sortChecklistPendingToTop, sortChecklistAlphabetical, setCheckStateForAllToTodo, setCheckStateForAllToDone, setCheckStateForAllToDisabled, exportChecklistAsPlainText } from '../prose-mirror-bundle.js';

var _page;

// Initializes the prosemirror editor
export function initialize(dotnetPage, editorContainer, shoppingModeActive) {
    _page = dotnetPage;
    _page.editor = initializeChecklist(editorContainer);
    _page.isBundlingNoteContentChanges = false;
    _page.shoppingModeActive = shoppingModeActive;
     registerIsShoppingModeActive(function () { return _page.shoppingModeActive; });

    _page.editor.on('selectionUpdate', onSelectionUpdate);
    _page.editor.on('update', onUpdate);
}

// Finalizes the prosemirror editor
export function finalize() {
    if (_page) {
        _page.editor.off('selectionUpdate', onSelectionUpdate);
        _page.editor.off('update', onUpdate);
    }
    _page.editor = null;
    _page = null;
}

function IsInitialized() {
    return _page && _page.editor;
}

// By setting the content after loading the page, we can avoid that the content has to be
// declared pre rendered as javascript and therefore would occupy memory twice.
export function setNoteContent(text) {
    if (!IsInitialized()) return;

    try {
        _page.editor.chain().setMeta('addToHistory', false).setContent(text).scrollToTop().run();
    }
    catch (ex) {
        _page.editor.setEditable(false);
    }
}

export function getAsPlainText() {
    return exportChecklistAsPlainText(_page.editor);
}

export function setEditable(editable) {
    if (!IsInitialized()) return;

    _page.editor.setEditable(editable);
}

export function setShoppingModeActive(shoppingModeActive) {
    if (!IsInitialized()) return;

    _page.shoppingModeActive = shoppingModeActive;
}

export function toggleFormatAndRefresh(formatName, formatParameter) {
    if (!IsInitialized()) return;

    toggleFormat(_page.editor, formatName, formatParameter);
    onActiveFormatStateChanged();
}

function onActiveFormatStateChanged() {
    if (!IsInitialized()) return;

    var states = [
        isFormatActive(_page.editor, 'heading', { level: 1 }),
        isFormatActive(_page.editor, 'heading', { level: 2 }),
        isFormatActive(_page.editor, 'heading', { level: 3 }),
        isFormatActive(_page.editor, 'bold'),
        isFormatActive(_page.editor, 'italic'),
        isFormatActive(_page.editor, 'underline'),
        isFormatActive(_page.editor, 'strike'),
        isFormatActive(_page.editor, 'codeblock'),
        isFormatActive(_page.editor, 'blockquote'),
        isFormatActive(_page.editor, 'bulletlist'),
        isFormatActive(_page.editor, 'orderedlist')
    ];
    _page.invokeMethodAsync('RefreshActiveFormatState', states);
}

function onSelectionUpdate(params) {
    onActiveFormatStateChanged();
}

function onUpdate(params) {
    onNoteContentChanged(params.editor);
}

function onNoteContentChanged(editor) {
    if (!IsInitialized()) return;

    if (!_page.isBundlingNoteContentChanges) {
        var noteContent = editor.getHTML();
        _page.invokeMethodAsync('SetNoteContent', noteContent);
    }
}

/* Collects all calls to onNoteContentChanged() and bundles them to a single call. */
function bundleNoteContentChanged(delegate) {
    if (!IsInitialized()) return;

    try {
        _page.isBundlingNoteContentChanges = true;
        delegate();
    }
    finally {
        _page.isBundlingNoteContentChanges = false;
    }
    onNoteContentChanged(_page.editor);
}

export function undo() {
    if (!IsInitialized()) return;

    _page.editor.commands.undo();
}

export function redo() {
    if (!IsInitialized()) return;

    _page.editor.commands.redo();
}

export function search(searchPattern) {
    if (!IsInitialized()) return;

    searchAndHighlight(_page.editor, searchPattern);
    selectNextWhileTyping(_page.editor);
}

export function findNext() {
    if (!IsInitialized()) return;

    selectNext(_page.editor);
}

export function findPrevious() {
    if (!IsInitialized()) return;

    selectPrevious(_page.editor);
}

export function moveChecklistItem(upwards, singleStep) {
    if (!IsInitialized()) return;

    moveChecklist(_page.editor, upwards, singleStep);
}

export function sortPendingToTop() {
    if (!IsInitialized()) return;

    bundleNoteContentChanged(function () { sortChecklistPendingToTop(_page.editor); });
}

export function sortAlphabetical() {
    if (!IsInitialized()) return;

    bundleNoteContentChanged(function () { sortChecklistAlphabetical(_page.editor); });
}

export function setForAllToTodo() {
    if (!IsInitialized()) return;

    bundleNoteContentChanged(function () { setCheckStateForAllToTodo(_page.editor); });
}

export function setForAllToDone() {
    if (!IsInitialized()) return;

    bundleNoteContentChanged(function () { setCheckStateForAllToDone(_page.editor); });
}

export function setForAllToDisabled() {
    if (!IsInitialized()) return;

    bundleNoteContentChanged(function () { setCheckStateForAllToDisabled(_page.editor); });
}
