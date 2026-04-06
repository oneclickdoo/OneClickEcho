"use client";

import { useMemo, useRef, useState } from "react";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { makeShortcut, ShortcutHeader } from "@/context/ShortcutsContext";

import { Button } from "@/components/tremor/Button";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle
} from "@/components/tremor/Dialog";
import { Textarea } from "@/components/tremor/Textarea";

import type { CampaignLeadsTableHandle } from "@/elements/dashboard/singleCampaign/tables/CampaignLeadsTable";
import { CampaignLeadsTable } from "@/elements/dashboard/singleCampaign/tables/CampaignLeadsTable";
import { downloadExampleLeadCsv, exportCompanyLeads, uploadLeads } from "@/elements/dashboard/singleCampaign/fetchData";

import { useToast } from "@/lib/useToast";
import { downloadFile } from "@/lib/download";
import { buildMinimalLeadsCsv, parsePhoneListFromText } from "@/lib/phoneImport";

export default function LeadsPage() {
    const t = useTranslations("LeadsPage");

    const { dashboardManager, authFetch } = useAuth();

    const { toast } = useToast();

    const tableRef = useRef<CampaignLeadsTableHandle | null>(null);

    const [importOpen, setImportOpen] = useState(false);
    const [importText, setImportText] = useState("");
    const [importLoading, setImportLoading] = useState(false);

    const parsed = useMemo(() => parsePhoneListFromText(importText), [importText]);

    const exportLeads = async () => {
        if (!dashboardManager?.currentCompany?.companyId) return;

        try {
            const blob: Blob = await exportCompanyLeads(dashboardManager.currentCompany.companyId, authFetch);

            downloadFile(blob, `leads-${dashboardManager.currentCompany.companyId}.csv`);

            toast({
                title: t("toasts.successTitle"),
                description: t("toasts.exported"),
                variant: "success",
                duration: 2000
            });
        } catch (e) {
            console.error(e);
            toast({
                title: t("toasts.errorTitle"),
                description: t("errors.networkNotOk"),
                variant: "error",
                duration: 3000
            });
        }
    };

    const exportBlacklistedLeads = async () => {
        if (!dashboardManager?.currentCompany?.companyId) return;

        try {
            const blob: Blob = await exportCompanyLeads(dashboardManager.currentCompany.companyId, authFetch, {
                blacklistedOnly: true
            });

            downloadFile(blob, `leads-blacklisted-${dashboardManager.currentCompany.companyId}.csv`);

            toast({
                title: t("toasts.successTitle"),
                description: t("toasts.blacklistExported"),
                variant: "success",
                duration: 2000
            });
        } catch (e) {
            console.error(e);
            toast({
                title: t("toasts.errorTitle"),
                description: t("errors.networkNotOk"),
                variant: "error",
                duration: 3000
            });
        }
    };

    const downloadExampleCsv = async () => {
        try {
            const blob: Blob = await downloadExampleLeadCsv(authFetch);

            downloadFile(blob, "leads.csv");

            toast({
                title: t("toasts.successTitle"),
                description: t("toasts.exampleDownloaded"),
                variant: "success",
                duration: 2000
            });
        } catch (e) {
            console.error(e);
            toast({
                title: t("toasts.errorTitle"),
                description: t("errors.networkNotOk"),
                variant: "error",
                duration: 3000
            });
        }
    };

    const submitImportPhones = async () => {
        const companyId = dashboardManager?.currentCompany?.companyId;
        if (!companyId) return;

        if (parsed.validPhones.length === 0) {
            toast({
                title: t("toasts.errorTitle"),
                description: t("importPhones.noValid"),
                variant: "error",
                duration: 3000
            });
            return;
        }

        setImportLoading(true);

        try {
            const csv = buildMinimalLeadsCsv(parsed.validPhones);
            const blob = new Blob([csv], { type: "text/csv;charset=utf-8" });
            const file = new File([blob], "phone-import.csv", { type: "text/csv" });

            await uploadLeads(file, companyId, null, authFetch);

            toast({
                title: t("toasts.successTitle"),
                description: t("toasts.imported"),
                variant: "success",
                duration: 2000
            });

            tableRef.current?.refetchData();
            setImportOpen(false);
            setImportText("");
        } catch (e) {
            console.error(e);
            toast({
                title: t("toasts.errorTitle"),
                description: t("errors.networkNotOk"),
                variant: "error",
                duration: 3000
            });
        } finally {
            setImportLoading(false);
        }
    };

    return (
        <div className="flex flex-col">
            <div className="flex items-center justify-between max-md:flex-col gap-4 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                <ShortcutHeader shortcut={makeShortcut(t("title"), "/leads")}>
                    <h1 className="ml-4 md:ml-0 text-lg font-semibold text-gray-900 sm:text-xl dark:text-gray-50">
                        {t("title")}
                    </h1>
                </ShortcutHeader>

                <div className="flex flex-wrap items-center justify-end gap-3">
                    <Button
                        variant="primary"
                        className="h-8 text-white border-transparent bg-teal-600 outline-teal-500 hover:bg-teal-500 disabled:bg-teal-100 disabled:text-gray-400 dark:bg-teal-500 dark:text-gray-900 dark:outline-teal-500 dark:hover:bg-teal-600 disabled:dark:bg-teal-800 disabled:dark:text-teal-400"
                        onClick={() => setImportOpen(true)}
                    >
                        {t("buttons.importPhones")}
                    </Button>

                    <Button
                        variant="primary"
                        className="h-8 text-white border-transparent bg-teal-600 outline-teal-500 hover:bg-teal-500 disabled:bg-teal-100 disabled:text-gray-400 dark:bg-teal-500 dark:text-gray-900 dark:outline-teal-500 dark:hover:bg-teal-600 disabled:dark:bg-teal-800 disabled:dark:text-teal-400"
                        onClick={exportLeads}
                    >
                        {t("buttons.export")}
                    </Button>

                    <Button variant="secondary" className="h-8" onClick={exportBlacklistedLeads}>
                        {t("buttons.exportBlacklist")}
                    </Button>
                </div>
            </div>

            <Dialog
                open={importOpen}
                onOpenChange={(open) => {
                    setImportOpen(open);
                    if (!open) setImportText("");
                }}
            >
                <DialogContent className="sm:max-w-lg" aria-describedby="import-phones-dialog-desc">
                    <DialogHeader>
                        <DialogTitle>{t("importPhones.title")}</DialogTitle>
                        <p id="import-phones-dialog-desc" className="text-sm text-gray-600 dark:text-gray-400">
                            {t("importPhones.description")}
                        </p>
                    </DialogHeader>

                    <Textarea
                        className="min-h-[10rem] font-mono text-sm"
                        placeholder={t("importPhones.placeholder")}
                        value={importText}
                        onChange={(e) => setImportText(e.target.value)}
                        disabled={importLoading}
                    />

                    <div className="space-y-1 text-sm text-gray-700 dark:text-gray-300">
                        <p>{t("importPhones.validCount", { count: parsed.validPhones.length })}</p>
                        {parsed.duplicateCount > 0 ? (
                            <p className="text-amber-700 dark:text-amber-400">
                                {t("importPhones.duplicatesRemoved", { count: parsed.duplicateCount })}
                            </p>
                        ) : null}
                        {parsed.invalidTokens.length > 0 ? (
                            <div>
                                <p className="font-medium text-red-700 dark:text-red-400">{t("importPhones.invalidLabel")}</p>
                                <ul className="mt-1 max-h-24 list-inside list-disc overflow-y-auto text-xs text-gray-600 dark:text-gray-400">
                                    {parsed.invalidTokens.slice(0, 40).map((x, i) => (
                                        <li key={`${i}-${x}`}>{x}</li>
                                    ))}
                                    {parsed.invalidTokens.length > 40 ? <li>…</li> : null}
                                </ul>
                            </div>
                        ) : null}
                    </div>

                    <DialogFooter className="mt-2 flex flex-col gap-2 sm:flex-row sm:justify-end">
                        <DialogClose asChild>
                            <Button variant="secondary" className="h-8" disabled={importLoading}>
                                {t("importPhones.cancel")}
                            </Button>
                        </DialogClose>
                        <Button
                            variant="primary"
                            className="h-8 text-white border-transparent bg-teal-600 hover:bg-teal-500 dark:bg-teal-500 dark:text-gray-900 dark:hover:bg-teal-600"
                            onClick={submitImportPhones}
                            disabled={importLoading || parsed.validPhones.length === 0}
                        >
                            {importLoading ? "…" : t("importPhones.submit")}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

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
