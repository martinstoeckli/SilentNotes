import "core-js";
import { Editor } from '@tiptap/core'
import Blockquote from '@tiptap/extension-blockquote'
import Bold from '@tiptap/extension-bold'
import Code from '@tiptap/extension-code'
import CodeBlock from '@tiptap/extension-code-block'
import Document from '@tiptap/extension-document'
import HardBreak from '@tiptap/extension-hard-break'
import History from '@tiptap/extension-history'
import HorizontalRule from '@tiptap/extension-horizontal-rule'
import Italic from '@tiptap/extension-italic'
import Paragraph from '@tiptap/extension-paragraph'
import Strike from '@tiptap/extension-strike'
import Text from '@tiptap/extension-text'
import { TextStyleKit } from '@tiptap/extension-text-style'
import Underline from '@tiptap/extension-underline'
import { BulletList, OrderedList, ListItem } from '@tiptap/extension-list'

import { CustomHeading } from "./custom-heading-extension";
import { CustomLink } from "./custom-link-extension";
import { SearchNReplace } from './search-n-replace'
import { CheckableParagraph, registerIsShoppingModeActive as checklistRegisterIsShoppingModeActive, moveChecklistUp, moveChecklistDown, sortPendingToTop, setCheckStateForAll, sortAlphabetical } from "./checkable-paragraph-extension";
import { ScrollTo } from './scroll-to-extension'
import { TabHandler } from './tab-handler-extension'
import { TiptapHelper } from "./tiptap-helper";
export { exportAsPlainText, exportChecklistAsPlainText } from './tiptap-plain-text-exporter'
export { TiptapHelper } from './tiptap-helper'
export { TiptapMarkdownConverter } from './tiptap-markdown-converter'

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
        HorizontalRule,
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
        TextStyleKit,
        Underline,
        SearchNReplace.configure({
          searchResultClass: "search-result", // css class to give to found items. default 'search-result'
          caseSensitive: false,
          disableRegex: true,
        }),
        ScrollTo,
        TabHandler,
      ],
      editable: false,
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
        TextStyleKit,
        SearchNReplace.configure({
          searchResultClass: "search-result", // css class to give to found items. default 'search-result'
          caseSensitive: false,
          disableRegex: true,
        }),
        ScrollTo,
      ],
      editable: false,
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
  TiptapHelper.scrollToSelection(editor);
}

/**
 * Searches for the next occurence of a given text in the note and selects the finding.
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function selectNext(editor: Editor): void {
  editor.chain().focus().selectNext(false, false).run();
  TiptapHelper.scrollToSelection(editor);
}

/**
 * Searches for the next occurence of a given text in the note and selects the finding.
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function selectPrevious(editor: Editor): void {
  editor.chain().focus().selectPrevious().run();
  TiptapHelper.scrollToSelection(editor);
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
