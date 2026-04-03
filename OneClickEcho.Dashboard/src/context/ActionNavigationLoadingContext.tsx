"use client";

import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { usePathname } from "next/navigation";
import { useTranslations } from "next-intl";

type ActionNavigationLoadingContextValue = {
    beginActionLoading: () => void;
    endActionLoading: () => void;
};

const ActionNavigationLoadingContext = createContext<ActionNavigationLoadingContextValue | null>(null);

/**
 * Full-screen loading overlay + wait cursor for slow client navigations (e.g. after create campaign).
 * Clears automatically when the URL pathname changes. Call `endActionLoading` if navigation might not happen.
 */
export function ActionNavigationLoadingProvider({ children }: { children: ReactNode }) {
    const [active, setActive] = useState(false);
    const pathname = usePathname();
    const t = useTranslations("Common");

    useEffect(() => {
        setActive(false);
    }, [pathname]);

    const beginActionLoading = useCallback(() => setActive(true), []);
    const endActionLoading = useCallback(() => setActive(false), []);

    return (
        <ActionNavigationLoadingContext.Provider value={{ beginActionLoading, endActionLoading }}>
            {active ? (
                <div
                    role="status"
                    aria-live="polite"
                    aria-busy="true"
                    className="fixed inset-0 z-[200] flex cursor-wait items-center justify-center bg-black/45 backdrop-blur-[2px]"
                >
                    <div className="cursor-wait rounded-lg bg-white px-8 py-5 text-base font-medium text-gray-900 shadow-xl dark:bg-gray-900 dark:text-gray-100">
                        {t("loading")}
                    </div>
                </div>
            ) : null}
            {children}
        </ActionNavigationLoadingContext.Provider>
    );
}

export function useActionNavigationLoading(): ActionNavigationLoadingContextValue {
    const ctx = useContext(ActionNavigationLoadingContext);
    if (!ctx) {
        throw new Error("useActionNavigationLoading must be used within ActionNavigationLoadingProvider");
    }
    return ctx;
}
