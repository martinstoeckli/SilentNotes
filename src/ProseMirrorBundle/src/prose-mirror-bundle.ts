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
import { CheckableParagraph, registerIsShoppingModeActive as checklistRegisterIsShoppingModeActive } from "./checkable-paragraph-extension";
import { ScrollTo } from './scroll-to-extension'
import { TabHandler } from './tab-handler-extension'
import { TiptapHelper } from "./tiptap-helper";

export { exportAsPlainText, exportChecklistAsPlainText } from './tiptap-plain-text-exporter'
export { CheckableParagraphHelper } from './checkable-paragraph-extension'
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
      editorProps: {
        transformPastedHTML(html) {
          return TiptapHelper.cleanPastedHTML(html);
        },
      },
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
      editorProps: {
        transformPastedHTML(html) {
          return TiptapHelper.cleanPastedHTML(html);
        },
      },
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
