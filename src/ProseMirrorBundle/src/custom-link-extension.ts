import "core-js";
import Link from '@tiptap/extension-link'
import { Editor, getAttributes } from '@tiptap/core'
import { Plugin, PluginKey } from 'prosemirror-state'
import { MarkType } from 'prosemirror-model'
import { find } from 'linkifyjs'

export const CustomLink = Link.extend({

  // Newer versions than 2.0.0-beta.36 define a tag which is not compatible with WebViews
  // of UWP or Android 6/7, so we redefine the tag to be more compatible.
  parseHTML() {
    return [
      { tag: 'a[href]' },
    ]
  },

  addProseMirrorPlugins() {
    const plugins = []

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

// Taken and modified from clickHandler.ts
export type CustomClickHandlerOptions = {
  type: MarkType,
}

// Taken and modified from clickHandler.ts
// Instead of directly opening the link, the 'custom-link-clicked' event is triggered.
function customClickHandler(options: CustomClickHandlerOptions): Plugin {
  return new Plugin({
    key: new PluginKey('handleClickLink'),
    props: {
      handleClick: (view, pos, event) => {
        const attrs = getAttributes(view.state, options.type.name)
        const link = (event.target as HTMLElement)?.closest('a')

        if (link && attrs.href) {
          const customClickEvent = new CustomEvent("custom-link-clicked", { "detail": attrs.href });
          document.dispatchEvent(customClickEvent);
          return true
        }

        return false
      },
    },
  })
}

// Copied from pasteHandler.ts
export type CustomPasteHandlerOptions = {
  editor: Editor,
  type: MarkType,
}

// Copied from pasteHandler.ts
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
