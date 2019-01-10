// Makes child elements of class .sortable sortable, using
// https://github.com/RubaXa/Sortable
function makeSortable() {
	if (isMobile()) {
		makeSortableMobile();
	}
	else {
		makeSortableDesktop();
	}
}

function makeSortableDesktop()
{
	$('.sortable').each(function () {
		Sortable.create(
			this,
			{
				onEnd: function (evt) {
					onSortableOrderChanged(evt.oldIndex, evt.newIndex);
				},
			});
	});
}

function makeSortableMobile()
{
	$('.sortable').each(function () {
		Sortable.create(
			this,
			{
				ghostClass: "sortable-ghost",
				delay: 360, // on touch screen, sorting should not prevent scrolling
				onEnd: function (evt) {
					onSortableOrderChanged(evt.oldIndex, evt.newIndex);
				},
			});
	});
}

function makeSelectable()
{
	if (isMobile())
		makeSelectableListMobile();
	else
		makeSelectableListDesktop();
}

function makeSelectableListDesktop()
{
	$('.selectable').find('.selectable-item').each(function ()
	{
		// mouse down selects item
		$(this).on('mousedown', function (evt) {
			// Ignore duplicate event in case of double click
			if (evt.originalEvent.detail <= 1) {
				evt.type = 'list-select';
				bind(evt);
			}
		});

		// double click triggers action
		$(this).on('dblclick', function (evt) {
			evt.type = 'list-open';
			bind(evt);
		});
	});
}

function makeSelectableListMobile()
{
	var startX = 0;
	var startY = 0;
	var lastX = 0;
	var lastY = 0;
	var moveThreshold = 10;
	var lastLongPressTime = new Date().getTime();
	$('.selectable').find('.selectable-item').each(function () {
		$(this).mousedown(function (evt) {
			startX = evt.pageX;
			startY = evt.pageY;
			lastX = evt.pageX;
			lastY = evt.pageY;
		});
		$(this).mousemove(function (evt) {
			lastX = evt.pageX;
			lastY = evt.pageY;
		});

		$(this).longpress(
			function (evt) {
				lastLongPressTime = new Date().getTime();

				// long press selects item
				var dragged = Math.pow(startX - lastX, 2) + Math.pow(startY - lastY, 2) >= Math.pow(moveThreshold, 2);
				dragged = false;
				if (!dragged) {
					evt.type = 'list-select';
					bind(evt);
				}
			},
			function (evt) {
				var tooShortAfterLongPress = new Date().getTime() < lastLongPressTime + 640;
				if (!tooShortAfterLongPress) {
					// short press triggers action
					var dragged = Math.pow(startX - lastX, 2) + Math.pow(startY - lastY, 2) >= Math.pow(moveThreshold, 2);
					if (!dragged) {
						evt.type = 'list-open';
						bind(evt);
					}
				}
			},
			360);
	});
}

/**
 * Can be called by the app to close all dropdowns when the devices back button is pressed on the
 * main form. If no dropdown is open, the function can signal the application to handle the event.
 */
function closeDropdownOrSignalBackPressed()
{
	var hasOpenDropdown = $('.dropdown-menu').hasClass('show');
	if (hasOpenDropdown)
		$('.dropdown-menu').removeClass('show');
	else
		location.href = "BackPressed";
}

/*
 * This event is triggered, when the user reordered notes with drag&drop
 */
function onSortableOrderChanged(oldIndex, newIndex)
{
	if (oldIndex !== newIndex)
		location.href = "HtmlViewBinding?event-type=list-orderchanged&oldIndex=" + oldIndex + "&newIndex=" + newIndex;
}

function isMobile()
{
	return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/.test(navigator.userAgent.toLowerCase());
}

/*
 * Functions required by the HtmlViewBindings.cs class
 */
function bind(event) {
	event = event || window.event;

	var params = [];
	params['event-type'] = event.type;
	params['id'] = event.currentTarget.id;
	params['value'] = event.currentTarget.value;
	params['checked'] = event.currentTarget.checked;

	var eventAttributes = event.currentTarget.attributes;
	if (eventAttributes) {
		for (let i = 0; i < eventAttributes.length; i++) {
			var attr = eventAttributes[i];
			if (/^data-/.test(attr.name))
				params[attr.name] = attr.value;
		}
	}

	var parts = [];
	for (var key in params) {
		var value = params[key];
		if (value)
			parts.push(key + '=' + encodeURIComponent(value));
	}

	var url = 'HtmlViewBinding?' + parts.join('&');
	location.href = url;
}

function htmlViewBindingsSetValue(bindingName, value) {
	var selector = '[data-binding="' + bindingName + '"]';
	$(selector).each(function () {
		var oldValue = $(this).val();
		if (value != oldValue)
			$(this).val(value);
	});
}

function htmlViewBindingsSetVisibility(bindingName, visible) {
	var selector = '[data-binding="' + bindingName + '"]';
	if (visible)
		$(selector).show();
	else
		$(selector).hide();
}

function htmlViewBindingsSetCss(bindingName, cssAttributeName, cssAttributeValue) {
	var selector = '[data-binding="' + bindingName + '"]';
	$(selector).css(cssAttributeName, cssAttributeValue);
}
