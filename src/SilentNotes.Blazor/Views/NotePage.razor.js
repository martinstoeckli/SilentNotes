import { initializeEditor } from '../prose-mirror-bundle.js';

// Initializes the prosemirror editor
export function initialize(editorContainer) {
    var editor = initializeEditor(editorContainer);
    return editor;
}

// By setting the content after loading the page, we can avoid that the content has to be
// declared statically as javascript and therefore would occupy memory twice.
export function setNoteContent(editor, text) {
    try {
        editor.chain().setMeta('addToHistory', false).setContent(text).scrollToTop().run();
    }
    catch (ex) {
        editor.setEditable(false);
    }
}
