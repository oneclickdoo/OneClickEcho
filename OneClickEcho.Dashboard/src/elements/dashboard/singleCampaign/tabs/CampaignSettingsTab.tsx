"use client";

import type { Dispatch, SetStateAction } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";

import { Card } from "@/components/tremor/Card";
import { Divider } from "@/components/tremor/Divider";
import { Label } from "@/components/tremor/Label";
import { Input } from "@/components/tremor/Input";
import { Button } from "@/components/tremor/Button";

import type { CampaignDto } from "../fetchData";
import { deleteCampaign, updateCampaign } from "../fetchData";

import { useToast } from "@/lib/useToast";
import { CampaignStatus } from "@/lib/enums";

interface ICampaignSettingsTabProps {
    campaign: CampaignDto;

    // TS71007 warning je "Next lint" stvar (serializable props), ali ovo je client component
    // i normalno je da proslediš setter iz parent client state-a.
    setCampaign: Dispatch<SetStateAction<CampaignDto>>;
}

export const CampaignSettingsTab = (props: ICampaignSettingsTabProps) => {
    const t = useTranslations("SingleCampaign.Tabs.CampaignSettings");
    const tCommon = useTranslations("Common");

    const router = useRouter();
    const { authFetch } = useAuth();
    const { toast } = useToast();

    const handleChange = (value: string) => {
        props.setCampaign((prev): CampaignDto => ({ ...prev, name: value }));
    };

    const handleSubmit = async () => {
        try {
            await updateCampaign(props.campaign, authFetch);

            toast({
                title: t("toasts.savedTitle"),
                description: t("toasts.savedDescription"),
                variant: "success",
                duration: 2000
            });
        } catch (e) {
            console.error(e);

            toast({
                title: tCommon("error"),
                description: tCommon("somethingWentWrong"),
                variant: "error",
                duration: 2500
            });
        }
    };

    const handleDelete = async () => {
        try {
            await deleteCampaign(props.campaign.campaignId, authFetch);

            toast({
                title: t("toasts.deletedTitle"),
                description: t("toasts.deletedDescription"),
                variant: "success",
                duration: 2000
            });

            router.push("/campaigns");
        } catch (e) {
            console.log(e);

            toast({
                title: tCommon("error"),
                description: tCommon("somethingWentWrong"),
                variant: "error",
                duration: 2500
            });
        }
    };

    return (
        <div>
            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                <div>
                    <h2 id="campaign-main" className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {t("main.title")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">{t("main.subtitle")}</p>
                </div>

                <div className="md:col-span-2">
                    <div className="mb-5">
                        <Label>{t("main.nameLabel")}</Label>
                        <Input
                            disabled={props.campaign.status !== CampaignStatus.Draft}
                            value={props.campaign.name}
                            onChange={(e) => handleChange(e.target.value)}
                        />
                    </div>

                    <div className="flex justify-end">
                        <Button
                            disabled={props.campaign.status !== CampaignStatus.Draft}
                            className="border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                            type="button"
                            color="primary"
                            onClick={handleSubmit}
                        >
                            {t("actions.save")}
                        </Button>
                    </div>
                </div>
            </section>

            <Divider />

            <section className="grid grid-cols-1 gap-x-14 gap-y-8 md:grid-cols-3">
                <div>
                    <h2 id="campaign-danger" className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                        {t("danger.title")}
                    </h2>
                    <p className="mt-1 text-sm leading-6 text-gray-500">{t("danger.subtitle")}</p>
                </div>

                <div className="md:col-span-2">
                    <Card className="overflow-hidden p-0">
                        <div className="border-b border-tremor-border bg-tremor-background-muted px-4 py-3 text-gray-900 dark:border-dark-tremor-border dark:bg-dark-tremor-background-muted dark:text-gray-50">
                            <label className="font-medium text-tremor-content-strong dark:text-dark-tremor-content-strong">
                                {t("danger.deleteTitle")}
                            </label>
                        </div>

                        <div className="flex items-center justify-between space-x-10 px-4 py-3 text-gray-500">
                            <p className="text-sm leading-6 text-tremor-content dark:text-dark-tremor-content">
                                {t("danger.deleteDescription")}
                            </p>

                            <Button
                                variant="destructive"
                                disabled={props.campaign.status === CampaignStatus.Done}
                                className="py-1"
                                onClick={handleDelete}
                            >
                                {t("actions.delete")}
                            </Button>
                        </div>
                    </Card>
                </div>
            </section>
        </div>
    );
};
