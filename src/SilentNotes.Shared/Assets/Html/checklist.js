/**
 * Compatibility restrictions:
 * 1) The keyword "let" is not yet known in Android 5
 * 2) Default function parameters are not yet known in Android 5
 */
const editorId = '#myeditor';
const doneClass = 'done';

$(document).ready(function () {
	var editor = document.querySelector(editorId);
	editor.addEventListener('click', function (ev) {
		if (isParagraph(ev.target)) {
			onParagraphClicked(ev, ev.target);
		}
	},
		false);
});

function onParagraphClicked(ev, paragraphNode) {
	var targetRect = paragraphNode.getBoundingClientRect();
	var targetMiddleX = (targetRect.left + targetRect.right) / 2;
	var clickedOnLeftSide = ev.clientX < targetMiddleX;

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
	var parent = paragraphNode.parentNode;
	parent.removeChild(paragraphNode);
	notifyTextChanged();
}

function movePendingToTop() {
	var containerNode = $(editorId).find('p').first().parent();
	var children = $(containerNode).children();
	var insertAfter = -1;
	for (var index = 0; index < children.length; index++) {
		var child = children[index];
		var isParagraphElement = isParagraph(child);
		var isPending = isParagraphElement && !child.classList.contains(doneClass);

		if (isParagraphElement) {
			if (isPending) {
				var shouldMove = (index - insertAfter > 1);
				if (shouldMove) {
					var insertBefore = insertAfter + 1;
					var parent = child.parentNode;
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
	notifyTextChanged();
}

function moveUp(toTop) {
	var paragraphElement = findSelectedParagraph();
	var previousElement = paragraphElement.previousElementSibling;
	if (isParagraph(previousElement)) {
		while (toTop && isParagraph(previousElement.previousElementSibling)) {
			previousElement = previousElement.previousElementSibling;
		}

		var parent = paragraphElement.parentElement;
		parent.insertBefore(paragraphElement, previousElement);
		selectParagraph(paragraphElement);
	}
	notifyTextChanged();
}

function moveDown(toBottom) {
	var paragraphElement = findSelectedParagraph();
	var nextElement = paragraphElement.nextElementSibling;
	if (isParagraph(nextElement)) {
		while (toBottom && isParagraph(nextElement.nextElementSibling)) {
			nextElement = nextElement.nextElementSibling;
		}

		var parent = paragraphElement.parentElement;
		parent.insertBefore(paragraphElement, nextElement.nextElementSibling);
		selectParagraph(paragraphElement);
	}
	notifyTextChanged();
}

function findSelectedParagraph() {
	quill.focus();
	var range = quill.getSelection();
	if (range) {
		var node = quill.getLeaf(range.index)[0].domNode;
		while (!isParagraph(node)) {
			node = node.parentElement;
		}
		return node;
	}
	return null;
}

function selectParagraph(paragraphElement) {
	var index = quill.getIndex(Quill.find(paragraphElement));
	quill.setSelection(index, 0, 'api');
}

function saveDoneStates() {
	var result = [];
	var containerNode = $(editorId).find('p').first().parent();
	var children = $(containerNode).children();
	for (var index = 0; index < children.length; index++) {
		var child = children[index];
		if (isParagraph(child)) {
			var isDone = child.classList.contains(doneClass);
			result.push(isDone);
		}
	}
	return result;
}

function restoreDoneStates(doneStates) {
	var containerNode = $(editorId).find('p').first().parent();
	var children = $(containerNode).children();
	for (var index = 0; index < children.length; index++) {
		var child = children[index];
		if (isParagraph(child)) {
			var isDone = doneStates.shift();
			if (isDone === true) {
				child.classList.add(doneClass);
			}
		}
	}
}

function isParagraph(element) {
	return element && (element.nodeName.toLowerCase() === 'p');
}

/**
 * Signals a text changed notification to the view model.
 * Some action like moving a paragraph up or down will trigger a text-changed event on their own,
 * but not if the paragraph is empty.
 */
function notifyTextChanged() {
	location.href = 'HtmlViewBinding?event-type=text-change&data-binding=quill';
}