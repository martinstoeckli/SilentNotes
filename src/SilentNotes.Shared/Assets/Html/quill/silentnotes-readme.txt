The quill.min.js file was slightliy modified, so the link tooltip editor is not placed outside of the editor when editing a link on the topmost line. The changed code looks like this in quill.js:


/////////////////////////////////////////
// quill.js Tooltip function position()

var verticalShift = reference.bottom - reference.top + height;
...
// instead of placing the link editor above and outside of visible scope, place it below
if (top - verticalShift < 0) {
  verticalShift = -16; // counter css ql-flip transform
}
...
this.root.style.top = top - verticalShift + 'px';


/////////////////////////////////////////
// quill.min.js [line:7 char:108150]

if(n-a<0){a=-16;}
