import { Extension } from '@tiptap/core'
import { TextSelection, EditorState, Transaction } from 'prosemirror-state'

declare module '@tiptap/core' {
  /**
   * Supports scrolling to the top [Ctrl-Home] or to the bottom [Ctrl-End] of the document without
   * requiring to set the focus. This can be useful, if the document is shown readonly.
   */
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
        const minPos = TextSelection.atStart(tr.doc).from;
        scrollToPos(minPos, tr, state, dispatch)
        return true
      },

      scrollToBottom: () => ({ tr, state, dispatch }) => {
        const maxPos = TextSelection.atEnd(tr.doc).to;
        scrollToPos(maxPos, tr, state, dispatch)
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

function scrollToPos(pos: number, tr: Transaction, state: EditorState, dispatch: any) {
  const selection = TextSelection.create(tr.doc, pos, pos)
  const isSameSelection = state.selection.eq(selection)

  if (dispatch && !isSameSelection) {
    tr.setSelection(selection).scrollIntoView()
  }
}