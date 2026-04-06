"use client";

import { useState, useEffect, useRef } from "react";
import { useTranslations } from "next-intl";

import { FileWithPath } from "react-dropzone";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { PaginationState, SortingState } from "@tanstack/react-table";

import { useAuth } from "@/context/AuthContext";

import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger
} from "@/components/tremor/Dialog";
import { CsvUpload } from "@/components/custom/CsvUpload";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";
import Loading from "@/components/tremor/Loading";

import { CollectionLeadsTable } from "../tables/CollectionLeadsTable";
import { uploadAndAssignCollectionLeads, createAndAssignLead, getCollectionLeads } from "../fetchData";

import { useToast } from "@/lib/useToast";
import { FilterManager } from "@/components/filtering/FilterManager";
import { exportCompanyLeadsByCollection } from "@/elements/dashboard/singleCampaign/fetchData";
import { downloadFile } from "@/lib/download";

interface ICollectionLeadsTab {
    collectionId: string;
}

export const CollectionLeadsTab = ({ collectionId }: ICollectionLeadsTab) => {
    const tCommon = useTranslations("Common");

    const [leadPhoneNumber, setLeadPhoneNumber] = useState<string>("");
    const [file, setFile] = useState<FileWithPath | null>(null);
    const [errorMessages, setErrorMessages] = useState<string>("");
    const [isLoading, setIsLoading] = useState<boolean>(false);

    // NOTE: ako endpoint ne podržava createdAt, promeni na validno polje (npr phoneNumber)
    const [sorting, setSorting] = useState<SortingState>([{ id: "createdAt", desc: true }]);
    const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 10 });

    const { dashboardManager, authFetch } = useAuth();
    const [filterManager] = useState(() => new FilterManager(dashboardManager!.currentCompany!.companyId));

    const uploadInputRef = useRef<HTMLInputElement>(null);

    const { toast } = useToast();

    const dataQuery = useQuery({
        queryKey: ["collection-leads", collectionId, pagination, sorting, filterManager.filters],
        queryFn: () => getCollectionLeads(pagination, sorting, filterManager.generate(), collectionId, authFetch),
        placeholderData: keepPreviousData
    });

    const createAndAssignNewLead = async () => {
        if (!dashboardManager?.currentCompany?.companyId) return;

        setIsLoading(true);

        try {
            const result = await createAndAssignLead(
                {
                    companyId: dashboardManager.currentCompany.companyId,
                    leadCollectionId: collectionId,
                    phoneNumber: leadPhoneNumber
                },
                authFetch
            );

            const response = await result.json();

            if (!result.ok) {
                setErrorMessages(
                    response.errors?.["PhoneNumber"]?.reduce((acc: string, item: string) => `${acc} ${item}`, "") ||
                    tCommon("errors.unexpected")
                );
                return;
            }

            await dataQuery.refetch();

            setErrorMessages("");
            setLeadPhoneNumber("");

            toast({
                variant: "success",
                title: tCommon("toasts.success"),
                description: tCommon("toasts.leadCreatedAssigned"),
                duration: 2000
            });
        } catch (e: any) {
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

    const handleUpload = async () => {
        if (!file) return;

        setIsLoading(true);

        try {
            await uploadAndAssignCollectionLeads(file, collectionId, authFetch);

            window.scrollTo({ top: 0, behavior: "smooth" });

            toast({
                variant: "success",
                title: tCommon("toasts.success"),
                description: tCommon("toasts.leadsUploaded"),
                duration: 2000
            });

            await dataQuery.refetch();
            setFile(null);
        } catch (e: any) {
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

    const uploadMore = () => {
        uploadInputRef.current?.click();
    };

    const exportLeads = async () => {
        if (!dashboardManager?.currentCompany?.companyId) return;

        const blob: Blob = await exportCompanyLeadsByCollection(dashboardManager.currentCompany.companyId, collectionId, authFetch);
        downloadFile(blob, `leads-${collectionId}.csv`);

        toast({
            variant: "success",
            title: tCommon("toasts.success"),
            description: tCommon("toasts.companyLeadsExported"),
            duration: 2000
        });
    };

    const newLeadDialog = (): JSX.Element => {
        return (
            <Dialog
                onOpenChange={() => {
                    setLeadPhoneNumber("");
                    setErrorMessages("");
                    setIsLoading(false);
                }}
            >
                <DialogTrigger asChild>
                    <Button
                        variant="primary"
                        className="text-white border-transparent bg-indigo-600 outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                        disabled={isLoading}
                    >
                        {tCommon("lead.newLead")}
                    </Button>
                </DialogTrigger>

                <DialogContent className="sm:max-w-lg" aria-describedby="create-new-lead-dialog">
                    <DialogHeader>
                        <DialogTitle>{tCommon("lead.createAndAssignTitle")}</DialogTitle>

                        <p className="text-sm text-gray-400 mt-1">{tCommon("lead.createAndAssignHint")}</p>

                        <div className="mt-5">
                            <Input
                                value={leadPhoneNumber}
                                onChange={(e) => setLeadPhoneNumber(e.target.value)}
                                className="mx-auto w-full"
                                hasError={!!errorMessages}
                                placeholder={tCommon("lead.enterPhonePlaceholder")}
                            />
                            {errorMessages && <p className="mt-1 text-xs text-red-600">{errorMessages}</p>}
                        </div>
                    </DialogHeader>

                    <DialogFooter className="mt-6 flex justify-end sm:justify-end flex-col gap-2">
                        <DialogClose asChild>
                            <Button className="mt-2 w-full sm:mt-0 sm:w-fit" variant="secondary">
                                {tCommon("common.goBack")}
                            </Button>
                        </DialogClose>

                        <Button
                            isLoading={isLoading}
                            className="w-full sm:w-fit border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            onClick={createAndAssignNewLead}
                        >
                            {tCommon("lead.createAndAssign")}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        );
    };

    useEffect(() => {
        if (file && dataQuery.data && dataQuery.data.rowCount > 0) {
            handleUpload();
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [file]);

    const hasLeads = !!dataQuery.data?.rowCount && dataQuery.data.rowCount > 0;

    return (
        <div>
            {dataQuery.isLoading ? (
                <Loading text={tCommon("lead.loadingLeads")} />
            ) : (
                <div className={`${hasLeads ? "hidden" : ""}`}>
                    <CsvUpload file={file} setFile={setFile} refObject={uploadInputRef} />

                    {file ? (
                        <div className="flex justify-center mt-3">
                            <Button
                                variant="primary"
                                className="text-white border-transparent bg-green-600 outline-green-500 hover:bg-green-500 disabled:bg-green-100 disabled:text-gray-400 dark:bg-green-500 dark:text-gray-900 dark:outline-green-500 dark:hover:bg-green-600 disabled:dark:bg-green-800 disabled:dark:text-green-400"
                                onClick={handleUpload}
                                isLoading={isLoading}
                                disabled={isLoading}
                            >
                                {tCommon("lead.uploadCsv")}
                            </Button>
                        </div>
                    ) : null}

                    <div className="flex justify-center mt-3">{newLeadDialog()}</div>
                </div>
            )}

            {!dataQuery.isLoading && hasLeads ? (
                <>
                    <div className="flex justify-center gap-5 mb-5">
                        {newLeadDialog()}

                        <Button
                            variant="primary"
                            className="text-white border-transparent bg-green-600 outline-green-500 hover:bg-green-500 disabled:bg-green-100 disabled:text-gray-400 dark:bg-green-500 dark:text-gray-900 dark:outline-green-500 dark:hover:bg-green-600 disabled:dark:bg-green-800 disabled:dark:text-green-400"
                            onClick={uploadMore}
                        >
                            {tCommon("lead.uploadMoreLeads")}
                        </Button>

                        <Button
                            variant="primary"
                            className="text-white border-transparent bg-blue-600 outline-blue-500 hover:bg-blue-500 disabled:bg-blue-100 disabled:text-gray-400 dark:bg-blue-500 dark:text-gray-900 dark:outline-blue-500 dark:hover:bg-blue-600 disabled:dark:bg-blue-800 disabled:dark:text-blue-400"
                            onClick={exportLeads}
                        >
                            {tCommon("lead.exportLeads")}
                        </Button>
                    </div>

                    <CollectionLeadsTable
                        dataQuery={dataQuery}
                        pagination={pagination}
                        setPagination={setPagination}
                        sorting={sorting}
                        setSorting={setSorting}
                        filterManager={filterManager}
                    />
                </>
            ) : null}
        </div>
    );
};
