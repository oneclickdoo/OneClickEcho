"use client";

import React, { useState, useEffect, useMemo, useCallback } from "react";
import { useParams } from "next/navigation";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { Card } from "@/components/tremor/Card";
import { DateRange, DateRangePicker } from "@/components/tremor/DatePicker";
import { DonutChart } from "@/components/tremor/DonutChart";

import { CompanyAnalyticsDto, getCompanyAnalytics } from "@/elements/dashboard/singleCompany/fetchData";

interface ICompanyAnalytics {
    companyId: string;
}

function startOfMonth(d: Date) {
    return new Date(d.getFullYear(), d.getMonth(), 1, 0, 0, 0, 0);
}
function endOfMonth(d: Date) {
    return new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59, 999);
}
function addDays(d: Date, days: number) {
    const x = new Date(d);
    x.setDate(x.getDate() + days);
    return x;
}

/** API / legacy payloads may send non-numeric values; keeps charts and rates free of NaN. */
function safeNonNegativeInt(value: unknown): number {
    const n = Number(value);
    if (!Number.isFinite(n) || n < 0) return 0;
    return Math.trunc(n);
}

const statCardClass =
    "flex h-full min-h-[11.5rem] w-full flex-col !p-5 shadow-md border-gray-200 dark:border-gray-800 dark:bg-gray-900";
const chartCardClass =
    "flex h-full min-h-[28rem] w-full flex-col !p-5 shadow-md border-gray-200 dark:border-gray-800 dark:bg-gray-900";

export const CompanyAnalytics = (props: ICompanyAnalytics) => {
    const t = useTranslations("CompanyAnalytics");

    const routeParams = useParams<{ locale?: string }>();
    const locale = (routeParams?.locale as string) || "en";

    const [analytics, setAnalytics] = useState<CompanyAnalyticsDto | undefined>(undefined);
    const [selectedDate, setSelectedDate] = useState<DateRange | undefined>(undefined);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string>("");

    const { authFetch } = useAuth();

    const numberFmt = useMemo(
        () =>
            new Intl.NumberFormat(locale, {
                maximumFractionDigits: 0
            }),
        [locale]
    );

    const moneyFmt = useMemo(
        () =>
            new Intl.NumberFormat(locale, {
                style: "currency",
                currency: "EUR",
                maximumFractionDigits: 2
            }),
        [locale]
    );

    const percentFmt = useMemo(
        () =>
            new Intl.NumberFormat(locale, {
                maximumFractionDigits: 2,
                minimumFractionDigits: 2
            }),
        [locale]
    );

    const getAnalytics = useCallback(
        async (dateFrom?: string, dateTo?: string, options?: { silent?: boolean }) => {
            const silent = options?.silent === true;
            if (!silent) {
                setIsLoading(true);
            }

            setError("");
            try {
                const data = await getCompanyAnalytics(props.companyId, authFetch, dateFrom, dateTo);
                setAnalytics(data);
            } catch (e: any) {
                console.error(e);
                setAnalytics(undefined);
                setError(e?.message ?? "Error");
            } finally {
                if (!silent) {
                    setIsLoading(false);
                }
            }
        },
        [props.companyId, authFetch]
    );

    useEffect(() => {
        const now = new Date();
        setSelectedDate({
            from: startOfMonth(now),
            to: endOfMonth(now)
        });
    }, []);

    useEffect(() => {
        if (!selectedDate?.from) return;

        const fromIso = selectedDate.from.toISOString();
        const toIso = selectedDate.to ? addDays(selectedDate.to, 1).toISOString() : addDays(selectedDate.from, 1).toISOString();

        getAnalytics(fromIso, toIso);
    }, [selectedDate, getAnalytics]);

    /** Polling: Viber statusi se menjaju u pozadini (delivery job) — osveži Overview analitiku. */
    useEffect(() => {
        if (!selectedDate?.from) return;

        const fromIso = selectedDate.from.toISOString();
        const toIso = selectedDate.to ? addDays(selectedDate.to, 1).toISOString() : addDays(selectedDate.from, 1).toISOString();

        const id = window.setInterval(() => {
            void getAnalytics(fromIso, toIso, { silent: true });
        }, 15_000);

        return () => window.clearInterval(id);
    }, [selectedDate, getAnalytics]);

    const chartData1 = useMemo(() => {
        const viberLeads = safeNonNegativeInt(analytics?.analyticsResults?.viberTotalLeads);
        const viberNotSent = safeNonNegativeInt(analytics?.analyticsResults?.viberNotSent);
        const viberSubmitted = Math.max(viberLeads - viberNotSent, 0);
        const smsSent = safeNonNegativeInt(analytics?.analyticsResults?.smsTotalSent);

        return [
            {
                name: t("charts.viberNotSent"),
                amount: viberNotSent,
                color: "#9CA3AF"
            },
            {
                name: t("charts.viberSubmitted"),
                amount: viberSubmitted,
                color: "#4F46E5"
            },
            {
                name: t("charts.sms"),
                amount: smsSent,
                color: "#3A82F6"
            }
        ];
    }, [analytics, t]);

    const chartData2 = useMemo(() => {
        const received = safeNonNegativeInt(analytics?.analyticsResults?.viberReceived);
        const deliveredOnDevice = safeNonNegativeInt(analytics?.analyticsResults?.viberDeliveredOnly);
        const clicked = safeNonNegativeInt(analytics?.analyticsResults?.viberClicked);
        const seen = safeNonNegativeInt(analytics?.analyticsResults?.viberSeen);

        const segments = [
            ...(received > 0
                ? [
                      {
                          name: t("charts.viberReceived"),
                          amount: received,
                          color: "#06B6D4"
                      }
                  ]
                : []),
            {
                name: t("charts.viberDeliveredOnDevice"),
                amount: deliveredOnDevice,
                color: "#4F46E5"
            },
            {
                name: t("charts.viberUndelivered"),
                amount: safeNonNegativeInt(analytics?.analyticsResults?.viberUndelivered),
                color: "#EF4444"
            },
            {
                name: t("charts.viberExpired"),
                amount: safeNonNegativeInt(analytics?.analyticsResults?.viberExpired),
                color: "#0EB981"
            },
            {
                name: t("charts.viberSeen"),
                amount: seen,
                color: "#8B5CF6"
            },
            {
                name: t("charts.viberClicked"),
                amount: clicked,
                color: "#F59E0A"
            }
        ];

        return segments;
    }, [analytics, t]);

    const chartData3 = useMemo(() => {
        return [
            {
                name: t("charts.smsDelivered"),
                amount: safeNonNegativeInt(analytics?.analyticsResults?.smsDelivered),
                color: "#4F46E5"
            },
            {
                name: t("charts.smsFailed"),
                amount: safeNonNegativeInt(analytics?.analyticsResults?.smsFailed),
                color: "#3A82F6"
            }
        ];
    }, [analytics, t]);

    const hasRange = Boolean(selectedDate?.from);

    const donutValueFormatter = useCallback(
        (value: number) => numberFmt.format(value),
        [numberFmt]
    );

    const viberTotalLeads = safeNonNegativeInt(analytics?.analyticsResults?.viberTotalLeads);
    const viberNotSent = safeNonNegativeInt(analytics?.analyticsResults?.viberNotSent);
    const totalSmsSent = safeNonNegativeInt(analytics?.analyticsResults?.smsTotalSent);

    const viberDelivered = safeNonNegativeInt(analytics?.analyticsResults?.viberDelivered);
    const viberReceived = safeNonNegativeInt(analytics?.analyticsResults?.viberReceived);
    const viberDeliveredOnly = safeNonNegativeInt(analytics?.analyticsResults?.viberDeliveredOnly);
    const viberUndelivered = safeNonNegativeInt(analytics?.analyticsResults?.viberUndelivered);
    const viberClicked = safeNonNegativeInt(analytics?.analyticsResults?.viberClicked);
    const smsDelivered = safeNonNegativeInt(analytics?.analyticsResults?.smsDelivered);

    const uniqueContacts = safeNonNegativeInt(analytics?.analyticsResults?.uniquePhoneNumbers);
    const unsubscribed = safeNonNegativeInt(analytics?.analyticsResults?.totalUnsubscribed);

    const numberOfCampaigns = safeNonNegativeInt(analytics?.analyticsResults?.numberOfCampaigns);
    const numberOfTestsSms = safeNonNegativeInt(analytics?.analyticsResults?.numberOfTestsSms);
    const numberOfTestsViber = safeNonNegativeInt(analytics?.analyticsResults?.numberOfTestsViber);

    const numberOfApiSms = safeNonNegativeInt(analytics?.analyticsResults?.numberOfApiSms);
    const numberOfApiViber = safeNonNegativeInt(analytics?.analyticsResults?.numberOfApiViber);

    const viberPrice = Number(analytics?.viberPrice ?? 0);
    const smsPrice = Number(analytics?.smsPrice ?? 0);
    const safeViberPrice = Number.isFinite(viberPrice) && viberPrice >= 0 ? viberPrice : 0;
    const safeSmsPrice = Number.isFinite(smsPrice) && smsPrice >= 0 ? smsPrice : 0;

    const viberCost = viberDelivered * safeViberPrice;
    const smsCost = smsDelivered * safeSmsPrice;

    const viberClickRate = viberDelivered > 0 ? (viberClicked / viberDelivered) * 100 : 0;
    const displayViberClickRate = Number.isFinite(viberClickRate) ? viberClickRate : 0;

    return (
        <div>
            <h1 className="font-semibold text-lg text-gray-900 sm:text-xl dark:text-gray-50">{t("title")}</h1>

            <div className="mt-5">
                <DateRangePicker
                    className="max-w-[300px] text-sm text-gray-900 dark:text-gray-50 bg-white dark:bg-gray-950 rounded-md border-gray-300 dark:border-gray-800 disabled:border-gray-300 disabled:bg-gray-100 disabled:text-gray-400 disabled:dark:border-gray-700 disabled:dark:bg-gray-800 disabled:dark:text-gray-500"
                    value={selectedDate}
                    onChange={setSelectedDate}
                />

                {!hasRange ? (
                    <div className="mt-5 p-4 text-center text-gray-700 dark:text-gray-300 border border-gray-200 rounded-md dark:border-gray-800 bg-white dark:bg-gray-900">
                        {t("selectDateRange")}
                    </div>
                ) : isLoading ? (
                    <div className="flex items-center justify-center h-32 text-gray-700 dark:text-gray-300 mt-5">
                        <svg
                            className="animate-spin h-5 w-5 mr-3 text-gray-700 dark:text-gray-300"
                            xmlns="http://www.w3.org/2000/svg"
                            fill="none"
                            viewBox="0 0 24 24"
                            aria-hidden
                        >
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C3.58 0 0 5.82 0 12h4z" />
                        </svg>
                        <span className="text-lg font-medium">{t("loading")}</span>
                    </div>
                ) : error ? (
                    <div className="mt-5 p-4 text-center text-red-600 border border-red-200 rounded-md dark:border-red-900 bg-white dark:bg-gray-900">
                        {t("error")}: {error}
                    </div>
                ) : analytics ? (
                    <>
                        <div className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.messagesSentViber")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(viberTotalLeads)}
                                    <div className="mt-1 flex-1 space-y-0.5 text-xs leading-relaxed text-gray-500">
                                        {viberReceived > 0 ? (
                                            <div>
                                                {t("labels.viberReceivedCount", { count: numberFmt.format(viberReceived) })}
                                            </div>
                                        ) : null}
                                        <div>
                                            {t("labels.viberDeliveredOnDeviceCount", { count: numberFmt.format(viberDeliveredOnly) })}
                                        </div>
                                        <div>{t("labels.viberDeliveredCount", { count: numberFmt.format(viberDelivered) })}</div>
                                        <div>{t("labels.viberUndeliveredCount", { count: numberFmt.format(viberUndelivered) })}</div>
                                        <div>{t("labels.viberNotSentCount", { count: numberFmt.format(viberNotSent) })}</div>
                                    </div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.messagesSentSms")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(totalSmsSent)}
                                    <div className="mt-auto text-xs text-gray-500">
                                        {t("labels.delivery")}: {numberFmt.format(smsDelivered)} / {numberFmt.format(totalSmsSent)}
                                    </div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.viberReactions")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {percentFmt.format(displayViberClickRate)}%
                                    <div className="mt-auto text-xs text-gray-500">
                                        {t("labels.clickedOf", {
                                            clicked: numberFmt.format(viberClicked),
                                            delivered: numberFmt.format(viberDelivered)
                                        })}
                                    </div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.contacts")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(uniqueContacts)}
                                    <div className="mt-auto text-xs text-gray-500">{t("labels.uniqueContacts")}</div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.viberCost")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {moneyFmt.format(viberCost)}
                                    <div className="mt-auto text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.smsCost")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {moneyFmt.format(smsCost)}
                                    <div className="mt-auto text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.unsubscribed")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(unsubscribed)}
                                    <div className="mt-auto text-xs text-gray-500">{t("labels.contacts")}</div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.numberOfCampaigns")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(numberOfCampaigns)}
                                    <div className="mt-auto text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.numberOfTests")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(numberOfTestsViber + numberOfTestsSms)}
                                    <div className="mt-auto text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className={statCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.numberOfApiMessages")}</h3>
                                <div className="flex flex-1 flex-col font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(numberOfApiViber + numberOfApiSms)}
                                    <div className="mt-auto text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>
                        </div>

                        <div className="mt-2 grid grid-cols-1 gap-3 lg:grid-cols-3">
                            <Card className={chartCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("charts.messagesSent")}</h3>

                                <div className="mt-3 flex flex-1 flex-col items-center justify-center gap-4">
                                    <DonutChart
                                        data={chartData1}
                                        category="name"
                                        value="amount"
                                        showLabel={true}
                                        valueFormatter={donutValueFormatter}
                                    />

                                    <div className="flex flex-wrap justify-center mt-4 gap-2">
                                        {chartData1.map((item) => (
                                            <div key={item.name} className="flex items-center gap-2">
                                                <span style={{ backgroundColor: item.color }} className="w-3 h-3 inline-block rounded-full" />
                                                <span className="text-sm text-gray-700 dark:text-gray-300">
                                                    {item.name} ({numberFmt.format(item.amount)})
                                                </span>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            </Card>

                            <Card className={chartCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("charts.viberDelivery")}</h3>
                                <p className="text-xs text-gray-500 dark:text-gray-400">{t("charts.viberDeliveryScopeHint")}</p>

                                <div className="mt-3 flex flex-1 flex-col items-center justify-center gap-4">
                                    <DonutChart
                                        data={chartData2}
                                        category="name"
                                        value="amount"
                                        showLabel={true}
                                        valueFormatter={donutValueFormatter}
                                    />

                                    <div className="flex flex-wrap justify-center mt-4 gap-2">
                                        {chartData2.map((item) => (
                                            <div key={item.name} className="flex items-center gap-2">
                                                <span style={{ backgroundColor: item.color }} className="w-3 h-3 inline-block rounded-full" />
                                                <span className="text-sm text-gray-700 dark:text-gray-300">
                                                    {item.name} ({numberFmt.format(item.amount)})
                                                </span>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            </Card>

                            <Card className={chartCardClass}>
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("charts.smsDelivery")}</h3>

                                <div className="mt-3 flex flex-1 flex-col items-center justify-center gap-4">
                                    <DonutChart
                                        data={chartData3}
                                        category="name"
                                        value="amount"
                                        showLabel={true}
                                        valueFormatter={donutValueFormatter}
                                    />

                                    <div className="flex flex-wrap justify-center mt-4 gap-2">
                                        {chartData3.map((item) => (
                                            <div key={item.name} className="flex items-center gap-2">
                                                <span style={{ backgroundColor: item.color }} className="w-3 h-3 inline-block rounded-full" />
                                                <span className="text-sm text-gray-700 dark:text-gray-300">
                                                    {item.name} ({numberFmt.format(item.amount)})
                                                </span>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            </Card>
                        </div>
                    </>
                ) : (
                    <div className="mt-5 p-4 text-center text-gray-700 dark:text-gray-300 border border-gray-200 rounded-md dark:border-gray-800 bg-white dark:bg-gray-900">
                        {t("noData")}
                    </div>
                )}
            </div>
        </div>
    );
};
