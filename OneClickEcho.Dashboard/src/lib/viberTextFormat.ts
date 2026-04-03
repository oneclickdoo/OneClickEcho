/**
 * Viber Business messages use markdown (14.6+), not HTML in MessageText:
 * *bold*, _italic_, ~strikethrough~, ```monospace```
 * @see https://developers.viber.com/docs/tools/text-formatting/
 */

const maxNestedPasses = 8;

/**
 * Converts legacy <b>, <i>, <u> from the editor into Viber markdown.
 * Runs repeatedly to handle simple nesting (e.g. <b><i>x</i></b>).
 */
export function migrateLegacyViberHtmlToMarkdown(text: string | undefined | null): string | undefined {
    if (text == null || text === "") {
        return text ?? undefined;
    }

    if (!/<\s*\/?\s*[biu]\s*>/i.test(text)) {
        return text;
    }

    let out = text;
    for (let p = 0; p < maxNestedPasses; p++) {
        const next = out
            .replace(/<b>([\s\S]*?)<\/b>/gi, "*$1*")
            .replace(/<i>([\s\S]*?)<\/i>/gi, "_$1_")
            .replace(/<u>([\s\S]*?)<\/u>/gi, "$1");
        if (next === out) {
            break;
        }
        out = next;
    }

    return out;
}

/**
 * Read-only preview: escape HTML, then map Viber markdown to simple tags.
 */
export function viberMarkdownToPreviewHtml(raw: string): string {
    let s = raw
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");

    s = s.replace(/```([\s\S]*?)```/g, "<code class=\"rounded bg-gray-100 px-1 dark:bg-gray-800\">$1</code>");
    s = s.replace(/\*([^*]+)\*/g, "<strong>$1</strong>");
    s = s.replace(/_([^_]+)_/g, "<em>$1</em>");
    s = s.replace(/~([^~]+)~/g, "<del>$1</del>");
    s = s.replace(/\n/g, "<br />");
    return s;
}
