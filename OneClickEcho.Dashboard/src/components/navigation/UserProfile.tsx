"use client";

import { RiMore2Fill } from "@remixicon/react";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { DropdownUserProfile } from "./DropdownUserProfile";

import { Button } from "@/components/tremor/Button";
import { cx, focusRing } from "@/lib/utils";

const getInitial = (email?: string | null) => {
    const c = (email ?? "").trim().charAt(0);
    return (c || "?").toUpperCase();
};

export const UserProfileDesktop = () => {
    const t = useTranslations("Common");
    const { user } = useAuth();

    return (
        <DropdownUserProfile>
            <Button
                aria-label={t("userSettings")}
                variant="ghost"
                className={cx(
                    focusRing,
                    "group flex w-full items-center justify-between rounded-md p-2 text-sm font-medium text-gray-900 hover:bg-gray-100 data-[state=open]:bg-gray-100 data-[state=open]:bg-gray-400/10 hover:dark:bg-gray-400/10"
                )}
            >
                <span className="flex items-center gap-3">
                    <span
                        className="flex size-8 shrink-0 items-center justify-center rounded-full border border-gray-300 bg-white text-xs text-gray-700 dark:border-gray-800 dark:bg-gray-950 dark:text-gray-300"
                        aria-hidden
                    >
                        {getInitial(user?.email)}
                    </span>
                    <span className="w-40 overflow-hidden overflow-ellipsis">{user?.email}</span>
                </span>
                <RiMore2Fill
                    className="size-4 shrink-0 text-gray-500 group-hover:text-gray-700 group-hover:dark:text-gray-400"
                    aria-hidden
                />
            </Button>
        </DropdownUserProfile>
    );
};

export const UserProfileMobile = () => {
    const t = useTranslations("Common");
    const { user } = useAuth();

    return (
        <DropdownUserProfile align="end">
            <Button
                aria-label={t("userSettings")}
                variant="ghost"
                className={cx(
                    "group flex items-center rounded-md p-1 text-sm font-medium text-gray-900 hover:bg-gray-100 data-[state=open]:bg-gray-100 data-[state=open]:bg-gray-400/10 hover:dark:bg-gray-400/10"
                )}
            >
                <span
                    className="flex size-7 shrink-0 items-center justify-center rounded-full border border-gray-300 bg-white text-xs text-gray-700 dark:border-gray-800 dark:bg-gray-950 dark:text-gray-300"
                    aria-hidden
                >
                    {getInitial(user?.email)}
                </span>
            </Button>
        </DropdownUserProfile>
    );
};
