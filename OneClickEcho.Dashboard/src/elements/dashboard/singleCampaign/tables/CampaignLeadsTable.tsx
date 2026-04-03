"use client";

import { forwardRef, useImperativeHandle, useMemo, useRef, useState } from "react";

import type { Cell, ColumnDef, PaginationState, SortingState } from "@tanstack/react-table";
import { createColumnHelper, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { useLocale, useTranslations } from "next-intl";

import type { CampaignLeadDto } from "@/elements/dashboard/singleCampaign/fetchData";
import { fetchLeadsData } from "@/elements/dashboard/singleCampaign/fetchData";

import { Filterbar } from "@/components/data-table/DataTableFilterbar";
import { GenericSearchbar } from "@/components/generics/GenericSearchbar";
import { GenericTable } from "@/components/generics/GenericTable";
import { DataTableRowActions } from "@/components/data-table/DataTableRowActions";
import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";
import { DataTableFilter } from "@/components/data-table/DataTableFilter";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";

import { Badge } from "@/components/tremor/Badge";
import { DropdownMenuItem } from "@/components/tremor/Dropdown";
import { Button } from "@/components/tremor/Button";

import { useAuth } from "@/context/AuthContext";
import { FilterManager } from "@/components/filtering/FilterManager";

import { CampaignLeadsTableModal } from "@/elements/dashboard/singleCampaign/modals/CampaignLeadsTableModal";
import { LeadGender } from "@/lib/enums";

export type CampaignLeadsTableHandle = {
    refetchData: () => void;
};

// ✅ BITNO: umesto Record<string, never> stavi {} (da key/ref rade normalno)
export const CampaignLeadsTable = forwardRef<CampaignLeadsTableHandle, {}>((_, ref) => {
    const t = useTranslations("SingleCampaign.Tables.CampaignLeads");
    const tCommon = useTranslations("Common");
    const locale = useLocale();

    const [sorting, setSorting] = useState<SortingState>([{ id: "createdAt", desc: true }]);
    const [pagination, setPagination] = useState<PaginationState>({
        pageIndex: 0,
        pageSize: 10
    });

    const searchbarRef = useRef<{ resetInput: () => void }>(null);

    const { dashboardManager, authFetch } = useAuth();
    const [filterManager] = useState(() => new FilterManager(dashboardManager!.currentCompany!.companyId));

    const dataQuery = useQuery({
        queryKey: ["campaign-leads", pagination, sorting, filterManager.filters],
        queryFn: () => fetchLeadsData(pagination, sorting, filterManager.generate(), null, authFetch),
        placeholderData: keepPreviousData
    });

    useImperativeHandle(ref, () => ({
        refetchData: () => dataQuery.refetch()
    }));

    const drawerRef = useRef<{ useDrawer: (row: CampaignLeadDto) => void }>(null);

    const drawerOpen = (row: CampaignLeadDto) => {
        drawerRef.current?.useDrawer(row);
    };

    const resetAllFilters = () => {
        filterManager.clearFilters();
        searchbarRef.current?.resetInput();
        dataQuery.refetch();
    };

    const columnHelper = createColumnHelper<CampaignLeadDto>();

    // ✅ Forsiraj sr-RS kad je locale "sr" (ili "sr-*" -> sr-RS)
    const resolvedLocale = useMemo(() => {
        if (locale === "sr" || locale.startsWith("sr-")) return "sr-RS";
        if (locale === "en" || locale.startsWith("en-")) return "en-US";
        return locale;
    }, [locale]);

    // ✅ DOB formatter koji daje dd.MM.yyyy (stabilno, bez “US” varijacije)
    const dobPartsFormatter = useMemo(() => {
        return new Intl.DateTimeFormat(resolvedLocale, {
            year: "numeric",
            month: "2-digit",
            day: "2-digit"
        });
    }, [resolvedLocale]);

    const formatDob = (value: unknown) => {
        if (!value) return "";

        const d = value instanceof Date ? value : new Date(value as any);
        if (Number.isNaN(d.getTime())) return "";

        // formatToParts -> ručno složi dd.MM.yyyy
        const parts = dobPartsFormatter.formatToParts(d);
        const day = parts.find((p) => p.type === "day")?.value ?? "";
        const month = parts.find((p) => p.type === "month")?.value ?? "";
        const year = parts.find((p) => p.type === "year")?.value ?? "";

        if (!day || !month || !year) return dobPartsFormatter.format(d);

        return `${day}.${month}.${year}`;
    };

    const tableColumns = useMemo<ColumnDef<CampaignLeadDto, any>[]>(() => {
        return [
            columnHelper.accessor("phoneNumber", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("phoneNumber")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("phoneNumber") }
            }),

            columnHelper.accessor("email", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("email")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("email") }
            }),

            columnHelper.accessor("isBlacklisted", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("blacklisted")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("blacklisted") },
                cell: ({
                    row
                }: {
                    row: { getVisibleCells: () => Cell<CampaignLeadDto, unknown>[]; original: CampaignLeadDto };
                }) => (row.original.isBlacklisted ? <Badge variant="error">{t("blacklisted")}</Badge> : null)
            }),

            columnHelper.accessor("firstName", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("firstName")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("firstName") }
            }),

            columnHelper.accessor("lastName", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("lastName")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("lastName") }
            }),

            columnHelper.accessor("gender", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("gender")} />,
                enableSorting: false,
                meta: { className: "text-left", displayName: t("gender") },
                cell: ({
                    row
                }: {
                    row: { getVisibleCells: () => Cell<CampaignLeadDto, unknown>[]; original: CampaignLeadDto };
                }) => {
                    const g = row.original.gender;

                    if (g === LeadGender.Male) return <div>{tCommon("leadGender.male")}</div>;
                    if (g === LeadGender.Female) return <div>{tCommon("leadGender.female")}</div>;

                    return <div />;
                }
            }),

            columnHelper.accessor("dateOfBirth", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("dob")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("dob") },
                cell: ({ row }: { row: { original: CampaignLeadDto } }) => (
                    <div>{formatDob(row.original.dateOfBirth)}</div>
                )
            }),

            columnHelper.accessor("city", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("city")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("city") }
            }),

            columnHelper.accessor("country", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("country")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: t("country") }
            }),

            columnHelper.display({
                id: "edit",
                header: t("edit"),
                enableSorting: false,
                enableHiding: false,
                meta: { className: "text-left", displayName: t("edit") },
                cell: ({ row }) => (
                    <DataTableRowActions row={row}>
                        <DropdownMenuItem onClick={() => drawerOpen(row.original)}>{t("edit")}</DropdownMenuItem>
                    </DataTableRowActions>
                )
            })
        ];
    }, [columnHelper, formatDob, t, tCommon]);

    const table = useReactTable({
        data: dataQuery.data?.rows ?? [],
        columns: tableColumns,
        getCoreRowModel: getCoreRowModel(),
        state: { pagination, sorting },
        onSortingChange: setSorting,
        onPaginationChange: setPagination,
        manualPagination: true,
        manualFiltering: true,
        manualSorting: true,
        enableMultiSort: true,
        rowCount: dataQuery.data?.rowCount ?? 0
    });

    return (
        <div className="space-y-3">
            <Filterbar table={table} isFiltered={Object.keys(filterManager.filters).length > 0}>
                {table.getColumn("phoneNumber")?.getIsVisible() && (
                    <GenericSearchbar
                        ref={searchbarRef}
                        placeholder={t("searchPhonePlaceholder")}
                        onSearchChange={(value) => {
                            if (value !== "") {
                                filterManager.setFilter("phoneNumber", { type: "search", value });
                            } else {
                                filterManager.removeFilter("phoneNumber");
                            }
                            dataQuery.refetch();
                        }}
                    />
                )}

                {table.getColumn("isBlacklisted")?.getIsVisible() && (
                    <DataTableFilter
                        column={table.getColumn("isBlacklisted")}
                        title={t("blacklisted")}
                        options={[
                            { label: tCommon("yes"), value: "1" },
                            { label: tCommon("no"), value: "0" }
                        ]}
                        type="select"
                        onFilter={(filter, column, type) => {
                            if (filter) {
                                filterManager.setFilter(column, { type, value: filter });
                            } else {
                                filterManager.removeFilter(column);
                            }
                            dataQuery.refetch();
                        }}
                    />
                )}

                <Button variant="secondary" className="h-8" onClick={resetAllFilters}>
                    {t("resetFilters")}
                </Button>
            </Filterbar>

            <div className="relative overflow-hidden overflow-x-auto">
                <GenericTable table={table} tableColumns={tableColumns} />
            </div>

            <DataTablePagination table={table} pagination={pagination} />

            <CampaignLeadsTableModal refreshData={dataQuery.refetch} ref={drawerRef} />
        </div>
    );
});

CampaignLeadsTable.displayName = "CampaignLeadsTable";
