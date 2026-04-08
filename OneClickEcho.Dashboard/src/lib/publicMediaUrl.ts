/**
 * URL in the browser for a campaign file stored as a name in UploadedFiles (served at /uploads/{name}).
 * NEXT_PUBLIC_API_URL is often https://host/api — appending /uploads there yields a wrong /api/uploads/... path.
 */
export function publicUploadFileUrl(storedFileNameOrUrl: string): string {
    const s = storedFileNameOrUrl.trim();
    if (!s) {
        return "";
    }
    if (s.startsWith("http://") || s.startsWith("https://")) {
        return s;
    }
    return `/uploads/${encodeURIComponent(s)}`;
}
