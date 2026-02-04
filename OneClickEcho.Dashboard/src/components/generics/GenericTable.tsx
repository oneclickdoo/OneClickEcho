import { useTranslations } from "next-intl";

import type { ColumnDef, Table } from "@tanstack/react-table";
import { flexRender } from "@tanstack/react-table";

import { Table as TremorTable, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "@/components/tremor/Table";
import { cx } from "@/lib/utils";

export const GenericTable = <TData,>({
    table,
    tableColumns
}: {
    table: Table<TData>;
    tableColumns: Array<ColumnDef<TData, any>>;
}) => {
    const t = useTranslations("Common");

    return (
        <TremorTable>
            <TableHead>
                {table.getHeaderGroups().map((headerGroup) => (
                    <TableRow key={headerGroup.id} className="border-y border-gray-200 dark:border-gray-800">
                        {headerGroup.headers.map((header) => (
                            <TableHeaderCell
                                key={header.id}
                                className={cx(
                                    "whitespace-nowrap select-none py-1 text-sm sm:text-xs",
                                    header.column.columnDef.meta?.className
                                )}
                            >
                                {flexRender(header.column.columnDef.header, header.getContext())}
                            </TableHeaderCell>
                        ))}
                    </TableRow>
                ))}
            </TableHead>

            <TableBody>
                {table.getRowModel().rows?.length ? (
                    table.getRowModel().rows.map((row) => (
                        <TableRow key={row.id} className="group hover:bg-gray-50 hover:dark:bg-gray-900">
                            {row.getVisibleCells().map((cell, index) => (
                                <TableCell
                                    key={cell.id}
                                    className={cx(
                                        row.getIsSelected() ? "bg-gray-50 dark:bg-gray-900" : "",
                                        "relative whitespace-nowrap py-1 text-gray-600 first:w-10 dark:text-gray-400",
                                        cell.column.columnDef.meta?.className
                                    )}
                                >
                                    {index === 0 && row.getIsSelected() && (
                                        <div className="absolute inset-y-0 left-0 w-0.5 bg-indigo-600 dark:bg-indigo-500" />
                                    )}
                                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                </TableCell>
                            ))}
                        </TableRow>
                    ))
                ) : (
                    <TableRow>
                        <TableCell colSpan={tableColumns.length} className="h-24 text-center">
                            {t("table.noResults")}
                        </TableCell>
                    </TableRow>
                )}
            </TableBody>
        </TremorTable>
    );
};
