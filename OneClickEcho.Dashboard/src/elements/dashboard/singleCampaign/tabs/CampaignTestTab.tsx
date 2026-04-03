"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { Input } from "@/components/tremor/Input";
import { Divider } from "@/components/tremor/Divider";
import { Button } from "@/components/tremor/Button";

import { CampaignDto, testCampaign, updateCampaign } from "../fetchData";

import { useToast } from "@/lib/useToast";
import { validateCampaignChannels } from "@/lib/utils";

import { PHONE_NUMBER_REGEX_DIRECT } from "@/lib/regex";
import { CampaignStatus } from "@/lib/enums";

interface ICampaignTestTab {
    campaign: CampaignDto;
    setCampaign: (value: CampaignDto) => void;
}

type TestCampaignErrorBody = {
    code?: string;
    message?: string;
    title?: string;
    Code?: string;
    Message?: string;
    Title?: string;
};

export const CampaignTestTab = (props: ICampaignTestTab) => {
    const t = useTranslations("SingleCampaign.Tabs.CampaignTest");
    const tCommon = useTranslations("Common");

    const [testPhoneNumber, setTestPhoneNumber] = useState<string>();
    const [isLoading, setIsLoading] = useState(false);

    const { authFetch } = useAuth();
    const { toast } = useToast();

    const showErrorMessage = (message: string) => {
        toast({
            title: t("toasts.errorTitle"),
            description: message,
            variant: "error",
            duration: 2000
        });
    };

    const testCampaignErrorDescription = (data: TestCampaignErrorBody | undefined) => {
        const code = data?.code ?? data?.Code;
        if (code === "Phone.Blocked") {
            return t("toasts.phoneBlockedDescription");
        }
        return data?.message ?? data?.Message ?? data?.title ?? data?.Title ?? tCommon("somethingWentWrong");
    };

    const sendMessage = async () => {
        if (!testPhoneNumber || !PHONE_NUMBER_REGEX_DIRECT.test(testPhoneNumber)) {
            toast({
                title: t("toasts.invalidPhoneTitle"),
                description: t("toasts.invalidPhoneDescription"),
                variant: "error",
                duration: 2000
            });
            return;
        }

        if (!validateCampaignChannels(props.campaign, showErrorMessage)) return;

        updateCampaign(
            {
                ...props.campaign,
                testPhoneNumber
            },
            authFetch
        ).catch((e) => console.error(e));

        props.setCampaign({
            ...props.campaign,
            testPhoneNumber
        });

        setIsLoading(true);

        try {
            const response = await testCampaign(props.campaign.campaignId, testPhoneNumber, authFetch);
            const data = await response.json();

            if (response.ok) {
                toast({
                    title: t("toasts.successTitle"),
                    description: t("toasts.successDescription"),
                    variant: "success",
                    duration: 2000
                });
            } else {
                toast({
                    title: t("toasts.errorTitle"),
                    description: testCampaignErrorDescription(data as TestCampaignErrorBody),
                    variant: "error",
                    duration: 2000
                });
            }
        } catch (e) {
            toast({
                title: t("toasts.unknownTitle"),
                description: t("toasts.unknownDescription"),
                variant: "error",
                duration: 2000
            });
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        setTestPhoneNumber(props.campaign.testPhoneNumber);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <div>
            <h2 id="campaign-test-phone" className="scroll-mt-10 font-semibold text-gray-900 dark:text-gray-50">
                {t("title")}
            </h2>
            <p className="mt-1 text-sm leading-6 text-gray-500">{t("subtitle")}</p>

            <div className="flex items-center gap-x-3 my-4">
                <Input
                    disabled={props.campaign.status !== CampaignStatus.Draft}
                    type="tel"
                    value={testPhoneNumber}
                    className="w-[50%]"
                    onChange={(e) => setTestPhoneNumber(e.target.value)}
                />
            </div>

            <Divider />

            <div className="flex justify-end gap-x-3">
                <Button
                    disabled={props.campaign.status !== CampaignStatus.Draft || !testPhoneNumber}
                    className="border-transparent bg-green-600 text-white outline-green-500 hover:bg-green-500 disabled:bg-green-100 disabled:text-gray-400 dark:bg-green-500 dark:text-gray-900 dark:outline-green-500 dark:hover:bg-green-600 disabled:dark:bg-green-800 disabled:dark:text-green-400"
                    type="button"
                    color="primary"
                    isLoading={isLoading}
                    onClick={sendMessage}
                >
                    {t("actions.send")}
                </Button>
            </div>
        </div>
    );
};
