"use client";

import React, { useMemo, useState, useEffect, useCallback } from "react";
import { useParams } from "next/navigation";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { Card } from "@/components/tremor/Card";
import { DateRange, DateRangePicker } from "@/components/tremor/DatePicker";
import { AdminAnalyticsDto, getAdminAnalytics } from "@/elements/dashboard/overview/fetchData";

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

const adminStatCardClass =
    "flex h-full min-h-[7.5rem] w-full flex-col !p-5 shadow-md border-gray-200 dark:border-gray-800 dark:bg-gray-900";

export const AdminAnalytics = () => {
    const t = useTranslations("AdminAnalytics");

    const routeParams = useParams<{ locale?: string }>();
    const locale = (routeParams?.locale as string) || "en";

    const [analytics, setAnalytics] = useState<AdminAnalyticsDto | undefined>(undefined);
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
                const data = await getAdminAnalytics(authFetch, dateFrom, dateTo);
                setAnalytics(data);
            } catch (e: any) {
                console.error(e);
                setAnalytics(undefined);
                setError(e?.message ?? "Error");
            } finally {
                setIsLoading(false);
            }
        },
        [authFetch]
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
        const toIso = selectedDate.to
            ? addDays(selectedDate.to, 1).toISOString()
            : addDays(selectedDate.from, 1).toISOString();

        getAnalytics(fromIso, toIso);
    }, [selectedDate, getAnalytics]);

    const hasRange = Boolean(selectedDate?.from);

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
                            <path
                                className="opacity-75"
                                fill="currentColor"
                                d="M4 12a8 8 0 018-8V0C3.58 0 0 5.82 0 12h4z"
                            />
                        </svg>
                        <span className="text-lg font-medium">{t("loading")}</span>
                    </div>
                ) : error ? (
                    <div className="mt-5 p-4 text-center text-red-600 border border-red-200 rounded-md dark:border-red-900 bg-white dark:bg-gray-900">
                        {t("error")}: {error}
                    </div>
                ) : analytics ? (
                    <>
                        <h2 className="mt-5 mb-3 font-medium text-gray-900 dark:text-gray-50">{t("messagesSent")}</h2>
                        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                            <Card className={adminStatCardClass}>
                                <h3 className="mb-1 text-sm">{t("viber")}</h3>
                                <p className="mt-auto font-semibold text-2xl">
                                    {numberFmt.format(analytics.totalViberMessagesSent)}{" "}
                                    <span className="text-xs text-gray-500">{t("period.range")}</span>
                                </p>
                            </Card>

                            <Card className={adminStatCardClass}>
                                <h3 className="mb-1 text-sm">{t("sms")}</h3>
                                <p className="mt-auto font-semibold text-2xl">
                                    {numberFmt.format(analytics.totalSmsMessagesSent)}{" "}
                                    <span className="text-xs text-gray-500">{t("period.range")}</span>
                                </p>
                            </Card>
                        </div>

                        <h2 className="my-3 font-medium text-gray-900 dark:text-gray-50">{t("messagesDelivered")}</h2>
                        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                            <Card className={adminStatCardClass}>
                                <h3 className="mb-1 text-sm">{t("viber")}</h3>
                                <p className="mt-auto font-semibold text-2xl">
                                    {numberFmt.format(analytics.totalViberMessagesDelivered)}{" "}
                                    <span className="text-xs text-gray-500">{t("period.range")}</span>
                                </p>
                            </Card>

                            <Card className={adminStatCardClass}>
                                <h3 className="mb-1 text-sm">{t("sms")}</h3>
                                <p className="mt-auto font-semibold text-2xl">
                                    {numberFmt.format(analytics.totalSmsMessagesDelivered)}{" "}
                                    <span className="text-xs text-gray-500">{t("period.range")}</span>
                                </p>
                            </Card>
                        </div>

                        <h2 className="my-3 font-medium text-gray-900 dark:text-gray-50">{t("financial")}</h2>
                        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                            <Card className={adminStatCardClass}>
                                <h3 className="mb-1 text-sm">{t("revenue")}</h3>
                                <p className="mt-auto font-semibold text-2xl">
                                    {moneyFmt.format(analytics.totalRevenue)}{" "}
                                    <span className="text-xs text-gray-500">{t("period.range")}</span>
                                </p>
                            </Card>

                            <Card className={adminStatCardClass}>
                                <h3 className="mb-1 text-sm">{t("profit")}</h3>
                                <p
                                    className={`mt-auto font-semibold text-2xl${analytics.totalProfit > 0 ? " text-green-500" : ""}`}
                                >
                                    {analytics.totalProfit > 0 ? "+" : ""}
                                    {moneyFmt.format(analytics.totalProfit)}{" "}
                                    <span className="text-xs text-gray-500">{t("period.range")}</span>
                                </p>
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
