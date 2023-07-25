import { initializeEditor, toggleFormat, isFormatActive, searchAndHighlight, selectNext, selectPrevious } from '../prose-mirror-bundle.js';

var page;
var editor;

// Initializes the prosemirror editor
export function initialize(dotnetPage, editorContainer) {
    page = dotnetPage;
    editor = initializeEditor(editorContainer);

    editor.on('selectionUpdate', function (editor) {
        refreshActiveFormatState();
    });

    return editor;
}

// By setting the content after loading the page, we can avoid that the content has to be
// declared statically as javascript and therefore would occupy memory twice.
export function setNoteContent(text) {
    try {
        editor.chain().setMeta('addToHistory', false).setContent(text).scrollToTop().run();
    }
    catch (ex) {
        editor.setEditable(false);
    }
}

export function setEditable(editable) {
    editor.setEditable(editable);
}

export function toggleFormatAndRefresh(formatName, formatParameter) {
    toggleFormat(editor, formatName, formatParameter);
    refreshActiveFormatState();
}

function refreshActiveFormatState() {
    var states = [
        isFormatActive(editor, 'heading', { level: 1 }),
        isFormatActive(editor, 'heading', { level: 2 }),
        isFormatActive(editor, 'heading', { level: 3 }),
        isFormatActive(editor, 'bold'),
        isFormatActive(editor, 'italic'),
        isFormatActive(editor, 'underline'),
        isFormatActive(editor, 'strike'),
        isFormatActive(editor, 'codeblock'),
        isFormatActive(editor, 'blockquote'),
        isFormatActive(editor, 'bulletlist'),
        isFormatActive(editor, 'orderedlist')
    ];
    page.invokeMethodAsync('RefreshActiveFormatState', states);
}

export function undo() {
    editor.commands.undo();
}

export function redo() {
    editor.commands.redo();
}

export function search(searchPattern) {
    searchAndHighlight(editor, searchPattern);
}

export function findNext() {
    selectNext(editor);
}

export function findPrevious() {
    selectPrevious(editor);
}