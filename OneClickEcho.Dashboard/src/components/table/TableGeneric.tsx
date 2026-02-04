import { useMemo, useImperativeHandle, forwardRef, Ref } from "react";
import { getCoreRowModel, useReactTable, ColumnDef, PaginationState, SortingState, OnChangeFn } from "@tanstack/react-table";

import { TableMain } from "@/components/table/TableMain";
import { TablePagination } from "@/components/table/TablePagination";

export interface IFetchResult<T> {
    rows: T[];
    pageCount: number;
    rowCount: number;
}

export interface IFetchTable<T> {
    (options: PaginationState, sorting: SortingState, authFetch: any): Promise<IFetchResult<T>>;
}

interface TableDefaultProps<T> {
    dataQuery: any;
    tableColumns: ColumnDef<T, any>[];
    pagination: PaginationState;
    setPagination: OnChangeFn<PaginationState>;
    sorting: SortingState;
    setSorting: OnChangeFn<SortingState>;
    className?: string;
}

const TableGenericInner = <T,>(
    { dataQuery, tableColumns, pagination, setPagination, sorting, setSorting, className }: TableDefaultProps<T>,
    ref: Ref<any>
) => {
    const defaultData = useMemo<T[]>(() => [], []);

    const customColumns = useMemo(() => {
        return tableColumns.reduce<any[]>((acc, column: any) => {
            if (Object.prototype.hasOwnProperty.call(column, "cell")) {
                acc.push(column.header);
            }
            return acc;
        }, []);
    }, [tableColumns]);

    const table = useReactTable({
        data: dataQuery.data?.rows ?? defaultData,
        columns: tableColumns,
        getCoreRowModel: getCoreRowModel(),
        state: { pagination, sorting },
        onSortingChange: setSorting,
        onPaginationChange: setPagination,
        manualPagination: true,
        rowCount: dataQuery.data?.rowCount
    });

    useImperativeHandle(ref, () => ({}));

    return (
        <div className={className ?? ""}>
            <TableMain table={table} customColumns={customColumns} />
            <TablePagination pagination={pagination} table={table} />
        </div>
    );
};

export const TableGeneric = forwardRef(TableGenericInner) as <T>(
    props: TableDefaultProps<T> & { ref?: Ref<any> }
) => ReturnType<typeof TableGenericInner>;
