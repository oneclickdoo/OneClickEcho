import { AppRouterInstance } from "next/dist/shared/lib/app-router-context.shared-runtime";
import { withLocale } from "@/lib/routing";

export function navigateWithLocale(
    router: AppRouterInstance,
    locale: string,
    href: string
) {
    router.push(withLocale(locale, href));
}

export function replaceWithLocale(
    router: AppRouterInstance,
    locale: string,
    href: string
) {
    router.replace(withLocale(locale, href));
}