"use client";
import { useTranslations } from "next-intl";

import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/tremor/Tabs";
import { SettingsGeneralTab } from "@/elements/dashboard/settings/tabs/SettingsGeneralTab";

export default function SettingsPage() {
    const t = useTranslations("SettingsPage");

    return (
        <div className="flex flex-col">
            <div className="flex items-center justify-between max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <ShortcutHeader shortcut={makeShortcut(t("title"), "/settings")}>
                    <h1 className="ml-4 md:ml-0 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                        {t("title")}
                    </h1>
                </ShortcutHeader>
            </div>

            <Tabs defaultValue="tab1" className="mt-4 sm:mt-6 lg:mt-10">
                <TabsList variant="line">
                    <TabsTrigger value="tab1">{t("tabs.general")}</TabsTrigger>
                </TabsList>

                <div className="pt-6">
                    <TabsContent value="tab1">
                        <SettingsGeneralTab />
                    </TabsContent>
                </div>
            </Tabs>
        </div>
    );
}
