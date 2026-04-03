"use client";

import { useEffect, useRef, useState } from "react";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";

import { Button } from "@/components/tremor/Button";

import type { CampaignLeadsTableHandle } from "@/elements/dashboard/singleCampaign/tables/CampaignLeadsTable";
import { CampaignLeadsTable } from "@/elements/dashboard/singleCampaign/tables/CampaignLeadsTable";
import { exportCompanyLeads, downloadExampleLeadCsv } from "@/elements/dashboard/singleCampaign/fetchData";

import { useToast } from "@/lib/useToast";
import { downloadFile } from "@/lib/download";

import { CsvUpload } from "@/components/custom/CsvUpload";
import type { FileWithPath } from "react-dropzone";

export default function LeadsPage() {
    const t = useTranslations("LeadsPage");

    const { dashboardManager, authFetch } = useAuth();
    const [file, setFile] = useState<FileWithPath | null>(null);

    const { toast } = useToast();

    const uploadInputRef = useRef<HTMLInputElement>(null);
    const tableRef = useRef<CampaignLeadsTableHandle | null>(null);

    const exportLeads = async () => {
        if (!dashboardManager?.currentCompany?.companyId) return;

        const blob: Blob = await exportCompanyLeads(dashboardManager.currentCompany.companyId, authFetch);

        downloadFile(blob, `leads-${dashboardManager.currentCompany.companyId}.csv`);

        toast({
            title: t("toasts.successTitle"),
            description: t("toasts.exported"),
            variant: "success",
            duration: 2000
        });
    };

    const downloadExampleCsv = async () => {
        const blob: Blob = await downloadExampleLeadCsv(authFetch);

        downloadFile(blob, "leads.csv");

        toast({
            title: t("toasts.successTitle"),
            description: t("toasts.exampleDownloaded"),
            variant: "success",
            duration: 2000
        });
    };

    const uploadBlacklist = () => {
        uploadInputRef.current?.click();
    };

    useEffect(() => {
        const uploadFile = async (currentFile: File, companyId: string) => {
            const url = new URL(`/api/Company/${companyId}/UploadBlacklist`, window.location.origin);

            const formData = new FormData();
            formData.append("file", currentFile);

            const response = await authFetch(url.toString(), {
                method: "POST",
                body: formData,
                credentials: "include"
            });

            if (!response.ok) {
                throw new Error(t("errors.networkNotOk"));
            }

            return await response.json();
        };

        if (!file || !dashboardManager?.currentCompany?.companyId) return;

        uploadFile(file, dashboardManager.currentCompany.companyId)
            .then(() => {
                toast({
                    title: t("toasts.successTitle"),
                    description: t("toasts.blacklistUploaded"),
                    variant: "success",
                    duration: 2000
                });

                // ✅ refresh tabela bez remount-a
                tableRef.current?.refetchData();

                // ✅ reset file da se ne ponovi upload
                setFile(null);
            })
            .catch((err) => {
                console.error(err);
                toast({
                    title: t("toasts.errorTitle"),
                    description: err?.message ?? t("errors.networkNotOk"),
                    variant: "error",
                    duration: 2000
                });
            });

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [file]);

    return (
        <div className="flex flex-col">
            <div className="flex items-center justify-between max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <ShortcutHeader shortcut={makeShortcut(t("title"), "/leads")}>
                    <h1 className="ml-4 md:ml-0 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                        {t("title")}
                    </h1>
                </ShortcutHeader>

                <div className="flex items-center justify-between max-md:flex-col gap-3">
                    <Button
                        variant="primary"
                        className="h-8 text-white border-transparent bg-teal-600 outline-teal-500 hover:bg-teal-500 disabled:bg-teal-100 disabled:text-gray-400 dark:bg-teal-500 dark:text-gray-900 dark:outline-teal-500 dark:hover:bg-teal-600 disabled:dark:bg-teal-800 disabled:dark:text-teal-400"
                        onClick={exportLeads}
                    >
                        {t("buttons.export")}
                    </Button>

                    <Button variant="destructive" className="h-8" onClick={uploadBlacklist}>
                        {t("buttons.uploadBlacklist")}
                    </Button>

                    <div className="hidden">
                        <CsvUpload file={file} setFile={setFile} refObject={uploadInputRef} />
                    </div>
                </div>
            </div>

            <p
                className="mb-4 max-w-[215px] text-indigo-400 cursor-pointer hover:underline hover:text-indigo-500"
                onClick={downloadExampleCsv}
            >
                {t("downloadExample")}
            </p>

            <CampaignLeadsTable ref={tableRef} />
        </div>
    );
}
