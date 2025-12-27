import { TiptapMarkdownConverter } from '../prose-mirror-bundle.js';

let _markdownConverter;

export function convertHtmlToMarkdown(htmlContent) {
    if (!_markdownConverter)
        _markdownConverter = new TiptapMarkdownConverter();
    return _markdownConverter.convertHtmlToMarkdown(htmlContent);
}
