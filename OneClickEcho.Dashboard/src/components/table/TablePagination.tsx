"use client";

import { useTranslations } from "next-intl";
import { RiArrowLeftDoubleLine, RiArrowLeftSLine, RiArrowRightDoubleLine, RiArrowRightSLine } from "@remixicon/react";

import { TablePaginationButton } from "@/components/tremor/Table";

export const TablePagination = (props: {
    pagination: {
        pageIndex: number;
        pageSize: number;
    };
    table: {
        getRowCount: () => number;
        getCanPreviousPage: () => boolean;
        getCanNextPage: () => boolean;
        firstPage: () => void;
        previousPage: () => void;
        nextPage: () => void;
        lastPage: () => void;
    };
}) => {
    const t = useTranslations("Common");
    const { pagination, table } = props;

    const total = table.getRowCount();
    const from = total === 0 ? 0 : pagination.pageIndex * pagination.pageSize + 1;
    const to = Math.min((pagination.pageIndex + 1) * pagination.pageSize, total);

    return (
        <div className="mt-3 flex items-center justify-end gap-5">
            <p className="text-sm tabular-nums text-gray-500 dark:text-gray-500">
                {t("showingRange", { from, to, total })}
            </p>

            <div className="flex justify-between gap-2">
                <TablePaginationButton onClick={table.firstPage} disabled={!table.getCanPreviousPage()}>
                    <span className="sr-only">{t("firstPage")}</span>
                    <RiArrowLeftDoubleLine
                        className="size-4 text-gray-700 group-hover:text-gray-900 dark:text-gray-300 group-hover:dark:text-gray-50"
                        aria-hidden
                    />
                </TablePaginationButton>

                <TablePaginationButton onClick={table.previousPage} disabled={!table.getCanPreviousPage()}>
                    <span className="sr-only">{t("previous")}</span>
                    <RiArrowLeftSLine
                        className="size-4 text-gray-700 group-hover:text-gray-900 dark:text-gray-300 group-hover:dark:text-gray-50"
                        aria-hidden
                    />
                </TablePaginationButton>

                <TablePaginationButton onClick={table.nextPage} disabled={!table.getCanNextPage()}>
                    <span className="sr-only">{t("next")}</span>
                    <RiArrowRightSLine
                        className="size-4 text-gray-700 group-hover:text-gray-900 dark:text-gray-300 group-hover:dark:text-gray-50"
                        aria-hidden
                    />
                </TablePaginationButton>

                <TablePaginationButton onClick={table.lastPage} disabled={!table.getCanNextPage()}>
                    <span className="sr-only">{t("lastPage")}</span>
                    <RiArrowRightDoubleLine
                        className="size-4 text-gray-700 group-hover:text-gray-900 dark:text-gray-300 group-hover:dark:text-gray-50"
                        aria-hidden
                    />
                </TablePaginationButton>
            </div>
        </div>
    );
};
