"use client";

import { RowSelectionState, Table } from "@tanstack/react-table";
import { useTranslations } from "next-intl";

import {
    CommandBar,
    CommandBarBar,
    CommandBarCommand,
    CommandBarSeperator,
    CommandBarValue
} from "../tremor/CommandBar";

type DataTableBulkEditorProps<TData> = {
    table: Table<TData>;
    rowSelection: RowSelectionState;
};

function DataTableBulkEditor<TData>({ table, rowSelection }: DataTableBulkEditorProps<TData>) {
    const t = useTranslations("Common");

    const selectedCount = Object.keys(rowSelection).length;
    const hasSelectedRows = selectedCount > 0;

    return (
        <CommandBar open={hasSelectedRows}>
            <CommandBarBar>
                <CommandBarValue>
                    {selectedCount} {t("selected")}
                </CommandBarValue>

                <CommandBarSeperator />

                <CommandBarCommand
                    label={t("edit")}
                    action={() => {
                        console.log("Edit");
                    }}
                    shortcut={{ shortcut: "e" }}
                />

                <CommandBarSeperator />

                <CommandBarCommand
                    label={t("delete")}
                    action={() => {
                        console.log("Delete");
                    }}
                    shortcut={{ shortcut: "d" }}
                />

                <CommandBarSeperator />

                <CommandBarCommand
                    label={t("reset")}
                    action={() => {
                        table.resetRowSelection();
                    }}
                    shortcut={{ shortcut: "Escape", label: "esc" }}
                />
            </CommandBarBar>
        </CommandBar>
    );
}

export { DataTableBulkEditor };
