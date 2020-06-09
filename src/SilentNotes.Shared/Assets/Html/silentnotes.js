/**
 * Collection of helper functions based on: http://youmightnotneedjquery.com/
 */
function ready(fn) {
	if (document.readyState != 'loading') {
		fn();
	} else {
		document.addEventListener('DOMContentLoaded', fn);
	}
}

function findAll(selector) {
	return document.querySelectorAll(selector);
}

function findFirst(selector) {
	var matches = findAll(selector);
	if (matches.length >= 0)
		return matches[0];
	return null;
}

function typeOf(obj) {
	return Object.prototype.toString.call(obj).replace(/^\[object (.+)\]$/, '$1').toLowerCase();
}

/**
 * Functions specific to SilentNotes
 */
function addShortcuts() {
    $('body').keyup(function(evt) {
        if (evt.which === 13) {
            $('.shortcut-enter')[0].click();
        }
        if (evt.which === 27) {
            $('.shortcut-escape')[0].click();
        }
    });
}

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
	if (event.currentTarget.parentNode !== null) {
		$.each(event.currentTarget.parentNode.attributes, function(index, attr) {
			if (/^data-/.test(attr.name))
				params['parent.' + attr.name] = attr.value;
		});
	}

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
		if (this.value !== undefined) {
			var oldValue = $(this).val();
			if (value != oldValue)
				$(this).val(value);
		}
		else {
			$(this).text(value);
		}
	});
}

function htmlViewBindingsSetVisibility(bindingName, visible) {
	var selector = '[data-binding="' + bindingName + '"]';
	if (visible)
		$(selector).removeClass('hidden');
	else
		$(selector).addClass('hidden');
}

function htmlViewBindingsSetEnabled(bindingName, enabled) {
	var selector = '[data-binding="' + bindingName + '"]';
	if (enabled)
		$(selector).removeClass('disabled');
	else
		$(selector).addClass('disabled');
}

function htmlViewBindingsSetInvalid(bindingName, invalid) {
	var selector = '[data-binding="' + bindingName + '"]';
    if (invalid)
        $(selector).addClass('is-invalid');
	else
        $(selector).removeClass('is-invalid');
}

function htmlViewBindingsAddClass(bindingName, className) {
	var selector = '[data-binding="' + bindingName + '"]';
	$(selector).addClass(className);
}

function htmlViewBindingsRemoveClass(bindingName, className) {
	var selector = '[data-binding="' + bindingName + '"]';
	$(selector).removeClass(className);
}

function htmlViewBindingsSetCss(bindingName, cssAttributeName, cssAttributeValue) {
	var selector = '[data-binding="' + bindingName + '"]';
	$(selector).css(cssAttributeName, cssAttributeValue);
}

function htmlViewBindingsSetBackgroundImage(bindingName, image) {
	var selector = '[data-binding="' + bindingName + '"]';
	$(selector).css('background-image', 'url(' + image + ')');
}
