"use client";

import { useCallback, useMemo, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import type { ColumnDef, PaginationState } from "@tanstack/react-table";
import { createColumnHelper, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import { useLocale, useTranslations } from "next-intl";

import { withLocale } from "@/lib/routing";
import { CampaignLeadSMSStatus, CampaignLeadViberStatusCollection, CampaignStatus } from "@/lib/enums";
import { useAuth } from "@/context/AuthContext";

import { Button } from "@/components/tremor/Button";
import { Card } from "@/components/tremor/Card";
import { GenericTable } from "@/components/generics/GenericTable";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";
import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";

import {
    type CampaignLeadReportRowDto,
    fetchCampaignLeadReport,
    getCampaignById,
    type CampaignDto
} from "@/elements/dashboard/singleCampaign/fetchData";

const columnHelper = createColumnHelper<CampaignLeadReportRowDto>();

const VIBER_FILTER_VALUES: number[] = [
    CampaignLeadViberStatusCollection.NotSent,
    CampaignLeadViberStatusCollection.Received,
    CampaignLeadViberStatusCollection.Pending,
    CampaignLeadViberStatusCollection.Delivered,
    CampaignLeadViberStatusCollection.Seen,
    CampaignLeadViberStatusCollection.Undelivered,
    CampaignLeadViberStatusCollection.Expired,
    CampaignLeadViberStatusCollection.Clicked
];

const SMS_FILTER_VALUES: number[] = Array.from(
    new Set(Object.values(CampaignLeadSMSStatus).filter((x): x is number => typeof x === "number"))
).sort((a, b) => a - b);

export default function CampaignLeadReportPage() {
    const params = useParams<{ campaignId: string }>();
    const campaignId = params.campaignId;
    const locale = useLocale();

    const t = useTranslations("CampaignLeadReport");
    const tCommon = useTranslations("Common");
    const tViberLabels = useTranslations("CampaignLeadReport.viberFilterLabels");
    const tSmsLabels = useTranslations("CampaignLeadReport.smsFilterLabels");

    const { authFetch } = useAuth();

    const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 10 });

    const [draftPhone, setDraftPhone] = useState("");
    const [draftViber, setDraftViber] = useState("");
    const [draftSms, setDraftSms] = useState("");
    const [draftUnsub, setDraftUnsub] = useState("");

    const [appliedPhone, setAppliedPhone] = useState("");
    const [appliedViber, setAppliedViber] = useState("");
    const [appliedSms, setAppliedSms] = useState("");
    const [appliedUnsub, setAppliedUnsub] = useState("");

    const campaignQuery = useQuery({
        queryKey: ["campaign", campaignId],
        queryFn: () => getCampaignById(campaignId, authFetch),
        enabled: Boolean(campaignId)
    });

    const campaign = campaignQuery.data as CampaignDto | undefined;

    const parseOptionalInt = (raw: string): number | null => {
        if (!raw) return null;
        const n = Number(raw);
        return Number.isFinite(n) ? n : null;
    };

    const parseUnsub = (raw: string): boolean | null => {
        if (raw === "true") return true;
        if (raw === "false") return false;
        return null;
    };

    const viberForApi = useMemo(() => parseOptionalInt(appliedViber), [appliedViber]);
    const smsForApi = useMemo(() => parseOptionalInt(appliedSms), [appliedSms]);
    const unsubForApi = useMemo(() => parseUnsub(appliedUnsub), [appliedUnsub]);

    const reportEnabled = Boolean(campaignId) && campaign?.status === CampaignStatus.Done;

    const reportQuery = useQuery({
        queryKey: [
            "campaign-lead-report",
            campaignId,
            pagination.pageIndex,
            pagination.pageSize,
            appliedPhone,
            appliedViber,
            appliedSms,
            appliedUnsub
        ],
        queryFn: () =>
            fetchCampaignLeadReport(
                campaignId,
                pagination,
                appliedPhone,
                viberForApi,
                smsForApi,
                unsubForApi,
                authFetch
            ),
        enabled: reportEnabled,
        placeholderData: keepPreviousData
    });

    const applyFilters = useCallback(() => {
        setAppliedPhone(draftPhone);
        setAppliedViber(draftViber);
        setAppliedSms(draftSms);
        setAppliedUnsub(draftUnsub);
        setPagination((p) => ({ ...p, pageIndex: 0 }));
    }, [draftPhone, draftViber, draftSms, draftUnsub]);

    const resetFilters = useCallback(() => {
        setDraftPhone("");
        setDraftViber("");
        setDraftSms("");
        setDraftUnsub("");
        setAppliedPhone("");
        setAppliedViber("");
        setAppliedSms("");
        setAppliedUnsub("");
        setPagination((p) => ({ ...p, pageIndex: 0 }));
    }, []);

    const columns = useMemo<ColumnDef<CampaignLeadReportRowDto, any>[]>(
        () => [
            columnHelper.accessor("phoneNumber", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.phoneNumber")} />,
                meta: { className: "text-left", displayName: t("columns.phoneNumber") }
            }),
            columnHelper.accessor("viberStatus", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.viberStatus")} />,
                meta: { className: "tabular-nums", displayName: t("columns.viberStatus") }
            }),
            columnHelper.accessor("viberStatusDescription", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.viberDescription")} />,
                cell: ({ getValue }) => <span className="max-w-xs truncate">{getValue() ?? "—"}</span>,
                meta: { className: "text-left max-w-xs", displayName: t("columns.viberDescription") }
            }),
            columnHelper.accessor("smsStatus", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.smsStatus")} />,
                meta: { className: "tabular-nums", displayName: t("columns.smsStatus") }
            }),
            columnHelper.accessor("smsStatusDescription", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.smsDescription")} />,
                cell: ({ getValue }) => <span className="max-w-xs truncate">{getValue() ?? "—"}</span>,
                meta: { className: "text-left max-w-xs", displayName: t("columns.smsDescription") }
            }),
            columnHelper.accessor("isUnsubscribed", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.unsubscribed")} />,
                cell: ({ getValue }) => (getValue() ? tCommon("yes") : tCommon("no")),
                meta: { className: "text-left", displayName: t("columns.unsubscribed") }
            })
        ],
        [t, tCommon]
    );

    const rowCount = reportQuery.data?.rowCount ?? 0;

    const table = useReactTable({
        data: reportQuery.data?.rows ?? [],
        columns,
        getCoreRowModel: getCoreRowModel(),
        state: { pagination },
        onPaginationChange: setPagination,
        manualPagination: true,
        pageCount: reportQuery.data?.pageCount ?? 0,
        rowCount
    });

    if (campaignQuery.isError) {
        return (
            <div className="p-6">
                <p className="text-red-600">{t("errorLoad")}</p>
                <Link href={withLocale(locale, `/campaigns/${campaignId}`)} className="mt-4 inline-block text-blue-600">
                    {t("backToCampaign")}
                </Link>
            </div>
        );
    }

    if (campaign && campaign.status !== CampaignStatus.Done) {
        return (
            <div className="space-y-4 p-6">
                <p className="text-gray-700 dark:text-gray-300">{t("errorNotDone")}</p>
                <Link href={withLocale(locale, `/campaigns/${campaignId}`)}>
                    <Button variant="secondary">{t("backToCampaign")}</Button>
                </Link>
            </div>
        );
    }

    return (
        <div className="space-y-6 p-4 md:p-6">
            <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                <div>
                    <h1 className="text-lg font-semibold text-gray-900 dark:text-gray-50">
                        {campaign ? t("title", { name: campaign.name }) : tCommon("loading")}
                    </h1>
                    <p className="text-sm text-gray-600 dark:text-gray-400">{t("subtitle")}</p>
                </div>
                <Link href={withLocale(locale, `/campaigns/${campaignId}`)}>
                    <Button variant="secondary">{t("backToCampaign")}</Button>
                </Link>
            </div>

            <Card className="p-4">
                <div className="flex flex-col gap-4 lg:flex-row lg:flex-wrap lg:items-end">
                    <div className="min-w-[200px] flex-1">
                        <label className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
                            {t("searchPhone")}
                        </label>
                        <input
                            type="text"
                            value={draftPhone}
                            onChange={(e) => setDraftPhone(e.target.value)}
                            placeholder={t("searchPhonePlaceholder")}
                            className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none focus:border-indigo-500 dark:border-gray-700 dark:bg-gray-950 dark:text-gray-50"
                        />
                    </div>

                    <div className="min-w-[180px]">
                        <label className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
                            {t("filterViber")}
                        </label>
                        <select
                            value={draftViber}
                            onChange={(e) => setDraftViber(e.target.value)}
                            className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-950 dark:text-gray-50"
                        >
                            <option value="">{t("filterAll")}</option>
                            {VIBER_FILTER_VALUES.map((v) => (
                                <option key={v} value={String(v)}>
                                    {tViberLabels(String(v))}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="min-w-[200px]">
                        <label className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
                            {t("filterSms")}
                        </label>
                        <select
                            value={draftSms}
                            onChange={(e) => setDraftSms(e.target.value)}
                            className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-950 dark:text-gray-50"
                        >
                            <option value="">{t("filterAll")}</option>
                            {SMS_FILTER_VALUES.map((v) => (
                                <option key={v} value={String(v)}>
                                    {tSmsLabels(String(v))}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="min-w-[160px]">
                        <label className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
                            {t("filterUnsubscribed")}
                        </label>
                        <select
                            value={draftUnsub}
                            onChange={(e) => setDraftUnsub(e.target.value)}
                            className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-950 dark:text-gray-50"
                        >
                            <option value="">{t("filterAll")}</option>
                            <option value="true">{t("filterYes")}</option>
                            <option value="false">{t("filterNo")}</option>
                        </select>
                    </div>

                    <div className="flex gap-2">
                        <Button type="button" onClick={applyFilters}>
                            {t("applyFilters")}
                        </Button>
                        <Button type="button" variant="secondary" onClick={resetFilters}>
                            {t("resetFilters")}
                        </Button>
                    </div>
                </div>
            </Card>

            {reportQuery.isError ? (
                <div className="space-y-1">
                    <p className="text-red-600">{t("errorLoad")}</p>
                    {reportQuery.error instanceof Error && reportQuery.error.message ? (
                        <p className="text-sm text-red-600/90 dark:text-red-400">{reportQuery.error.message}</p>
                    ) : null}
                </div>
            ) : (
                <>
                    {!reportQuery.isFetching && rowCount === 0 ? (
                        <p className="text-sm text-gray-600 dark:text-gray-400">{t("empty")}</p>
                    ) : null}

                    <div className="relative overflow-hidden overflow-x-auto">
                        <GenericTable table={table} tableColumns={columns} />
                    </div>

                    <DataTablePagination table={table} pagination={pagination} />
                </>
            )}
        </div>
    );
}
