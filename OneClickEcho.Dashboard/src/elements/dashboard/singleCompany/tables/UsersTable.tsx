"use client";

import { useRef, useMemo } from "react";
import { useTranslations } from "next-intl";

import { Cell, ColumnDef } from "@tanstack/react-table";
import { RiMoreLine } from "@remixicon/react";

import { TableCell } from "@/components/tremor/Table";
import { TableGeneric } from "@/components/table/TableGeneric";

import { UserDto } from "@/elements/dashboard/singleCompany/fetchData";
import { CompanyUsersTableModal } from "@/elements/dashboard/singleCompany/modals/CompanyUsersTableModal";

export const UsersTable = ({
    dataQuery,
    className,
    pagination,
    setPagination,
    sorting,
    setSorting
}: {
    dataQuery: any;
    className?: string;
    pagination: any;
    setPagination: any;
    sorting: any;
    setSorting: any;
}) => {
    const tCommon = useTranslations("Common");

    const drawerRef = useRef<{ useDrawer: (row: UserDto) => void }>(null);

    const drawerOpen = (row: UserDto) => {
        drawerRef.current?.useDrawer(row);
    };

    const usersColumns: ColumnDef<UserDto, any>[] = useMemo(
        () => [
            {
                header: tCommon("usersTable.email"),
                accessorKey: "email",
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: tCommon("usersTable.email")
                }
            },
            {
                header: tCommon("usersTable.userId"),
                accessorKey: "id",
                enableSorting: true,
                meta: {
                    className: "text-left",
                    displayName: tCommon("usersTable.userId")
                }
            },
            {
                header: tCommon("common.edit"),
                accessorKey: "edit",
                enableSorting: false,
                meta: {
                    className: "text-left",
                    displayName: tCommon("common.edit")
                },
                cell: ({ row }: { row: { getVisibleCells: () => Cell<UserDto, unknown>[]; original: UserDto } }) => (
                    <TableCell
                        key={row.getVisibleCells()[2].id}
                        className="my-auto px-auto cursor-pointer select-none"
                        onClick={() => drawerOpen(row.original)}
                    >
                        <RiMoreLine className="size-4" />
                    </TableCell>
                )
            }
        ],
        [tCommon]
    );

    return (
        <>
            <TableGeneric
                dataQuery={dataQuery}
                tableColumns={usersColumns}
                pagination={pagination}
                setPagination={setPagination}
                sorting={sorting}
                setSorting={setSorting}
                className={className}
            />
            <CompanyUsersTableModal refreshData={dataQuery.refetch} ref={drawerRef} />
        </>
    );
};
