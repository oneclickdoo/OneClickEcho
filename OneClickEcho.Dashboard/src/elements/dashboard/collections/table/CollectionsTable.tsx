"use client";

import { useMemo, useRef, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { useTranslations } from "next-intl";
import { withLocale } from "@/lib/routing";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import {
    Cell,
    ColumnDef,
    ColumnHelper,
    createColumnHelper,
    getCoreRowModel,
    PaginationState,
    SortingState,
    useReactTable
} from "@tanstack/react-table";

import { useAuth } from "@/context/AuthContext";

import { FilterManager } from "@/components/filtering/FilterManager";
import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";
import { Filterbar } from "@/components/data-table/DataTableFilterbar";
import { DataTableFilter } from "@/components/data-table/DataTableFilter";
import { GenericSearchbar } from "@/components/generics/GenericSearchbar";
import { GenericTable } from "@/components/generics/GenericTable";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";
import { DataTableRowActions } from "@/components/data-table/DataTableRowActions";

import { CollectionDto, fetchCollectionsData } from "../fetchData";

export function CollectionsTable() {
    const t = useTranslations("Collections");
    const tCommon = useTranslations("Common");

    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const [sorting, setSorting] = useState<SortingState>([{ id: "createdAt", desc: true }]);
    const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 10 });

    const router = useRouter();
    const { dashboardManager, authFetch } = useAuth();
    const [filterManager] = useState(() => new FilterManager(dashboardManager!.currentCompany!.companyId));

    const dataQuery = useQuery({
        queryKey: ["collections", pagination, sorting, filterManager.filters],
        queryFn: () => fetchCollectionsData(pagination, sorting, filterManager.generate(), authFetch),
        placeholderData: keepPreviousData
    });

    const searchbarRef = useRef<{ resetInput: () => void }>(null);

    const dtf = useMemo(() => {
        const intlLocale = locale === "sr" ? "sr-RS" : "en-US";
        return new Intl.DateTimeFormat(intlLocale, {
            year: "numeric",
            month: "long",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        });
    }, [locale]);

    const columnHelper = createColumnHelper<CollectionDto>();

    // @ts-ignore (ako ti je tipovanje previše strogo, ostavi kao kod tebe)
    const tableColumns: ColumnDef<CollectionDto, ColumnHelper<CollectionDto>>[] = useMemo(
        () => [
            columnHelper.accessor("collectionName", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("table.name")} />,
                enableSorting: true,
                // @ts-ignore
                meta: { className: "text-left", displayName: t("table.name") },
                cell: ({ row }: { row: { getVisibleCells: () => Cell<CollectionDto, any>[]; original: CollectionDto } }) => (
                    <Link
                        href={withLocale(locale, `/collections/${row.original.leadCollectionId}`)}
                        className="text-blue-500 hover:text-blue-600"
                    >
                        {row.original.collectionName}
                    </Link>
                )
            }),

            columnHelper.accessor("createdAt", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("table.createdAt")} />,
                enableSorting: true,
                // @ts-ignore
                meta: { className: "text-left", displayName: t("table.createdAt") },
                cell: ({ row }: { row: { getVisibleCells: () => Cell<CollectionDto, any>[]; original: CollectionDto } }) => {
                    const d = row.original.createdAt ? new Date(row.original.createdAt as any) : null;
                    return <div>{d ? dtf.format(d) : ""}</div>;
                }
            }),

            columnHelper.display({
                id: "edit",
                header: t("table.edit"),
                enableSorting: false,
                enableHiding: false,
                // @ts-ignore
                meta: { className: "text-center", displayName: t("table.edit") },
                cell: ({ row }) => (
                    <DataTableRowActions
                        row={row}
                        onEdit={() => {
                            router.push(withLocale(locale, `/collections/${row.original.leadCollectionId}`));
                        }}
                    />
                )
            })
        ],
        [columnHelper, dtf, locale, router, t]
    );

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

    const resetAllFilters = () => {
        filterManager.clearFilters();
        table.resetColumnFilters();
        table.resetGlobalFilter?.();
        searchbarRef.current?.resetInput();
        dataQuery.refetch();
    };

    return (
        <div className="space-y-3">
            <Filterbar
                table={table}
                isFiltered={Object.keys(filterManager.filters).length > 0}
                onFilterReset={resetAllFilters}
            >
                {table.getColumn("createdAt")?.getIsVisible() && (
                    <DataTableFilter
                        column={table.getColumn("createdAt")}
                        title={t("table.createdAt")}
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
                        placeholder={t("table.searchByNamePlaceholder")}
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

                <button
                    type="button"
                    onClick={resetAllFilters}
                    className="w-40 h-8 rounded-md border border-gray-200 px-3 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:border-gray-800 dark:text-gray-200 dark:hover:bg-gray-900"
                >
                    {tCommon("resetFilters")}
                </button>
            </Filterbar>

            <div className="relative overflow-hidden overflow-x-auto">
                <GenericTable table={table} tableColumns={tableColumns} />
            </div>

            <DataTablePagination table={table} pagination={pagination} />
        </div>
    );
}
