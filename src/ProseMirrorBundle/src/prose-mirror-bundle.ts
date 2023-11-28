import "core-js";
import { Editor } from '@tiptap/core'
import Blockquote from '@tiptap/extension-blockquote'
import Bold from '@tiptap/extension-bold'
import BulletList from '@tiptap/extension-bullet-list'
import Code from '@tiptap/extension-code'
import CodeBlock from '@tiptap/extension-code-block'
import Document from '@tiptap/extension-document'
import HardBreak from '@tiptap/extension-hard-break'
import History from '@tiptap/extension-history'
import Italic from '@tiptap/extension-italic'
import ListItem from '@tiptap/extension-list-item'
import OrderedList from '@tiptap/extension-ordered-list'
import Paragraph from '@tiptap/extension-paragraph'
import Strike from '@tiptap/extension-strike'
import Text from '@tiptap/extension-text'
import TextStyle from '@tiptap/extension-text-style'
import Underline from '@tiptap/extension-underline'
import { Selection } from 'prosemirror-state'

import { CustomHeading } from "./custom-heading-extension";
import { CustomLink } from "./custom-link-extension";
import { SearchNReplace } from './search-n-replace'
import { CheckableParagraph, registerIsShoppingModeActive as checklistRegisterIsShoppingModeActive, moveChecklistUp, moveChecklistDown, sortPendingToTop, setCheckStateForAll, sortAlphabetical } from "./checkable-paragraph-extension";
import { ScrollTo } from './scroll-to-extension'

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
        CustomHeading.configure({
          levels: [1, 2, 3],
        }),
        History.configure({
          depth: 200,
        }),        
        Italic,
        CustomLink.configure({
          autolink: false,
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
        ScrollTo,
      ],
      editable: true,
    });
  } 
  catch ( e ) {
      return e.message + ' ' + e.stack;
  }
}

/**
 * This method will be exported and can be called from the HTML document with the "prose_mirror_bundle"
 * namespace. The namespace is defined in the webpack config.
 * The function names of the TipTap/ProseMirror editor are preserved (not minified), so that it is
 * possible to call functions inside the HTML page.
 * @example
 *   var editor = ProseMirrorBundle.initializeChecklist(document.getElementById('myeditor'));
 *   editor.commands.setContent('<p>Hello World!</p>');
 *   editor.chain().focus().toggleBold().run();
 * @param {HTMLScriptElement}  editorElement - Usually a DIV element from the HTML document which
 *   becomes the container of the TipTap editor.
 * @returns {Editor} The new TipTap editor instance.
 */
 export function initializeChecklist(editorElement: HTMLElement): any {
  try {
    return new Editor({
      element: editorElement,
      extensions: [
        Document,
        HardBreak,
        CustomHeading.configure({
          levels: [1, 2],
        }),
        History.configure({
          depth: 200,
        }),        
        CheckableParagraph, // Preserves the class attribute needed for the checkboxes
        Text,
        TextStyle,
        SearchNReplace.configure({
          searchResultClass: "search-result", // css class to give to found items. default 'search-result'
          caseSensitive: false,
          disableRegex: true,
        }),
        ScrollTo,
      ],
      editable: true,
    });
  } 
  catch ( e ) {
      return e.message + ' ' + e.stack;
  }
}

export function registerIsShoppingModeActive(delegate: () => boolean) {
  checklistRegisterIsShoppingModeActive(delegate);
}

/**
 * Toggles a given format like heading, bold or italic.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  formatName - Name of the format to toggle, one of those:
 *   (heading, bold, italic, underline, strike, codeblock, blockquote, bulletlist, orderedlist).
 * @param {string}  formatParameter - Optional parameter for the heading format.
*/
export function toggleFormat(editor: Editor, formatName: string, formatParameter: any): void {
  let lowerFormatName: string = formatName.toLowerCase();
  switch (lowerFormatName) {
    case 'heading':
      editor.chain().focus().toggleHeading({ level: formatParameter }).run(); break;
    case 'codeblock':
      editor.chain().focus().toggleCodeBlock().run(); break;
    case 'blockquote':
      editor.chain().focus().toggleBlockquote().run(); break;
    case 'bulletlist':
      editor.chain().focus().toggleBulletList().run(); break;
    case 'orderedlist':
      editor.chain().focus().toggleOrderedList().run(); break;
    default:
      editor.chain().focus().toggleMark(formatName).run(); break;
  }
}

/**
 * Checks whether a given format is active at the current position.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  formatName - Name of the format to toggle, one of those:
 *   (heading, bold, italic, underline, strike, codeblock, blockquote, bulletlist, orderedlist).
 * @param {string}  formatParameter - Optional parameter for the heading format.
 * @returns {bool} Returns true if the format is active, false otherwise.
*/
export function isFormatActive(editor: Editor, formatName: string, formatParameter: any): boolean {
  formatName = getCaseSensitiveFormat(formatName);
  return editor.isActive(formatName, formatParameter);
}

function getCaseSensitiveFormat(formatName: string): string {
  let lowerFormatName: string = formatName.toLowerCase();
  switch (lowerFormatName) {
    case 'codeblock':
      return 'codeBlock'; break;
    case 'bulletlist':
      return 'bulletList'; break;
    case 'orderedlist':
      return 'orderedList'; break;
    default:
      return lowerFormatName; break;
  }
}

/**
 * Searches for all occurences of a given text in the note and highlights the findings.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  needle - The text to search for.
*/
export function searchAndHighlight(editor: Editor, needle: string, minLength: number = 2): void {
  if ((needle === null) || (needle.length < minLength))
    needle = '';
  editor.commands.setSearchTerm(needle);
}

/**
 * Searches for the next occurence of a given text in the note and selects the finding.
 * If the current selection matches, it is kept. The focus is not set to the editor, so the
 * search input field does not loose the focus.
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function selectNextWhileTyping(editor: Editor): void {
  editor.chain().selectNext(true, true).run();
  scrollToSelection(editor);
}

/**
 * Searches for the next occurence of a given text in the note and selects the finding.
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function selectNext(editor: Editor): void {
  editor.chain().focus().selectNext(false, false).run();
  scrollToSelection(editor);
}

/**
 * Searches for the next occurence of a given text in the note and selects the finding.
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function selectPrevious(editor: Editor): void {
  editor.chain().focus().selectPrevious().run();
  scrollToSelection(editor);
}

/**
 * Scrolls to the current position/selection of the document. It does the same as scrollIntoView()
 * but without requiring the focus on the editor, thus it can be called from the search box while
 * typing or in shopping mode when the editor is disabled.
 * @param {Editor}  editor - A TipTap editor instance.
*/
function scrollToSelection(editor: Editor): void {
  const { node } = editor.view.domAtPos(editor.state.selection.anchor);
  if (node) {
      (node as any).scrollIntoView?.(false);
  }
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
  if (textFromPos != textToPos)
    toRight--; // for the case that the selection includes a trailing whitespace (after a double click)
  while ((textToPos + toRight < text.length) && !isWhitespace(text[textToPos + toRight])) {
      toRight++;
  }

  editor.commands.setTextSelection({ from: selection.$from.pos - toLeft, to: selection.$to.pos + toRight });
  return text.substring(textFromPos - toLeft, textToPos + toRight);
}

/*
 * Searches for all checklist items (paragraphs) and sets their html class attribute
 * to the new check state "todo".
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function setCheckStateForAllToTodo(editor: Editor): void {
  setCheckStateForAll(editor, '');
}

/*
 * Searches for all checklist items (paragraphs) and sets their html class attribute
 * to the new check state "done".
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function setCheckStateForAllToDone(editor: Editor): void {
  setCheckStateForAll(editor, 'done');
}

/*
 * Searches for all checklist items (paragraphs) and sets their html class attribute
 * to the new check state "disabled".
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function setCheckStateForAllToDisabled(editor: Editor): void {
  setCheckStateForAll(editor, 'disabled');
}

export function moveChecklist(editor: Editor, upwards: boolean, singleStep: boolean): void {
  if (upwards) {
    moveChecklistUp(editor, singleStep);
  }
  else {
    moveChecklistDown(editor, singleStep);
  }
}

export function sortChecklistPendingToTop(editor: Editor): void {
  sortPendingToTop(editor);
}

export function sortChecklistAlphabetical(editor: Editor): void {
  sortAlphabetical(editor);
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
