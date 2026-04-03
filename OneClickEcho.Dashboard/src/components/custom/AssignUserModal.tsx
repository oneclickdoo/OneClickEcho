"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";

import { SingleValue } from "react-select";
import AsyncSelect from "react-select/async";

import { useAuth } from "@/context/AuthContext";

import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger
} from "@/components/tremor/Dialog";
import { Button } from "@/components/tremor/Button";

import { searchUsers } from "@/elements/dashboard/singleCompany/fetchData";
import { Option } from "@/data/schema";

interface IAssignUserModalProps {
    companyId: string;
    onAssign: (userId: string) => void;
}

export const AssignUserModal = (props: IAssignUserModalProps) => {
    const tCommon = useTranslations("Common");

    const [selectedUser, setSelectedUser] = useState<SingleValue<Option | null>>(null);
    const [open, setOpen] = useState<boolean>(false);

    const { authFetch } = useAuth();

    const searchOptions = async (query: string): Promise<Option[]> => {
        if (!query) return [];

        const users = await searchUsers(props.companyId, query, authFetch);
        return users.map((user) => ({ value: user.id, label: user.email }));
    };

    const onUserChange = (value: SingleValue<Option>) => {
        setSelectedUser(value);
    };

    return (
        <Dialog
            open={open}
            onOpenChange={(isOpen) => {
                setOpen(isOpen);
                if (!isOpen) setSelectedUser(null);
            }}
        >
            <DialogTrigger asChild>
                <Button
                    type="submit"
                    variant="primary"
                    className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                    onClick={() => setOpen(true)}
                >
                    {tCommon("companyUsers.assignUserButton")}
                </Button>
            </DialogTrigger>

            <DialogContent className="sm:max-w-lg" aria-describedby="assign-user-dialog">
                <DialogHeader className="min-h-[300px]">
                    <DialogTitle>{tCommon("companyUsers.assignUserTitle")}</DialogTitle>
                    <DialogDescription>
                        <p className="text-sm text-gray-400 mt-1">{tCommon("companyUsers.assignUserHint")}</p>

                        <div className="mt-5">
                            <AsyncSelect
                                value={selectedUser}
                                defaultOptions
                                placeholder={tCommon("companyUsers.emailPlaceholder")}
                                className="w-full mx-auto"
                                loadOptions={searchOptions}
                                onChange={(value: SingleValue<Option>) => onUserChange(value)}
                                noOptionsMessage={() => tCommon("companyUsers.noUsersFound")}
                                loadingMessage={() => tCommon("companyUsers.loading")}
                            />
                        </div>
                    </DialogDescription>
                </DialogHeader>

                <DialogFooter className="mt-6 flex justify-end sm:justify-end flex-col gap-2">
                    <Button variant="secondary" className="mt-2 w-full sm:mt-0 sm:w-fit" onClick={() => setOpen(false)}>
                        {tCommon("common.goBack")}
                    </Button>

                    <Button
                        disabled={!selectedUser}
                        className="flex gap-x-2 w-full sm:w-fit border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                        onClick={() => {
                            props.onAssign(selectedUser!.value);
                            setOpen(false);
                        }}
                    >
                        {tCommon("companyUsers.assign")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
