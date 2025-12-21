import "core-js";
import { Editor } from '@tiptap/core'
import Blockquote from '@tiptap/extension-blockquote'
import Bold from '@tiptap/extension-bold'
import Code from '@tiptap/extension-code'
import CodeBlock from '@tiptap/extension-code-block'
import Document from '@tiptap/extension-document'
import HardBreak from '@tiptap/extension-hard-break'
import Heading from '@tiptap/extension-heading'
import HorizontalRule from '@tiptap/extension-horizontal-rule'
import Italic from '@tiptap/extension-italic'
import Paragraph from '@tiptap/extension-paragraph'
import Strike from '@tiptap/extension-strike'
import Text from '@tiptap/extension-text'
import { TextStyleKit } from '@tiptap/extension-text-style'
import Underline from '@tiptap/extension-underline'
import { BulletList, OrderedList, ListItem } from '@tiptap/extension-list'
import { Markdown } from '@tiptap/markdown'

export class TiptapMarkdownConverter {
  private _editor: Editor;

  /*
  * Converts the Markdown content to HTML text.
  * @param {html} string - The Html text to convert.
  * @returns Markdown text.
  */
  public convertHtmlToMarkdown(htmlContent: string): string {
    let editor: Editor = this.getOrCreateEditor();
    editor.commands.setContent(htmlContent);
    let result: string = editor.getMarkdown();
    return result;
  }

  public convertMarkdownToHtml(markdownContent: string): string {
    let editor: Editor = this.getOrCreateEditor();
    editor.commands.setContent(markdownContent, { contentType: 'markdown' })
    let result = editor.getHTML();
    return result;
  }

  /*
   * Lazy creation of the editor, allows to reuse the editor instance to convert multiple notes.
   */
  private getOrCreateEditor(): Editor {
    if (!this._editor) {
      this._editor = new Editor({
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
          HorizontalRule,
          Italic,
          ListItem,
          Markdown.configure({
            indentation: { style: 'space', size: 2 },
          }),
          OrderedList,
          Paragraph,
          Strike,
          Text,
          TextStyleKit,
          Underline,
        ],
        editable: false,
        contentType: 'html',
      });
    }
    return this._editor;
  }
}
