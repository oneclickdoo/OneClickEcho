"use client";

import type { ReactNode } from "react";
import { useEffect } from "react";
import { useLocale } from "next-intl";

import { registerLocale, setDefaultLocale } from "react-datepicker";

// ✅ named exports (nema default export u tvom date-fns build-u)
import { enGB } from "date-fns/locale/en-GB";
import { srLatn } from "date-fns/locale/sr-Latn";

// ✅ registruj jednom (na nivou modula)
registerLocale("en", enGB);
registerLocale("sr", srLatn);

function normalizeDatepickerLocale(locale: string): "en" | "sr" {
    // prihvati i varijante tipa "sr-Latn", "sr_RS", "en-GB", itd.
    if (locale.toLowerCase().startsWith("sr")) return "sr";
    return "en";
}

export default function LocaleProviders({ children }: { children: ReactNode }) {
    const locale = useLocale();

    useEffect(() => {
        setDefaultLocale(normalizeDatepickerLocale(locale));
    }, [locale]);

    return <>{children}</>;
}
