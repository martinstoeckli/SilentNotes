const editorId = '#myeditor';
const doneClass = 'done';

$(document).ready(function () {
    let editor = document.querySelector(editorId);
    editor.addEventListener('click', function (ev) {
        if (isParagraph(ev.target)) {
            onParagraphClicked(ev, ev.target);
        }
    },
        false);
});

function onParagraphClicked(ev, paragraphNode) {
    let targetRect = paragraphNode.getBoundingClientRect();
    let targetMiddleX = (targetRect.left + targetRect.right) / 2;
    let clickedOnLeftSide = ev.clientX < targetMiddleX;

    if (clickedOnLeftSide) {
        toggleCheckedState(paragraphNode);
    }
    else {
        deleteParagraph(paragraphNode);
    }
}

function toggleCheckedState(paragraphNode) {
    paragraphNode.classList.toggle(doneClass);
    notifyTextChanged();
}

function setCheckedStateForAll(done) {
    if (done) {
        $(editorId).find('p').addClass(doneClass);
    }
    else {
        $(editorId).find('p').removeClass(doneClass);
    }
    notifyTextChanged();
}

function deleteParagraph(paragraphNode) {
    let parent = paragraphNode.parentNode;
    parent.removeChild(paragraphNode);
}

function movePendingToTop() {
    let containerNode = $(editorId).find('p').first().parent();
    let children = $(containerNode).children();
    let insertAfter = -1;
    for (let index = 0; index < children.length; index++) {
        let child = children[index];
        let isParagraphElement = isParagraph(child);
        let isPending = isParagraphElement && !child.classList.contains(doneClass);

        if (isParagraphElement) {
            if (isPending) {
                let shouldMove = (index - insertAfter > 1);
                if (shouldMove) {
                    let insertBefore = insertAfter + 1;
                    let parent = child.parentNode;
                    parent.insertBefore(child, $(containerNode).children()[insertBefore]);
                    insertAfter = insertBefore;
                }
                else {
                    insertAfter = index;
                }
            }
        }
        else {
            insertAfter = index; // non paragraph elements start new checklist
        }
    }
}

function moveUp(toTop = false) {
    let paragraphElement = findSelectedParagraph();
    let previousElement = paragraphElement.previousElementSibling;
    if (isParagraph(previousElement)) {
        while (toTop && isParagraph(previousElement.previousElementSibling)) {
            previousElement = previousElement.previousElementSibling;
        }

        let parent = paragraphElement.parentElement;
        parent.insertBefore(paragraphElement, previousElement);
        selectParagraph(paragraphElement);
    }
}

function moveDown(toBottom = false) {
    let paragraphElement = findSelectedParagraph();
    let nextElement = paragraphElement.nextElementSibling;
    if (isParagraph(nextElement)) {
        while (toBottom && isParagraph(nextElement.nextElementSibling)) {
            nextElement = nextElement.nextElementSibling;
        }

        let parent = paragraphElement.parentElement;
        parent.insertBefore(paragraphElement, nextElement.nextElementSibling);
        selectParagraph(paragraphElement);
    }
}

function findSelectedParagraph() {
    quill.focus();
    var range = quill.getSelection();
    if (range) {
        let node = quill.getLeaf(range.index)[0].domNode;
        while (!isParagraph(node)) {
            node = node.parentElement;
        }
        return node;
    }
    return null;
}

function selectParagraph(paragraphElement) {
    let index = quill.getIndex(Quill.find(paragraphElement));
    quill.setSelection(index, 0, 'api');
}

function isParagraph(element) {
    return element && (element.nodeName.toLowerCase() === 'p');
}

function saveDoneStates() {
    let result = [];
    let containerNode = $(editorId).find('p').first().parent();
    let children = $(containerNode).children();
    for (let index = 0; index < children.length; index++) {
        let child = children[index];
        if (isParagraph(child)) {
            let isDone = child.classList.contains(doneClass);
            result.push(isDone);
        }
    }
    return result;
}

function restoreDoneStates(doneStates) {
    let containerNode = $(editorId).find('p').first().parent();
    let children = $(containerNode).children();
    for (let index = 0; index < children.length; index++) {
        let child = children[index];
        if (isParagraph(child)) {
            let isDone2 = child.classList.contains(doneClass);
            let isDone = doneStates.shift();
            if (isDone === true) {
                child.classList.add(doneClass);
            }
        }
    }
}

function notifyTextChanged() {
    location.href = 'HtmlViewBinding?event-type=text-change&data-binding=quill';
}