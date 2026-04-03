import { redirect } from "next/navigation";
import { withLocale } from "@/lib/routing";

export function redirectWithLocale(locale: string, href: string): never {
    redirect(withLocale(locale, href));
}