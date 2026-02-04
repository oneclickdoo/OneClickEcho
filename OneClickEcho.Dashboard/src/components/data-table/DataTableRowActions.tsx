"use client";

import React from "react";
import { Row } from "@tanstack/react-table";
import { RiMoreFill } from "@remixicon/react";
import { useTranslations } from "next-intl";

import { Button } from "@/components/tremor/Button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/tremor/Dropdown";

interface DataTableRowActionsProps<TData> {
    children?: React.ReactNode;
    row: Row<TData>;
    onEdit?: (row: Row<TData>) => void;
    onDelete?: (row: Row<TData>) => void;
}

export function DataTableRowActions<TData>({ children, row, onEdit, onDelete }: DataTableRowActionsProps<TData>) {
    const t = useTranslations("Common");

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button
                    variant="ghost"
                    className="group aspect-square p-1.5 hover:border hover:border-gray-300 data-[state=open]:border-gray-300 data-[state=open]:bg-gray-50 hover:dark:border-gray-700 data-[state=open]:dark:border-gray-700 data-[state=open]:dark:bg-gray-900"
                >
                    <RiMoreFill
                        className="size-4 shrink-0 text-gray-500 group-hover:text-gray-700 group-data-[state=open]:text-gray-700 group-hover:dark:text-gray-300 group-data-[state=open]:dark:text-gray-300"
                        aria-hidden="true"
                    />
                </Button>
            </DropdownMenuTrigger>

            <DropdownMenuContent align="end" className="min-w-40">
                {onEdit ? <DropdownMenuItem onClick={() => onEdit(row)}>{t("edit")}</DropdownMenuItem> : null}

                {onDelete ? (
                    <DropdownMenuItem onClick={() => onDelete(row)} className="text-red-600 dark:text-red-500">
                        {t("delete")}
                    </DropdownMenuItem>
                ) : null}

                {children}
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
