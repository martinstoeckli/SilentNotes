import { initializeEditor, toggleFormat, isFormatActive, searchAndHighlight, selectNextWhileTyping, selectNext, selectPrevious, selectWordAtCurrentPosition, exportAsPlainText } from '../prose-mirror-bundle.js';

var _page;

// Initializes the prosemirror editor
export function initialize(dotnetPage, editorContainer, shoppingModeActive) {
    _page = dotnetPage;
    _page.editor = initializeEditor(editorContainer);

    _page.editor.on('selectionUpdate', onSelectionUpdate);
    _page.editor.on('update', onUpdate);

    document.addEventListener('custom-link-clicked', function () {
        _page.invokeMethodAsync('OpenLinkDialog');
    });
}

// Finalizes the prosemirror editor
export function finalize() {
    if (_page.editor) {
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
    return exportAsPlainText(_page.editor);
}

export function setEditable(editable) {
    if (!IsInitialized()) return;

    _page.editor.setEditable(editable);
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

    var noteContent = editor.getHTML();
    _page.invokeMethodAsync('SetNoteContent', noteContent);
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

export function prepareLinkDialog() {
    if (!IsInitialized()) return;

    _page.editor.chain().focus().run();
    var selectedUrl = _page.editor.getAttributes('link').href;
    if (selectedUrl) {
        _page.editor.commands.extendMarkRange('link');
        var text = selectWordAtCurrentPosition(_page.editor);
        return [
            selectedUrl,
            text,
        ];
    }
    else {
        var text = selectWordAtCurrentPosition(_page.editor);
        return [
            '',
            text,
        ];
    }
}

export function linkDialogOkPressed(oldLinkUrl, newLinkUrl, oldLinkTitle, newLinkTitle) {
    if (!IsInitialized()) return;

    var newLinkIsEmpty = !newLinkUrl;
    var urlChanged = newLinkUrl != oldLinkUrl;
    var titleChanged = newLinkTitle != oldLinkTitle;
    if (!urlChanged && !titleChanged)
        return;

    var commandChain = _page.editor.chain().focus().extendMarkRange('link');

    if (newLinkIsEmpty) {
        commandChain = commandChain.unsetLink();
    }
    else if (titleChanged) {
        var selection = _page.editor.view.state.selection;
        commandChain = commandChain.command(({ tr }) => {
            tr.insertText(newLinkTitle);
            return true;
        });
        commandChain = commandChain.setTextSelection({ from: selection.$from.pos, to: selection.$from.pos + newLinkTitle.length });
        commandChain = commandChain.setLink({ href: newLinkUrl });
    }
    else if (urlChanged) {
        commandChain = commandChain.setLink({ href: newLinkUrl });
    }
    commandChain.run();
}
