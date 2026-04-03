"use client";

import { usePathname } from "next/navigation";
import Link from "next/link";
import { useTranslations } from "next-intl";

import { RiLinkM, RiMenuLine } from "@remixicon/react";

import { IDashboardManager } from "@/context/AuthContext";
import { useShortcuts } from "@/context/ShortcutsContext";

import { Drawer, DrawerBody, DrawerClose, DrawerContent, DrawerHeader, DrawerTitle, DrawerTrigger } from "@/components/tremor/Drawer";
import { SidebarNavigationItem } from "@/components/navigation/sidebar";
import { Button } from "@/components/tremor/Button";

import { cx, focusRing } from "@/lib/utils";

export default function MobileSidebar({
    navigation,
    dashboardManager
}: {
    navigation: SidebarNavigationItem[];
    dashboardManager: IDashboardManager | null;
}) {
    const t = useTranslations("Sidebar");
    const pathname = usePathname();
    const { shortcuts } = useShortcuts();

    const isActive = (itemHref: string) => {
        return pathname === itemHref || pathname.startsWith(`${itemHref}/`);
    };

    return (
        <Drawer>
            <DrawerTrigger asChild>
                <Button
                    variant="ghost"
                    aria-label={t("openSidebar")}
                    className="group flex items-center rounded-md p-2 text-sm font-medium hover:bg-gray-100 data-[state=open]:bg-gray-100 data-[state=open]:bg-gray-400/10 hover:dark:bg-gray-400/10"
                >
                    <RiMenuLine className="size-6 shrink-0 sm:size-5" aria-hidden />
                </Button>
            </DrawerTrigger>

            <DrawerContent className="sm:max-w-lg">
                <DrawerHeader>
                    <DrawerTitle>{t("mobileTitle")}</DrawerTitle>
                </DrawerHeader>

                <DrawerBody>
                    <nav aria-label={t("coreMobileNavigation")} className="flex flex-1 flex-col space-y-10">
                        <ul role="list" className="space-y-0.5">
                            {navigation.map((item) => {
                                const allowed =
                                    item.isAdminLink === null ||
                                    (dashboardManager?.currentCompany?.companyId === null) === item.isAdminLink;

                                if (!allowed) return null;

                                return (
                                    <li key={item.labelKey}>
                                        <DrawerClose asChild>
                                            <Link
                                                href={item.href}
                                                className={cx(
                                                    isActive(item.href)
                                                        ? "text-indigo-600 dark:text-indigo-400"
                                                        : "text-gray-700 hover:text-gray-900 dark:text-gray-400 hover:dark:text-gray-50",
                                                    "flex items-center gap-x-2.5 rounded-md px-2 py-1.5 text-sm font-medium transition hover:bg-gray-100 hover:dark:bg-gray-900",
                                                    focusRing
                                                )}
                                            >
                                                <item.icon className="size-4 shrink-0" aria-hidden />
                                                {t(item.labelKey)}
                                            </Link>
                                        </DrawerClose>
                                    </li>
                                );
                            })}
                        </ul>

                        <div>
                            <span className="text-sm font-medium leading-6 text-gray-500 sm:text-xs">{t("shortcuts")}</span>

                            <ul aria-label={t("shortcutsAria")} role="list" className="space-y-0.5">
                                {shortcuts.map((item) => (
                                    <li key={(item as any)?.id ?? item.href}>
                                        <Link
                                            href={item.href}
                                            className={cx(
                                                pathname === item.href || pathname.includes(item.href)
                                                    ? "text-indigo-600 dark:text-indigo-400"
                                                    : "text-gray-700 hover:text-gray-900 dark:text-gray-400 hover:dark:text-gray-50",
                                                "flex items-center gap-x-2.5 rounded-md px-2 py-1.5 font-medium transition hover:bg-gray-100 sm:text-sm hover:dark:bg-gray-900",
                                                focusRing
                                            )}
                                        >
                                            {item.icon ? (
                                                <item.icon className="size-4 shrink-0" />
                                            ) : (
                                                <RiLinkM className="size-4 shrink-0" />
                                            )}
                                            {item.name}
                                        </Link>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </nav>
                </DrawerBody>
            </DrawerContent>
        </Drawer>
    );
}
