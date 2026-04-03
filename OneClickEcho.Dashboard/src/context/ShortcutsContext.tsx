"use client";

import { useState, useContext, createContext } from "react";
import { usePathname } from "next/navigation";
import Link from "next/link";
import { useLocale, useTranslations } from "next-intl";

import { RemixiconComponentType, RiCloseLine, RiLinkM, RiPushpin2Line } from "@remixicon/react";
import { cx, focusRing } from "@/lib/utils";

export const ShortcutsContext = createContext<IShortcutsContext>({} as IShortcutsContext);

export interface IShortcutsContext {
    shortcuts: IShortcut[];
    addShortcut: (shortcut: IShortcut) => void;
    removeShortcut: (shortcut: IShortcut) => void;
}

export interface IShortcut {
    guid: string;
    name: string;
    href: string;
    icon: RemixiconComponentType;
}

const newGuid = () => {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
        var r = (Math.random() * 16) | 0,
            v = c == "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
};

const ensureLocaleHref = (href: string, locale: string) => {
    // već locale-aware
    if (href.startsWith(`/${locale}/`)) return href;

    // ako je već neki drugi locale /en/... ili /sr/..., ostavi ga (ne možemo pouzdano prepoznati sve locale-ove bez liste)
    // ali u tvojoj app su en/sr, pa je dovoljno:
    if (href.startsWith("/en/") || href.startsWith("/sr/")) return href;

    // normalizuj
    const normalized = href.startsWith("/") ? href : `/${href}`;
    return `/${locale}${normalized}`;
};

export const makeShortcut = (name: string, href: string, icon: RemixiconComponentType = RiLinkM): IShortcut => {
    // NOTE: ovo je "pure" funkcija bez locale-a.
    // Locale ćemo dodati pri pozivu (u ShortcutHeader) da href uvek bude ispravan.
    return {
        guid: newGuid(),
        name,
        href,
        icon
    } as IShortcut;
};

// TODO: Icon wont load because of the bad why its saved in the local storage. Currently, we only use the default
export const ShortcutsProvider = ({ children }: { children: JSX.Element | JSX.Element[] }) => {
    const [shortcuts, setShortcuts] = useState<IShortcut[]>(() => {
        try {
            const storedShortcuts = localStorage.getItem("shortcuts");
            return storedShortcuts ? JSON.parse(storedShortcuts) : [];
        } catch (e) {
            return [];
        }
    });

    const addShortcut = (shortcut: IShortcut): void => {
        if (shortcuts.find((item) => item.href === shortcut.href)) {
            return;
        }

        const updatedShortcuts = [...shortcuts, shortcut];

        setShortcuts(updatedShortcuts);

        localStorage.setItem("shortcuts", JSON.stringify(updatedShortcuts));
    };

    const removeShortcut = (shortcut: IShortcut): void => {
        const updatedShortcuts = shortcuts.filter((item) => item.guid !== shortcut.guid);

        setShortcuts(updatedShortcuts);

        localStorage.setItem("shortcuts", JSON.stringify(updatedShortcuts));
    };

    return <ShortcutsContext.Provider value={{ shortcuts, addShortcut, removeShortcut }}>{children}</ShortcutsContext.Provider>;
};

export const ShortcutHeader = ({ shortcut, children }: { shortcut: IShortcut; children: JSX.Element | JSX.Element[] }) => {
    const { addShortcut } = useShortcuts();
    const [showPin, setShowPin] = useState(false);

    const locale = useLocale();

    return (
        <div
            className="inline-flex items-center float-start"
            onMouseEnter={() => setTimeout(() => setShowPin(true), 200)}
            onMouseLeave={() => setShowPin(false)}
        >
            {children}
            <button
                onClick={() =>
                    addShortcut({
                        ...shortcut,
                        href: ensureLocaleHref(shortcut.href, locale)
                    })
                }
                className="ml-1.5 text-gray-500 hover:text-gray-800 dark:hover:text-gray-200"
            >
                <RiPushpin2Line
                    className={cx("w-4 h-4", "transition-opacity duration-100 ease-in-out", showPin ? "opacity-100" : "opacity-0")}
                />
            </button>
        </div>
    );
};

export const ShortcutListItem = ({ shortcut }: { shortcut: IShortcut }) => {
    const tCommon = useTranslations("Common");
    const locale = useLocale();

    const [showPin, setShowPin] = useState(false);
    const pathname = usePathname();

    const { removeShortcut } = useShortcuts();

    const href = ensureLocaleHref(shortcut.href, locale);

    return (
        <li
            key={shortcut.name}
            className="flex justify-between items-center gap-x-0.5"
            onMouseEnter={() => setShowPin(true)}
            onMouseLeave={() => setShowPin(false)}
        >
            <Link
                href={href}
                className={cx(
                    "w-full",
                    pathname === href || pathname.startsWith(href)
                        ? "text-indigo-600 dark:text-indigo-400"
                        : "text-gray-700 hover:text-gray-900 dark:text-gray-400 hover:dark:text-gray-50",
                    "flex items-center gap-x-2.5 rounded-md px-2 py-1.5 text-sm font-medium transition hover:bg-gray-100 hover:dark:bg-gray-900",
                    focusRing
                )}
            >
                <div className="flex items-center gap-x-0.5 w-full">
                    <div>
                        {shortcut.icon ? (
                            <shortcut.icon className="w-4 h-4 mr-1.5 text-gray-500" />
                        ) : (
                            <RiLinkM className="size-4 min-w-4 min-h-4 mr-1.5 text-gray-500" />
                        )}
                    </div>

                    <div className="line-clamp-2">{shortcut.name}</div>
                </div>
            </Link>

            <div
                className={cx(
                    "cursor-pointer",
                    pathname === href ? "text-indigo-600 dark:text-indigo-400" : "text-gray-700 hover:text-gray-900 dark:text-gray-400 hover:dark:text-gray-50",
                    "flex items-center rounded-md p-2 text-sm font-medium transition hover:bg-gray-100 hover:dark:bg-gray-900",
                    focusRing
                )}
                onClick={() => removeShortcut(shortcut)}
            >
                <button
                    aria-label={tCommon("shortcuts.remove")}
                    className={cx(
                        "w-4 h-4 text-red-500 hover:text-red-600",
                        "transition-opacity duration-100 ease-in-out",
                        showPin ? "opacity-100" : "opacity-0"
                    )}
                >
                    <RiCloseLine className="w-4 h-4" />
                </button>
            </div>
        </li>
    );
};

export default ShortcutsContext;

export const useShortcuts = () => useContext(ShortcutsContext);
