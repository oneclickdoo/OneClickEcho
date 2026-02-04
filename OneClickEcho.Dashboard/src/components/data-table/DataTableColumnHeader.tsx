import React from "react";
import { Column } from "@tanstack/react-table";
import { RiArrowDownSLine, RiArrowUpSLine } from "@remixicon/react";

import { cx } from "@/lib/utils";

interface DataTableColumnHeaderProps<TData, TValue> extends React.HTMLAttributes<HTMLDivElement> {
    column: Column<TData, TValue>;
    title: string;
}

export function DataTableColumnHeader<TData, TValue>({ column, title, className }: DataTableColumnHeaderProps<TData, TValue>) {
    if (!column.getCanSort()) {
        return <div className={cx(className)}>{title}</div>;
    }

    const isInteractive = column.columnDef.enableSorting === true;

    return (
        <div
            role={isInteractive ? "button" : undefined}
            tabIndex={isInteractive ? 0 : undefined}
            onClick={column.getToggleSortingHandler()}
            onKeyDown={(e) => {
                if (!isInteractive) return;

                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    column.getToggleSortingHandler()?.(e);
                }
            }}
            className={cx(
                className,
                isInteractive
                    ? "-mx-2 inline-flex cursor-pointer select-none items-center gap-2 rounded-md px-2 py-1 hover:bg-gray-50 hover:dark:bg-gray-900"
                    : ""
            )}
        >
            <span>{title}</span>

            <div className="-space-y-2" aria-hidden="true">
                <RiArrowUpSLine
                    className={cx("size-3.5 text-gray-900 dark:text-gray-50", column.getIsSorted() === "desc" ? "opacity-30" : "")}
                />
                <RiArrowDownSLine
                    className={cx("size-3.5 text-gray-900 dark:text-gray-50", column.getIsSorted() === "asc" ? "opacity-30" : "")}
                />
            </div>
        </div>
    );
}
