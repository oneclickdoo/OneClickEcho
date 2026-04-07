/**
 * Server-side (Route Handlers, Middleware): prefer Docker service URL so Nginx is not required
 * for /api/connect/token. Browser still uses NEXT_PUBLIC_API_URL for image URLs etc.
 */
export function getApiInternalBase(): string {
    const internal = process.env.API_INTERNAL_URL?.trim().replace(/\/$/, "");
    if (internal) {
        return internal;
    }
    return process.env.NEXT_PUBLIC_API_URL?.trim().replace(/\/$/, "") ?? "";
}

export function getConnectTokenUrl(): string {
    const internal = process.env.API_INTERNAL_URL?.trim().replace(/\/$/, "");
    if (internal) {
        return `${internal}/connect/token`;
    }
    const pub = process.env.NEXT_PUBLIC_API_URL?.trim().replace(/\/$/, "") ?? "";
    return `${pub}/connect/token`;
}
