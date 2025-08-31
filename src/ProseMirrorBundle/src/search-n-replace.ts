import "core-js";
import { Extension } from '@tiptap/core'
import { Decoration, DecorationSet } from 'prosemirror-view'
import { EditorState, Plugin, PluginKey, TextSelection } from 'prosemirror-state'
import { Node as ProsemirrorNode } from 'prosemirror-model'

/*
Credits to:
https://github.com/sereneinserenade/tiptap/blob/1898e72db05fbd36ae37078b4f504ef056686173/packages/extension-search-n-replace/src/search-n-replace.ts
*/

declare module '@tiptap/core' {
  interface Commands<ReturnType> {
    search: {
      /**
       * @description Set search term in extension.
       */
      setSearchTerm: (searchTerm: string) => ReturnType,

      /**
       * @description Searches for the next match and sets the selection to this match.
       * @param {boolean}  canKeepPos - If set to true, the selection will be kept if it already
       * is a match. If set to false, the next occurence will be found.
       * @param {boolean}  continueAtBegin - If set to true, the search will be continued at the
       * begin of the document, if no match could be found after the selection.
       */
      selectNext: (canKeepPos: boolean, continueAtBegin: boolean) => ReturnType,

      /**
       * @description Searches for the previous match and sets the selection to this match.
       */
      selectPrevious: () => ReturnType,
    }
  }
}

interface Result {
  from: number;
  to: number;
}

interface SearchOptions {
  searchResultClass: string;
  caseSensitive: boolean;
  disableRegex: boolean;
}

interface TextNodesWithPosition {
  text: string;
  pos: number;
}

const regex = (s: string, disableRegex: boolean, caseSensitive: boolean): RegExp => {
  return RegExp(disableRegex ? s.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&') : s, caseSensitive ? 'gu' : 'gui')
}

function processSearches(doc: ProsemirrorNode, searchTerm: RegExp, searchResultClass: string): { decorationsToReturn: DecorationSet, results: Result[] } {
  const decorations: Decoration[] = []
  let textNodesWithPosition: TextNodesWithPosition[] = []
  const results: Result[] = []

  let index = 0

  if (!searchTerm) return { decorationsToReturn: DecorationSet.empty, results: [] }

  doc?.descendants((node, pos) => {
    if (node.isText) {
      if (textNodesWithPosition[index]) {
        textNodesWithPosition[index] = {
          text: textNodesWithPosition[index].text + node.text,
          pos: textNodesWithPosition[index].pos,
        }
      } else {
        textNodesWithPosition[index] = {
          text: `${node.text}`,
          pos,
        }
      }
    } else {
      index += 1
    }
  })

  textNodesWithPosition = textNodesWithPosition.filter(Boolean)

  for (let i = 0; i < textNodesWithPosition.length; i += 1) {
    const { text, pos } = textNodesWithPosition[i]

    const matches = [...text.matchAll(searchTerm)]

    for (let j = 0; j < matches.length; j += 1) {
      const m = matches[j]

      if (m[0] === '') break

      if (m.index !== undefined) {
        results.push({
          from: pos + m.index,
          to: pos + m.index + m[0].length,
        })
      }
    }
  }

  for (let i = 0; i < results.length; i += 1) {
    const r = results[i]
    decorations.push(Decoration.inline(r.from, r.to, { class: searchResultClass }))
  }

  return {
    decorationsToReturn: DecorationSet.create(doc, decorations),
    results,
  }
}

const selectNext = (canKeepPos: boolean, continueAtBegin: boolean, results: Result[], { state }: any) => {
  const { from } = state.selection

  let nextResult = null
  let i = 0
  while ((nextResult == null) && (i < results.length)) {
    if (canKeepPos) {
      if (results[i].from >= from)
        nextResult = results[i]
    }
    else {
      if (results[i].from > from)
        nextResult = results[i]
    }
    i++
  }

  if ((nextResult == null) && continueAtBegin && (results.length > 0)) {
    nextResult = results[0];
  }

  if (nextResult) {
    const selection = TextSelection.create(state.doc, nextResult.from, nextResult.to)
    state.tr.setSelection(selection)
  }
}

const selectPrevious = (results: Result[], { state }: any) => {
  const { from } = state.selection

  let nextResult = null
  let i = results.length - 1
  while ((nextResult == null) && (i >= 0)) {
    if (results[i].from < from)
      nextResult = results[i]
    i--
  }

  if (nextResult) {
    const selection = TextSelection.create(state.doc, nextResult.from, nextResult.to)
    state.tr.setSelection(selection)
  }
}

// eslint-disable-next-line @typescript-eslint/ban-types
export const SearchNReplace = Extension.create<SearchOptions>({
  name: 'snr-Search',

  addOptions() {
    return {
      searchResultClass: 'search-result',
      caseSensitive: false,
      disableRegex: false,
    }
  },

  addCommands() {
    const thisExtension = this

    return {
      setSearchTerm: (searchTerm: string) => () => {
        const editor: any = thisExtension.editor;
        editor['snr-SearchResult'] = [];
        editor['snr-SearchTerm'] = searchTerm;
        return false
      },
      
      selectNext: (canKeepPos: boolean, continueAtBegin: boolean) => ({ state, dispatch }) => {
        const editor: any = thisExtension.editor;
        const results = editor['snr-SearchResult']
        selectNext(canKeepPos, continueAtBegin, results, { state })
        dispatch(state.tr)
        return false
      },

      selectPrevious: () => ({ state, dispatch }) => {
        const editor: any = thisExtension.editor;
        const results = editor['snr-SearchResult']
        selectPrevious(results, { state })
        dispatch(state.tr)
        return false
      },
  }
  },

  addProseMirrorPlugins() {
    const thisExtension = this

    return [
      new Plugin({
        key: new PluginKey('snr-Search'),
        state: {
          init() {
            return DecorationSet.empty
          },
          apply(tr) {
            const { searchResultClass, disableRegex, caseSensitive } = thisExtension.options
            const editor: any = thisExtension.editor;
            const searchTerm = editor['snr-SearchTerm'];

            if (searchTerm) {
              const { decorationsToReturn, results } = processSearches(
                tr.doc,
                regex(searchTerm, disableRegex, caseSensitive),
                searchResultClass)

              editor['snr-SearchResult'] = results;
              return decorationsToReturn
            }
            return DecorationSet.empty
          },
        },
        props: {
          decorations(state: EditorState) {
            return this.getState(state)
          },
        },
      }),
    ]
  },
})
