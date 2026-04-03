"use client";
import { useState, useEffect } from "react";
import { useTranslations } from "next-intl";

import { useQuery } from "@tanstack/react-query";

import { useAuth } from "@/context/AuthContext";
import { ShortcutHeader, makeShortcut } from "@/context/ShortcutsContext";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/tremor/Tabs";

import { CollectionLeadsTab } from "@/elements/dashboard/singleCollection/tabs/CollectionLeadsTab";
import { CollectionSettingsTab } from "@/elements/dashboard/singleCollection/tabs/CollectionSettingsTab";
import { CollectionDto } from "@/elements/dashboard/collections/fetchData";
import { getCollectionById } from "@/elements/dashboard/singleCollection/fetchData";

import { downloadExampleLeadCsv } from "@/elements/dashboard/singleCampaign/fetchData";
import { downloadFile } from "@/lib/download";
import { useToast } from "@/lib/useToast";

export default function CollectionPage({ params }: { params: { collectionId: string } }) {
    const t = useTranslations("CollectionPage");

    const [collection, setCollection] = useState<CollectionDto>({
        leadCollectionId: "",
        collectionName: ""
    });

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const downloadExampleCsv = async () => {
        const blob: Blob = await downloadExampleLeadCsv(authFetch);

        downloadFile(blob, "leads.csv");

        toast({
            title: t("toasts.successTitle"),
            description: t("toasts.exampleDownloaded"),
            variant: "success",
            duration: 2000
        });
    };

    const { data: fetchCollection, refetch } = useQuery({
        queryKey: ["data", params.collectionId],
        queryFn: () => getCollectionById(params.collectionId, authFetch),
        enabled: false
    });

    useEffect(() => {
        refetch();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        if (fetchCollection) {
            setCollection(fetchCollection);
        }
    }, [fetchCollection]);

    return (
        <>
            <div className="flex justify-between items-center max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <div className="md:mr-20">
                    <ShortcutHeader
                        shortcut={makeShortcut(
                            t("shortcutTitle", { name: collection.collectionName }),
                            `/collections/${params.collectionId}`
                        )}
                    >
                        <h1 className="text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                            {t("pageTitle", { name: collection.collectionName })}
                        </h1>
                    </ShortcutHeader>
                </div>
            </div>

            <p
                className="mb-2 text-indigo-400 cursor-pointer hover:underline hover:text-indigo-500"
                onClick={downloadExampleCsv}
            >
                {t("downloadExample")}
            </p>

            <Tabs defaultValue="tab1" className="mt-4 sm:mt-6 lg:mt-10">
                <TabsList variant="line">
                    <TabsTrigger value="tab1">{t("tabs.leads")}</TabsTrigger>
                    <TabsTrigger value="tab2">{t("tabs.settings")}</TabsTrigger>
                </TabsList>

                {fetchCollection ? (
                    <div className="pt-6">
                        <TabsContent value="tab1">
                            <CollectionLeadsTab collectionId={params.collectionId} />
                        </TabsContent>
                        <TabsContent value="tab2">
                            <CollectionSettingsTab collection={collection} setCollection={setCollection} />
                        </TabsContent>
                    </div>
                ) : null}
            </Tabs>
        </>
    );
}
