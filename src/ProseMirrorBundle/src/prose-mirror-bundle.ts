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
import ListItem from '@tiptap/extension-list-item'
import OrderedList from '@tiptap/extension-ordered-list'
import Paragraph from '@tiptap/extension-paragraph'
import Strike from '@tiptap/extension-strike'
import Text from '@tiptap/extension-text'
import TextStyle from '@tiptap/extension-text-style'
import Underline from '@tiptap/extension-underline'
import { Selection } from 'prosemirror-state'

import { CustomLink } from "./custom-link-extension";
import { SearchNReplace } from './search-n-replace'

/**
 * This method will be exported and can be called from the HTML document with the "prose_mirror_bundle"
 * namespace. The namespace is defined in the webpack config.
 * The function names of the TipTap/ProseMirror editor are preserved (not minified), so that it is
 * possible to call functions inside the HTML page.
 * @example
 *   var editor = ProseMirrorBundle.initializeEditor(document.getElementById('myeditor'));
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
        CustomLink.configure({
          autolink: true,
          openOnClick: false,
        }),
        ListItem,
        OrderedList,
        Paragraph,
        Strike,
        Text,
        TextStyle,
        Underline,
        SearchNReplace.configure({
          searchResultClass: "search-result", // css class to give to found items. default 'search-result'
          caseSensitive: false,
          disableRegex: true,
        }),
      ],
      editable: true,
    });
  } 
  catch ( e ) {
      return e.message + ' ' + e.stack;
  }
}

/**
 * Scrolls to the top of the document. It does the same as focus('start') but without setting the focus.
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function scrollToTop(editor: Editor): void {
  editor.chain().setTextSelection({ from: 0, to: 0 }).scrollIntoView().run();
}

/**
 * Scrolls to the end of the document. It does the same as focus('end') but without setting the focus.
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function scrollToBottom(editor: Editor): void {
  const selection = Selection.atEnd(editor.state.doc);
  editor.chain().setTextSelection({ from: selection.$from.pos, to: selection.$to.pos }).scrollIntoView().run();
}

/**
 * Searches for all occurences of a given text in the note and highlights the findings.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  needle - The text to search for.
*/
export function searchAndHighlight(editor: Editor, needle: string): void {
  editor.commands.setSearchTerm(needle);
}

/**
 * Gets the selected text.
 * @param {Editor}  editor - A TipTap editor instance.
 * @returns { string } The selected text, or null if the selection is empty.
*/
export function getSelectedText(editor: Editor): string {
  const { from, to, empty } = editor.state.selection;
  if (empty) {
    return null;
  }
  return editor.state.doc.textBetween(from, to, ' ');
}

/**
 * Searches for word boundaries around the current cursor position and selects the word.
 * @param {Editor}  editor - A TipTap editor instance.
 * @returns {string} The text content of the new selection.
*/
export function selectWordAtCurrentPosition(editor: Editor): string {
  let selection = editor.view.state.selection;
  let text = selection.$from.parent.textContent;

  let textFromPos = selection.$from.parentOffset;
  let toLeft = 0;
  while ((textFromPos - toLeft - 1 >= 0) && !isWhitespace(text[textFromPos - toLeft - 1])) {
      toLeft++;
  }

  let textToPos = selection.$to.parentOffset;
  let toRight = 0;
  while ((textToPos + toRight < text.length) && !isWhitespace(text[textToPos + toRight])) {
      toRight++;
  }

  editor.commands.setTextSelection({ from: selection.$from.pos - toLeft, to: selection.$to.pos + toRight });
  return text.substring(textFromPos - toLeft, textToPos + toRight);
}

/**
 * Tries to guess how an url with the given text would look like.
 * @param {string}  text - A text which may or may not contain an url.
 * @returns {string} A suggestion for an url derrived from the text.
*/
export function makeLinkSuggestion(text: string): string {
  if (!isValidUrl(text)) {
    text = 'https://' + text;
    if (!isValidUrl(text)) {
        text = 'https://';
    }
  }
  return text;
}

function isValidUrl(text: string): boolean {
  try {
      new URL(text);
      return true;
  } catch {
      return false;
  }
}

function isWhitespace(char: string): boolean {
  let regex = /[\s]/;
  return regex.test(char);
}
