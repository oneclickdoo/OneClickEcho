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

    const getAnalytics = useCallback(
        async (dateFrom?: string, dateTo?: string) => {
            setIsLoading(true);
            setError("");
            try {
                const data = await getCompanyAnalytics(props.companyId, authFetch, dateFrom, dateTo);
                setAnalytics(data);
            } catch (e: any) {
                console.error(e);
                setAnalytics(undefined);
                setError(e?.message ?? "Error");
            } finally {
                setIsLoading(false);
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

    const chartData1 = useMemo(() => {
        return [
            {
                name: t("charts.viber"),
                amount: analytics?.analyticsResults?.viberTotalSent ?? 0,
                color: "#4F46E5"
            },
            {
                name: t("charts.sms"),
                amount: analytics?.analyticsResults?.smsTotalSent ?? 0,
                color: "#3A82F6"
            }
        ];
    }, [analytics, t]);

    const chartData2 = useMemo(() => {
        const delivered = analytics?.analyticsResults?.viberDelivered ?? 0;
        const clicked = analytics?.analyticsResults?.viberClicked ?? 0;
        const seen = analytics?.analyticsResults?.viberSeen ?? 0;

        return [
            {
                name: t("charts.viberDelivered"),
                amount: Math.max(delivered - clicked - seen, 0),
                color: "#4F46E5"
            },
            {
                name: t("charts.viberUndelivered"),
                amount: analytics?.analyticsResults?.viberUndelivered ?? 0,
                color: "#3A82F6"
            },
            {
                name: t("charts.viberExpired"),
                amount: analytics?.analyticsResults?.viberExpired ?? 0,
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
    }, [analytics, t]);

    const chartData3 = useMemo(() => {
        return [
            {
                name: t("charts.smsDelivered"),
                amount: analytics?.analyticsResults?.smsDelivered ?? 0,
                color: "#4F46E5"
            },
            {
                name: t("charts.smsFailed"),
                amount: analytics?.analyticsResults?.smsFailed ?? 0,
                color: "#3A82F6"
            }
        ];
    }, [analytics, t]);

    const hasRange = Boolean(selectedDate?.from);

    const donutValueFormatter = useCallback(
        (value: number) => numberFmt.format(value),
        [numberFmt]
    );

    const totalViberSent = analytics?.analyticsResults?.viberTotalSent ?? 0;
    const totalSmsSent = analytics?.analyticsResults?.smsTotalSent ?? 0;

    const viberDelivered = analytics?.analyticsResults?.viberDelivered ?? 0;
    const smsDelivered = analytics?.analyticsResults?.smsDelivered ?? 0;

    const uniqueContacts = analytics?.analyticsResults?.uniquePhoneNumbers ?? 0;
    const unsubscribed = analytics?.analyticsResults?.totalUnsubscribed ?? 0;

    const numberOfCampaigns = analytics?.analyticsResults?.numberOfCampaigns ?? 0;
    const numberOfTestsSms = analytics?.analyticsResults?.numberOfTestsSms ?? 0;
    const numberOfTestsViber = analytics?.analyticsResults?.numberOfTestsViber ?? 0;

    const numberOfApiSms = analytics?.analyticsResults?.numberOfApiSms ?? 0;
    const numberOfApiViber = analytics?.analyticsResults?.numberOfApiViber ?? 0;

    const viberPrice = analytics?.viberPrice ?? 0;
    const smsPrice = analytics?.smsPrice ?? 0;

    const viberCost = viberDelivered * viberPrice;
    const smsCost = smsDelivered * smsPrice;

    const viberClickRate =
        viberDelivered > 0 ? ((analytics?.analyticsResults?.viberClicked ?? 0) / viberDelivered) * 100 : 0;

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
                        <div className="flex flex-wrap gap-2 mt-5">
                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.messagesSentViber")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(totalViberSent)}
                                    <div className="text-xs text-gray-500">
                                        {t("labels.delivery")}: {numberFmt.format(viberDelivered)} / {numberFmt.format(totalViberSent)}
                                    </div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.messagesSentSms")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(totalSmsSent)}
                                    <div className="text-xs text-gray-500">
                                        {t("labels.delivery")}: {numberFmt.format(smsDelivered)} / {numberFmt.format(totalSmsSent)}
                                    </div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.viberReactions")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {new Intl.NumberFormat(locale, { maximumFractionDigits: 2, minimumFractionDigits: 2 }).format(viberClickRate)}%
                                    <div className="text-xs text-gray-500">
                                        {t("labels.clickedOf", {
                                            clicked: numberFmt.format(analytics.analyticsResults.viberClicked ?? 0),
                                            delivered: numberFmt.format(viberDelivered)
                                        })}
                                    </div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.contacts")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(uniqueContacts)}
                                    <div className="text-xs text-gray-500">{t("labels.uniqueContacts")}</div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.viberCost")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {moneyFmt.format(viberCost)}
                                    <div className="text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.smsCost")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {moneyFmt.format(smsCost)}
                                    <div className="text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.unsubscribed")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(unsubscribed)}
                                    <div className="text-xs text-gray-500">{t("labels.contacts")}</div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.numberOfCampaigns")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(numberOfCampaigns)}
                                    <div className="text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.numberOfTests")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(numberOfTestsViber + numberOfTestsSms)}
                                    <div className="text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("cards.numberOfApiMessages")}</h3>
                                <div className="font-semibold text-2xl text-gray-800 dark:text-gray-100">
                                    {numberFmt.format(numberOfApiViber + numberOfApiSms)}
                                    <div className="text-xs text-gray-500">{t("labels.forPeriod")}</div>
                                </div>
                            </Card>
                        </div>

                        <div className="flex flex-wrap gap-2 mt-2">
                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("charts.messagesSent")}</h3>

                                <div className="flex flex-col items-center justify-center gap-4 mt-3">
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

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("charts.viberDelivery")}</h3>

                                <div className="flex flex-col items-center justify-center gap-4 mt-3">
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

                            <Card className="max-w-[360px] p-5 shadow-md border border-gray-200 dark:border-gray-800 dark:bg-gray-900">
                                <h3 className="mb-1 text-sm text-gray-600 dark:text-gray-400">{t("charts.smsDelivery")}</h3>

                                <div className="flex flex-col items-center justify-center gap-4 mt-3">
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
