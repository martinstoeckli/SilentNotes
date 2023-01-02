import Heading from '@tiptap/extension-heading'

export const CustomHeading = Heading.extend({
  addKeyboardShortcuts() {
    return {
      // The standard shortcuts Mod-Alt-${level} will prevent to type the AltGr-${level} characters
      // on a Windows keyboard ('@', '#'), thus we remove them.
    }
  },
})