/**
 * HttpOnly session cookies must use Secure on HTTPS in production.
 * Set COOKIE_SECURE=true if NODE_ENV is not "production" but the site is served over HTTPS (e.g. some Docker setups).
 */
export function useSecureSessionCookies(): boolean {
    if (process.env.COOKIE_SECURE === "false") {
        return false;
    }

    if (process.env.COOKIE_SECURE === "true") {
        return true;
    }

    return process.env.NODE_ENV === "production";
}
