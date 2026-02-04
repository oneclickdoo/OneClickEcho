"use client";

import { useMemo, useRef } from "react";
import { useTranslations } from "next-intl";

import {
    Cell,
    ColumnDef,
    ColumnHelper,
    createColumnHelper,
    getCoreRowModel,
    useReactTable
} from "@tanstack/react-table";

import { DropdownMenuItem } from "@/components/tremor/Dropdown";
import { Button } from "@/components/tremor/Button";

import { GenericTable } from "@/components/generics/GenericTable";
import { Filterbar } from "@/components/data-table/DataTableFilterbar";
import { DataTableFilter } from "@/components/data-table/DataTableFilter";
import { GenericSearchbar } from "@/components/generics/GenericSearchbar";
import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";
import { DataTableRowActions } from "@/components/data-table/DataTableRowActions";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";

import { CollectionLeadsTableModal } from "../modals/CollectionLeadsTableModal";
import { CampaignLeadDto } from "@/elements/dashboard/singleCampaign/fetchData";

export const CollectionLeadsTable = ({
    pagination,
    sorting,
    setPagination,
    setSorting,
    dataQuery,
    filterManager
}: {
    // NOTE: ostavljeno kao any namerno (Next.js client boundary "serializable props" TS pravilo)
    pagination: any;
    sorting: any;
    setPagination: any;
    setSorting: any;
    dataQuery: any;
    filterManager: any;
}) => {
    const tCommon = useTranslations("Common");

    const searchbarRef = useRef<{ resetInput: () => void }>(null);
    const drawerRef = useRef<{ useDrawer: (row: CampaignLeadDto) => void }>(null);

    const drawerOpen = (row: CampaignLeadDto) => {
        drawerRef.current?.useDrawer(row);
    };

    const columnHelper = createColumnHelper<CampaignLeadDto>();

    const leadsColumns = useMemo(() => {
        return [
            columnHelper.accessor("phoneNumber", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.phoneNumber")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.phoneNumber") }
            }),
            columnHelper.accessor("email", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.email")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.email") }
            }),
            columnHelper.accessor("firstName", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.firstName")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.firstName") }
            }),
            columnHelper.accessor("lastName", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.lastName")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.lastName") }
            }),
            columnHelper.accessor("gender", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.gender")} />,
                enableSorting: false,
                meta: { className: "text-left", displayName: tCommon("lead.gender") },
                cell: ({
                    row
                }: {
                    row: { getVisibleCells: () => Cell<CampaignLeadDto, unknown>[]; original: CampaignLeadDto };
                }) => {
                    const g = row.original.gender;
                    const label =
                        g === 1 ? tCommon("leadGender.male") : g === 2 ? tCommon("leadGender.female") : "";
                    return <div>{label}</div>;
                }
            }),
            columnHelper.accessor("dateOfBirth", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.dateOfBirth")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.dateOfBirth") }
            }),
            columnHelper.accessor("city", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.city")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.city") }
            }),
            columnHelper.accessor("state", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.state")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.state") }
            }),
            columnHelper.accessor("country", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={tCommon("lead.country")} />,
                enableSorting: true,
                meta: { className: "text-left", displayName: tCommon("lead.country") }
            }),
            columnHelper.display({
                id: "edit",
                header: tCommon("common.edit"),
                enableSorting: false,
                enableHiding: false,
                meta: { className: "text-center", displayName: tCommon("common.edit") },
                cell: ({ row }) => (
                    <DataTableRowActions row={row}>
                        <DropdownMenuItem onClick={() => drawerOpen(row.original)}>
                            {tCommon("common.edit")}
                        </DropdownMenuItem>
                    </DataTableRowActions>
                )
            })
        ];
    }, [columnHelper, tCommon]);

    const table = useReactTable({
        data: dataQuery.data?.rows ?? [],
        columns: leadsColumns,
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

    const hasFilters = Object.keys(filterManager.filters || {}).length > 0;

    return (
        <>
            <Filterbar table={table} isFiltered={hasFilters}>
                {hasFilters && (
                    <Button
                        variant="secondary"
                        className="h-8"
                        onClick={() => {
                            filterManager.clearFilters();
                            table.resetColumnFilters();
                            searchbarRef.current?.resetInput();
                            setPagination({ pageIndex: 0, pageSize: 10 });
                            setSorting([{ id: "phoneNumber", desc: false }]);
                            dataQuery.refetch();
                        }}
                    >
                        {tCommon("common.reset")}
                    </Button>
                )}

                {table.getColumn("dateOfBirth")?.getIsVisible() && (
                    <DataTableFilter
                        column={table.getColumn("dateOfBirth")}
                        title={tCommon("lead.dateOfBirth")}
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

                {table.getColumn("phoneNumber")?.getIsVisible() && (
                    <GenericSearchbar
                        ref={searchbarRef}
                        placeholder={tCommon("lead.searchPhonePlaceholder")}
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
            </Filterbar>

            <div className="relative overflow-hidden overflow-x-auto mt-3 mb-2">
                {/* FIX: GenericTable tipovi su pogrešni (traži ColumnDef<any, ColumnHelper<any>>[]).
                   Castujemo ovde, lokalno, bez menjanja ostatka. */}
                <GenericTable
                    table={table}
                    tableColumns={leadsColumns as unknown as ColumnDef<any, ColumnHelper<any>>[]}
                />
            </div>

            <DataTablePagination table={table} pagination={pagination} />

            <CollectionLeadsTableModal refreshData={dataQuery.refetch} ref={drawerRef} />
        </>
    );
};
