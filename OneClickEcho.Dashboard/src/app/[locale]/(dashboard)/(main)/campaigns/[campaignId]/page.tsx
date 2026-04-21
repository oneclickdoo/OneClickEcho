"use client";

import { useEffect, useMemo, useState } from "react";
import { useTranslations } from "next-intl";

import { useQuery } from "@tanstack/react-query";

import { useAuth } from "@/context/AuthContext";
import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/tremor/Tabs";
import { Badge } from "@/components/tremor/Badge";
import { Button } from "@/components/tremor/Button";

import { CampaignMessagingTab } from "@/elements/dashboard/singleCampaign/tabs/CampaignMessagingTab";
import { CampaignCollectionsTab } from "@/elements/dashboard/singleCampaign/tabs/CampaignCollectionsTab";
import { CampaignTestTab } from "@/elements/dashboard/singleCampaign/tabs/CampaignTestTab";
import { CampaignAnalyticsTab } from "@/elements/dashboard/singleCampaign/tabs/CampaignAnalyticsTab";
import { CampaignSettingsTab } from "@/elements/dashboard/singleCampaign/tabs/CampaignSettingsTab";

import {
    CampaignDto,
    endCampaign,
    getCampaignById,
    launchCampaign,
    pauseCampaign,
    updateCampaign
} from "@/elements/dashboard/singleCampaign/fetchData";

import { getCampaignStatusOptions } from "@/lib/selects";
import { useToast } from "@/lib/useToast";
import { isOutOfWorkingHours, validateCampaignChannels } from "@/lib/utils";
import { migrateLegacyViberHtmlToMarkdown } from "@/lib/viberTextFormat";

import { CampaignSendingType, CampaignStatus, CampaignViberContentKind } from "@/lib/enums";

// ✅ Match what <Badge variant> actually accepts (from TS error message)
type BadgeVariant = "default" | "success" | "warning" | "error" | "neutral";

const isBadgeVariant = (v: unknown): v is BadgeVariant =>
    v === "default" || v === "success" || v === "warning" || v === "error" || v === "neutral";

const ANALYTICS_TAB = "tab4";

export default function CampaignPage({ params }: { params: { campaignId: string } }) {
    const t = useTranslations("CampaignPage");
    const tCommon = useTranslations("Common");

    const [campaign, setCampaign] = useState<CampaignDto>({
        campaignId: "",
        status: CampaignStatus.Draft,
        companyId: "",
        name: "",
        isViber: false,
        fallbackToSMS: false,
        isViberReceivable: false,
        isSms: false,
        sendingType: CampaignSendingType.Immediate,
        // Viber "Validity" = delivery retry window (seconds), not max video length. 86400 = 24h (matches API default).
        viberValidity: 86400,
        viberContentKind: CampaignViberContentKind.Text
    });

    const [currentTab, setCurrentTab] = useState<string>("tab1");
    const [isLoadedLaunch, setIsLoadedLaunch] = useState<boolean>(false);

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const { data: fetchCampaign, refetch } = useQuery({
        queryKey: ["data", params.campaignId],
        queryFn: () => getCampaignById(params.campaignId, authFetch),
        enabled: Boolean(params.campaignId)
    });

    // ✅ next-intl aware options (labels translated from Common.campaignStatus.*)
    const statusOptions = useMemo(() => getCampaignStatusOptions(tCommon), [tCommon]);

    const currentStatusOption = useMemo(() => {
        return statusOptions.find((item) => item.value === campaign.status);
    }, [statusOptions, campaign.status]);

    // ✅ Coerce/select a safe variant for the <Badge /> component
    const badgeVariant: BadgeVariant = useMemo(() => {
        const v = currentStatusOption?.variant;
        return isBadgeVariant(v) ? v : "default";
    }, [currentStatusOption]);

    const handleLaunch = async () => {
        setIsLoadedLaunch(true);

        try {
            // check communication channels
            const ok = validateCampaignChannels(campaign, (message: string) => {
                toast({
                    title: t("toasts.errorTitle"),
                    description: message, // dolazi iz validateCampaignChannels (trenutno EN) - prevodimo kasnije
                    variant: "error",
                    duration: 2000
                });
            });

            if (!ok) {
                setIsLoadedLaunch(false);
                return;
            }

            // if campaign is immediate, check date & time
            if (campaign.sendingType === CampaignSendingType.Immediate) {
                const currentDate = new Date();

                if (campaign.isViber && isOutOfWorkingHours(currentDate)) {
                    toast({
                        title: t("toasts.errorTitle"),
                        description: t("toasts.viberOutOfHours"),
                        variant: "error",
                        duration: 2000
                    });

                    setIsLoadedLaunch(false);
                    return;
                }
            }

            await updateCampaign(campaign, authFetch);
            await launchCampaign(params.campaignId, authFetch);

            const { data: freshCampaign } = await refetch();
            if (freshCampaign) {
                setCampaign({
                    ...freshCampaign,
                    viberMessage: migrateLegacyViberHtmlToMarkdown(freshCampaign.viberMessage)
                });
            }

            setCurrentTab(ANALYTICS_TAB);

            toast({
                title: t("toasts.successTitle"),
                description: t("toasts.launchSuccess"),
                variant: "success",
                duration: 2000
            });
        } catch (e: any) {
            toast({
                title: t("toasts.errorTitle"),
                description: e?.message ?? "",
                variant: "error",
                duration: 2000
            });
        }

        setIsLoadedLaunch(false);
    };

    const handlePause = async () => {
        try {
            await pauseCampaign(params.campaignId, authFetch);
            await refetch();

            toast({
                title: t("toasts.successTitle"),
                description: t("toasts.pauseSuccess"),
                variant: "success",
                duration: 2000
            });
        } catch (e: any) {
            toast({
                title: t("toasts.errorTitle"),
                description: e?.message ?? "",
                variant: "error",
                duration: 2000
            });
        }
    };

    const handleEnd = async () => {
        try {
            await endCampaign(params.campaignId, authFetch);
            await refetch();

            toast({
                title: t("toasts.successTitle"),
                description: t("toasts.endSuccess"),
                variant: "success",
                duration: 2000
            });
        } catch (e: any) {
            toast({
                title: t("toasts.errorTitle"),
                description: e?.message ?? "",
                variant: "error",
                duration: 2000
            });
        }
    };

    const saveCampaign = async () => {
        try {
            await updateCampaign(campaign, authFetch);
            const { data } = await refetch();
            if (data) {
                setCampaign({
                    ...data,
                    viberMessage: migrateLegacyViberHtmlToMarkdown(data.viberMessage)
                });
            }
        } catch (e) {
            console.error(e);
        }
    };

    useEffect(() => {
        if (fetchCampaign) {
            setCampaign({
                ...fetchCampaign,
                viberMessage: migrateLegacyViberHtmlToMarkdown(fetchCampaign.viberMessage)
            });
        }
    }, [fetchCampaign]);

    const campaignTitle = t("pageTitle", { name: campaign.name });

    return (
        <>
            <div className="flex justify-between items-center max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <div className="md:mr-20">
                    <ShortcutHeader
                        shortcut={makeShortcut(
                            t("shortcutTitle", { name: campaign.name }),
                            `/campaigns/${params.campaignId}`
                        )}
                    >
                        <h1 className="text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">{campaignTitle}</h1>
                    </ShortcutHeader>
                </div>

                <div className="flex justify-between items-center max-md:flex-col gap-3">
                    {/* ✅ Use Badge variant prop (not CSS class) */}
                    <Badge variant={badgeVariant} className="rounded-full px-2.5 text-sm">
                        {currentStatusOption?.label ?? t("statusUnknown")}
                    </Badge>

                    {campaign.status === CampaignStatus.InProgress &&
                        campaign.sendingType === CampaignSendingType.ByDateOfBirth ? (
                        <Button
                            type="button"
                            variant="secondary"
                            className="border-transparent bg-red-600 text-white outline-red-500 hover:bg-red-500 disabled:bg-red-100 disabled:text-gray-400 dark:bg-red-500 dark:text-gray-900 dark:outline-red-500 dark:hover:bg-red-600 disabled:dark:bg-red-800 disabled:dark:text-red-400"
                            onClick={handleEnd}
                        >
                            {t("buttons.end")}
                        </Button>
                    ) : null}

                    {campaign.status === CampaignStatus.Queued && campaign.sendingType !== CampaignSendingType.Immediate ? (
                        <Button
                            type="button"
                            variant="secondary"
                            className="border-transparent bg-yellow-600 text-white outline-yellow-500 hover:bg-yellow-500 disabled:bg-yellow-100 disabled:text-gray-400 dark:bg-yellow-500 dark:text-gray-900 dark:outline-yellow-500 dark:hover:bg-yellow-600 disabled:dark:bg-yellow-800 disabled:dark:text-yellow-400"
                            onClick={handlePause}
                        >
                            {t("buttons.pause")}
                        </Button>
                    ) : null}

                    <Button
                        type="button"
                        variant="primary"
                        disabled={campaign.status !== CampaignStatus.Draft || isLoadedLaunch}
                        isLoading={isLoadedLaunch}
                        className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                        onClick={handleLaunch}
                    >
                        {t("buttons.saveLaunch")}
                    </Button>
                </div>
            </div>

            <Tabs
                value={currentTab}
                className="mt-4 sm:mt-6 lg:mt-10"
                onValueChange={async (value) => {
                    if (currentTab === "tab1" && campaign.status === CampaignStatus.Draft) await saveCampaign();
                    setCurrentTab(value);
                }}
            >
                <TabsList variant="line">
                    <TabsTrigger value="tab1">{t("tabs.messaging")}</TabsTrigger>
                    <TabsTrigger value="tab2">{t("tabs.leadLists")}</TabsTrigger>
                    <TabsTrigger value="tab3">{t("tabs.test")}</TabsTrigger>
                    <TabsTrigger value="tab4">{t("tabs.analytics")}</TabsTrigger>
                    <TabsTrigger value="tab5">{t("tabs.settings")}</TabsTrigger>
                </TabsList>

                {fetchCampaign ? (
                    <div className="pt-6">
                        <TabsContent value="tab1">
                            <CampaignMessagingTab formData={campaign} setFormData={setCampaign} />
                        </TabsContent>

                        <TabsContent value="tab2">
                            <CampaignCollectionsTab campaign={campaign} campaignId={params.campaignId} />
                        </TabsContent>

                        <TabsContent value="tab3">
                            <CampaignTestTab campaign={campaign} setCampaign={setCampaign} />
                        </TabsContent>

                        <TabsContent value="tab4">
                            <CampaignAnalyticsTab
                                campaignId={params.campaignId}
                                status={campaign.status}
                                isActive={currentTab === ANALYTICS_TAB}
                                campaignCreatedAt={campaign.createdAt}
                                includeSmsChannel={campaign.isSms || campaign.fallbackToSMS}
                            />
                        </TabsContent>

                        <TabsContent value="tab5">
                            <CampaignSettingsTab campaign={campaign} setCampaign={setCampaign} />
                        </TabsContent>
                    </div>
                ) : null}
            </Tabs>
        </>
    );
}
