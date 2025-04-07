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
    const element = editor.options.element;
    element.style.position = "relative";

    // Initialize a hidden resize overlay
    const resizeLayer = document.createElement("div");
    resizeLayer.className = "resize-layer";
    resizeLayer.style.display = "none";
    resizeLayer.style.position = "absolute";
    Object.entries(this.options.layerStyle).forEach(([key, value]) => {
      resizeLayer.style[key] = value;
    });

    // Create the 4 corner handles
    const handlerNames = ["top-left", "top-right", "bottom-left", "bottom-right"];
    const fragment = document.createDocumentFragment();
    for (let handlerName of handlerNames) {
      const handle = document.createElement("div");
      handle.className = HandlerClass + ' ' + handlerName;
      handle.style.position = "absolute";
      Object.entries(this.options.handlerStyle).forEach(([key, value]) => {
        handle.style[key] = value;
      });

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
    resizeLayer.appendChild(fragment);
    editor.resizeLayer = resizeLayer;
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
        const mouseMoveListener = (event: MouseEvent) => {
          const width = resizeElement.clientWidth;
          const distanceX = event.screenX - startX;
          const newWidth = width + dir * distanceX;
          // resizeElement
          resizeElement.style.width = newWidth + "px";
          resizeNode.attrs.width = newWidth + "px";
          // resizeLayer
          const pos: DOMRect = getRelativeRect(resizeElement, element);
          resizeLayer.style.top = pos.top + "px";
          resizeLayer.style.left = pos.left + "px";
          resizeLayer.style.width = resizeElement.clientWidth + "px";
          resizeLayer.style.height = resizeElement.clientHeight + "px";
          startX = event.screenX;
        };
        document.addEventListener("mousemove", mouseMoveListener);

        // Add the mouse up listener on mouse down
        document.addEventListener("mouseup", () => {
          // Remove the mouse move listener on mouse up
          document.removeEventListener("mousemove", mouseMoveListener);
        });
      }
    });
  },

  // onTransaction: throttle(function ({ editor }) {
  //   const resizeLayer = editor.resizeLayer;
  //   const isResizeLayerVisible = resizeLayer && resizeLayer.style.display === "block";
  //   if (isResizeLayerVisible) {
  //     const dom: Element = this.storage.resizeElement;
  //     const element: Element = editor.options.element;
  //     const pos = getRelativePosition(dom, element);
  //     resizeLayer.style.top = pos.top + "px";
  //     resizeLayer.style.left = pos.left + "px";
  //     resizeLayer.style.width = dom.clientWidth + "px";
  //     resizeLayer.style.height = dom.clientHeight + "px";
  //   }
  // }, 240),

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
});

// Calculates the position of element relative to the ancestor, taking into account the scrolling
// function getRelativePosition(element: Element, ancestor: Element) {
//   const elementRect: DOMRect = element.getBoundingClientRect();
//   const ancestorRect: DOMRect = ancestor.getBoundingClientRect();
//   const relativePosition = {
//     top: elementRect.top - ancestorRect.top + ancestor.scrollTop,
//     left: elementRect.left - ancestorRect.left + ancestor.scrollLeft,
//   };
//   return relativePosition;
// }

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

function throttle(func: Function, delay: number) {
  let isWaiting = false;

  return function (...args) {
      if (!isWaiting) {
          func.apply(this, args);
          isWaiting = true;

          setTimeout(() => {
              isWaiting = false;
          }, delay);
      }
  };
}