"use client";

import { useState } from "react";

import type { SingleValue } from "react-select";
import AsyncSelect from "react-select/async";
import { useTranslations } from "next-intl";

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

import { addToCollectionFromStatus, searchLeadCollections } from "@/elements/dashboard/singleCampaign/fetchData";

import type { Option } from "@/data/schema";
import type { CampaignLeadSMSStatus, CampaignLeadViberStatusCollection } from "@/lib/enums";
import { useToast } from "@/lib/useToast";

interface IAddToCollectionFromStatusModalProps {
    campaignId: string;
    viberStatus: CampaignLeadViberStatusCollection | null;
    smsStatus: CampaignLeadSMSStatus | null;
}

export const AddToCollectionFromStatusModal = (props: IAddToCollectionFromStatusModalProps) => {
    const t = useTranslations("SingleCampaign.Modals.AddToCollectionFromStatus");
    const [selectedCollection, setSelectedCollection] = useState<SingleValue<Option | null>>(null);
    const [open, setOpen] = useState(false);

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const searchOptions = async (query: string): Promise<Option[]> => {
        if (!query) return [];

        const leadCollections = await searchLeadCollections(props.campaignId, query, authFetch);
        return leadCollections.map((leadCollection) => ({
            value: leadCollection.leadCollectionId,
            label: leadCollection.collectionName
        }));
    };

    const onCollectionChange = (value: SingleValue<Option>) => {
        setSelectedCollection(value);
    };

    const handleSubmit = async () => {
        if (!selectedCollection) return;

        try {
            const result = await addToCollectionFromStatus(
                props.campaignId,
                selectedCollection.value,
                props.viberStatus,
                props.smsStatus,
                authFetch
            );

            if (result.ok) {
                toast({
                    title: t("toastSuccessTitle"),
                    description: t("toastSuccessDescription"),
                    variant: "success",
                    duration: 2000
                });

                setOpen(false);
                setSelectedCollection(null);
                return;
            }

            // ako backend vraća ok=false bez exception-a
            toast({
                title: t("toastErrorTitle"),
                description: t("toastErrorDescription"),
                variant: "error",
                duration: 2500
            });
        } catch (e) {
            console.error(e);
            toast({
                title: t("toastErrorTitle"),
                description: t("toastErrorDescription"),
                variant: "error",
                duration: 2500
            });
        }
    };

    return (
        <Dialog
            open={open}
            onOpenChange={(nextOpen) => {
                setOpen(nextOpen);
                if (!nextOpen) setSelectedCollection(null);
            }}
        >
            <DialogTrigger asChild>
                <Button className="h-[25px] bg-green-500 dark:bg-green-400" onClick={() => setOpen(true)}>
                    {t("trigger")}
                </Button>
            </DialogTrigger>

            <DialogContent className="sm:max-w-lg" aria-describedby="assign-list-dialog">
                <DialogHeader className="min-h-[150px]">
                    <DialogTitle>{t("title")}</DialogTitle>
                    <DialogDescription>
                        <p className="mt-1 text-sm text-gray-400">{t("description")}</p>

                        <div className="mt-5">
                            <AsyncSelect
                                value={selectedCollection}
                                defaultOptions
                                placeholder={t("placeholder")}
                                className="mx-auto w-full"
                                loadOptions={searchOptions}
                                onChange={(value: SingleValue<Option>) => onCollectionChange(value)}
                            />
                        </div>
                    </DialogDescription>
                </DialogHeader>

                <DialogFooter className="mt-6 flex flex-col justify-end gap-2 sm:justify-end">
                    <Button variant="secondary" className="mt-2 w-full sm:mt-0 sm:w-fit" onClick={() => setOpen(false)}>
                        {t("goBack")}
                    </Button>

                    <Button
                        disabled={!selectedCollection}
                        className="flex w-full gap-x-2 border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400 sm:w-fit"
                        onClick={() => {
                            handleSubmit().then();
                        }}
                    >
                        {t("assign")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
