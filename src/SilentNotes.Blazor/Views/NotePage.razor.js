import { initializeEditor, toggleFormat, isFormatActive, searchAndHighlight, selectNextWhileTyping, selectNext, selectPrevious, selectWordAtCurrentPosition } from '../prose-mirror-bundle.js';

var _page;
var _editor;

// Initializes the prosemirror editor
export function initialize(dotnetPage, editorContainer, shoppingModeActive) {
    _page = dotnetPage;
    _editor = initializeEditor(editorContainer);

    _editor.on('selectionUpdate', onSelectionUpdate);
    _editor.on('update', onUpdate);

    document.addEventListener('custom-link-clicked', function () {
        _page.invokeMethodAsync('OpenLinkDialog');
    });
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
    var noteContent = _editor?.getHTML();
    _page?.invokeMethodAsync('SetNoteContent', noteContent);
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

export function prepareLinkDialog() {
    _editor.chain().focus().run();
    var selectedUrl = _editor.getAttributes('link').href;
    if (selectedUrl) {
        _editor.commands.extendMarkRange('link');
        var text = selectWordAtCurrentPosition(_editor);
        return [
            selectedUrl,
            text,
        ];
    }
    else {
        var text = selectWordAtCurrentPosition(_editor);
        return [
            '',
            text,
        ];
    }
}

export function linkDialogOkPressed(oldLinkUrl, newLinkUrl, oldLinkTitle, newLinkTitle) {
    var newLinkIsEmpty = !newLinkUrl;
    var urlChanged = newLinkUrl != oldLinkUrl;
    var titleChanged = newLinkTitle != oldLinkTitle;
    if (!urlChanged && !titleChanged)
        return;

    var commandChain = _editor.chain().focus().extendMarkRange('link');

    if (newLinkIsEmpty) {
        commandChain = commandChain.unsetLink();
    }
    else if (titleChanged) {
        var selection = _editor.view.state.selection;
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
