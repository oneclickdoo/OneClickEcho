"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import {
    Cell,
    ColumnDef,
    ColumnHelper,
    ColumnFiltersState,
    createColumnHelper,
    getCoreRowModel,
    PaginationState,
    SortingState,
    useReactTable
} from "@tanstack/react-table";
import { useLocale, useTranslations } from "next-intl";

import { Badge } from "@/components/tremor/Badge";
import { DropdownMenuItem } from "@/components/tremor/Dropdown";

import { useAuth } from "@/context/AuthContext";

import { CampaignsDto, fetchCampaignsData } from "@/elements/dashboard/campaigns/fetchData";

import { CampaignStatus } from "@/lib/enums";

import { DataTableRowActions } from "@/components/data-table/DataTableRowActions";
import { GenericTable } from "@/components/generics/GenericTable";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";
import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";
import { Filterbar } from "@/components/data-table/DataTableFilterbar";
import { DataTableFilter } from "@/components/data-table/DataTableFilter";
import { GenericSearchbar } from "@/components/generics/GenericSearchbar";
import { FilterManager } from "@/components/filtering/FilterManager";

import { cloneCampaign } from "../../singleCampaign/fetchData";

type BadgeVariant = "default" | "neutral" | "success" | "error" | "warning";

const numericStatusToCommonKey: Record<number, string> = {
    [CampaignStatus.Draft]: "Common.campaignStatus.draft",
    [CampaignStatus.Queued]: "Common.campaignStatus.queued",
    [CampaignStatus.InProgress]: "Common.campaignStatus.inProgress",
    [CampaignStatus.Done]: "Common.campaignStatus.done"
};

export function CampaignsTable() {
    const t = useTranslations("Campaigns");
    const tCommon = useTranslations("Common");
    const tEnums = useTranslations("Enums"); // ✅ ovde su ti stvarni prevodi za campaignStatus
    const locale = useLocale();

    const formatDateTime = (value?: string | Date) => {
        if (!value) return "";
        const d = value instanceof Date ? value : new Date(value);
        if (Number.isNaN(d.getTime())) return "";

        return new Intl.DateTimeFormat(locale, {
            day: "numeric",
            month: "long",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            hour12: false
        }).format(d);
    };

    // returns backend key (string), either passed as-is or from numeric enum
    const toCommonStatusKey = (value: unknown): string => {
        if (typeof value === "string") return value;
        if (typeof value === "number") return numericStatusToCommonKey[value] ?? String(value);
        return "";
    };

    // "Common.campaignStatus.inProgress" -> "campaignStatus.inProgress"
    const toEnumSubKey = (value: unknown): string => {
        const raw = toCommonStatusKey(value);
        if (!raw) return "";

        // allow values coming as already "campaignStatus.inProgress"
        if (raw.startsWith("campaignStatus.")) return raw;

        // map Common.* or Enums.* to subkey
        if (raw.startsWith("Common.")) return raw.slice("Common.".length);
        if (raw.startsWith("Enums.")) return raw.slice("Enums.".length);

        // last resort: if it contains "campaignStatus." somewhere, cut from there
        const idx = raw.indexOf("campaignStatus.");
        return idx >= 0 ? raw.slice(idx) : "";
    };

    const translateStatus = (value: unknown): string => {
        const subKey = toEnumSubKey(value); // "campaignStatus.inProgress"
        if (!subKey) return tCommon("unknown") ?? "Unknown";

        try {
            // ✅ uses sr.json: Enums.campaignStatus.inProgress
            return tEnums(subKey);
        } catch {
            // fallback (no raw key in UI)
            return tCommon("unknown") ?? "Unknown";
        }
    };

    const getStatusVariant = (value: unknown): BadgeVariant => {
        const subKey = toEnumSubKey(value); // "campaignStatus.done"
        if (!subKey) return "default";

        if (subKey.endsWith(".done")) return "success";
        if (subKey.endsWith(".failed") || subKey.endsWith(".error")) return "error";
        if (subKey.endsWith(".inProgress") || subKey.endsWith(".queued")) return "warning";
        if (subKey.endsWith(".draft")) return "neutral";
        return "default";
    };

    const isDoneStatus = (value: unknown): boolean => {
        const raw = toCommonStatusKey(value);
        return raw === "Common.campaignStatus.done" || value === CampaignStatus.Done;
    };

    const [sorting, setSorting] = useState<SortingState>([{ id: "createdAt", desc: true }]);
    const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 10 });
    const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
    const searchbarRef = useRef<{ resetInput: () => void }>(null);

    const { dashboardManager, authFetch } = useAuth();
    const [filterManager] = useState(() => new FilterManager(dashboardManager!.currentCompany!.companyId));

    const dataQuery = useQuery({
        queryKey: ["data", pagination, sorting, filterManager.filters],
        queryFn: () => fetchCampaignsData(pagination, sorting, filterManager.generate(), authFetch, tCommon),
        placeholderData: keepPreviousData
    });

    const duplicateCampaign = async (campaignId: string) => {
        try {
            await cloneCampaign(campaignId, authFetch);
            dataQuery.refetch();
        } catch (e) {
            console.log(e);
        }
    };

    // backend expects Common.* values (since it returns Common.*), so keep values like that
    const statusFilterOptions = useMemo(
        () => [
            { value: "Common.campaignStatus.draft", label: tEnums("campaignStatus.draft") },
            { value: "Common.campaignStatus.queued", label: tEnums("campaignStatus.queued") },
            { value: "Common.campaignStatus.inProgress", label: tEnums("campaignStatus.inProgress") },
            { value: "Common.campaignStatus.done", label: tEnums("campaignStatus.done") }
        ],
        [tEnums]
    );

    const columnHelper = createColumnHelper<CampaignsDto>();

    // @ts-ignore
    const tableColumns: ColumnDef<CampaignsDto, ColumnHelper<CampaignsDto>>[] = useMemo(
        () => [
            columnHelper.accessor("name", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.name")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("columns.name") },
                cell: ({ row }: { row: { original: CampaignsDto } }) => (
                    <Link href={`/campaigns/${row.original.campaignId}`} className="text-blue-500 hover:text-blue-600">
                        {row.original.name}
                    </Link>
                )
            }),

            columnHelper.accessor("status", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.status")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("columns.status") },
                cell: ({ row }: { row: { original: CampaignsDto } }) => {
                    const variant = getStatusVariant(row.original.status);
                    const label = translateStatus(row.original.status);

                    return (
                        <Badge variant={variant} className="rounded-full text-xs">
                            {label}
                        </Badge>
                    );
                }
            }),

            columnHelper.accessor("createdAt", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.createdAt")} />,
                enableSorting: true,
                meta: { className: "tabular-nums", displayName: t("columns.createdAt") },
                cell: ({ row }: { row: { original: CampaignsDto } }) => <div>{formatDateTime(row.original.createdAt)}</div>
            }),

            columnHelper.accessor("sendingDatetime", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.sentAt")} />,
                enableSorting: true,
                meta: { className: "tabular-nums", displayName: t("columns.sentAt") },
                cell: ({ row }: { row: { original: CampaignsDto } }) =>
                    isDoneStatus(row.original.status) ? <div>{formatDateTime(row.original.sendingDatetime)}</div> : <div />
            }),

            columnHelper.display({
                id: "edit",
                header: t("columns.actions"),
                enableSorting: false,
                enableHiding: false,
                meta: { className: "text-center", displayName: t("columns.actions") },
                cell: ({ row }) => (
                    <DataTableRowActions row={row}>
                        <DropdownMenuItem
                            onClick={() => duplicateCampaign(row.original.campaignId)}
                            className="text-orange-600 dark:text-orange-500"
                        >
                            {t("actions.duplicate")}
                        </DropdownMenuItem>
                    </DataTableRowActions>
                )
            })
        ],
        [t, locale, tEnums]
    );

    const table = useReactTable({
        data: dataQuery.data?.rows ?? [],
        columns: tableColumns,
        getCoreRowModel: getCoreRowModel(),
        state: { pagination, sorting, columnFilters },
        onSortingChange: setSorting,
        onPaginationChange: setPagination,
        onColumnFiltersChange: setColumnFilters,
        manualPagination: true,
        manualFiltering: true,
        manualSorting: true,
        enableMultiSort: true,
        rowCount: dataQuery.data?.rowCount ?? 0
    });

    useEffect(() => {
        if (columnFilters.length === 0 && Object.keys(filterManager.filters).length > 0) {
            filterManager.clearFilters();
            void searchbarRef.current?.resetInput();
            dataQuery.refetch();
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [columnFilters]);

    return (
        <div className="space-y-3">
            <Filterbar table={table} isFiltered={Object.keys(filterManager.filters).length > 0}>
                {table.getColumn("status")?.getIsVisible() && (
                    <DataTableFilter
                        column={table.getColumn("status")}
                        title={t("filters.status")}
                        options={statusFilterOptions}
                        type="checkbox"
                        onFilter={(filter, column, type) => {
                            if (filter) filterManager.setFilter(column, { type, value: filter });
                            else filterManager.removeFilter(column);
                            dataQuery.refetch();
                        }}
                    />
                )}

                {table.getColumn("createdAt")?.getIsVisible() && (
                    <DataTableFilter
                        column={table.getColumn("createdAt")}
                        title={t("filters.createdAt")}
                        type="date"
                        onFilter={(filter, column, type) => {
                            if (filter) filterManager.setFilter(column, { type, value: filter });
                            else filterManager.removeFilter(column);
                            dataQuery.refetch();
                        }}
                    />
                )}

                {table.getColumn("sendingDatetime")?.getIsVisible() && (
                    <DataTableFilter
                        column={table.getColumn("sendingDatetime")}
                        title={t("filters.sentAt")}
                        type="date"
                        onFilter={(filter, column, type) => {
                            if (filter) filterManager.setFilter(column, { type, value: filter });
                            else filterManager.removeFilter(column);
                            dataQuery.refetch();
                        }}
                    />
                )}

                {table.getColumn("name")?.getIsVisible() && (
                    <GenericSearchbar
                        ref={searchbarRef}
                        placeholder={t("search.fieldName")}
                        onSearchChange={(value) => {
                            if (value !== "") filterManager.setFilter("name", { type: "search", value });
                            else filterManager.removeFilter("name");
                            dataQuery.refetch();
                        }}
                    />
                )}
            </Filterbar>

            <div className="relative overflow-hidden overflow-x-auto">
                <GenericTable table={table} tableColumns={tableColumns} />
            </div>

            <DataTablePagination table={table} pagination={pagination} />
        </div>
    );
}
