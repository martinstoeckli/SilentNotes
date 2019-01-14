/**
 * Longpress is a jQuery plugin that makes it easy to support long press
 * events on mobile devices and desktop borwsers.
 * Modified by Martin Stoeckli to suppress short press events after dragging
 * and to reduce move event handlers.
 *
 * @name longpress
 * @version 0.1.2
 * @requires jQuery v1.2.3+
 * @author Vaidik Kapoor
 * @license MIT License - http://www.opensource.org/licenses/mit-license.php
 *
 * For usage and examples, check out the README at:
 * http://github.com/vaidik/jquery-longpress/
 *
 * Copyright (c) 2008-2013, Vaidik Kapoor (kapoor [*dot*] vaidik -[at]- gmail [*dot*] com)
 */

(function($) {
    $.fn.longpress = function(longCallback, shortCallback, duration) {
        if (typeof duration === "undefined") {
            duration = 500;
        }

        return this.each(function() {
            var $this = $(this);

            // to keep track of how long something was pressed
            var mouse_down_time;
            var timeout = null;

            // mousedown or touchstart callback
            function mousedown_callback(e) {
                mouse_down_time = new Date().getTime();
                var context = $(this);

                // set a timeout to call the longpress callback when time elapses
                timeout = setTimeout(function() {
                    if (typeof longCallback === "function") {
                        longCallback.call(context, e);
                    } else {
                        $.error('Callback required for long press. You provided: ' + typeof longCallback);
                    }
                }, duration);
                subscribeMoveEvents();
            }

            // mouseup or touchend callback
            function mouseup_callback(e) {
                var dragged = timeout === null;
                clearTimeout(timeout);
                timeout = null;
                unsubscribeMoveEvents();

                var press_time = new Date().getTime() - mouse_down_time;
                if (!dragged && press_time < duration) {
                    // call the shortCallback if provided
                    if (typeof shortCallback === "function") {
                        shortCallback.call($(this), e);
                    } else if (typeof shortCallback === "undefined") {
                        ;
                    } else {
                        $.error('Optional callback for short press should be a function.');
                    }
                }
            }

            // cancel long press event if the finger or mouse was moved
            function move_callback(e) {
                unsubscribeMoveEvents();
                clearTimeout(timeout);
                timeout = null;
            }

            // subscribe to move events only as long as necessary
            function subscribeMoveEvents() {
                //$this.on('mousemove', move_callback);
                $this.on('touchmove', move_callback);
            }

            function unsubscribeMoveEvents() {
                //$this.off('mousemove', move_callback);
                $this.off('touchmove', move_callback);
            }

            // Browser Support
            $this.on('mousedown', mousedown_callback);
            $this.on('mouseup', mouseup_callback);

            // Mobile Support
            $this.on('touchstart', mousedown_callback);
            $this.on('touchend', mouseup_callback);
        });
    };
}(jQuery));
