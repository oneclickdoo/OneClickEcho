"use client";

import { useState, useEffect, ReactNode } from "react";
import Link from "next/link";
import { useTheme } from "next-themes";
import { RiArrowRightUpLine, RiComputerLine, RiMoonLine, RiSunLine } from "@remixicon/react";
import { useTranslations, useLocale } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuGroup,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuRadioGroup,
    DropdownMenuRadioItem,
    DropdownMenuSeparator,
    DropdownMenuSubMenu,
    DropdownMenuSubMenuContent,
    DropdownMenuSubMenuTrigger,
    DropdownMenuTrigger
} from "@/components/tremor/Dropdown";

export type DropdownUserProfileProps = {
    children: ReactNode;
    align?: "center" | "start" | "end";
};

export function DropdownUserProfile({ children, align = "start" }: DropdownUserProfileProps) {
    const t = useTranslations("Common");
    const locale = useLocale();

    const [mounted, setMounted] = useState(false);

    const { user, logout } = useAuth();
    const { theme, setTheme } = useTheme();

    useEffect(() => {
        setMounted(true);
    }, []);

    if (!mounted) {
        return null;
    }

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>{children}</DropdownMenuTrigger>
            <DropdownMenuContent align={align}>
                <DropdownMenuLabel>{user?.email}</DropdownMenuLabel>

                <DropdownMenuGroup>
                    <DropdownMenuSubMenu>
                        <DropdownMenuSubMenuTrigger>{t("theme")}</DropdownMenuSubMenuTrigger>
                        <DropdownMenuSubMenuContent>
                            <DropdownMenuRadioGroup value={theme} onValueChange={setTheme}>
                                <DropdownMenuRadioItem aria-label={t("switchToLightMode")} value="light" iconType="check">
                                    <RiSunLine className="size-4 shrink-0" aria-hidden />
                                    {t("light")}
                                </DropdownMenuRadioItem>

                                <DropdownMenuRadioItem aria-label={t("switchToDarkMode")} value="dark" iconType="check">
                                    <RiMoonLine className="size-4 shrink-0" aria-hidden />
                                    {t("dark")}
                                </DropdownMenuRadioItem>

                                <DropdownMenuRadioItem aria-label={t("switchToSystemMode")} value="system" iconType="check">
                                    <RiComputerLine className="size-4 shrink-0" aria-hidden />
                                    {t("system")}
                                </DropdownMenuRadioItem>
                            </DropdownMenuRadioGroup>
                        </DropdownMenuSubMenuContent>
                    </DropdownMenuSubMenu>
                </DropdownMenuGroup>

                <DropdownMenuSeparator />

                <DropdownMenuGroup>
                    <Link href={`/${locale}/settings`}>
                        <DropdownMenuItem>{t("settings")}</DropdownMenuItem>
                    </Link>

                    <DropdownMenuItem disabled>
                        {t("presentation")}
                        <RiArrowRightUpLine className="mb-1 ml-1 size-2.5 shrink-0 text-gray-500" aria-hidden />
                    </DropdownMenuItem>
                </DropdownMenuGroup>

                <DropdownMenuSeparator />

                <DropdownMenuGroup>
                    <DropdownMenuItem onClick={logout}>{t("signOut")}</DropdownMenuItem>
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
