import { TiptapMarkdownConverter } from '../prose-mirror-bundle.js';

let _markdownConverter;

export function convertMarkdownToHtml(markdownContent) {
    if (!_markdownConverter)
        _markdownConverter = new TiptapMarkdownConverter();
    return _markdownConverter.convertMarkdownToHtml(markdownContent);
}
