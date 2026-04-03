"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { useTranslations } from "next-intl";

import { useAuth } from "@/context/AuthContext";
import { useActionNavigationLoading } from "@/context/ActionNavigationLoadingContext";

import { AdminAnalytics } from "@/components/analytics/AdminAnalytics";
import { CompanyAnalytics } from "@/components/analytics/CompanyAnalytics";

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

import { createCampaign } from "@/elements/dashboard/campaigns/fetchData";

function extractErrorMessage(response: any, fallback: string): string {
    if (!response) return fallback;

    if (typeof response.error?.description === "string" && response.error.description.trim()) {
        return response.error.description;
    }

    if (typeof response.error?.message === "string" && response.error.message.trim()) {
        return response.error.message;
    }

    if (typeof response.message === "string" && response.message.trim()) {
        return response.message;
    }

    if (response.errors && typeof response.errors === "object") {
        const messages = Object.values(response.errors)
            .flatMap((value) => (Array.isArray(value) ? value : [value]))
            .filter((value): value is string => typeof value === "string" && value.trim().length > 0);

        if (messages.length > 0) {
            return messages.join(" ");
        }
    }

    return fallback;
}

const Overview = () => {
    const t = useTranslations("Overview");

    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const [campaignName, setCampaignName] = useState<string>("");
    const [errorMessages, setErrorMessages] = useState<string>("");
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const { dashboardManager, authFetch } = useAuth();
    const router = useRouter();
    const { beginActionLoading } = useActionNavigationLoading();

    if (!dashboardManager) return null;

    const handleSubmit = async () => {
        const companyId = dashboardManager?.currentCompany?.companyId;

        console.log("HANDLE SUBMIT START");
        console.log("campaignName", campaignName);
        console.log("companyId", companyId);

        if (!campaignName.trim()) {
            setErrorMessages(t("errors.unexpected"));
            return;
        }

        if (!companyId) {
            setErrorMessages("CompanyId is required.");
            return;
        }

        setIsLoading(true);
        setErrorMessages("");

        try {
            const result = await createCampaign(
                { name: campaignName.trim(), companyId },
                authFetch
            );

            const responseText = await result.text();
            console.log("CREATE STATUS", result.status);
            console.log("CREATE RESPONSE TEXT", responseText);

            let response: any = null;
            try {
                response = responseText ? JSON.parse(responseText) : null;
            } catch {
                response = null;
            }

            if (result.ok) {
                const createdCampaignId =
                    response?.id ??
                    response?.campaignId ??
                    response?.value;

                console.log("CREATED CAMPAIGN ID", createdCampaignId);

                setCampaignName("");
                setErrorMessages("");

                if (createdCampaignId) {
                    beginActionLoading();
                    setIsDialogOpen(false);
                    router.push(`/${locale}/campaigns/${createdCampaignId}`);
                }
                return;
            }

            setErrorMessages(extractErrorMessage(response, t("errors.unexpected")));
        } catch (e) {
            console.error(e);
            setErrorMessages(t("errors.unexpected"));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <section>
            {dashboardManager.isAdministrator && !dashboardManager.currentCompany?.companyId ? <AdminAnalytics /> : null}

            {dashboardManager.currentCompany?.companyId ? (
                <div>
                    <div className="flex justify-center">
                        <Dialog
                            open={isDialogOpen}
                            onOpenChange={(open) => {
                                setIsDialogOpen(open);
                                if (open) {
                                    setCampaignName("");
                                    setErrorMessages("");
                                }
                            }}
                        >
                            <DialogTrigger asChild>
                                <Button
                                    className="h-8 text-sm border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                                    type="button"
                                    variant="primary"
                                >
                                    {t("newCampaign")}
                                </Button>
                            </DialogTrigger>

                            <DialogContent className="sm:max-w-lg" aria-describedby="create new campaign dialog">
                                <DialogHeader>
                                    <DialogTitle>{t("createCampaignTitle")}</DialogTitle>
                                    <p className="text-sm text-gray-400 mt-1">{t("createCampaignDescription")}</p>

                                    <div className="mt-5">
                                        <Input
                                            value={campaignName}
                                            onChange={(e) => setCampaignName(e.target.value)}
                                            className="mx-auto w-full"
                                            hasError={!!errorMessages}
                                            placeholder={t("campaignNamePlaceholder")}
                                            onKeyDown={(e) => {
                                                if (e.key === "Enter" && !isLoading) {
                                                    e.preventDefault();
                                                    handleSubmit();
                                                }
                                            }}
                                        />
                                        {errorMessages && <p className="mt-1 text-xs text-red-600">{errorMessages}</p>}
                                    </div>
                                </DialogHeader>

                                <DialogFooter className="mt-6 flex justify-end sm:justify-end flex-col gap-2">
                                    <DialogClose asChild>
                                        <Button className="mt-2 w-full sm:mt-0 sm:w-fit" variant="secondary">
                                            {t("goBack")}
                                        </Button>
                                    </DialogClose>

                                    <Button
                                        onClick={handleSubmit}
                                        isLoading={isLoading}
                                        disabled={isLoading || !campaignName.trim()}
                                        className="w-full sm:w-fit border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                                    >
                                        {t("create")}
                                    </Button>
                                </DialogFooter>
                            </DialogContent>
                        </Dialog>
                    </div>

                    <CompanyAnalytics companyId={dashboardManager.currentCompany.companyId} />
                </div>
            ) : null}
        </section>
    );
};

export default Overview;