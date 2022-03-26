import "core-js";
import Link from '@tiptap/extension-link'
import { Editor } from '@tiptap/core'

import {
  getAttributes,
  getMarksBetween,
  findChildrenInRange,
  combineTransactionSteps,
  getChangedRanges,
} from '@tiptap/core'
import { Plugin, PluginKey } from 'prosemirror-state'
import { MarkType } from 'prosemirror-model'
import { find, test } from 'linkifyjs'

export const CustomLink = Link.extend({
  addProseMirrorPlugins() {
    const plugins = []

    if (this.options.autolink) {
       plugins.push(customAutolink({
         type: this.type,
       }))
    }

    plugins.push(customClickHandler({
      type: this.type,
    }))

    if (this.options.linkOnPaste) {
       plugins.push(customPasteHandler({
         editor: this.editor,
         type: this.type,
       }))
    }

    return plugins
  },
})

export type CustomClickHandlerOptions = {
  type: MarkType,
}

function customClickHandler(options: CustomClickHandlerOptions): Plugin {
  return new Plugin({
    key: new PluginKey('handleClickLink'),
    props: {
      handleClick: (view, pos, event) => {
        const attrs = getAttributes(view.state, options.type.name)
        const link = (event.target as HTMLElement)?.closest('a')

        if (link && attrs.href) {
          let customClickEvent = new CustomEvent("custom-link-clicked", { "detail": attrs.href });
          document.dispatchEvent(customClickEvent);
          return true
        }

        return false
      },
    },
  })
}

export type CustomPasteHandlerOptions = {
  editor: Editor,
  type: MarkType,
}

export function customPasteHandler(options: CustomPasteHandlerOptions): Plugin {
  return new Plugin({
    key: new PluginKey('handlePasteLink'),
    props: {
      handlePaste: (view, event, slice) => {
        const { state } = view
        const { selection } = state
        const { empty } = selection

        if (empty) {
          return false
        }

        let textContent = ''

        slice.content.forEach(node => {
          textContent += node.textContent
        })

        const link = find(textContent).find(item => item.isLink && item.value === textContent)

        if (!textContent || !link) {
          return false
        }

        options.editor.commands.setMark(options.type, {
          href: link.href,
        })

        return true
      },
    },
  })
}

export type CustomAutolinkOptions = {
  type: MarkType,
}

export function customAutolink(options: CustomAutolinkOptions): Plugin {
  return new Plugin({
    key: new PluginKey('autolink'),
    appendTransaction: (transactions, oldState, newState) => {
      const docChanges = transactions.some(transaction => transaction.docChanged)
        && !oldState.doc.eq(newState.doc)
      const preventAutolink = transactions.some(transaction => transaction.getMeta('preventAutolink'))

      if (!docChanges || preventAutolink) {
        return
      }

      const { tr } = newState
      const transform = combineTransactionSteps(oldState.doc, transactions)
      const { mapping } = transform
      const changes = getChangedRanges(transform)

      changes.forEach(({ oldRange, newRange }) => {
        // at first we check if we have to remove links
        getMarksBetween(oldRange.from, oldRange.to, oldState.doc)
          .filter(item => item.mark.type === options.type)
          .forEach(oldMark => {
            const newFrom = mapping.map(oldMark.from)
            const newTo = mapping.map(oldMark.to)
            const newMarks = getMarksBetween(newFrom, newTo, newState.doc)
              .filter(item => item.mark.type === options.type)

            if (!newMarks.length) {
              return
            }

            const newMark = newMarks[0]
            const oldLinkText = oldState.doc.textBetween(oldMark.from, oldMark.to, undefined, ' ')
            const newLinkText = newState.doc.textBetween(newMark.from, newMark.to, undefined, ' ')
            const wasLink = test(oldLinkText)
            const isLink = test(newLinkText)

            // remove only the link, if it was a link before too
            // because we don’t want to remove links that were set manually
            if (wasLink && !isLink) {
              tr.removeMark(newMark.from, newMark.to, options.type)
            }
          })

        // now let’s see if we can add new links
        findChildrenInRange(newState.doc, newRange, node => node.isTextblock)
          .forEach(textBlock => {
            // we need to define a placeholder for leaf nodes
            // so that the link position can be calculated correctly
            const text = newState.doc.textBetween(
              textBlock.pos,
              textBlock.pos + textBlock.node.nodeSize,
              undefined,
              ' ',
            )

            find(text)
              .filter(link => link.isLink)
              // calculate link position
              .map(link => ({
                ...link,
                from: textBlock.pos + link.start + 1,
                to: textBlock.pos + link.end + 1,
              }))
              // check if link is within the changed range
              .filter(link => {
                const fromIsInRange = newRange.from >= link.from && newRange.from <= link.to
                const toIsInRange = newRange.to >= link.from && newRange.to <= link.to

                return fromIsInRange || toIsInRange
              })
              // add link mark
              .forEach(link => {
                tr.addMark(link.from, link.to, options.type.create({
                  href: link.href,
                }))
              })
          })
      })

      if (!tr.steps.length) {
        return
      }

      return tr
    },
  })
}
