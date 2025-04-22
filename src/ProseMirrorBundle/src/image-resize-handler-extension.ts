import { Extension } from "@tiptap/core";
import { Editor } from '@tiptap/core';
import { Transaction } from 'prosemirror-state'

/*
Credits to: https://github.com/KID-1912/tiptap-extension-resizable
*/

interface ResizeOptions {
  types: string[]; // Defines to which elements the handler will be applied (default = ["image"])
  layerStyle: {}; // Dictionary of all values to apply to the resizeable box
  handlerStyle: {}; // Dictionary of all values to apply to the corner handles
}

const HandlerClass: string = "image-resize-handler";

export const ImageResizeHandler = Extension.create<ResizeOptions>({
  name: "imageResizeHandler",

  defaultOptions: {
    types: ["image"],
    layerStyle: {
      border: "1px solid #2a81ac",
    },
    handlerStyle: {
      width: "8px",
      height: "8px",
      background: "#2a81ac",
    },
    minImageSize: 30,
  },

  addStorage() {
    return {
      resizeElement: null,
      resizeNode: null,
    };
  },

  addGlobalAttributes() {
    return [
      {
        types: this.options.types,
        attributes: {
          width: {
            default: null,
            parseHTML: (element) => element.style.width,
            renderHTML: (attributes) => {
              if (!attributes.width) return {};
              return { style: `width: ${attributes.width}` };
            },
          },
        },
      },
    ];
  },

  onCreate({ editor }) {
    // Initialize a hidden resize overlay
    const resizeLayer = createResizeLayer(this.options.layerStyle, this.options.handlerStyle);
    editor.resizeLayer = resizeLayer;

    const element = editor.options.element;
    element.style.position = "relative";
    element.appendChild(resizeLayer);

    // Add the mouse down listener
    resizeLayer.addEventListener("mousedown", (event: MouseEvent) => {
      event.preventDefault();
      const resizeElement = this.storage.resizeElement;
      const resizeNode = this.storage.resizeNode;
      if (!resizeElement)
        return;

      const isBottomLeftHandle = event.target.classList.contains("bottom-left");
      const isBottomRightHandle = event.target.classList.contains("bottom-right");
      if (isBottomLeftHandle || isBottomRightHandle) {
        let startX = event.screenX;
        const dir = isBottomRightHandle ? 1 : -1;

        // Add the mouse move listener on mouse down
        const mouseMoveListener = throttle((event: MouseEvent) => {
          const width = resizeElement.clientWidth;
          const distanceX = event.screenX - startX;
          const newWidth = width + dir * distanceX;
          // resizeElement
          resizeElement.style.width = newWidth + "px";
          resizeNode.attrs.width = newWidth + "px";
          // resizeLayer
          let pos: DOMRect = getRelativeRect(resizeElement, element);
          pos = ensureMinImageSize(pos, this.options.minImageSize);
          resizeLayer.style.top = pos.top + "px";
          resizeLayer.style.left = pos.left + "px";
          resizeLayer.style.width = pos.width + "px";
          resizeLayer.style.height = pos.height + "px";
          startX = event.screenX;
        });
        document.addEventListener("mousemove", mouseMoveListener);

        // Add the mouse up listener on mouse down
        document.addEventListener("mouseup", () => {
          // Remove the mouse move listener on mouse up
          document.removeEventListener("mousemove", mouseMoveListener);
          document.removeEventListener("mouseup", mouseMoveListener);
        });
      }
    });
  },

  onSelectionUpdate: function ({ editor, transaction }) {
    const node = transaction.curSelection.node;
    const resizeLayer = editor.resizeLayer;

    const isTypeListedInOptions : boolean = node && this.options.types.includes(node.type.name);
    if (isTypeListedInOptions) {
      resizeLayer.style.display = "block"; // make visible
      const element: Element = editor.options.element;
      let dom = editor.view.domAtPos(transaction.curSelection.from).node;
      dom = dom.querySelector(".ProseMirror-selectednode");
      this.storage.resizeElement = dom;
      this.storage.resizeNode = node;
      const pos: DOMRect = getRelativeRect(dom, element);
      resizeLayer.style.top = pos.top + "px";
      resizeLayer.style.left = pos.left + "px";
      resizeLayer.style.width = pos.width + "px";
      resizeLayer.style.height = pos.height + "px";
    }
    else {
      resizeLayer.style.display = "none"; // make invisible
    }
  },

  // onTransaction: throttle(function ({ editor }) {
  //   const resizeLayer = editor.resizeLayer;
  //   const isResizeLayerVisible = resizeLayer && resizeLayer.style.display === "block";
  //   if (isResizeLayerVisible) {
  //     const resizeElement: Element = this.storage.resizeElement;
  //     const element: Element = editor.options.element;
  //     const pos: DOMRect = getRelativeRect(resizeElement, element);
  //     resizeLayer.style.top = pos.top + "px";
  //     resizeLayer.style.left = pos.left + "px";
  //     resizeLayer.style.width = pos.width + "px";
  //     resizeLayer.style.height = pos.height + "px";
  //   }
  // }, 240),
});

// Applies a dictionary of style options (e.g. from a config) to a target html element.
function applyStyleOptions(styleDictionary: any, targetElement: HTMLElement) {
  Object.entries(styleDictionary).forEach(([key, value]) => {
    targetElement.style[key] = value;
  });
}

function getRelativeRect(element: Element, ancestor: Element): DOMRect {
  const elementRect: DOMRect = element.getBoundingClientRect();
  const ancestorRect: DOMRect = ancestor.getBoundingClientRect();
  const result = new DOMRect(
    elementRect.x - ancestorRect.x + ancestor.scrollTop,
    elementRect.y - ancestorRect.y + ancestor.scrollLeft,
    element.clientWidth,
    element.clientHeight
  );
  return result;
}

// Enlarges a rect to a minimum size, keeping the aspect ratio of the rect.
function ensureMinImageSize(rect: DOMRect, minSize: number): DOMRect {
  if ((rect.width >= minSize) && (rect.height >= minSize))
  {
    return rect;
  }
  else
  {
    const stretchFactor = (rect.height < rect.width)
      ? minSize / rect.height
      : minSize / rect.width;
    return new DOMRect(rect.x, rect.y, rect.width * stretchFactor, rect.height * stretchFactor);
  }
}

// Throttles the given function, so the function is not called more than once per the given delay.
// This can be used inside an event to avoid too many calls of the event handler function. The
// throttling can be used for multiple functions, without interference.
function throttle(func: Function, delayMs: number = 40) {
  let lastExecutions = new WeakMap<Function, number>();

  return function (...args: any[]) {
    const now = Date.now();
    const lastExecution = lastExecutions.get(func) || 0;

    if (now - lastExecution >= delayMs) {
      lastExecutions.set(func, now);
      func.apply(this, args);
    }
  };
}

// Builds an overlay div with handles in each corner to resize an image.
function createResizeLayer(layerStyleDictionary: any, handlerStyleDictionary: any) : HTMLElement {
  const result = document.createElement("div");
  result.className = "resize-layer";
  result.style.display = "none";
  result.style.position = "absolute";
  applyStyleOptions(layerStyleDictionary, result);

  // Create the 4 corner handles
  const fragment = document.createDocumentFragment();
  const handlerNames = ["top-left", "top-right", "bottom-left", "bottom-right"];
  for (let handlerName of handlerNames) {
    const handle = document.createElement("div");
    handle.className = HandlerClass + ' ' + handlerName;
    handle.style.position = "absolute";
    applyStyleOptions(handlerStyleDictionary, handle);

    const directions = handlerName.split("-");
    const verticalDirection: string = directions[0]; // top or bottom
    const horizontalDirection: string = directions[1]; // left or right
    handle.style[verticalDirection] = -(parseInt(handle.style.height) / 2) + "px";
    handle.style[horizontalDirection] = -(parseInt(handle.style.width) / 2) + "px";

    if (handlerName === "bottom-left")
      handle.style.cursor = "sw-resize";
    if (handlerName === "bottom-right")
      handle.style.cursor = "se-resize";
    fragment.appendChild(handle);
  }
  result.appendChild(fragment);
  return result;
}
