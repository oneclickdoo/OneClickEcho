"use client";

import { useCallback, useEffect, useMemo, useState } from "react";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { useTranslations } from "next-intl";

import * as echarts from "echarts";
import ReactEcharts from "echarts-for-react";

import { useAuth } from "@/context/AuthContext";

import type { KpiEntryExtended, KpiKey } from "@/data/schema";
import { CampaignLeadSMSStatus, CampaignLeadViberStatusCollection, CampaignStatus } from "@/lib/enums";

import { Button } from "@/components/tremor/Button";
import { Divider } from "@/components/tremor/Divider";
import { Card } from "@/components/tremor/Card";

import { CreateCollectionFromStatusModal } from "@/elements/dashboard/singleCampaign/modals/CreateCollectionFromStatusModal";
import { AddToCollectionFromStatusModal } from "@/elements/dashboard/singleCampaign/modals/AddToCollectionFromStatusModal";

import { downloadFile } from "@/lib/download";
import { useToast } from "@/lib/useToast";

import {
    type CampaignAnalytics,
    type CampaignSmsAnalytics,
    type CampaignViberAnalytics,
    exportFromStatus,
    getCampaignAnalytics
} from "../fetchData";

interface ICampaignAnalyticsTab {
    campaignId: string;
    status: CampaignStatus;
}

interface IFunnelChartData {
    total: number;
    delivered: number;
    seen: number;
    clicked: number;
}

type PieDatum = { name: string; value: number };

const campaignStatCardClass =
    "flex h-full min-h-[11.5rem] w-full flex-col !p-5 shadow-md border-gray-200 dark:border-gray-800 dark:bg-gray-900";

export function CampaignAnalyticsTab(props: ICampaignAnalyticsTab) {
    const t = useTranslations("SingleCampaign.Tabs.CampaignAnalytics");
    const tKpi = useTranslations("Kpi");
    const tCommon = useTranslations("Common");

    const routeParams = useParams<{ locale?: string }>();
    const locale = (routeParams?.locale as string) || "en";
    const numberFmt = useMemo(
        () =>
            new Intl.NumberFormat(locale, {
                maximumFractionDigits: 0
            }),
        [locale]
    );

    const [campaignAnalytics, setCampaignAnalytics] = useState<CampaignAnalytics>();
    const [viberStatuses, setViberStatuses] = useState<KpiEntryExtended[]>([]);
    const [smsStatuses, setSmsStatuses] = useState<KpiEntryExtended[]>([]);
    const [funnelChartData, setFunnelChartData] = useState<IFunnelChartData>();
    const [viberPieChartData, setViberPieChartData] = useState<PieDatum[]>([]);
    const [smsPieChartData, setSmsPieChartData] = useState<PieDatum[]>([]);

    const { dashboardManager, authFetch } = useAuth();
    const { toast } = useToast();

    const viberPieChartConfig = {
        tooltip: { show: true, trigger: "item" },
        legend: {
            left: 0,
            top: "92%",
            orient: "horizontal",
            selectedMode: false
        },
        series: [
            {
                label: { formatter: "{b}: {c} ({d}%)" },
                type: "pie",
                radius: ["40%", "70%"],
                avoidLabelOverlap: true,
                emphasis: { label: { show: true, fontSize: 16, fontWeight: "normal" } },
                labelLine: { show: true },
                data: viberPieChartData
            }
        ]
    };

    const smsPieChartConfig = {
        tooltip: { show: true, trigger: "item" },
        legend: {
            left: 0,
            top: "92%",
            orient: "horizontal",
            selectedMode: false
        },
        series: [
            {
                label: { formatter: "{b}: {c} ({d}%)" },
                type: "pie",
                radius: ["40%", "70%"],
                avoidLabelOverlap: true,
                emphasis: { label: { show: true, fontSize: 16, fontWeight: "normal" } },
                labelLine: { show: true },
                data: smsPieChartData
            }
        ]
    };

    const { data: fetchedAnalytics } = useQuery({
        queryKey: ["campaign-analytics", props.campaignId, props.status],
        queryFn: () => getCampaignAnalytics(props.campaignId, authFetch),
        enabled: props.status !== CampaignStatus.Draft
    });

    //const getViberData = useCallback(
    //    (analytics: CampaignViberAnalytics) => {
    //        const viberData: KpiEntryExtended[] = [];

    //        const pushKpi = (key: KpiKey, value: number, color: string) => {
    //            viberData.push({
    //                key,
    //                title: tKpi(key),
    //                value: value.toString(),
    //                percentage: value ? Math.round((value / analytics.total) * 100) : 0,
    //                color
    //            });
    //        };

    //        pushKpi("systemReceived", analytics.received, "bg-sky-600 dark:bg-sky-500");
    //        pushKpi("pending", analytics.pending, "bg-blue-600 dark:bg-blue-500");
    //        pushKpi("delivered", analytics.delivered, "bg-green-600 dark:bg-green-500");
    //        pushKpi("seen", analytics.seen, "bg-emerald-600 dark:bg-emerald-500");
    //        pushKpi("clicked", analytics.clicked, "bg-orange-600 dark:bg-orange-500");
    //        pushKpi("undelivered", analytics.undelivered, "bg-red-600 dark:bg-red-500");
    //        pushKpi("unsubscribed", analytics.unsubscribed, "bg-yellow-600 dark:bg-yellow-500");
    //        pushKpi("expired", analytics.expired, "bg-gray-600 dark:bg-gray-500");

    //        setViberStatuses(viberData);

    //        setFunnelChartData({
    //            total: analytics.total,
    //            delivered: analytics.delivered + analytics.seen + analytics.clicked,
    //            seen: analytics.seen + analytics.clicked,
    //            clicked: analytics.clicked
    //        });

    //        const pie: PieDatum[] = [];
    //        if (analytics.received > 0) pie.push({ name: tKpi("systemReceived"), value: analytics.received });
    //        if (analytics.pending > 0) pie.push({ name: tKpi("pending"), value: analytics.pending });

    //        const deliveredSeenClicked = analytics.delivered + analytics.seen + analytics.clicked;
    //        if (deliveredSeenClicked > 0) pie.push({ name: tKpi("delivered"), value: deliveredSeenClicked });

    //        if (analytics.undelivered > 0) pie.push({ name: tKpi("undelivered"), value: analytics.undelivered });
    //        if (analytics.unsubscribed > 0) pie.push({ name: tKpi("unsubscribed"), value: analytics.unsubscribed });
    //        if (analytics.expired > 0) pie.push({ name: tKpi("expired"), value: analytics.expired });

    //        setViberPieChartData(pie);
    //    },
    //    [tKpi]
    //);

    //const getSmsData = useCallback(
    //    (analytics: CampaignSmsAnalytics) => {
    //        const smsData: KpiEntryExtended[] = [];

    //        const pushKpi = (key: KpiKey, value: number, color: string) => {
    //            smsData.push({
    //                key,
    //                title: tKpi(key),
    //                value: value.toString(),
    //                percentage: value ? Math.round((value / analytics.total) * 100) : 0,
    //                color
    //            });
    //        };

    //        pushKpi("pending", analytics.pending, "bg-blue-600 dark:bg-blue-500");
    //        pushKpi("delivered", analytics.delivered, "bg-green-600 dark:bg-green-500");
    //        pushKpi("undelivered", analytics.undelivered, "bg-rose-600 dark:bg-rose-500");
    //        pushKpi("blacklisted", analytics.blacklisted, "bg-slate-900 dark:bg-slate-900");
    //        pushKpi("error", analytics.error, "bg-red-600 dark:bg-red-500");

    //        setSmsStatuses(smsData);

    //        const pie: PieDatum[] = [];
    //        if (analytics.pending > 0) pie.push({ name: tKpi("pending"), value: analytics.pending });
    //        if (analytics.delivered > 0) pie.push({ name: tKpi("delivered"), value: analytics.delivered });
    //        if (analytics.undelivered > 0) pie.push({ name: tKpi("undelivered"), value: analytics.undelivered });
    //        if (analytics.blacklisted > 0) pie.push({ name: tKpi("blacklisted"), value: analytics.blacklisted });
    //        if (analytics.error > 0) pie.push({ name: tKpi("error"), value: analytics.error });

    //        setSmsPieChartData(pie);
    //    },
    //    [tKpi]
    //);

    const getViberData = useCallback(
        (analytics: CampaignViberAnalytics) => {
            const viberData: KpiEntryExtended[] = [];

            const pushKpi = (key: KpiKey, value: number | undefined, color: string) => {
                const n = value ?? 0;
                viberData.push({
                    key,
                    title: tKpi(key),
                    value: String(n),
                    percentage: n && analytics.total ? Math.round((n / analytics.total) * 100) : 0,
                    color
                });
            };

            // NEW: NotSent
            pushKpi("notSent", analytics.notSent, "bg-zinc-500 dark:bg-zinc-400");

            pushKpi("systemReceived", analytics.received, "bg-sky-600 dark:bg-sky-500");
            pushKpi("pending", analytics.pending, "bg-blue-600 dark:bg-blue-500");
            pushKpi("delivered", analytics.delivered, "bg-green-600 dark:bg-green-500");
            pushKpi("seen", analytics.seen, "bg-emerald-600 dark:bg-emerald-500");
            pushKpi("clicked", analytics.clicked, "bg-orange-600 dark:bg-orange-500");
            pushKpi("undelivered", analytics.undelivered, "bg-red-600 dark:bg-red-500");
            pushKpi("unsubscribed", analytics.unsubscribed, "bg-yellow-600 dark:bg-yellow-500");
            pushKpi("expired", analytics.expired, "bg-gray-600 dark:bg-gray-500");

            setViberStatuses(viberData);

            // NEW: funnel total should be SENT (exclude notSent)
            setFunnelChartData({
                total: analytics.sent,
                delivered: analytics.funnelDelivered,
                seen: analytics.funnelSeen,
                clicked: analytics.clicked
            });

            const deliveredSeenClicked = analytics.delivered + analytics.seen + analytics.clicked;

            // Always include slices so undelivered / not sent stay visible in the legend (even at 0).
            const pie: PieDatum[] = [
                { name: tKpi("notSent"), value: analytics.notSent },
                { name: tKpi("systemReceived"), value: analytics.received },
                { name: tKpi("pending"), value: analytics.pending },
                { name: tKpi("delivered"), value: deliveredSeenClicked },
                { name: tKpi("undelivered"), value: analytics.undelivered },
                { name: tKpi("unsubscribed"), value: analytics.unsubscribed },
                { name: tKpi("expired"), value: analytics.expired }
            ];

            setViberPieChartData(pie);
        },
        [tKpi]
    );

    const getSmsData = useCallback(
        (analytics: CampaignSmsAnalytics) => {
            const smsData: KpiEntryExtended[] = [];

            const pushKpi = (key: KpiKey, value: number | undefined, color: string) => {
                const n = value ?? 0;
                smsData.push({
                    key,
                    title: tKpi(key),
                    value: String(n),
                    percentage: n && analytics.total ? Math.round((n / analytics.total) * 100) : 0,
                    color
                });
            };

            // NEW: NotSent
            pushKpi("notSent", analytics.notSent, "bg-zinc-500 dark:bg-zinc-400");

            pushKpi("pending", analytics.pending, "bg-blue-600 dark:bg-blue-500");
            pushKpi("delivered", analytics.delivered, "bg-green-600 dark:bg-green-500");
            pushKpi("undelivered", analytics.undelivered, "bg-rose-600 dark:bg-rose-500");
            pushKpi("blacklisted", analytics.blacklisted, "bg-slate-900 dark:bg-slate-900");
            pushKpi("error", analytics.error, "bg-red-600 dark:bg-red-500");

            setSmsStatuses(smsData);

            const pie: PieDatum[] = [
                { name: tKpi("notSent"), value: analytics.notSent },
                { name: tKpi("pending"), value: analytics.pending },
                { name: tKpi("delivered"), value: analytics.delivered },
                { name: tKpi("undelivered"), value: analytics.undelivered },
                { name: tKpi("blacklisted"), value: analytics.blacklisted },
                { name: tKpi("error"), value: analytics.error }
            ];

            setSmsPieChartData(pie);
        },
        [tKpi]
    );

    const exportCollectionFromStatus = async (
        viberStatus: CampaignLeadViberStatusCollection | null,
        smsStatus: CampaignLeadSMSStatus | null
    ) => {
        if (!dashboardManager?.currentCompany?.companyId) return;

        try {
            const blob: Blob = await exportFromStatus(props.campaignId, viberStatus, smsStatus, authFetch);
            downloadFile(blob, `leads-${props.campaignId}.csv`);

            toast({
                title: tCommon("success"),
                description: t("export.successDescription"),
                variant: "success",
                duration: 2000
            });
        } catch (e) {
            console.error(e);
            toast({
                title: tCommon("error"),
                description: t("export.errorDescription"),
                variant: "error",
                duration: 2500
            });
        }
    };

    useEffect(() => {
        if (!fetchedAnalytics) return;

        setCampaignAnalytics(fetchedAnalytics);
        if (fetchedAnalytics.viber) getViberData(fetchedAnalytics.viber);
        if (fetchedAnalytics.sms) getSmsData(fetchedAnalytics.sms);
    }, [fetchedAnalytics, getSmsData, getViberData]);

    const viberStatusActions: Array<{ key: KpiKey; value: CampaignLeadViberStatusCollection }> = [
        { key: "notSent", value: CampaignLeadViberStatusCollection.NotSent },
        { key: "systemReceived", value: 1 },
        { key: "pending", value: 2 },
        { key: "delivered", value: 3 },
        { key: "seen", value: 4 },
        { key: "undelivered", value: 5 },
        { key: "expired", value: 6 },
        { key: "clicked", value: 7 }
    ];

    const smsStatusActions: Array<{ key: KpiKey; value: CampaignLeadSMSStatus }> = [
        { key: "notSent", value: CampaignLeadSMSStatus.None },
        { key: "pending", value: 5 },
        { key: "delivered", value: 1 },
        { key: "undelivered", value: 2 },
        { key: "blacklisted", value: 8 },
        { key: "error", value: 6 }
    ];

    const isNotLaunched =
        !props.status || [CampaignStatus.Draft, CampaignStatus.Queued, CampaignStatus.InProgress].includes(props.status);

    return (
        <section className="mt-2" aria-labelledby="current-billing-cycle">
            {isNotLaunched ? (
                <p className="text-center">{t("notLaunchedYet")}</p>
            ) : campaignAnalytics ? (
                <div className="flex flex-col gap-5">
                    {campaignAnalytics.viber && viberStatuses.length > 0 ? (
                        <>
                            <div className="grid w-full grid-cols-1 gap-3 md:grid-cols-3">
                                <Card className={campaignStatCardClass}>
                                    <h3 className="mb-1 text-md">{t("cards.contacts.title")}</h3>
                                    <div className="flex flex-1 flex-col">
                                        <p className="text-2xl font-semibold">{campaignAnalytics.viber.total}</p>
                                        <span className="mt-auto text-xs text-gray-500">{t("cards.contacts.subtitle")}</span>
                                    </div>
                                </Card>

                                <Card className={campaignStatCardClass}>
                                    <h3 className="mb-1 text-md">{t("cards.messagesSent.title")}</h3>
                                    <div className="flex flex-1 flex-col">
                                        <p className="text-2xl font-semibold">
                                            {numberFmt.format(campaignAnalytics.viber.total)}
                                        </p>
                                        <div className="mt-1 flex-1 space-y-0.5 text-xs leading-relaxed text-gray-500 dark:text-gray-400">
                                            <div>
                                                {t("cards.messagesSent.deliveredToUser", {
                                                    count: numberFmt.format(campaignAnalytics.viber.funnelDelivered)
                                                })}
                                            </div>
                                            <div>
                                                {t("cards.messagesSent.undelivered", {
                                                    count: numberFmt.format(campaignAnalytics.viber.undelivered)
                                                })}
                                            </div>
                                            <div>
                                                {t("cards.messagesSent.notSentToViber", {
                                                    count: numberFmt.format(campaignAnalytics.viber.notSent)
                                                })}
                                            </div>
                                        </div>
                                    </div>
                                </Card>

                                <Card className={campaignStatCardClass}>
                                    <h3 className="mb-1 text-md">{t("cards.reaction.title")}</h3>
                                    <div className="mt-1 flex flex-1 flex-row justify-between gap-4">
                                        <div className="flex flex-col">
                                            <p className="text-2xl font-semibold">{campaignAnalytics.viber.clicked}</p>
                                            <span className="mt-auto text-xs text-gray-500">{t("cards.reaction.clicks")}</span>
                                        </div>
                                        <div className="flex flex-col text-right">
                                            <p className="text-2xl font-semibold">{campaignAnalytics.viber.unsubscribed}</p>
                                            <span className="mt-auto text-xs text-gray-500">{t("cards.reaction.unsubscribed")}</span>
                                        </div>
                                    </div>
                                </Card>
                            </div>

                            <div className="mt-10 flex flex-col gap-y-10 md:flex-row md:items-stretch">
                                <div className="flex min-h-[28rem] w-full flex-col items-center justify-center gap-1 text-white md:w-1/2">
                                    <div
                                        className="flex h-20 w-[95%] items-center justify-center bg-sky-600 dark:bg-sky-500"
                                        style={{ clipPath: "polygon(0% 0%, 100% 0%, 85% 100%, 15% 100%)" }}
                                    >
                                        <p className="text-center">
                                            {t("funnel.total")}
                                            <br /> {funnelChartData?.total ?? 0}
                                            <br />(100%)
                                        </p>
                                    </div>

                                    <div
                                        className="flex h-20 w-[65%] items-center justify-center bg-green-600 dark:bg-green-500"
                                        style={{ clipPath: "polygon(0% 0%, 100% 0%, 85% 100%, 15% 100%)" }}
                                    >
                                        <p className="text-center">
                                            {tKpi("delivered")}
                                            <br /> {funnelChartData?.delivered ?? 0}
                                            <br />(
                                            {Math.round(
                                                ((funnelChartData?.delivered ?? 0) / Math.max(funnelChartData?.total ?? 0, 1)) *
                                                    10000
                                            ) / 100}
                                            %)
                                        </p>
                                    </div>

                                    <div
                                        className="flex h-20 w-[45%] items-center justify-center bg-emerald-600 dark:bg-emerald-500"
                                        style={{ clipPath: "polygon(0% 0%, 100% 0%, 85% 100%, 15% 100%)" }}
                                    >
                                        <p className="text-center">
                                            {tKpi("seen")}
                                            <br /> {funnelChartData?.seen ?? 0}
                                            <br />(
                                            {Math.round(
                                                ((funnelChartData?.seen ?? 0) / Math.max(funnelChartData?.total ?? 0, 1)) * 10000
                                            ) / 100}
                                            %)
                                        </p>
                                    </div>

                                    <div
                                        className="flex h-20 w-[30%] items-center justify-center bg-orange-600 dark:bg-orange-500"
                                        style={{ clipPath: "polygon(0% 0%, 100% 0%, 85% 100%, 15% 100%)" }}
                                    >
                                        <p className="text-center">
                                            {tKpi("clicked")}
                                            <br /> {funnelChartData?.clicked ?? 0}
                                            <br />(
                                            {Math.round(
                                                ((funnelChartData?.clicked ?? 0) / Math.max(funnelChartData?.total ?? 0, 1)) *
                                                    10000
                                            ) / 100}
                                            %)
                                        </p>
                                    </div>
                                </div>

                                <div className="flex min-h-[28rem] w-full items-center md:w-1/2">
                                    <ReactEcharts className="h-[22rem] w-full" echarts={echarts} option={viberPieChartConfig} />
                                </div>
                            </div>

                            <Divider className="mt-10" />

                            <div>{t("viber.mutuallyExclusiveHint")}</div>

                            <div className="flex flex-col items-center justify-between gap-5 md:flex-row">
                                <div className="flex w-full flex-col gap-2">
                                    {viberStatusActions.map((s) => (
                                        <div
                                            key={s.value}
                                            className="flex justify-between gap-x-3 rounded-md bg-gray-50 p-1.5 dark:bg-gray-900"
                                        >
                                            <div>{tKpi(s.key)}</div>
                                            <div className="flex gap-x-1">
                                                <CreateCollectionFromStatusModal campaignId={props.campaignId} viberStatus={s.value} smsStatus={null} />
                                                <AddToCollectionFromStatusModal campaignId={props.campaignId} viberStatus={s.value} smsStatus={null} />
                                                <Button
                                                    className="h-[25px] bg-amber-500 dark:bg-amber-400"
                                                    onClick={() => void exportCollectionFromStatus(s.value, null)}
                                                >
                                                    {tCommon("export")}
                                                </Button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </>
                    ) : null}

                    {campaignAnalytics.sms && smsStatuses.length > 0 ? (
                        <div className="flex flex-col gap-5 md:flex-row md:items-stretch">
                            <div className="flex min-h-[22rem] w-full items-center md:w-1/2">
                                <ReactEcharts className="h-[22rem] w-full" echarts={echarts} option={smsPieChartConfig} />
                            </div>

                            <div className="flex w-full flex-col justify-center gap-2 md:w-1/2">
                                {smsStatusActions.map((s) => (
                                    <div
                                        key={s.value}
                                        className="flex justify-between gap-x-3 rounded-md bg-gray-50 p-1.5 dark:bg-gray-900"
                                    >
                                        <div>{tKpi(s.key)}</div>
                                        <div className="flex gap-x-1">
                                            <CreateCollectionFromStatusModal campaignId={props.campaignId} viberStatus={null} smsStatus={s.value} />
                                            <AddToCollectionFromStatusModal campaignId={props.campaignId} viberStatus={null} smsStatus={s.value} />
                                            <Button
                                                className="h-[25px] bg-amber-500 dark:bg-amber-400"
                                                onClick={() => void exportCollectionFromStatus(null, s.value)}
                                            >
                                                {tCommon("export")}
                                            </Button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    ) : null}
                </div>
            ) : null}
        </section>
    );
}
