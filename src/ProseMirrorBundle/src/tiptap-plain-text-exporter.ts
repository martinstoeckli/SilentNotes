import { Editor, getTextSerializersFromSchema, getTextBetween, TextSerializer } from '@tiptap/core'
import { NodeType, Node } from 'prosemirror-model'

/*
 * Converts the HTML content of the editor to plain text. In contrast to the native serialization,
 * lists are exported with bullets or numbers, even when the lists are nested.
 * @param {Editor} editor - A TipTap editor instance.
 * @returns Plain text to be used in a text editor or for sending in an e-mail.
 */
export function exportAsPlainText(editor: Editor): string {
  const _indentation = "  ";
  const _separator = '\n';
  const listHierarchy: ListHierarchyItem[] = [];

  const options = {
    blockSeparator: _separator,
    textSerializers: getTextSerializersFromSchema(editor.schema),
  };

  // Add or replace the text serializers for lists, to remember the current position in a nested
  // list structure.
  options.textSerializers['bulletList'] = (params) => {
    listHierarchy.push(new ListHierarchyItem(editor.view.state.schema.nodes.bulletList));
    const otionsNoSeparator = { blockSeparator: '', textSerializers: options.textSerializers };
    let result = invokeDefaultSerialization(params.node, otionsNoSeparator);
    listHierarchy.pop();
    if (listHierarchy.length == 0) // no nested list
      result = _separator + result;
    return result;
  }

  options.textSerializers['orderedList'] = (params) => {
    listHierarchy.push(new ListHierarchyItem(editor.view.state.schema.nodes.orderedList));
    const otionsNoSeparator = { blockSeparator: '', textSerializers: options.textSerializers };
    let result = invokeDefaultSerialization(params.node, otionsNoSeparator);
    listHierarchy.pop();
    if (listHierarchy.length == 0) // no nested list
      result = _separator + result;
    return result;
  }

  // Add or replace the text serializer for paragraphs, because the content of list items is always
  // enclosed in a paragraph.
  options.textSerializers['paragraph'] = (params) => {
    // params contains: node, pos, parent, index, range
    const { node, parent } = params
    let result: string = invokeDefaultSerialization(node, options);

    // Direct child paragraphs of list items (li) are the content of the items.
    if ((listHierarchy.length > 0) && (parent?.type === editor.view.state.schema.nodes.listItem)) {
      let listHierarchyItem = peek(listHierarchy);
      listHierarchyItem.listItemIndex++;

      // Decorate the list items
      if (listHierarchyItem.listType === editor.view.state.schema.nodes.bulletList)
        result = '• ' + result;
      else if (listHierarchyItem.listType === editor.view.state.schema.nodes.orderedList)
        result = listHierarchyItem.listItemIndex + '. ' + result;

      // Indent the list items
      result = _indentation.repeat(listHierarchy.length) + result;

      // Apply the separator, since we suppressed it in the options to avoid double NL
      result = result + _separator;
    }
    return result;
  }

  return editor.getText(options);
}

// Invokes the normal serialization of a given node.
function invokeDefaultSerialization(node: Node, options: { blockSeparator: string; textSerializers: Record<string, TextSerializer>; }) {
  const nodeRange = { from: 0, to: node.content.size };
  return getTextBetween(node, nodeRange, options);
}

// Gets the last item of a stack, without removing it as pop() would do.
function peek<T>(stack: T[], defaultValue: T = null): T {
  if (stack.length == 0)
    return defaultValue;
  else
    return stack[stack.length - 1];
}

// A stack of those can remember the current level in nested lists (ol/ul)
// and the current index (li) required for numbering the items.
class ListHierarchyItem {
  public listType: NodeType;

  public listItemIndex: number;

  constructor(listType: NodeType) {
      this.listType = listType;
      this.listItemIndex = 0;
  }
}

/*
 * Converts the HTML content of a SilentNotes specific checklist to plain text.
 * @param {Editor} editor - A TipTap editor instance.
 * @returns Plain text to be used in a text editor or for sending in an e-mail.
 */
export function exportChecklistAsPlainText(editor: Editor): string {
  const _separator = '\n';

  const options = {
    blockSeparator: _separator,
    textSerializers: getTextSerializersFromSchema(editor.schema),
  };

  // Add or replace the text serializer for paragraphs, because the content of list items is always
  // enclosed in a paragraph.
  options.textSerializers['paragraph'] = (params) => {
    // params contains: node, pos, parent, index, range
    const { node, parent } = params
    let result: string = invokeDefaultSerialization(node, options);

    switch (node.attrs.htmlElementClass) {
      case 'done':
        result = '🗹 ' + result;
        break;
      case 'disabled':
        result = '— ' + result;
        break;
      default:
        result = '☐ ' + result;
        break;
    }
    return result;
  }

  return editor.getText(options);
}
