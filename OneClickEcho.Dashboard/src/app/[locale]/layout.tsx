import { ReactNode } from "react";
import { NextIntlClientProvider } from "next-intl";
import LocaleProviders from "./providers";

export default async function LocaleLayout({
    children,
    params
}: {
    children: ReactNode;
    params: { locale: "en" | "sr" };
}) {
    const messages = (await import(`../../../messages/${params.locale}.json`)).default;

    return (
        <NextIntlClientProvider locale={params.locale} messages={messages}>
            <LocaleProviders>{children}</LocaleProviders>
        </NextIntlClientProvider>
    );
}
