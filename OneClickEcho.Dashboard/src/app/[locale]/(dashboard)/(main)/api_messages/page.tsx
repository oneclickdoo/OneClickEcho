"use client";

import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";
import { useRef } from "react";
import { useTranslations } from "next-intl";

import { ApiMessagesTable } from "@/elements/dashboard/api_messages/tables/ApiMessagesTable";

export default function ApiMessagesPage() {
    const t = useTranslations("ApiMessages");
    const tableRef = useRef<{ refetchData: () => void } | null>(null);

    return (
        <div className="flex flex-col">
            <div className="flex items-center justify-between max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50 mb-2">
                <ShortcutHeader shortcut={makeShortcut(t("title"), "/api_messages")}>
                    <h1 className="ml-4 md:ml-0 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                        {t("title")}
                    </h1>
                </ShortcutHeader>
            </div>

            <ApiMessagesTable ref={tableRef} />
        </div>
    );
}
