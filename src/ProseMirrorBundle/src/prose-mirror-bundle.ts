import "core-js";
import { Editor } from '@tiptap/core'
import Blockquote from '@tiptap/extension-blockquote'
import Bold from '@tiptap/extension-bold'
import BulletList from '@tiptap/extension-bullet-list'
import Code from '@tiptap/extension-code'
import CodeBlock from '@tiptap/extension-code-block'
import Document from '@tiptap/extension-document'
import HardBreak from '@tiptap/extension-hard-break'
import Heading from '@tiptap/extension-heading'
import Italic from '@tiptap/extension-italic'
import Link from '@tiptap/extension-link'
import ListItem from '@tiptap/extension-list-item'
import OrderedList from '@tiptap/extension-ordered-list'
import Paragraph from '@tiptap/extension-paragraph'
import Strike from '@tiptap/extension-strike'
import Text from '@tiptap/extension-text'
import TextStyle from '@tiptap/extension-text-style'
import Underline from '@tiptap/extension-underline'


/**
 * This method will be exported and can be called from the HTML document with the "prose_mirror_bundle"
 * namespace. The namespace is defined in the webpack config.
 * The function names of the TipTap editor are preserved (not minified), so that it is possible to
 * call functions inside the HTML page.
 * @example
 *   var editor = window.prose_mirror_bundle.initializeEditor(document.getElementById('myeditor'));
 *   editor.commands.setContent('<p>Hello World!</p>');
 *   editor.chain().focus().toggleBold().run();
 * @param {HTMLScriptElement}  editorElement - Usually a DIV element from the HTML document which
 *   becomes the container of the TipTap editor.
 * @returns {Editor} The new TipTap editor instance.
 */
export function initializeEditor(editorElement: HTMLElement): any {
  try {

  return new Editor({
    element: editorElement,
    extensions: [
      Blockquote,
      Bold,
      BulletList,
      Code,
      CodeBlock,
      Document,
      HardBreak,
      Heading.configure({
        levels: [1, 2, 3],
      }),
      Italic,
      Link,
      ListItem,
      OrderedList,
      Paragraph,
      Strike,
      Text,
      TextStyle,
      Underline,
    ],
    editable: true,
  });

  } 
  catch ( e ) {
      return e.message + ' ' + e.stack;
  }
}
