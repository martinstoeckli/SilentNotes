import Paragraph from '@tiptap/extension-paragraph'

/*
Extends the Paragraph node of TipTap, but preserves the class attribute of the HTML element.
*/
export const ClassifiedParagraph = Paragraph.extend({
  addAttributes() {
    return {
      class: {
        parseHTML: element => element.className,
      },
    }
  },
})
