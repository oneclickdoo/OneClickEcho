import { useTranslations } from "next-intl";

import { Table } from "@tanstack/react-table";
import { PaginationState } from "@tanstack/table-core";
import { RiArrowLeftDoubleLine, RiArrowLeftSLine, RiArrowRightDoubleLine, RiArrowRightSLine } from "@remixicon/react";

import { Button } from "@/components/tremor/Button";

import { cx } from "@/lib/utils";

interface DataTablePaginationProps<TData> {
    table: Table<TData>;
    pagination: PaginationState;
    isSelectable?: boolean;
}

export function DataTablePagination<TData>({ table, pagination, isSelectable }: DataTablePaginationProps<TData>) {
    const t = useTranslations("Common");

    const paginationButtons = [
        {
            icon: RiArrowLeftDoubleLine,
            onClick: () => table.firstPage(),
            disabled: !table.getCanPreviousPage(),
            srText: t("pagination.firstPage"),
            mobileView: "hidden sm:block"
        },
        {
            icon: RiArrowLeftSLine,
            onClick: () => table.previousPage(),
            disabled: !table.getCanPreviousPage(),
            srText: t("pagination.previousPage"),
            mobileView: ""
        },
        {
            icon: RiArrowRightSLine,
            onClick: () => table.nextPage(),
            disabled: !table.getCanNextPage(),
            srText: t("pagination.nextPage"),
            mobileView: ""
        },
        {
            icon: RiArrowRightDoubleLine,
            onClick: () => table.lastPage(),
            disabled: !table.getCanNextPage(),
            srText: t("pagination.lastPage"),
            mobileView: "hidden sm:block"
        }
    ];

    const totalRows = table.getRowCount();
    const pageCount = table.getPageCount();

    if (pageCount <= 1) {
        return null;
    }

    const currentPage = table.getState().pagination.pageIndex;
    const firstRowIndex = currentPage * pagination.pageSize + 1;
    const lastRowIndex = Math.min(totalRows, firstRowIndex + pagination.pageSize - 1);

    return (
        <div className="flex items-center justify-between">
            {isSelectable ? (
                <div className="text-sm tabular-nums text-gray-500">
                    {t("pagination.selectedCount", {
                        selected: table.getFilteredSelectedRowModel().rows.length,
                        total: totalRows
                    })}
                </div>
            ) : (
                <div></div>
            )}
            <div className="flex items-center gap-x-6 lg:gap-x-8">
                <p className="hidden text-sm tabular-nums text-gray-500 sm:block">
                    {t("pagination.showing")}{" "}
                    <span className="font-medium text-gray-900 dark:text-gray-50">
                        {firstRowIndex}-{lastRowIndex}
                    </span>{" "}
                    {t("pagination.of")} <span className="font-medium text-gray-900 dark:text-gray-50">{totalRows}</span>
                </p>
                <div className="flex items-center gap-x-1.5">
                    {paginationButtons.map((button, index) => (
                        <Button
                            key={index}
                            variant="secondary"
                            className={cx(button.mobileView, "p-1.5")}
                            onClick={() => {
                                button.onClick();
                                table.resetRowSelection();
                            }}
                            disabled={button.disabled}
                        >
                            <span className="sr-only">{button.srText}</span>
                            <button.icon className="size-4 shrink-0" aria-hidden="true" />
                        </Button>
                    ))}
                </div>
            </div>
        </div>
    );
}
