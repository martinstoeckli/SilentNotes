import { Extension, Editor } from "@tiptap/core";

/*
Inspired by: https://github.com/KID-1912/tiptap-extension-resizable
*/

interface ResizeOptions {
  types: string[]; // Defines to which elements the handler will be applied (default = ["image"])
  layerStyle: {}; // Dictionary of all values to apply to the resizeable box
  handlerStyle: {}; // Dictionary of all values to apply to the corner handles
  minImageSize: number; // Minimum width or height for resizing (default = )
}

const HandlerClass: string = "image-resize-handler";
const ImageSizeUpdateEvent: string = "imageSizeUpdate";

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
    minImageSize: 10,
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
              if (!attributes.width)
                return {};
              else
                return { style: `width: ${attributes.width}` };
            },
          },
        },
      },
    ];
  },

  // Event called when the extension is created.
  onCreate({ editor }) {
    editor.imgResizeStorage = {
      resizeLayer: HTMLDivElement = null,
      imgElement: HTMLImageElement = null,
      imgNode: Node = null,
    }
    const element = editor.options.element;
    element.style.position = "relative";
  },

  // Event called when the selection of the editor changed.
  onSelectionUpdate: function ({ editor, transaction }) {
    const imgNode: Node = transaction.curSelection.node;

    const isTypeListedInOptions : boolean = imgNode && this.options.types.includes(imgNode.type.name);
    if (!isTypeListedInOptions) {
      // Unselected the image
      hideResizeLayer(editor);
    }
    else {
      // Selected an image
      hideResizeLayer(editor); // Remove layer from possible previous elements
      const resizeLayer = createResizeLayer(this.options.layerStyle, this.options.handlerStyle);

      // Add the mouse down listener
      resizeLayer.addEventListener("mousedown", (event: MouseEvent) => {
        event.preventDefault();

        if (!editor.imgResizeStorage.imgElement)
          return;

        const isBottomLeftHandle = event.target.classList.contains("bottom-left");
        const isBottomRightHandle = event.target.classList.contains("bottom-right");
        if (isBottomLeftHandle || isBottomRightHandle) {
          let startX = event.screenX;
          const dir = isBottomRightHandle ? 1 : -1;

          // Add the mouse move listener on mouse down
          const mouseMoveListener = throttle((event: MouseEvent) => {
            const width = editor.imgResizeStorage.imgElement.clientWidth;
            const distanceX = event.screenX - startX;
            const newWidth = width + dir * distanceX;
            // Resize image
            const imgBox: DOMRect = editor.imgResizeStorage.imgElement.getBoundingClientRect();
            const newImgBox = resizeImgRect(imgBox, newWidth, this.options.minImageSize);
            editor.imgResizeStorage.imgElement.style.width = newImgBox.width + "px";
            editor.imgResizeStorage.imgNode.attrs.width = newImgBox.width + "px";
            // Resize layer
            let pos: DOMRect = getRelativeRect(editor.imgResizeStorage.imgElement, editor.options.element);
            applyStylePosition(pos, editor.imgResizeStorage.resizeLayer);
            startX = event.screenX;
          });
          document.addEventListener("mousemove", mouseMoveListener);

          // Add the mouse up listener on mouse down
          document.addEventListener("mouseup", () => {
            // Remove the mouse move listener on mouse up
            document.removeEventListener("mousemove", mouseMoveListener);
            document.removeEventListener("mouseup", mouseMoveListener);
            // Trigger the update event
            if (editor.imgResizeStorage.imgElement)
              editor.emit(ImageSizeUpdateEvent, { editor });
          });
        }
      });

      const dom = editor.view.domAtPos(transaction.curSelection.from).node;
      const imgElement: HTMLImageElement = dom.querySelector(".ProseMirror-selectednode");

      if (imgElement.clientWidth == 0 || imgElement.clientHeight == 0) {
        // After an Undo the image height will be 0. Remove the selection, so that the next mouse
        // click can select the image again.
        editor.commands.selectParentNode();
      }
      else {
        const pos: DOMRect = getRelativeRect(imgElement, editor.options.element);
        applyStylePosition(pos, resizeLayer);
        showResizeLayer(editor, resizeLayer, imgElement, imgNode);
      }
    }
  },
});

// Adds the resize layer to the DOM
function showResizeLayer(editor, resizeLayer: HTMLDivElement, imgElement: HTMLImageElement, imgNode: Node)
{
  const storage = editor.imgResizeStorage;
  storage.resizeLayer = resizeLayer;
  storage.imgElement = imgElement;
  storage.imgNode = imgNode;
  editor.options.element.appendChild(resizeLayer);
}

// Removes the resize layer from the DOM
function hideResizeLayer(editor)
{
  const storage = editor.imgResizeStorage;
  if (storage.resizeLayer)
  {
    editor.options.element.removeChild(storage.resizeLayer);
    storage.resizeLayer = null;
  }
  storage.imgElement = null;
  storage.imgNode = null;
}

// Applies the position given by the rect to a target html element.
function applyStylePosition(rect: DOMRect, targetElement: HTMLElement) {
  const posStyles = {
    top: rect.top + "px",
    left: rect.left + "px",
    width: rect.width + "px",
    height: rect.height + "px"
  }
  applyStyleOptions(posStyles, targetElement);
}

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

// Resizes the size box of an image, keeping the aspect ration and respecting a minimum size
function resizeImgRect(imgRect: DOMRect, newWidth: number, minSize: number): DOMRect {
  const isLandscape: boolean = imgRect.width >= imgRect.height;
  let newHeight: number = 0;
  if (isLandscape)
  {
    newHeight = imgRect.height / imgRect.width * newWidth;
    if (newHeight < minSize)
    {
      newHeight = minSize;
      newWidth = imgRect.width / imgRect.height * newHeight;
    }
  }
  else
  {
    if (newWidth < minSize)
      newWidth = minSize;
    newHeight = imgRect.height / imgRect.width * newWidth;
  }
  return new DOMRect(imgRect.x, imgRect.y, newWidth, newHeight);
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
function createResizeLayer(layerStyleDictionary: any, handlerStyleDictionary: any) : HTMLDivElement {
  const result = document.createElement("div");
  result.className = "resize-layer";
  result.style.display = "block";
  // result.style.display = "none";
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
