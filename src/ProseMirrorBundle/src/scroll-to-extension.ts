import { Extension } from '@tiptap/core';
import { TextSelection } from 'prosemirror-state'

declare module '@tiptap/core' {
  interface Commands<ReturnType> {
    scrollto: {
      /**
       * @description Scrolls to the top of the document. It does the same as focus('start') but without setting the focus.
       */
       scrollToTop: () => ReturnType,

      /**
       * @description Scrolls to the bottom of the document. It does the same as focus('end') but without setting the focus.
       */
       scrollToBottom: () => ReturnType,
    }
  }
}

export const ScrollTo = Extension.create({
  name: 'scrollto',

  addCommands() {
    return {
      scrollToTop: () => ({ tr, state, dispatch }) => {
        const { doc } = tr
        
        const minPos = TextSelection.atStart(doc).from;
        const selection = TextSelection.create(doc, minPos, minPos)
        const isSameSelection = state.selection.eq(selection)
      
        // const selection = TextSelection.create(doc, 0, 0)
        if (dispatch) {
          if (!isSameSelection) {
            tr.setSelection(selection).scrollIntoView()
          }
      
          // `tr.setSelection` resets the stored marks
          // so we’ll restore them if the selection is the same as before
          if (isSameSelection && tr.storedMarks) {
            tr.setStoredMarks(tr.storedMarks)
          }
        }
        return true
      },

      scrollToBottom: () => ({ tr, state, dispatch }) => {
        const { doc } = tr
        const maxPos = TextSelection.atEnd(doc).to;
        const selection = TextSelection.create(doc, maxPos, maxPos)
        const isSameSelection = state.selection.eq(selection)
      
        if (dispatch) {
          if (!isSameSelection) {
            tr.setSelection(selection).scrollIntoView()
          }
      
          // `tr.setSelection` resets the stored marks
          // so we’ll restore them if the selection is the same as before
          if (isSameSelection && tr.storedMarks) {
            tr.setStoredMarks(tr.storedMarks)
          }
        }
        return true
      },
    }
  },

  addKeyboardShortcuts() {
    return {
      'Control-Home': () => this.editor.commands.scrollToTop(),
      'Control-End': () => this.editor.commands.scrollToBottom(),
    }
  },
})
