"use client";

import { useState, useRef } from "react";
import { useRouter, useParams } from "next/navigation";
import { useTranslations } from "next-intl";
import { withLocale } from "@/lib/routing";

import { RiArrowRightSLine, RiExpandUpDownLine } from "@remixicon/react";

import { ICompany, IDashboardManager } from "@/context/AuthContext";

import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuGroup,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuTrigger
} from "@/components/tremor/Dropdown";

import { cx, focusInput } from "@/lib/utils";

const getInitials = (name?: string | null) => {
    const safe = (name ?? "").trim();
    return (safe.slice(0, 2) || "--").toUpperCase();
};

const isAdminCompany = (name?: string | null) => name === "Admin Dashboard";

export const WorkspacesDropdownDesktop = ({
    workspaces,
    setCurrentCompany
}: {
    workspaces: IDashboardManager | null;
    setCurrentCompany: any;
}) => {
    const t = useTranslations("Sidebar");
    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const [dropdownOpen, setDropdownOpen] = useState(false);
    const [hasOpenDialog, _] = useState(false);

    const focusRef = useRef<null | HTMLButtonElement>(null);
    const router = useRouter();

    const currentName = workspaces?.currentCompany?.name;

    return (
        <DropdownMenu open={dropdownOpen} onOpenChange={setDropdownOpen} modal={false}>
            <DropdownMenuTrigger asChild>
                <button
                    className={cx(
                        "flex w-full items-center gap-x-2.5 rounded-md border border-gray-300 bg-white p-2 text-sm shadow-sm transition-all hover:bg-gray-50 dark:border-gray-800 dark:bg-gray-950 hover:dark:bg-gray-900",
                        focusInput
                    )}
                >
                    <span
                        className="flex aspect-square size-8 items-center justify-center rounded bg-indigo-600 p-2 text-xs font-medium text-white dark:bg-indigo-500"
                        aria-hidden
                    >
                        {getInitials(currentName)}
                    </span>

                    <div className="flex w-full items-center justify-between gap-x-4 truncate">
                        <div className="truncate">
                            <p className="truncate whitespace-nowrap text-sm font-medium text-gray-900 dark:text-gray-50">
                                {currentName}
                            </p>
                            <p className="whitespace-nowrap text-left text-xs text-gray-700 dark:text-gray-300">
                                {isAdminCompany(currentName) ? t("admin") : t("member")}
                            </p>
                        </div>

                        <RiExpandUpDownLine className="size-5 shrink-0 text-gray-500" aria-hidden />
                    </div>
                </button>
            </DropdownMenuTrigger>

            <DropdownMenuContent
                hidden={hasOpenDialog}
                onCloseAutoFocus={(event) => {
                    if (focusRef.current) {
                        focusRef.current.focus();
                        focusRef.current = null;
                        event.preventDefault();
                    }
                }}
            >
                <DropdownMenuGroup>
                    <DropdownMenuLabel>
                        {t("workspaces")} ({workspaces?.companies.length ?? 0})
                    </DropdownMenuLabel>

                    {workspaces?.companies.map((company: ICompany, index) => (
                        <DropdownMenuItem
                            key={company.companyId ?? `admin-${index}`}
                            onClick={() => {
                                setCurrentCompany(company);
                                router.push(withLocale(locale, "/overview"));
                            }}
                        >
                            <div className="flex w-full items-center gap-x-2.5">
                                <span
                                    className={cx(
                                        "bg-indigo-600 dark:bg-indigo-500",
                                        "flex aspect-square size-8 items-center justify-center rounded p-2 text-xs font-medium text-white"
                                    )}
                                    aria-hidden
                                >
                                    {getInitials(company.name)}
                                </span>
                                <div>
                                    <p className="text-sm font-medium text-gray-900 dark:text-gray-50">{company.name}</p>
                                    <p className="text-xs text-gray-700 dark:text-gray-400">
                                        {isAdminCompany(company.name) ? t("admin") : t("member")}
                                    </p>
                                </div>
                            </div>
                        </DropdownMenuItem>
                    ))}
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    );
};

export const WorkspacesDropdownMobile = ({
    workspaces,
    setCurrentCompany
}: {
    workspaces: IDashboardManager | null;
    setCurrentCompany: any;
}) => {
    const t = useTranslations("Sidebar");
    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const [dropdownOpen, setDropdownOpen] = useState(false);
    const [hasOpenDialog, _] = useState(false);

    const focusRef = useRef<null | HTMLButtonElement>(null);
    const router = useRouter();

    const currentName = workspaces?.currentCompany?.name;

    return (
        <DropdownMenu open={dropdownOpen} onOpenChange={setDropdownOpen} modal={false}>
            <DropdownMenuTrigger asChild>
                <button className="flex items-center gap-x-1.5 rounded-md p-2 hover:bg-gray-100 focus:outline-none hover:dark:bg-gray-900">
                    <span
                        className={cx(
                            "flex aspect-square size-7 items-center justify-center rounded bg-indigo-600 p-2 text-xs font-medium text-white dark:bg-indigo-500"
                        )}
                        aria-hidden
                    >
                        {getInitials(currentName)}
                    </span>

                    <RiArrowRightSLine className="size-4 shrink-0 text-gray-500" aria-hidden />

                    <div className="flex w-full items-center justify-between gap-x-3 truncate">
                        <p className="truncate whitespace-nowrap text-sm font-medium text-gray-900 dark:text-gray-50">
                            {currentName}
                        </p>
                        <RiExpandUpDownLine className="size-4 shrink-0 text-gray-500" aria-hidden />
                    </div>
                </button>
            </DropdownMenuTrigger>

            <DropdownMenuContent
                className="!min-w-72"
                hidden={hasOpenDialog}
                onCloseAutoFocus={(event) => {
                    if (focusRef.current) {
                        focusRef.current.focus();
                        focusRef.current = null;
                        event.preventDefault();
                    }
                }}
            >
                <DropdownMenuGroup>
                    <DropdownMenuLabel>
                        {t("workspaces")} ({workspaces?.companies?.length ?? 0})
                    </DropdownMenuLabel>

                    {workspaces?.companies?.map((company: any, index) => (
                        <DropdownMenuItem
                            key={company.companyId ?? company.id ?? `admin-${index}`}
                            onClick={() => {
                                setCurrentCompany(company);
                                router.push(withLocale(locale, "/overview"));
                            }}
                        >
                            <div className="flex w-full items-center gap-x-2.5">
                                <span
                                    className={cx(
                                        "bg-indigo-600 dark:bg-indigo-500",
                                        "flex size-8 items-center justify-center rounded p-2 text-xs font-medium text-white"
                                    )}
                                    aria-hidden
                                >
                                    {getInitials(company.name)}
                                </span>
                                <div>
                                    <p className="text-sm font-medium text-gray-900 dark:text-gray-50">{company.name}</p>
                                    <p className="text-xs text-gray-700 dark:text-gray-300">
                                        {isAdminCompany(company.name) ? t("admin") : t("member")}
                                    </p>
                                </div>
                            </div>
                        </DropdownMenuItem>
                    ))}
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    );
};
