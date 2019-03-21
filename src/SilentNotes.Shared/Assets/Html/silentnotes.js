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
				handle: '.sortable-handle',
				onEnd: function (evt) {
					onSortableOrderChanged(evt.oldIndex, evt.newIndex);
				}
			});
	});
}

function makeSortableMobile()
{
	$('.sortable').each(function () {
		Sortable.create(
			this,
			{
				handle: '.sortable-handle',
				onEnd: function (evt) {
					onSortableOrderChanged(evt.oldIndex, evt.newIndex);
				}
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
	var lastLongPressTime = new Date().getTime();
	$('.selectable').find('.selectable-item').each(function () {
		$(this).longpress(
			function (evt) {
				lastLongPressTime = new Date().getTime();

				// long press selects item
				evt.type = 'list-select';
				bind(evt);
			},
			function (evt) {
				// short press triggers action
				evt.type = 'list-open';
				bind(evt);
			},
			350);
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

	var params = new Object();
	params['event-type'] = event.type;
	params['id'] = event.currentTarget.id;
	params['value'] = event.currentTarget.value;
	params['checked'] = event.currentTarget.checked;
	$.each(event.currentTarget.attributes, function(index, attr) {
		if (/^data-/.test(attr.name))
			params[attr.name] = attr.value;
	});

	var parts = [];
	$.each(params, function(key, value) {
		if (value)
			parts.push(key + '=' + encodeURIComponent(value));
	});

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

function htmlViewBindingsSetBackgroundImage(bindingName, image) {
	var selector = '[data-binding="' + bindingName + '"]';
	$(selector).css('background-image', 'url(' + image + ')');
}

function selectNote(noteId) {
	$('#note-repository').find('.selectable-item').each(function () {
		var item = $(this);
		if (item.data('note') === noteId) {
			if (!item.hasClass('selected'))
				item.addClass('selected');
		}
		else {
			if (item.hasClass('selected'))
				item.removeClass('selected');
		}
	});
}