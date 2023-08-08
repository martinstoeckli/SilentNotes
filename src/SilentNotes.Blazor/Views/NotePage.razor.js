import { initializeEditor, toggleFormat, isFormatActive, searchAndHighlight, selectNext, selectPrevious, selectWordAtCurrentPosition, makeLinkSuggestion } from '../prose-mirror-bundle.js';

var page;
var editor;

// Initializes the prosemirror editor
export function initialize(dotnetPage, editorContainer) {
    page = dotnetPage;
    editor = initializeEditor(editorContainer);

    editor.on('selectionUpdate', function (editor) {
        onActiveFormatStateChanged();
    });
    editor.on('update', function (editor) {
        onNoteContentChanged();
    });

    document.addEventListener('custom-link-clicked', function () {
        page.invokeMethodAsync('OpenLinkDialog');
    });

    return editor;
}

// By setting the content after loading the page, we can avoid that the content has to be
// declared pre rendered as javascript and therefore would occupy memory twice.
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
    onActiveFormatStateChanged();
}

function onActiveFormatStateChanged() {
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

function onNoteContentChanged() {
    var noteContent = editor.getHTML();
    page.invokeMethodAsync('SetNoteContent', noteContent);
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

export function prepareLinkDialog() {
    editor.chain().focus().run();
    var selectedUrl = editor.getAttributes('link').href;
    if (selectedUrl) {
        editor.commands.extendMarkRange('link');
        var text = selectWordAtCurrentPosition(editor);
        return [
            selectedUrl,
            text,
        ];
    }
    else {
        var text = selectWordAtCurrentPosition(editor);
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

    var commandChain = editor.chain().focus().extendMarkRange('link');

    if (newLinkIsEmpty) {
        commandChain = commandChain.unsetLink();
    }
    else if (titleChanged) {
        var selection = editor.view.state.selection;
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
