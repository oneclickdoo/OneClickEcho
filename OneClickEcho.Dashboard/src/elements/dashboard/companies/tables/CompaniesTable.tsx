"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import { useLocale, useTranslations } from "next-intl";
import { withLocale } from "@/lib/routing";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { ColumnDef, PaginationState, SortingState } from "@tanstack/react-table";

import { useAuth } from "@/context/AuthContext";

import { TableCell } from "@/components/tremor/Table";
import { TableGeneric } from "@/components/table/TableGeneric";
import Loading from "@/components/tremor/Loading";

import { CompanyDto, fetchCompaniesData } from "@/elements/dashboard/companies/fetchData";

export function CompaniesTable() {
    const t = useTranslations("CompaniesTable");
    const locale = useLocale();

    const [sorting, setSorting] = useState<SortingState>([]);
    const [pagination, setPagination] = useState<PaginationState>({
        pageIndex: 0,
        pageSize: 10
    });

    const { authFetch } = useAuth();

    const dataQuery = useQuery({
        queryKey: ["data", pagination, sorting],
        queryFn: () => fetchCompaniesData(pagination, sorting, authFetch),
        placeholderData: keepPreviousData
    });

    const dateTimeFormatter = useMemo(
        () =>
            new Intl.DateTimeFormat(locale, {
                year: "numeric",
                month: "long",
                day: "2-digit",
                hour: "2-digit",
                minute: "2-digit",
                second: "2-digit"
            }),
        [locale]
    );

    const tableColumns: ColumnDef<CompanyDto, any>[] = useMemo(
        () => [
            {
                header: t("columns.name"),
                accessorKey: "name",
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.name")
                },
                cell: ({ row }) => (
                    <TableCell key={row.id}>
                        <Link
                            href={withLocale(locale, `/companies/${row.original.companyId}`)}
                            className="text-blue-500 hover:text-blue-600"
                        >
                            {row.original.name}
                        </Link>
                    </TableCell>
                )
            },
            {
                header: t("columns.createdAt"),
                accessorKey: "createdAt",
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: t("columns.createdAt")
                },
                cell: ({ row }) => {
                    const raw = row.original.createdAt;
                    const date = raw instanceof Date ? raw : new Date(raw as any);

                    return <TableCell key={row.id}>{Number.isNaN(date.getTime()) ? "" : dateTimeFormatter.format(date)}</TableCell>;
                }
            }
        ],
        [t, dateTimeFormatter]
    );

    return (
        <>
            {dataQuery.isLoading ? (
                <Loading text={t("loading")} />
            ) : (
                <TableGeneric
                    dataQuery={dataQuery}
                    tableColumns={tableColumns}
                    pagination={pagination}
                    setPagination={setPagination}
                    sorting={sorting}
                    setSorting={setSorting}
                />
            )}
        </>
    );
}
