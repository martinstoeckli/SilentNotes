import { Editor, getAttributes } from '@tiptap/core';
import Paragraph, { ParagraphOptions } from '@tiptap/extension-paragraph'
import { Plugin, PluginKey } from 'prosemirror-state'
import { NodeType, Node as ProsemirrorNode } from 'prosemirror-model'

const todoClass = '';
const doneClass = 'done';
const disabledClass = 'disabled';

/*
Extends the Paragraph node of TipTap, but preserves the class attribute of the HTML element.
*/
export const CheckableParagraph = Paragraph.extend({
  addAttributes() {
    return {
      ...this.parent?.(),
      htmlElementClass: {
        parseHTML: element => element.getAttribute('class'),
        renderHTML: attributes => {
          return { 'class': attributes.htmlElementClass };
        },
      },
    }
  },

  addProseMirrorPlugins() {
    const plugins = [];
    plugins.push(clickHandler({ editor: this.editor, type: this.type }));
    return plugins;
  },
})

type ClickHandlerOptions = {
  editor: Editor,
  type: NodeType,
}

function clickHandler(options: ClickHandlerOptions): Plugin {
  return new Plugin({
    key: new PluginKey('handleClickLink'),
    props: {
      handleClick: (view, pos, event) => {
        // The element is clickable only to the left (change state) and the right (delete) of the
        // text, the text itself has "pointer-events: none;"
        const paragraphHtmlElement = (event.target as HTMLElement)?.closest('p');
        if (paragraphHtmlElement == null)
          return false;

        const targetRect = paragraphHtmlElement.getBoundingClientRect();
        const targetMiddleX = (targetRect.left + targetRect.right) / 2;
        const clickedCheckbox: boolean = event.clientX < targetMiddleX;
        const clickedRecyclebin: boolean = !clickedCheckbox;
        if (!clickedCheckbox && !clickedRecyclebin)
          return false;

        // Clicking on the :before doesn't select the item, so we select it,
        // because most TipTap commands work with the current selection
        const editor: Editor = options.editor;
        const resolvedPos = view.state.doc.resolve(pos);
        editor.commands.setTextSelection({ from: resolvedPos.pos, to: resolvedPos.pos });

        if (clickedCheckbox) {
          // On Android 6 a click is sometimes ignored, if the click position is
          // too close to the text cursor
          editor.commands.selectTextblockEnd();

          let nodeAttributes = getAttributes(view.state, options.type);
          const oldState = nodeAttributes.htmlElementClass;
          const newState = rotateState(oldState);
          nodeAttributes.htmlElementClass = newState;
          editor.commands.updateAttributes(options.type, nodeAttributes);
        }
        if (clickedRecyclebin) {
          editor.commands.deleteNode(options.type);
        }
        return true;
      },
    },
  })
}

function rotateState(state: string): string {
  if (state == doneClass)
    return disabledClass;
  else if (state == disabledClass)
    return todoClass;
  else
    return doneClass;
}

/*
 * Searches for all checklist items (paragraphs) and sets their html class attribute
 * to the new check state.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  checkState - The new html class for the paragraph elements.
*/
export function setCheckedStateForAll(editor: Editor, checkState: string) {
  const commandChain = editor.chain();
  editor.state.doc.descendants((node, pos) => {
    if ((node.type == editor.view.state.schema.nodes.paragraph) &&
        (node.attrs.htmlElementClass != checkState))
    {
      const newAttributes = { htmlElementClass: checkState };
      commandChain.setNodeSelection(pos);
      commandChain.updateAttributes(node.type, {
        ...node.attrs,
        ...newAttributes
      });
    }
  });
  commandChain.run();
}