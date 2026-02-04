"use client";

import { Dispatch, SetStateAction, useState } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";

import { Card } from "@/components/tremor/Card";
import { Divider } from "@/components/tremor/Divider";
import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";

import { CollectionDto } from "../../collections/fetchData";
import { updateCollection, deleteCollection } from "../fetchData";

import { useToast } from "@/lib/useToast";

interface ICollectionSettingsTabProps {
    collection: CollectionDto;
    setCollection: Dispatch<SetStateAction<CollectionDto>>;
}

export const CollectionSettingsTab = (props: ICollectionSettingsTabProps) => {
    const tCommon = useTranslations("Common");
    const router = useRouter();

    const [isLoading, setIsLoading] = useState(false);

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const handleChange = (value: string) => {
        props.setCollection((prevData: CollectionDto): CollectionDto => ({
            ...prevData,
            collectionName: value
        }));
    };

    const handleSubmit = async () => {
        try {
            await updateCollection(props.collection, authFetch);

            toast({
                title: tCommon("toasts.success"),
                description: tCommon("toasts.leadListSaved"),
                variant: "success",
                duration: 2000
            });
        } catch (e) {
            console.error(e);

            toast({
                title: tCommon("toasts.error"),
                description: tCommon("errors.unexpected"),
                variant: "error",
                duration: 2000
            });
        }
    };

    const handleDelete = async () => {
        setIsLoading(true);

        try {
            await deleteCollection(props.collection.leadCollectionId, authFetch);

            toast({
                title: tCommon("toasts.success"),
                description: tCommon("toasts.leadListDeleted"),
                variant: "success",
                duration: 2000
            });

            // Ako koristiš /[locale]/... rute, ovde treba locale-aware push (npr. withLocale("/collections"))
            router.push("/collections");
        } catch (e) {
            console.error(e);

            toast({
                title: tCommon("toasts.error"),
                description: tCommon("errors.unexpected"),
                variant: "error",
                duration: 2000
            });
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div>
            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                <div>
                    <h2 className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {tCommon("collectionSettings.mainInformationTitle")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">
                        {tCommon("collectionSettings.mainInformationDescription")}
                    </p>
                </div>

                <div className="md:col-span-2">
                    <div className="mb-5">
                        <Label>{tCommon("collectionSettings.nameLabel")}</Label>
                        <Input value={props.collection.collectionName} onChange={(e) => handleChange(e.target.value)} />
                    </div>

                    <div className="flex justify-end">
                        <Button
                            className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            type="submit"
                            color="primary"
                            onClick={handleSubmit}
                        >
                            {tCommon("collectionSettings.saveLeadList")}
                        </Button>
                    </div>
                </div>
            </section>

            <Divider />

            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                <div>
                    <h2 className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {tCommon("collectionSettings.dangerZoneTitle")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">
                        {tCommon("collectionSettings.dangerZoneDescription")}
                    </p>
                </div>

                <div className="md:col-span-2">
                    <Card className="overflow-hidden p-0">
                        <div className="text-gray-900 dark:text-gray-50 border-b border-tremor-border bg-tremor-background-muted px-4 py-3 dark:border-dark-tremor-border dark:bg-dark-tremor-background-muted">
                            <label className="font-medium text-tremor-content-strong dark:text-dark-tremor-content-strong">
                                {tCommon("collectionSettings.deleteLeadListTitle")}
                            </label>
                        </div>

                        <div className="flex items-center justify-between space-x-10 px-4 py-3 text-gray-500">
                            <p className="text-sm leading-6 text-tremor-content dark:text-dark-tremor-content">
                                {tCommon("collectionSettings.deleteLeadListWarning")}
                            </p>

                            <Button variant="destructive" className="py-1" isLoading={isLoading} onClick={handleDelete}>
                                {tCommon("common.delete")}
                            </Button>
                        </div>
                    </Card>
                </div>
            </section>
        </div>
    );
};
