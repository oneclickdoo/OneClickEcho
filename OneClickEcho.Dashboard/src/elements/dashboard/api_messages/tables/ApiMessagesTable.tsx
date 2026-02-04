import { forwardRef, useImperativeHandle, useMemo, useRef, useState } from "react";

import {
    ColumnDef,
    ColumnHelper,
    createColumnHelper,
    getCoreRowModel,
    PaginationState,
    SortingState,
    useReactTable
} from "@tanstack/react-table";

import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { useLocale, useTranslations } from "next-intl";

import { ApiMessageDto } from "@/elements/dashboard/api_messages/fetchData";

import { Filterbar } from "@/components/data-table/DataTableFilterbar";
import { GenericSearchbar } from "@/components/generics/GenericSearchbar";
import { GenericTable } from "@/components/generics/GenericTable";
import { useAuth } from "@/context/AuthContext";
import { FilterManager } from "@/components/filtering/FilterManager";
import { DataTableColumnHeader } from "@/components/data-table/DataTableColumnHeader";
import { Badge } from "@/components/tremor/Badge";
import { DataTablePagination } from "@/components/data-table/DataTablePagination";
import { fetchApiMessagesData } from "@/elements/dashboard/api_messages/fetchData";
import { ApiMessageViberStatus, CampaignLeadSMSStatus, enumToArray } from "@/lib/enums";

export const ApiMessagesTable = forwardRef((_, ref) => {
    const t = useTranslations("ApiMessages.table");
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
        queryKey: ["data", pagination, sorting, filterManager.filters],
        queryFn: () => fetchApiMessagesData(pagination, sorting, filterManager.generate(), authFetch),
        placeholderData: keepPreviousData
    });

    useImperativeHandle(ref, () => ({
        refetchData: () => dataQuery.refetch()
    }));

    const columnHelper = createColumnHelper<ApiMessageDto>();

    // Formatter vezan za locale (sr/en), sa sekundama
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

    // @ts-ignore
    const tableColumns: ColumnDef<ApiMessageDto, ColumnHelper<ApiMessageDto>>[] = useMemo(
        () => [
            columnHelper.accessor("phoneNumber", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.phoneNumber")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.phoneNumber")
                }
            }),
            columnHelper.accessor("message", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.message")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.message")
                }
            }),
            columnHelper.accessor("messageType", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.type")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.type")
                },
                cell: ({ row }: { row: { original: ApiMessageDto } }) =>
                    row.original.messageType === 1 ? (
                        <Badge variant="default">{t("types.viber")}</Badge>
                    ) : (
                        <Badge variant="neutral">{t("types.sms")}</Badge>
                    )
            }),
            columnHelper.accessor("hasSmsFallback", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.smsFallback")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.smsFallback")
                },
                cell: ({ row }: { row: { original: ApiMessageDto } }) =>
                    row.original.hasSmsFallback ? (
                        <Badge variant="success">{tCommon("yes")}</Badge>
                    ) : (
                        <Badge variant="error">{tCommon("no")}</Badge>
                    )
            }),
            columnHelper.accessor("viberStatus", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.viberStatus")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.viberStatus")
                },
                cell: ({ row }: { row: { original: ApiMessageDto } }) => (
                    <Badge variant="default">
                        {enumToArray(ApiMessageViberStatus).find((x) => x.value === row.original.viberStatus)?.label ??
                            tCommon("unknown")}
                    </Badge>
                )
            }),
            columnHelper.accessor("smsStatus", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.smsStatus")} />,
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.smsStatus")
                },
                cell: ({ row }: { row: { original: ApiMessageDto } }) => (
                    <Badge variant="default">
                        {enumToArray(CampaignLeadSMSStatus).find((x) => x.value === row.original.smsStatus)?.label ??
                            tCommon("unknown")}
                    </Badge>
                )
            }),
            columnHelper.accessor("createdAt", {
                header: ({ column }) => <DataTableColumnHeader column={column} title={t("columns.createdAt")} />,
                enableSorting: true,
                meta: {
                    className: "tabular-nums",
                    displayName: t("columns.createdAt")
                },
                cell: ({ row }: { row: { original: ApiMessageDto } }) => {
                    return <div>{formatDateTime(row.original.createdAt)}</div>;
                }
            })
        ],
        [columnHelper, t, tCommon, formatDateTime]
    );

    const table = useReactTable({
        data: dataQuery.data?.rows ?? [],
        columns: tableColumns,
        getCoreRowModel: getCoreRowModel(),
        state: {
            pagination: pagination,
            sorting: sorting
        },
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
            <Filterbar
                table={table}
                isFiltered={Object.keys(filterManager.filters).length > 0}
                onFilterReset={() => {
                    filterManager.clearFilters();
                    table.resetColumnFilters();
                    searchbarRef.current?.resetInput();
                }}
            >
                {table.getColumn("phoneNumber")?.getIsVisible() && (
                    <GenericSearchbar
                        ref={searchbarRef}
                        placeholder={t("filters.phonePlaceholder")}
                        onSearchChange={(value) => {
                            if (value !== "") {
                                filterManager.setFilter("phoneNumber", {
                                    type: "search",
                                    value: value
                                });
                            } else {
                                filterManager.removeFilter("phoneNumber");
                            }
                            dataQuery.refetch();
                        }}
                    />
                )}
            </Filterbar>

            <div className="relative overflow-hidden overflow-x-auto">
                <GenericTable table={table} tableColumns={tableColumns}></GenericTable>
            </div>

            <DataTablePagination table={table} pagination={pagination} />
        </div>
    );
});
