export function withLocale(locale: string, href: string): string {
    const cleanLocale = locale.replace(/^\/+|\/+$/g, "");
    const normalized = href.startsWith("/") ? href : `/${href}`;
    return `/${cleanLocale}${normalized}`;
}