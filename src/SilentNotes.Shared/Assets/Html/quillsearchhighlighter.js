class QuillSearchHighlighter {
  /**
   * Initializes a new instance of the QuillSearchHighlighter class.
   * @param {Quill} quill - The quill editor.
   * @param {*} inputElement - The Html input element which receives the search string.
   */
  constructor(quill, inputElement) {
    var _this = this;
    _this.quill = quill;
    _this.inputElement = inputElement;
    _this.searchHighlights = [];
    _this.searchDelay = 1; // prevents subsequent events
    _this.isListening = null;
  }

  /**
   * Connects to the quill editor- and to global events.
   */
  startListening() {
    var _this = this;

    // Just reconnect?
    var isFirstTime = _this.isListening === null;
    _this.isListening = true;

    if (isFirstTime) {
      var timer; // scroll and resize events are called multiple times, the timer avoids duplicates
      _this.inputElement.addEventListener('input', function(e) {
        if (_this.isListening) {
          _this.searchAndHighlight(_this.inputElement.value, true, true);
        }
      });

      _this.quill.root.addEventListener('scroll', function(e) {
        if (_this.isListening) {
          window.clearTimeout(timer);
          timer = window.setTimeout(
          function() { _this.searchAndHighlight(_this.inputElement.value, false, false); }, _this.searchDelay); 
        }
      });

      window.addEventListener('resize', function(e) {
        if (_this.isListening) {
          window.clearTimeout(timer);
          timer = window.setTimeout(
          function() { _this.searchAndHighlight(_this.inputElement.value, false, false); }, _this.searchDelay); 
        }
      });
    }
  }

  stopListening() {
    var _this = this;
    _this.isListening = false;
  }

  /**
   * Adds highlight divs to the quill editor, for each found position of a given substring.
   * @param {string} needle - The substring to search for
   * @param {boolean} selection - Create a selection if the substring was found and bring it
   * into the visible range.
   * @param {boolean} focusBackToInput - Give back the focus to the input element, so the user
   * can continue typing.
   */
  searchAndHighlight(needle, scrollToFirst, focusBackToInput) {
    var _this = this;
    _this.clearSearchHighlights();
    if (!needle || needle.length < 2)
      return;

    needle = needle.toLocaleLowerCase();
    var quillText = _this.getLowerQuillText();

    // Search for matches
    var findings = [];
    var fromIndex = 0;
    var index = quillText.indexOf(needle, fromIndex);
    while (index >= 0) {
      findings.push(index);
      index = quillText.indexOf(needle, index + 1);
    }

    // Bring first match into view
    if (scrollToFirst && findings.length >= 1) {
      _this.quill.setSelection(findings[0], length);
    }

    // Create highlights divs inside visible range
    findings.forEach(function(finding){
      var bounds = _this.quill.getBounds(finding, needle.length);
      if (_this.isInVisibleArea(bounds)) {
        var highlight = _this.createSearchHighlight(bounds);
        _this.searchHighlights.push(highlight);
        var quillParent = _this.quill.root.parentElement;
        quillParent.appendChild(highlight);
      }
    });

    if (focusBackToInput) {
      _this.inputElement.focus();
    }
  }

  /**
   * Removes and frees all highlight divs from the quill editor.
   */
  clearSearchHighlights() {
    var _this = this;
    _this.searchHighlights.forEach(function(item) {
        item.remove();
    })            
  }

  /**
   * Creates a transparent div to highlight a search result.
   * @param {*} bounds - The bounding box of the div
   */
  createSearchHighlight(bounds) {
    var result = document.createElement('div');
    result.classList.add('qs-highlight');
    result.style.width = bounds.width + 'px';
    result.style.height = bounds.height + 'px';
    result.style.left = bounds.left + 'px';
    result.style.top = bounds.top + 'px';
    return result;
  }

  /**
   * Gets the pure text content of the quill editor in lower case.
   * The returned text can be used to get the position of a given subtext,
   * images are translated to a single space character to leave the index intact.
   */
  getLowerQuillText() {
    var _this = this;
    return _this.quill.getContents().filter(function (op) {
      return typeof op.insert === 'string' || op.insert.image;
    }).map(function (op) {
      if (op.insert.image) {
          return op.insert.image = ' ';
      }
      return op.insert.toLocaleLowerCase();
    }).join('');
  }

  /**
   * Checks whether a bounding box is in the visible area (scrolling) of the quill editor.
   * @param {*} bounds - The bounding box to check
   */
  isInVisibleArea(bounds) {
    var _this = this;
    var editorElement = _this.quill.root;
    var result = (bounds.bottom > editorElement.clientTop)
      && (bounds.top < editorElement.clientTop + editorElement.clientHeight);
    return result;
  }
}