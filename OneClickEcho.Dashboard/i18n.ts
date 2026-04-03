import { getRequestConfig } from "next-intl/server";
import { routing } from "./src/i18n/routing";

export default getRequestConfig(async ({ locale }) => {
    const safeLocale =
        locale && (routing.locales as readonly string[]).includes(locale)
            ? locale
            : routing.defaultLocale;

    return {
        locale: safeLocale,
        messages: (await import(`./messages/${safeLocale}.json`)).default
    };
});
