"use client";

import React from "react";
import { Table } from "@tanstack/react-table";
import { useTranslations } from "next-intl";

import { ViewOptions } from "./DataTableViewOptions";
import { Button } from "@/components/tremor/Button";

interface DataTableToolbarProps<TData> {
    children?: React.ReactNode;
    table: Table<TData>;
    isFiltered: boolean;

    /**
     * Optional hook for "hard reset" (server-side filters, search input, etc.)
     * Called after the table resets its own filters.
     */
    onFilterReset?: () => void;
}

export function Filterbar<TData>({ children, table, isFiltered, onFilterReset }: DataTableToolbarProps<TData>) {
    const t = useTranslations("Common");

    const handleClearFilters = () => {
        // TanStack column filters (facets)
        table.resetColumnFilters();

        // global filter (if used)
        table.resetGlobalFilter?.();

        // external reset (FilterManager, searchbar, etc.)
        onFilterReset?.();
    };

    return (
        <div className="flex flex-wrap items-center justify-between gap-2 sm:gap-x-6">
            <div className="flex w-full flex-col gap-2 sm:w-fit sm:flex-row sm:items-center">
                {children}

                {isFiltered && (
                    <Button
                        variant="ghost"
                        onClick={handleClearFilters}
                        className="border border-gray-200 px-2 font-semibold text-indigo-600 sm:border-none sm:py-1 dark:border-gray-800 dark:text-indigo-500"
                    >
                        {t("clearFilters")}
                    </Button>
                )}
            </div>

            <div className="flex items-center gap-2">
                <ViewOptions table={table} />
            </div>
        </div>
    );
}
