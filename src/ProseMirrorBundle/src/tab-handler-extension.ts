import { Extension } from '@tiptap/core'

// Leading tab characters '\u0009' are not stored, so we use 2 non breaking spaces instead.
const TAB_CHAR = '\u00A0\u00A0';

// This extension prevents, that the editor loses the focus when the user presses the tab key.
export const TabHandler = Extension.create({
  name: 'tabHandler',
  addKeyboardShortcuts() {
    return {
      Tab: ({ editor }) => {
        // Lists are indented with the tab key, so we handle tabs only when outside of a list.
        let isInsideList : boolean = (editor.isActive('bulletList') || editor.isActive('orderedList'));
        if (!isInsideList) {
          editor
            .chain()
            .command(({ tr }) => {
              tr.insertText(TAB_CHAR);
              return true;
            })
            .run();
            
          // Prevent default behavior (losing focus)
          return true;
        }
      },
    };
  },
});