import { Editor, Predicate, findParentNodeClosestToPos, NodeWithPos } from '@tiptap/core';
import Paragraph from '@tiptap/extension-paragraph'
import { Plugin, PluginKey, TextSelection } from 'prosemirror-state'
import { NodeType, Node, ResolvedPos } from 'prosemirror-model'

const todoClass = '';
const doneClass = 'done';
const disabledClass = 'disabled';

let isShoppingModeActive: () => boolean;

/*
Extends the Paragraph node of TipTap, but preserves the class attribute of the HTML element.
*/
export const CheckableParagraph = Paragraph.extend({
  addAttributes() {
    return {
      ...this.parent?.(),
      htmlElementClass: {
        parseHTML: element => element.getAttribute('class'),
        renderHTML: attributes => {
          return { 'class': attributes.htmlElementClass };
        },
      },
    }
  },

  addProseMirrorPlugins() {
    const plugins = [];
    plugins.push(clickHandler({ editor: this.editor, type: this.type }));
    return plugins;
  },
})

type ClickHandlerOptions = {
  editor: Editor,
  type: NodeType,
}

function clickHandler(options: ClickHandlerOptions): Plugin {
  return new Plugin({
    key: new PluginKey('handleClickLink'),
    props: {
      handleClick: (view, pos, event) => {
        // The element is clickable only to the left (change state) and the right (delete) of the
        // text, the text itself has "pointer-events: none;"
        const paragraphHtmlElement = (event.target as HTMLElement)?.closest('p');
        if (paragraphHtmlElement == null)
          return false;

        const targetRect = paragraphHtmlElement.getBoundingClientRect();
        const targetMiddleX = (targetRect.left + targetRect.right) / 2;
        const clickedOnTheLeftSide: boolean = event.clientX < targetMiddleX;

        // Clicking on the :before doesn't select the item, so we select it,
        // because most TipTap commands work with the current selection
        const editor: Editor = options.editor;
        if (clickedOnTheLeftSide) {
          rotateNodeState(editor, pos);
          // On Android the keyboard can popup, which changes the focus and causes a scrolling to
          // the selected element, thus we select the text to prevent scrolling to the top.
          if (editor.isEditable) {
            editor.commands.setTextSelection(pos);
          }
        }
        else {
          if (!isShoppingModeActive()) {
            deleteNodeAtPos(editor, pos);
          }
        }
        return true;
      },
    },
  })
}

function rotateNodeState(editor: Editor, pos: number) {
  const resolvedPos = resolvePos(editor, pos);
  const nodeInfo = findParentNodeClosestToPos(resolvedPos, node => isParagraph(editor, node));
  if (nodeInfo.node != null) {
    const {node, pos} = nodeInfo;

    const newAttributes = { htmlElementClass: rotateState(node.attrs.htmlElementClass) };
    const tr = editor.state.tr;
    tr.setNodeMarkup(pos, undefined, {
      ...node.attrs,
      ...newAttributes,
    })
    editor.view.dispatch(tr);
  }
}

function rotateState(state: string): string {
  if (state == doneClass)
    return disabledClass;
  else if (state == disabledClass)
    return todoClass;
  else
    return doneClass;
}

function deleteNodeAtPos(editor: Editor, pos: number) {
  const resolvedPos = resolvePos(editor, pos);
  const nodeInfo = findParentNodeClosestToPos(resolvedPos, node => isParagraph(editor, node));
  if (nodeInfo.node != null) {
    const from = resolvedPos.before(nodeInfo.depth);
    const to = resolvedPos.after(nodeInfo.depth);

    const tr = editor.state.tr;
    tr.delete(from, to);
    editor.view.dispatch(tr);
  }
}

/*
 * Registers a function which can be used to check whether the shopping mode is active or not.
*/
export function registerIsShoppingModeActive(delegate: () => boolean) {
  isShoppingModeActive = delegate;
}

/*
 * Searches for all checklist items (paragraphs) and sets their html class attribute
 * to the new check state.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  checkState - The new html class for the paragraph elements.
*/
export function setCheckStateForAll(editor: Editor, checkState: string) {
  const tr = editor.state.tr;
  const newAttributes = { htmlElementClass: checkState };
  const paragraphNodePositions = collectAllParagraphs(editor);

  // Check for each paragraph, if it has to be changed
  paragraphNodePositions.forEach(nodePosition => {
    const {node, pos} = nodePosition;

    if (!isOfCheckState(node, checkState))
    {
      tr.setNodeMarkup(pos, undefined, {
        ...node.attrs,
        ...newAttributes,
      })
    }
  });
  editor.view.dispatch(tr);
}

/*
 * Moves the currently selected paragraph (checklist item) upwards.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  singleStep - True if the paragraph should move 1 step upwards,
 *   false if it should move to the top.
*/
export function moveChecklistUp(editor: Editor, singleStep: boolean): void {
  editor.view.focus();
  const paragraphPos = editor.view.state.selection.$anchor;
  const paragraphInfo = findParentNodeClosestToPos(paragraphPos, node => isParagraph(editor, node));
  if (!paragraphInfo)
    return; // Selection is not on paragraph

  const previousParagraphInfo = searchPreviousParagraph(
    editor, paragraphPos, singleStep, node => isParagraph(editor, node));

  if (previousParagraphInfo) {
    moveParagraphUp(editor, paragraphInfo, previousParagraphInfo, true);
  }
}

function moveParagraphUp(editor: Editor, paragraphInfo: any, previousParagraphInfo: any, selectMovedNode: boolean): void {
  const paragraphPos = resolvePos(editor, paragraphInfo.start);
  const from = paragraphPos.before(paragraphPos.depth);
  const to = paragraphPos.after(paragraphPos.depth);

  const previousPos = resolvePos(editor, previousParagraphInfo.start);
  const insertAt = previousPos.before(previousParagraphInfo.depth);

  const tr = editor.state.tr;
  tr.delete(from, to).insert(insertAt, paragraphInfo.node);

  if (selectMovedNode) {
    const newSelection = TextSelection.near(tr.doc.resolve(insertAt));
    tr.setSelection(newSelection);
    tr.scrollIntoView();
  }
  editor.view.dispatch(tr);
}

/*
 * Moves the currently selected paragraph (checklist item) downwards.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {string}  singleStep - True if the paragraph should move 1 step downwards,
 *   false if it should move to the bottom.
*/
export function moveChecklistDown(editor: Editor, singleStep: boolean): void {
  editor.view.focus();
  const paragraphPos = editor.view.state.selection.$anchor;
  const paragraphInfo = findParentNodeClosestToPos(paragraphPos, node => isParagraph(editor, node));
  if (!paragraphInfo)
    return; // Selection is not on paragraph

  const followingParagraphInfo = searchFollowingParagraph(
    editor, paragraphPos, singleStep, node => isParagraph(editor, node));

  if (followingParagraphInfo) {
    const from = paragraphPos.before(paragraphInfo.depth);
    const to = paragraphPos.after(paragraphInfo.depth);
    const followingPos = resolvePos(editor, followingParagraphInfo.start);
    const insertAt = followingPos.after(followingParagraphInfo.depth);
    const insertAtAfterDeletion = insertAt - paragraphInfo.node.nodeSize;

    const tr = editor.state.tr;
    tr.delete(from, to).insert(insertAtAfterDeletion, paragraphInfo.node);

    const newSelection = TextSelection.near(tr.doc.resolve(insertAtAfterDeletion));
    tr.setSelection(newSelection);
    tr.scrollIntoView();
    
    editor.view.dispatch(tr);
  }
}

/*
 * Sorts all paragraphs (checklist items) accordig to their check state.
 * The sorting should result in fewest possible editor changes (small history).
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function sortPendingToTop(editor: Editor): void {
  const paragraphNodePositions = collectAllParagraphs(editor);

  // Check for each paragraph, if it should be moved upwards
  paragraphNodePositions.forEach(nodePosition => {
    const {node, pos} = nodePosition;
    const paragraphPos = resolvePos(editor, pos+1);
    const paragraphInfo = findParentNodeClosestToPos(paragraphPos, node => isParagraph(editor, node));
    const nodeState = numericCheckState(node.attrs.htmlElementClass);

    const previousParagraphInfo = searchPreviousParagraph(
      editor, paragraphPos, false, previousNode => shouldMovePendingToTop(editor, nodeState, previousNode));

    if (previousParagraphInfo) {
      moveParagraphUp(editor, paragraphInfo, previousParagraphInfo, false);
    }
  });
}

function collectAllParagraphs(editor: Editor): NodeWithPos[] {
  const result: NodeWithPos[] = [];
  editor.view.state.doc.descendants((node, pos, parent) => {
    if (isParagraph(editor, node)) {
      result.push({ node: node, pos: pos });
      return false;
    }
    return true;
  });
  return result;
}

function shouldMovePendingToTop(editor: Editor, nodeState: number, previousNode: Node): boolean {
  if (!isParagraph(editor, previousNode))
    return false;
  const previousNodeState = numericCheckState(previousNode.attrs.htmlElementClass);
  return nodeState < previousNodeState;
}

/*
 * Sorts all paragraphs (checklist items) accordig to their check state.
 * The sorting should result in fewest possible editor changes (small history).
 * @param {Editor}  editor - A TipTap editor instance.
*/
export function sortAlphabetical(editor: Editor): void {
  const paragraphNodePositions = collectAllParagraphs(editor);

  // Check for each paragraph, if it should be moved upwards
  paragraphNodePositions.forEach(nodePosition => {
    const {node, pos} = nodePosition;
    const paragraphPos = resolvePos(editor, pos+1);
    const paragraphInfo = findParentNodeClosestToPos(paragraphPos, node => isParagraph(editor, node));

    const previousParagraphInfo = searchPreviousParagraph(
      editor, paragraphPos, false, previousNode => shouldMoveAlphabetical(editor, node, previousNode));

    if (previousParagraphInfo) {
      moveParagraphUp(editor, paragraphInfo, previousParagraphInfo, false);
    }
  });
}

function shouldMoveAlphabetical(editor: Editor, node: Node, previousNode: Node): boolean {
  if (!isParagraph(editor, previousNode))
    return false;

  const comparison = node.textContent.localeCompare(previousNode.textContent, undefined, { sensitivity: 'base' });
  return comparison < 0;
}

/*
 * Searches for a node previous to the original node at a given position, both on the same depth.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {ResolvedPos}  pos - The position of the paragraph node from where to start the search.
 * @param {boolean}  singleStep - True if the node right before the original node should be found,
 *   false if the search should continue until no node matching the filter can be found.
 * @param {Predicate}  filter - Delegate which checks whether the found node is a valid result.
 *   Use it  e.g. to stop if a node is a heading instead of a paragraph.
 * @returns Dictionary with the found node or null if no previous node could be found.
 *   See the result of findParentNodeClosestToPos() for comparison.
 */
function searchPreviousParagraph(editor: Editor, pos: ResolvedPos, singleStep: boolean, filter: Predicate) {
  let result = null;
  let resultPos: ResolvedPos = null;

  // Check if there is a previous matching node.
  const paragraphInfo = findParentNodeClosestToPos(pos, node => isParagraph(editor, node));
  if (pos.before(paragraphInfo.depth) > 0) {
    resultPos = resolvePos(editor, pos.before(paragraphInfo.depth)-1);
    result = findParentNodeClosestToPos(resultPos, node => filter(node));
  }

  if (!singleStep) {
    let previousResult = result;
    let previousResultPos: ResolvedPos = resultPos;
    while (previousResult && previousResult.pos > 0) {
      previousResultPos = resolvePos(editor, previousResult.pos-1);
      previousResult = findParentNodeClosestToPos(previousResultPos, node => filter(node));
      if (previousResult) {
        result = previousResult;
      }
    }
  }
  return result;
}

/*
 * Searches for a node following the original node at a given position, both on the same depth.
 * @param {Editor}  editor - A TipTap editor instance.
 * @param {ResolvedPos}  pos - The position of the paragraph node from where to start the search.
 * @param {boolean}  singleStep - True if the node right after the original node should be found,
 *   false if the search should continue until no node matching the filter can be found.
 * @param {Predicate}  filter - Delegate which checks whether the found node is a valid result.
 *   Use it  e.g. to stop if a node is a heading instead of a paragraph.
 * @returns Dictionary with the found node or null if no previous node could be found.
 *   See the result of findParentNodeClosestToPos() for comparison.
 */
function searchFollowingParagraph(editor: Editor, pos: ResolvedPos, singleStep: boolean, filter: Predicate) {
  let result = null;
  let resultPos: ResolvedPos = null;
  const documentSize = editor.state.doc.content.size;

  // Check if there is a following matching node.
  const paragraphInfo = findParentNodeClosestToPos(pos, node => isParagraph(editor, node));
  if (pos.after(paragraphInfo.depth) < documentSize) {
    resultPos = resolvePos(editor, pos.after(paragraphInfo.depth)+1);
    result = findParentNodeClosestToPos(resultPos, node => filter(node));
  }

  // Iterate through all following nodes.
  if (!singleStep) {
    let followingResult = result;
    let followingResultPos: ResolvedPos = resultPos;
    while (followingResult && followingResultPos.after(paragraphInfo.depth) < documentSize) {
      followingResultPos = resolvePos(editor, followingResultPos.after(paragraphInfo.depth)+1);
      followingResult = findParentNodeClosestToPos(followingResultPos, node => filter(node));
      if (followingResult) {
        result = followingResult;
      }
    }
  }
  return result;
}

function isOfCheckState(node: Node, checkState: string) {
  return node.attrs.htmlElementClass == checkState;
}

function numericCheckState(state: string): number {
  if (state == null || state == todoClass)
    return 0;
  else if (state == doneClass)
    return 1;
  else
    return 2;
}

function isNodeOfType(node: Node, type: NodeType): boolean {
  return node && node.type == type;
}

function isParagraph(editor: Editor, node: Node) {
	return isNodeOfType(node, editor.view.state.schema.nodes.paragraph);
}

function resolvePos(editor: Editor, pos: number): ResolvedPos {
  return editor.state.doc.resolve(pos);
}
