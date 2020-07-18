/**
 * Compatibility restrictions:
 * 1) The keyword "let" is not yet known in Android 5
 * 2) Default function parameters are not yet known in Android 5
 */
const editorId = '#myeditor';
const doneClass = 'done';
const disabledClass = 'disabled';

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

/**
 * Sets the next state of a checklist item. The states are "pending", "done" and "disabled".
 */
function toggleCheckedState(paragraphNode) {
	var isDone = paragraphNode.classList.contains(doneClass);
	var isDisabled = paragraphNode.classList.contains(disabledClass);
	paragraphNode.classList.remove(doneClass, disabledClass);

	if (isDone) {
		paragraphNode.classList.add(disabledClass);
	}
	else if (isDisabled) {
		// already blank
	}
	else {
		paragraphNode.classList.add(doneClass);
	}
	notifyTextChanged();
}

/**
 * Sets the state for all checklist items.
 */
function setCheckedStateForAll(done, disabled) {
	$(editorId).find('p').removeClass(doneClass);
	$(editorId).find('p').removeClass(disabledClass);

	if (done) {
		$(editorId).find('p').addClass(doneClass);
	}
	else if (disabled) {
		$(editorId).find('p').addClass(disabledClass);
	}
	notifyTextChanged();
}

/**
 * Deletes a checklist item.
 */
function deleteParagraph(paragraphNode) {
	var parent = paragraphNode.parentNode;
	parent.removeChild(paragraphNode);
	notifyTextChanged();
}

/**
 * Reorders the checklist items, so that pending items are on top and deactivated at the bottom.
 */
function movePendingToTop() {
	var containerNode = $(editorId).find('p').first().parent();
	var children = $(containerNode).children();
	var insertPendingAfter = -1;
	var insertDoneAfter = -1;
	for (var index = 0; index < children.length; index++) {
		var child = children[index];
		var isParagraphElement = isParagraph(child);
		var isDone = isParagraphElement && child.classList.contains(doneClass);
		var isDisabled = isParagraphElement && child.classList.contains(disabledClass);
		var isPending = isParagraphElement && !isDone && !isDisabled;

		if (isParagraphElement) {
			if (isPending) {
				var shouldMove = (index - insertPendingAfter > 1);
				if (shouldMove)
					moveChild(child, insertPendingAfter);
				insertPendingAfter++;
				insertDoneAfter++;
			}
			else if (isDone) {
				var shouldMove = (index - insertDoneAfter > 1);
				if (shouldMove)
					moveChild(child, insertDoneAfter);
				insertDoneAfter++;
			}
		}
		else {
			// non paragraph elements start new checklist
			insertPendingAfter = index;
			insertDoneAfter = index;
		}
	}
	notifyTextChanged();
}

function moveChild(child, afterIndex) {
	var parent = child.parentNode;
	var insertBeforeChild = $(parent).children()[afterIndex + 1];
	parent.insertBefore(child, insertBeforeChild);
}

/**
 * Moves a checklist upwards.
 */
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

/**
 * Moves a checklist item downwards.
 */
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

function saveCheckStates() {
	var result = [];
	var containerNode = $(editorId).find('p').first().parent();
	var children = $(containerNode).children();
	for (var index = 0; index < children.length; index++) {
		var child = children[index];
		if (isParagraph(child)) {
			if (child.classList.contains(doneClass)) {
				result.push(doneClass);
			}
			else if (child.classList.contains(disabledClass)) {
				result.push(disabledClass);
			}
			else {
				result.push('none');
			}
		}
	}
	return result;
}

function restoreCheckStates(doneStates) {
	var containerNode = $(editorId).find('p').first().parent();
	var children = $(containerNode).children();
	for (var index = 0; index < children.length; index++) {
		var child = children[index];
		if (isParagraph(child)) {
			var state = doneStates.shift();
			if (state !== 'none') {
				child.classList.add(state);
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
	vuePropertyChanged('UnlockedHtmlContent', null);
}