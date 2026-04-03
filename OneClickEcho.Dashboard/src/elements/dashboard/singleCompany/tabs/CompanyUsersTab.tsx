"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";

import { PaginationState, SortingState } from "@tanstack/react-table";
import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { useAuth } from "@/context/AuthContext";

import { NewUserModal } from "@/components/custom/NewUserModal";
import { AssignUserModal } from "@/components/custom/AssignUserModal";

import { UsersTable } from "@/elements/dashboard/singleCompany/tables/UsersTable";
import { createUser, fetchUsersData, assignUser } from "@/elements/dashboard/singleCompany/fetchData";

import { useToast } from "@/lib/useToast";
import { UserRole } from "@/lib/enums";

interface ICompanyUsersTabProps {
    companyId: string;
}

export const CompanyUsersTab = ({ companyId }: ICompanyUsersTabProps) => {
    const tCommon = useTranslations("Common");

    const [userEmail, setUserEmail] = useState("");
    const [password, setPassword] = useState("");
    const [sorting, setSorting] = useState<SortingState>([]);
    const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 10 });
    const [errorMessages, setErrorMessages] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const dataQuery = useQuery({
        queryKey: ["company-users", companyId, pagination, sorting],
        queryFn: () => fetchUsersData(companyId, pagination, sorting, authFetch),
        placeholderData: keepPreviousData
    });

    const assignUserToCompany = async (userId: string) => {
        try {
            const response = await assignUser(userId, companyId, authFetch);

            if (response.ok) {
                await dataQuery.refetch();

                toast({
                    variant: "success",
                    title: tCommon("toasts.success"),
                    description: tCommon("toasts.userAssigned"),
                    duration: 2000
                });

                return;
            }

            toast({
                variant: "error",
                title: tCommon("toasts.error"),
                description: tCommon("errors.unexpected"),
                duration: 2000
            });
        } catch (e: any) {
            console.error(e);
            toast({
                variant: "error",
                title: tCommon("toasts.error"),
                description: e?.message ?? tCommon("errors.unexpected"),
                duration: 2000
            });
        }
    };

    const handleSubmit = async () => {
        setIsLoading(true);

        try {
            const result = await createUser(
                {
                    email: userEmail,
                    companyId,
                    role: UserRole.ContentManager
                },
                authFetch
            );

            const response = await result.json();

            if (result.ok) {
                await dataQuery.refetch();

                setErrorMessages("");
                setPassword(response.password);

                toast({
                    variant: "success",
                    title: tCommon("toasts.success"),
                    description: tCommon("toasts.userCreated"),
                    duration: 2000
                });

                return;
            }

            setErrorMessages(
                response.errors?.["Email"]?.reduce((acc: string, item: string) => `${acc} ${item}`, "") ||
                tCommon("errors.unexpected")
            );
        } catch (e: any) {
            console.error(e);
            toast({
                variant: "error",
                title: tCommon("toasts.error"),
                description: e?.message ?? tCommon("errors.unexpected"),
                duration: 2000
            });
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div>
            <div className="flex justify-end gap-x-3 mb-4">
                <AssignUserModal companyId={companyId} onAssign={assignUserToCompany} />

                <NewUserModal
                    email={userEmail}
                    setEmail={setUserEmail}
                    password={password}
                    setPassword={setPassword}
                    errorMessages={errorMessages}
                    setErrorMessages={setErrorMessages}
                    isLoading={isLoading}
                    setIsLoading={setIsLoading}
                    handleSubmit={handleSubmit}
                />
            </div>

            <UsersTable
                pagination={pagination}
                setPagination={setPagination}
                sorting={sorting}
                setSorting={setSorting}
                dataQuery={dataQuery}
            />
        </div>
    );
};
