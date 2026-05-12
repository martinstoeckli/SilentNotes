import { Editor } from "@tiptap/core";

/**
 * Convenience functions for the TipTap editor.
 */
export class TiptapHelper {
    /**
     * Toggles a given format like heading, bold or italic.
     * @param editor - A TipTap editor instance.
     * @param formatName - Name of the format to toggle, one of those:
     *   (heading, bold, italic, underline, strike, codeblock, blockquote, bulletlist, orderedlist).
     * @param formatParameter - Optional parameter for the heading format, e.g. 2.
     */
    public static toggleFormat(editor: Editor, formatName: string, formatParameter: any): void {
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
     * @param editor - A TipTap editor instance.
     * @param formatName - Name of the format to toggle, one of those:
     *   (heading, bold, italic, underline, strike, codeblock, blockquote, bulletlist, orderedlist).
     * @param formatParameter - Optional parameter for the heading format, e.g. 2.
     * @returns Returns true if the format is active, false otherwise.
     */
    public static isFormatActive(editor: Editor, formatName: string, formatParameter: any): boolean {
        formatName = TiptapHelper.getCaseSensitiveFormat(formatName);
        return editor.isActive(formatName, formatParameter);
    }

    private static getCaseSensitiveFormat(formatName: string): string {
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
     * Scrolls to the current position/selection of the document. It does the same as scrollIntoView()
     * but without requiring the focus on the editor, thus it can be called from the search box while
     * typing or in shopping mode when the editor is disabled.
     * @param editor - A TipTap editor instance.
     * @param scrollOptions - The same options one can pass to the Element.scrollIntoView() function,
     *   or null for the default options 'instant' and 'center'.
     */
    public static scrollToSelection(editor: Editor, scrollOptions: any = { behavior: "instant", block: "center" }): void {
        const { node } = editor.view.domAtPos(editor.state.selection.anchor);
        if (node instanceof Element) {
            node.scrollIntoView?.(scrollOptions);
        }
    }

    /**
     * Gets the selected text.
     * @param editor - A TipTap editor instance.
     * @returns The selected text, or null if the selection is empty.
     */
    private static getSelectedText(editor: Editor): string {
        const { from, to, empty } = editor.state.selection;
        if (empty) {
            return null;
        }
        return editor.state.doc.textBetween(from, to, ' ');
    }

    /**
     * Inserts a horizontal rule at the current position.
     * Requires an editor with the '@tiptap/extension-horizontal-rule' extension.
     * @param editor - A TipTap editor instance.
     */
    public static insertHorizontalRule(editor: Editor): void {
        editor.chain().focus().setHorizontalRule().run();
    }

    /**
     * Formatted text pasted from other applications (like MS Word) often contains styles, this
     * function strips away those unwanted formattings.
     * Credits to: https://www.linkedin.com/pulse/struggling-pasting-tiptap-editor-heres-how-fix-khoshbayan-m-sc--lnale
     * @param html The pasted html content.
     * @returns Cleaned html string.
     */
    public static cleanPastedHTML(html: string): string {
        try {
            // Create a document fragment
            const tempContainer = document.createElement('div');
            tempContainer.innerHTML = html;

            // Remove all style attributes
            const elementsWithStyle = tempContainer.querySelectorAll('*[style]');
            elementsWithStyle.forEach(el => el.removeAttribute('style'));

            // Remove all class attributes
            const elementsWithClass = tempContainer.querySelectorAll('*[class]');
            elementsWithClass.forEach(el => el.removeAttribute('class'));

            // Remove data attributes (often used for hidden content)
            const elementsWithDataAttrs  = tempContainer.querySelectorAll('*');
            elementsWithDataAttrs.forEach(el => {
                Array.from(el.attributes)
                .filter(attr => attr.name.startsWith('data-'))
                .forEach(attr => el.removeAttribute(attr.name));
            });

            const result = tempContainer.innerHTML;
            return result;
        } catch (error) {
            return html; // Fallback to original
        }
    }
}