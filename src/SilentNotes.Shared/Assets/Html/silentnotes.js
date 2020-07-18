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
		location.href = "vueCommandExecute?name=OrderChangedCommand&oldIndex=" + oldIndex + "&newIndex=" + newIndex;
}

function isMobile()
{
	return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/.test(navigator.userAgent.toLowerCase());
}
