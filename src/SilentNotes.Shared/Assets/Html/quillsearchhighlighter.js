class QuillSearchHighlighter {
  quill = null;
  inputElement = null;
  searchHighlights = [];

  /**
   * Initializes a new instance of the QuillSearchHighlighter class.
   * @param {Quill} quill - The quill editor.
   * @param {Object} inputElement - The Html input element which receives the search string.
   */
  constructor(quill, inputElement) {
    var _this = this;
    _this.quill = quill;
    _this.inputElement = inputElement;
  }

  /**
   * Connects to the quill editor- and to global events.
   */
  startListening() {
    var _this = this;
    var timer; // scroll and resize events are called multiple times, the timer avoids duplicates

    _this.inputElement.addEventListener('input', function(e) {
      window.clearTimeout(timer);
      timer = window.setTimeout(
        function() { _this.searchAndHighlight(_this.inputElement.value, true, true); }, 1); 
    });

    _this.quill.root.addEventListener('scroll', function(e) {
      window.clearTimeout(timer);
      timer = window.setTimeout(
        function() { _this.searchAndHighlight(_this.inputElement.value, false, false); }, 1); 
    });

    window.addEventListener('resize', function(e) {
      window.clearTimeout(timer);
      timer = window.setTimeout(
        function() { _this.searchAndHighlight(_this.inputElement.value, false, false); }, 1); 
    });
  }

  /**
   * Adds highlight divs to the quill editor, for each found position of a given substring.
   * @param {string} needle - The substring to search for
   * @param {boolean} createSelection - Create a quill selection if the substring was found and
   * bring it into the visible range.
   * @param {boolean} focusToInput - Give back the focus to the input element, so the user can
   * continue typing.
   */
  searchAndHighlight(needle, createSelection, focusToInput) {
    var _this = this;
    _this.clearSearchHighlights();
    if (!needle || needle.length < 2)
      return;

    needle = needle.toLocaleLowerCase();
    var quillText = _this.getLowerQuillText();

    var findings = [];
    var fromIndex = 0;
    var index = quillText.indexOf(needle, fromIndex);
    while (index >= 0) {
      findings.push(index);
      index = quillText.indexOf(needle, index + 1);
    }

    findings.forEach(function(finding){
      //     _this.quill.setSelection(index, needle.length);
      var bounds = _this.quill.getBounds(finding, needle.length);

      var highlight = document.createElement('div');
      highlight.classList.add('qs-highlight');
      _this.searchHighlights.push(highlight);

      var quillParent = _this.quill.root.parentElement;
      quillParent.appendChild(highlight);

      highlight.style.position = 'absolute';
      highlight.style.width = bounds.width + 'px';
      highlight.style.height = bounds.height + 'px';
      highlight.style.left = bounds.left + 'px';
      highlight.style.top = bounds.top + 'px';
    });

    if (focusToInput) {
      _this.inputElement.focus();
    }
  }

  /**
   * Removes and frees all highlight divs from the quill editor.
   */
  clearSearchHighlights() {
    var _this = this;
    _this.searchHighlights.forEach(function(item, index, array) {
        item.remove();
    })            
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
}