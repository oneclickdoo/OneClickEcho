import React from "react";
import { flexRender, SortDirection, HeaderGroup, Row } from "@tanstack/react-table";
import { RiArrowDownSLine, RiArrowUpSLine } from "@remixicon/react";

import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRoot, TableRow } from "@/components/tremor/Table";
import { cx } from "@/lib/utils";

export const TableMain = (props: {
    table: {
        getHeaderGroups: () => HeaderGroup<any>[];
        getRowModel: () => { rows: Row<any>[] };
    };
    customColumns: string[];
}) => {
    const { table, customColumns } = props;

    const getAriaSortValue = (isSorted: false | SortDirection) => {
        switch (isSorted) {
            case "asc":
                return "ascending";
            case "desc":
                return "descending";
            case false:
            default:
                return "none";
        }
    };

    return (
        <TableRoot>
            <Table>
                <TableHead>
                    {table.getHeaderGroups().map((headerGroup) => (
                        <TableRow key={headerGroup.id}>
                            {headerGroup.headers.map((header) => {
                                const sortingHandler = header.column.getToggleSortingHandler?.();

                                return (
                                    <TableHeaderCell
                                        key={header.id}
                                        tabIndex={header.column.getCanSort() ? 0 : -1}
                                        aria-sort={getAriaSortValue(header.column.getIsSorted())}
                                        className={cx("px-0.5 py-1.5 select-none")}
                                    >
                                        <div
                                            className={cx(
                                                header.column.getCanSort() ? "cursor-pointer select-none" : "",
                                                header.column.columnDef.enableSorting === true
                                                    ? "flex items-center justify-left gap-2 hover:bg-gray-50 hover:dark:bg-gray-900"
                                                    : "",
                                                "rounded-md px-3 py-1.5",
                                                "w-fit"
                                            )}
                                            onClick={sortingHandler}
                                            onKeyDown={(event) => {
                                                if (event.key === "Enter" && sortingHandler) {
                                                    sortingHandler(event);
                                                }
                                            }}
                                        >
                                            {flexRender(header.column.columnDef.header, header.getContext())}

                                            {header.column.getCanSort() ? (
                                                <div className="-space-y-2">
                                                    <RiArrowUpSLine
                                                        aria-hidden
                                                        className={cx(
                                                            "size-4 text-gray-900 dark:text-gray-50",
                                                            header.column.getIsSorted() === "desc" ? "opacity-30" : ""
                                                        )}
                                                    />
                                                    <RiArrowDownSLine
                                                        aria-hidden
                                                        className={cx(
                                                            "size-4 text-gray-900 dark:text-gray-50",
                                                            header.column.getIsSorted() === "asc" ? "opacity-30" : ""
                                                        )}
                                                    />
                                                </div>
                                            ) : null}
                                        </div>
                                    </TableHeaderCell>
                                );
                            })}
                        </TableRow>
                    ))}
                </TableHead>

                <TableBody>
                    {table.getRowModel().rows.map((row) => (
                        <TableRow key={row.id}>
                            {row.getVisibleCells().map((cell) => {
                                const headerKey = String(cell.column.columnDef.header ?? "");
                                const isCustom = customColumns.includes(headerKey);

                                if (isCustom) {
                                    return (
                                        <React.Fragment key={cell.id}>
                                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                        </React.Fragment>
                                    );
                                }

                                return <TableCell key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</TableCell>;
                            })}
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableRoot>
    );
};
