"use client";

import type { Dispatch, SetStateAction } from "react";
import { useMemo } from "react";
import Link from "next/link";

import type { ColumnDef } from "@tanstack/react-table";
import { createColumnHelper, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import type { PaginationState, SortingState } from "@tanstack/react-table";

import { RiDeleteBin6Line } from "@remixicon/react";
import { useLocale, useTranslations } from "next-intl";

import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";
import { GenericTable } from "@/components/generics/GenericTable";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";
import { Button } from "@/components/tremor/Button";

import type { CampaignDto, LeadCollectionDto } from "../fetchData";
import { CampaignStatus } from "@/lib/enums";

type RowType = LeadCollectionDto & { count?: number };

interface ICampaignLeadCollections {
    campaign: CampaignDto;
    leadsData:
    | {
        rows: RowType[];
        pageCount: number;
        rowCount: number;
    }
    | undefined;
    sorting: SortingState;
    setSorting: Dispatch<SetStateAction<SortingState>>;
    pagination: PaginationState;
    setPagination: Dispatch<SetStateAction<PaginationState>>;
    handleDelete: (leadCollectionId: string) => void;
}

export function CampaignCollectionsTable(props: ICampaignLeadCollections) {
    const t = useTranslations("SingleCampaign.Tables.CampaignCollections");
    const locale = useLocale();

    const columnHelper = createColumnHelper<RowType>();

    const dateTimeFormatter = useMemo(() => {
        return new Intl.DateTimeFormat(locale, {
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

    // NOTE: koristimo ColumnDef<RowType, any>[] da izbegnemo TanStack "unknown vs string" konflikt
    const tableColumns = useMemo<ColumnDef<RowType, any>[]>(() => {
        return [
            columnHelper.accessor("collectionName", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("name")} />,
                enableSorting: true,
                meta: {
                    className: "min-w-[200px] text-left",
                    displayName: t("name")
                },
                cell: ({ row }) => (
                    <Link
                        href={`/${locale}/collections/${row.original.leadCollectionId}`}
                        className="text-blue-500 hover:text-blue-600"
                    >
                        {row.original.collectionName}
                    </Link>
                )
            }),

            columnHelper.accessor("count", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("count")} />,
                enableSorting: false,
                meta: {
                    className: "text-left",
                    displayName: t("count")
                },
                cell: ({ row }) => <div>{row.original.count}</div>
            }),

            columnHelper.accessor("createdAt", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("createdAt")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("createdAt")
                },
                cell: ({ row }) => <div>{formatDateTime(row.original.createdAt)}</div>
            }),

            columnHelper.display({
                id: "unassign",
                header: t("unassign"),
                enableSorting: false,
                enableHiding: false,
                meta: {
                    className: "text-center",
                    displayName: t("unassign")
                },
                cell: ({ row }) => (
                    <Button
                        variant="destructive"
                        disabled={props.campaign.status !== CampaignStatus.Draft}
                        className="p-1"
                        onClick={() => props.handleDelete(row.original.leadCollectionId)}
                        aria-label={t("unassign")}
                        title={t("unassign")}
                    >
                        <RiDeleteBin6Line />
                    </Button>
                )
            })
        ];
    }, [columnHelper, formatDateTime, locale, props.campaign.status, props.handleDelete, t]);

    const table = useReactTable({
        data: props.leadsData?.rows ?? [],
        columns: tableColumns,
        getCoreRowModel: getCoreRowModel(),
        state: {
            pagination: props.pagination,
            sorting: props.sorting
        },
        onSortingChange: props.setSorting,
        onPaginationChange: props.setPagination,
        manualPagination: true,
        manualFiltering: true,
        manualSorting: true,
        enableMultiSort: true,
        rowCount: props.leadsData?.rowCount ?? 0
    });

    return (
        <div className="space-y-3">
            <div className="relative overflow-hidden overflow-x-auto">
                <GenericTable table={table} tableColumns={tableColumns} />
            </div>
            <DataTablePagination table={table} pagination={props.pagination} />
        </div>
    );
}
