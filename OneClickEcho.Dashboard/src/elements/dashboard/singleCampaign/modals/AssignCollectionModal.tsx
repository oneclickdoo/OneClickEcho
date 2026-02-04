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

import { searchLeadCollections } from "@/elements/dashboard/singleCampaign/fetchData";

import type { Option } from "@/data/schema";

interface IAssignCollectionModalProps {
    campaignId: string;
    onAssign: (collectionId: string) => void;
}

export const AssignCollectionModal = (props: IAssignCollectionModalProps) => {
    const t = useTranslations("SingleCampaign.Modals.AssignCollection");

    const [selectedCollection, setSelectedCollection] = useState<SingleValue<Option | null>>(null);
    const [open, setOpen] = useState(false);

    const { authFetch } = useAuth();

    const searchOptions = async (query: string): Promise<Option[]> => {
        const leadCollections = await searchLeadCollections(props.campaignId, query ?? "", authFetch);

        return leadCollections.map((leadCollection) => ({
            value: leadCollection.leadCollectionId,
            label: leadCollection.collectionName
        }));
    };

    const onCollectionChange = (value: SingleValue<Option>) => {
        setSelectedCollection(value);
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
                <Button
                    type="button"
                    variant="primary"
                    className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                    onClick={() => setOpen(true)}
                >
                    {t("trigger")}
                </Button>
            </DialogTrigger>

            <DialogContent className="sm:max-w-lg overflow-visible" aria-describedby="assign-collection-dialog">
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
                            if (!selectedCollection) return;
                            props.onAssign(selectedCollection.value);
                            setOpen(false);
                            setSelectedCollection(null);
                        }}
                    >
                        {t("assign")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
