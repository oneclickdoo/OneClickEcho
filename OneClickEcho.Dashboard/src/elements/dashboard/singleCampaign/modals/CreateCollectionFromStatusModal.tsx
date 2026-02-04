"use client";

import { useState } from "react";

import { useTranslations } from "next-intl";

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
import { Button } from "@/components/tremor/Button";
import { Input } from "@/components/tremor/Input";

import { createCollectionFromStatus } from "@/elements/dashboard/singleCampaign/fetchData";

import type { CampaignLeadSMSStatus, CampaignLeadViberStatusCollection } from "@/lib/enums";
import { useToast } from "@/lib/useToast";

interface ICreateCollectionFromStatusModalProps {
    campaignId: string;
    viberStatus: CampaignLeadViberStatusCollection | null;
    smsStatus: CampaignLeadSMSStatus | null;
}

export const CreateCollectionFromStatusModal = (props: ICreateCollectionFromStatusModalProps) => {
    const t = useTranslations("SingleCampaign.Modals.CreateCollectionFromStatus");
    const { toast } = useToast();

    const [collectionName, setCollectionName] = useState("");
    const [errorMessages, setErrorMessages] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [isOpen, setIsOpen] = useState(false);

    const { dashboardManager, authFetch } = useAuth();

    const resetForm = () => {
        setCollectionName("");
        setErrorMessages("");
        setIsLoading(false);
    };

    const handleSubmit = async () => {
        const nameTrimmed = collectionName.trim();
        if (!nameTrimmed) {
            setErrorMessages(t("validationRequired"));
            return;
        }

        setIsLoading(true);
        setErrorMessages("");

        try {
            const result = await createCollectionFromStatus(
                dashboardManager!.currentCompany!.companyId!,
                props.campaignId,
                props.viberStatus,
                props.smsStatus,
                nameTrimmed,
                authFetch
            );

            let response: any = null;
            try {
                response = await result.json();
            } catch {
                // ignore non-json responses
            }

            if (result.ok) {
                toast({
                    title: t("toastSuccessTitle"),
                    description: t("toastSuccessDescription"),
                    variant: "success",
                    duration: 2000
                });

                setIsOpen(false);
                resetForm();
                return;
            }

            const nameErrors: string[] | undefined = response?.errors?.["Name"];
            const serverMessage =
                (Array.isArray(nameErrors) && nameErrors.length
                    ? nameErrors.join(" ")
                    : response?.message) || t("errorUnexpected");

            setErrorMessages(serverMessage);

            toast({
                title: t("toastErrorTitle"),
                description: serverMessage,
                variant: "error",
                duration: 2500
            });
        } catch (e) {
            console.error(e);

            toast({
                title: t("toastErrorTitle"),
                description: t("errorUnexpected"),
                variant: "error",
                duration: 2500
            });

            setErrorMessages(t("errorUnexpected"));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <Dialog
            open={isOpen}
            onOpenChange={(nextOpen) => {
                setIsOpen(nextOpen);
                if (!nextOpen) resetForm();
            }}
        >
            <DialogTrigger asChild>
                <Button className="h-[25px] bg-blue-500 dark:bg-blue-400" type="button" onClick={() => setIsOpen(true)}>
                    {t("trigger")}
                </Button>
            </DialogTrigger>

            <DialogContent className="sm:max-w-lg" aria-describedby="create-new-list-dialog">
                <DialogHeader>
                    <DialogTitle>{t("title")}</DialogTitle>
                    <p className="mt-1 text-sm text-gray-400">{t("description")}</p>

                    <div className="mt-5">
                        <Input
                            value={collectionName}
                            onChange={(e) => setCollectionName(e.target.value)}
                            className="mx-auto w-full"
                            hasError={!!errorMessages}
                            placeholder={t("placeholder")}
                        />
                        {errorMessages ? <p className="mt-1 text-xs text-red-600">{errorMessages}</p> : null}
                    </div>
                </DialogHeader>

                <DialogFooter className="mt-6 flex flex-col justify-end gap-2 sm:justify-end">
                    <DialogClose asChild>
                        <Button
                            className="mt-2 w-full sm:mt-0 sm:w-fit"
                            variant="secondary"
                            onClick={() => setIsOpen(false)}
                        >
                            {t("goBack")}
                        </Button>
                    </DialogClose>

                    <Button
                        onClick={handleSubmit}
                        isLoading={isLoading}
                        disabled={isLoading || !collectionName.trim()}
                        className="w-full border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400 sm:w-fit"
                    >
                        {t("create")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
