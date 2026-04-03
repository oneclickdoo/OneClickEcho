"use client";

import { forwardRef, useImperativeHandle, useMemo, useRef, useState } from "react";
import Link from "next/link";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import type { Cell, ColumnDef, PaginationState, SortingState } from "@tanstack/react-table";
import { createColumnHelper, getCoreRowModel, useReactTable } from "@tanstack/react-table";

import { useLocale, useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";

import { FilterManager } from "@/components/filtering/FilterManager";
import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";
import { Filterbar } from "@/components/data-table/DataTableFilterbar";
import { DataTableFilter } from "@/components/data-table/DataTableFilter";
import { GenericSearchbar } from "@/components/generics/GenericSearchbar";
import { GenericTable } from "@/components/generics/GenericTable";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";

import { Button } from "@/components/tremor/Button";

import type { CollectionDto } from "@/elements/dashboard/collections/fetchData";
import type { LeadCollectionDto } from "@/elements/dashboard/singleCampaign/fetchData";

import type { IFetch, PaginatedItems } from "@/lib/networking";
import type { IFetchResult } from "@/components/table/TableGeneric";

export type CampaignLeadsTableAssignHandle = {
    refetchDataQuery: () => void;
};

type AssignedItems = {
    // ✅ dopuštamo readonly, jer query često vraća readonly nizove
    rows: readonly LeadCollectionDto[];
    pageCount: number;
    rowCount: number;
};

export const CampaignLeadsTableAssign = forwardRef<
    CampaignLeadsTableAssignHandle,
    {
        campaignId: string;
        onAssign: (collectionId: string) => void;
        assignedItems: AssignedItems | undefined;
    }
>(({ onAssign, assignedItems }, ref) => {
    const t = useTranslations("SingleCampaign.Tables.CampaignLeadsAssign");
    const locale = useLocale();

    const [sorting, setSorting] = useState<SortingState>([{ id: "createdAt", desc: true }]);
    const [pagination, setPagination] = useState<PaginationState>({
        pageIndex: 0,
        pageSize: 10
    });

    const { dashboardManager, authFetch } = useAuth();
    const [filterManager] = useState(() => new FilterManager(dashboardManager!.currentCompany!.companyId));

    // ✅ locale-aware datetime formatter (bez moment-a)
    const dateTimeFormatter = useMemo(() => {
        const resolvedLocale =
            locale === "sr" || locale.startsWith("sr-") ? "sr-RS" : locale === "en" || locale.startsWith("en-") ? "en-US" : locale;

        return new Intl.DateTimeFormat(resolvedLocale, {
            year: "numeric",
            month: "long",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        });
    }, [locale]);

    const formatDateTime = (value: unknown) => {
        if (!value) return "";
        const d = value instanceof Date ? value : new Date(value as any);
        if (Number.isNaN(d.getTime())) return "";
        return dateTimeFormatter.format(d);
    };

    const searchLeadCollections = async (
        options: PaginationState,
        sortingState: SortingState,
        filtering: string,
        authFetchFn: IFetch
    ): Promise<IFetchResult<CollectionDto>> => {
        const url = new URL("/api/LeadCollection", window.location.origin);

        url.searchParams.append("Page", (options.pageIndex + 1).toString());
        url.searchParams.append("PageSize", options.pageSize.toString());
        url.searchParams.append("OrderBy", sortingState.map((s) => `${s.id} ${s.desc ? "desc" : "asc"}`).join(","));
        url.searchParams.append("Filter", filtering);

        const response = await authFetchFn(url.toString(), {
            headers: {
                Accept: "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Network response was not ok");
        }

        const data: PaginatedItems<CollectionDto> = await response.json();

        return {
            // ✅ mutable copy
            rows: [...data.items],
            pageCount: data.totalPages,
            rowCount: data.totalCount
        };
    };

    const dataQuery = useQuery({
        queryKey: ["dataAllCollections", pagination, sorting, filterManager.filters, assignedItems],
        queryFn: () => searchLeadCollections(pagination, sorting, filterManager.generate(), authFetch),
        placeholderData: keepPreviousData
    });

    useImperativeHandle(ref, () => ({
        refetchDataQuery: () => dataQuery.refetch()
    }));

    const listOfAssigned = useMemo<string[]>(
        () => (assignedItems?.rows ?? []).map((item) => item.leadCollectionId),
        [assignedItems]
    );

    const searchbarRef = useRef<{ resetInput: () => void }>(null);

    const columnHelper = createColumnHelper<CollectionDto>();

    const tableColumns = useMemo<ColumnDef<CollectionDto, any>[]>(() => {
        return [
            columnHelper.accessor("collectionName", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("name")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("name")
                },
                cell: ({
                    row
                }: {
                    row: { getVisibleCells: () => Cell<CollectionDto, any>[]; original: CollectionDto };
                }) => (
                    <Link
                        href={`/${locale}/collections/${row.original.leadCollectionId}`}
                        className="text-blue-500 hover:text-blue-600"
                    >
                        {row.original.collectionName}
                    </Link>
                )
            }),

            columnHelper.accessor("createdAt", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("createdAt")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("createdAt")
                },
                cell: ({
                    row
                }: {
                    row: { getVisibleCells: () => Cell<CollectionDto, any>[]; original: CollectionDto };
                }) => <div>{formatDateTime(row.original.createdAt)}</div>
            }),

            columnHelper.display({
                id: "assign",
                header: t("assign"),
                enableSorting: false,
                enableHiding: false,
                meta: {
                    className: "text-center",
                    displayName: t("assign")
                },
                cell: ({ row }) => {
                    const disabled = listOfAssigned.includes(row.original.leadCollectionId);

                    return (
                        <Button
                            className="h-6"
                            disabled={disabled}
                            onClick={() => {
                                if (disabled) return;
                                onAssign(row.original.leadCollectionId);
                            }}
                        >
                            {t("assign")}
                        </Button>
                    );
                }
            })
        ];
    }, [columnHelper, formatDateTime, listOfAssigned, locale, onAssign, t]);

    const table = useReactTable({
        data: [...(dataQuery.data?.rows ?? [])],
        columns: tableColumns,
        getCoreRowModel: getCoreRowModel(),
        state: {
            pagination,
            sorting
        },
        onSortingChange: setSorting,
        onPaginationChange: setPagination,
        manualPagination: true,
        manualFiltering: true,
        manualSorting: true,
        enableMultiSort: true,
        rowCount: dataQuery.data?.rowCount ?? 0
    });

    const resetAllFilters = () => {
        filterManager.clearFilters();
        table.resetColumnFilters();
        searchbarRef.current?.resetInput();
        dataQuery.refetch();
    };

    return (
        <div className="space-y-3">
            <Filterbar table={table} isFiltered={Object.keys(filterManager.filters).length > 0}>
                {table.getColumn("createdAt")?.getIsVisible() && (
                    <DataTableFilter
                        column={table.getColumn("createdAt")}
                        title={t("createdAt")}
                        type="date"
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

                {table.getColumn("collectionName")?.getIsVisible() && (
                    <GenericSearchbar
                        ref={searchbarRef}
                        placeholder={t("searchNamePlaceholder")}
                        onSearchChange={(value) => {
                            if (value !== "") {
                                filterManager.setFilter("collectionName", { type: "search", value });
                            } else {
                                filterManager.removeFilter("collectionName");
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
        </div>
    );
});

CampaignLeadsTableAssign.displayName = "CampaignLeadsTableAssign";
