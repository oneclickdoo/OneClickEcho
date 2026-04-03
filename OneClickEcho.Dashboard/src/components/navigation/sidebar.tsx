"use client";

import { usePathname, useParams } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import { useTranslations } from "next-intl";
import { withLocale } from "@/lib/routing";

import {
    RemixiconComponentType,
    RiBuildingLine,
    RiGroupLine,
    RiHome2Line,
    RiListCheck,
    RiMessage2Line,
    RiStackLine
} from "@remixicon/react";

import { useAuth } from "@/context/AuthContext";
import { ShortcutListItem, useShortcuts } from "@/context/ShortcutsContext";

import MobileSidebar from "./MobileSidebar";
import { WorkspacesDropdownDesktop, WorkspacesDropdownMobile } from "./SidebarWorkspacesDropdown";
import { UserProfileDesktop, UserProfileMobile } from "./UserProfile";

import { cx, focusRing } from "@/lib/utils";
import { siteConfig } from "@/app/siteConfig";

type SidebarLabelKey = "overview" | "campaigns" | "leadLists" | "leads" | "apiMessages" | "companies";

export interface SidebarNavigationItem {
    labelKey: SidebarLabelKey;
    href: string; // base href (bez locale-a), npr "/overview"
    isAdminLink: boolean | null;
    icon: RemixiconComponentType;
}

const navigation: SidebarNavigationItem[] = [
    { labelKey: "overview", href: siteConfig.baseLinks.overview, icon: RiHome2Line, isAdminLink: null },
    { labelKey: "campaigns", href: siteConfig.baseLinks.campaigns, icon: RiListCheck, isAdminLink: false },
    { labelKey: "leadLists", href: siteConfig.baseLinks.collections, icon: RiStackLine, isAdminLink: false },
    { labelKey: "leads", href: siteConfig.baseLinks.leads, icon: RiGroupLine, isAdminLink: false },
    { labelKey: "apiMessages", href: siteConfig.baseLinks.api_messages, icon: RiMessage2Line, isAdminLink: false },
    { labelKey: "companies", href: siteConfig.baseLinks.companies, icon: RiBuildingLine, isAdminLink: true }
];

export function Sidebar() {
    const t = useTranslations("Sidebar");

    const { dashboardManager, setCurrentCompany } = useAuth();
    const pathname = usePathname();

    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const { shortcuts } = useShortcuts();

    const isActive = (baseHref: string) => {
        const fullHref = withLocale(locale, baseHref);
        return pathname === fullHref || pathname.startsWith(`${fullHref}/`);
    };

    return (
        <>
            {/* sidebar (lg+) */}
            <nav className="hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:w-72 lg:flex-col">
                <aside className="flex grow flex-col gap-y-6 overflow-y-auto border-r border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-950">
                    <Image
                        className="mx-auto my-auto w-auto text-center mt-4 h-10 dark:hidden"
                        width={100}
                        height={100}
                        src="/oneclick_echo_novi.svg"
                        alt="OneClick Echo Logo"
                    />
                    <Image
                        className="mx-auto my-auto w-auto text-center mt-4 h-10 hidden dark:block"
                        width={100}
                        height={100}
                        src="/oneclick_echo_novi_dark_theme.svg"
                        alt="OneClick Echo Logo"
                    />

                    <WorkspacesDropdownDesktop workspaces={dashboardManager} setCurrentCompany={setCurrentCompany} />

                    <nav aria-label={t("coreNavigation")} className="flex flex-1 flex-col space-y-10">
                        <ul role="list" className="space-y-0.5">
                            {navigation.map((item) => {
                                const allowed =
                                    item.isAdminLink === null ||
                                    (dashboardManager?.currentCompany?.companyId === null) === item.isAdminLink;

                                if (!allowed) return null;

                                const href = withLocale(locale, item.href);

                                return (
                                    <li key={item.labelKey}>
                                        <Link
                                            href={href}
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
                                    </li>
                                );
                            })}
                        </ul>

                        {shortcuts.length > 0 && (
                            <div>
                                <span className="text-xs font-medium leading-6 text-gray-500">{t("shortcuts")}</span>
                                <ul aria-label={t("shortcutsAria")} role="list" className="space-y-0.5">
                                    {shortcuts.map((shortcut) => (
                                        <ShortcutListItem key={(shortcut as any)?.id ?? shortcut.href} shortcut={shortcut} />
                                    ))}
                                </ul>
                            </div>
                        )}
                    </nav>

                    <div className="mt-auto">
                        <UserProfileDesktop />
                    </div>
                </aside>
            </nav>

            {/* top navbar (xs-lg) */}
            <div className="sticky top-0 z-40 flex h-16 shrink-0 items-center justify-between border-b border-gray-200 bg-white px-2 shadow-sm sm:gap-x-6 sm:px-4 lg:hidden dark:border-gray-800 dark:bg-gray-950">
                <WorkspacesDropdownMobile workspaces={dashboardManager} setCurrentCompany={setCurrentCompany} />
                <div className="flex items-center gap-1 sm:gap-2">
                    <UserProfileMobile />
                    <MobileSidebar
                        navigation={navigation.map((n) => ({ ...n, href: withLocale(locale, n.href) }))}
                        dashboardManager={dashboardManager}
                    />
                </div>
            </div>
        </>
    );
}
