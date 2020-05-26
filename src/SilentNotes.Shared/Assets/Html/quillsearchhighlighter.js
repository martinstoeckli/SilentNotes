class QuillSearchHighlighter {
  quill = null;
  inputElement = null;
  searchHighlights = [];

  constructor(quill, inputElement) {
    this.quill = quill;
    this.inputElement = inputElement;
  }

  startListening() {
    var self = this;
    var timer; // scroll and resize events are called several times, the timer avoids duplicates

    this.inputElement.addEventListener('input', function(e) {
      window.clearTimeout(timer);
      timer = window.setTimeout(
        function() { self.searchAndHighlight(self.inputElement.value, true); }, 1); 
    });

    this.quill.root.addEventListener('scroll', function(e) {
      window.clearTimeout(timer);
      timer = window.setTimeout(
        function() { self.searchAndHighlight(self.inputElement.value, true); }, 1); 
    });

    window.addEventListener('resize', function(e) {
      window.clearTimeout(timer);
      timer = window.setTimeout(
        function() { self.searchAndHighlight(self.inputElement.value, true); }, 1); 
    });
  }

  searchAndHighlight(needle, focusToInput) {
    this.clearSearchHighlights();
    var quillText = this.getQuillText();

    var index = quillText.indexOf(needle);
    if (index >= 0) {
        this.quill.setSelection(index, needle.length);
        var bounds = this.quill.getBounds(index, needle.length);

        var highlight = document.createElement('div');
        highlight.classList.add('qs-highlight');
        this.searchHighlights.push(highlight);

        var quillParent = this.quill.root.parentElement;
        quillParent.appendChild(highlight);

        highlight.style.position = 'absolute';
        highlight.style.width = bounds.width + 'px';
        highlight.style.height = bounds.height + 'px';
        highlight.style.left = bounds.left + 'px';
        highlight.style.top = bounds.top + 'px';
    }
    if (focusToInput) {
      this.inputElement.focus();
    }
  }

  clearSearchHighlights() {
    this.searchHighlights.forEach(function(item, index, array) {
        item.remove();
    })            
  }

  getQuillText() {
    return this.quill.getContents().filter(function (op) {
      return typeof op.insert === 'string' || op.insert.image;
    }).map(function (op) {
      if (op.insert.image) {
          return op.insert.image = ' ';
      }
      return op.insert;
    }).join('');
  }
}